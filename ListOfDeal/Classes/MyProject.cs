using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public class MyProject : MyBindableBase {
        public Project parentEntity;
        public MyProject(Project _p) {
            parentEntity = _p;
            InitiateProject();
        }


        public MyProject() {
            //  parentEntity = MainViewModel.generalEntity.Projects.Create();
            parentEntity = MainViewModel.DataProvider.CreateProject();
            InitiateProject();
        }

        private void InitiateProject() {
            Actions = new ObservableCollection<MyAction>();
            var listAction = parentEntity.Actions.Where(x => x.StatusId2 != (int)ActionsStatusEnum2.Done).OrderBy(x => x.OrderNumber);
            foreach (var a in listAction) {
                MyAction act = new MyAction(a);
                act.PropertyChanged += act_PropertyChanged;
                Actions.Add(act);
            }

        }
        public void AddAction(MyAction act) {
            if (IsSimpleProject && Actions.Count == 1) {
                var targetAction = Actions[0];
                targetAction.CopyProperties(act);
                IsSimpleProject = false;
            }
            else {
                if (Actions.Count == 0) { //set inwork? !!!
                    act.OrderNumber = 0;
                    act.Status2 = ActionsStatusEnum2.InWork;
                }
                else {
                    var maxOrderNumber = Actions.Max(x => x.OrderNumber);
                    act.OrderNumber = maxOrderNumber + 1;
                    if (Actions.Where(x => x.Status2 == ActionsStatusEnum2.InWork).Count() == 0)
                        act.Status2 = ActionsStatusEnum2.InWork;
                }
                act.PropertyChanged += act_PropertyChanged;
                Actions.Add(act);
                parentEntity.Actions.Add(act.parentEntity);
                RaisePropertyChanged("Actions");
            }
        }

        void act_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Status2") {
                MyAction act = sender as MyAction;
                if (!(act.Status2 == ActionsStatusEnum2.InWork)) {
                    bool isThereIsNoActiveActions = Actions.Where(x => x.Status2 == ActionsStatusEnum2.InWork).Count() == 0;
                    if (isThereIsNoActiveActions) {
                        var targetAct = Actions.Where(x => x.Status2 == ActionsStatusEnum2.Delay).OrderBy(x => x.OrderNumber).FirstOrDefault();
                        if (targetAct != null) {
                            targetAct.Status2 = ActionsStatusEnum2.InWork;
                        }
                        if (this.IsSimpleProject) {
                            this.Status = ProjectStatusEnum.Done;
                        }
                        RaisePropertyChanged("Actions");
                    }
                }
            }
            if (e.PropertyName == "ScheduledTime" || e.PropertyName == "IsActive") {
                RaisePropertyChanged("Actions");
            }
        }
        public void DeleteAction(MyAction act) {
            Actions.Remove(act);
            parentEntity.Actions.Remove(act.parentEntity);
        }

        public void Save() {
            if (parentEntity.Id <= 0)
                MainViewModel.DataProvider.AddProject(parentEntity);
            // MainViewModel.generalEntity.Projects.Add(parentEntity);
            MainViewModel.SaveChanges();
        }

        public string Name {
            get {
                return parentEntity.Name;
            }
            set {
                parentEntity.Name = value;
            }
        }
        public DateTime DateCreated {
            get {
                return parentEntity.DateCreated;
            }
            set {
                parentEntity.DateCreated = value;
            }
        }
        public int TypeId {
            get {
                return parentEntity.TypeId;
            }
            set {
                parentEntity.TypeId = value;

                RaisePropertyChanged("TypeId");
            }
        }
        public ProjectStatusEnum Status {
            get {
                return (ProjectStatusEnum)parentEntity.StatusId;
            }
            set {
                parentEntity.StatusId = (int)value;

                if ((ProjectStatusEnum)value != ProjectStatusEnum.InWork) {
                    foreach (var act in Actions) {
                        act.SetDeleteTaskIfNeeded();
                    }
                    if ((ProjectStatusEnum)value == ProjectStatusEnum.Done) {
                        this.parentEntity.CompleteTime = DateTime.Now;
                    }
                }
                RaisePropertyChanged("Status");
            }
        }
        public string Comment {
            get {
                return parentEntity.Comment;
            }
            set {
                parentEntity.Comment = value;
            }
        }
        public string DesiredResult {
            get {
                return parentEntity.DesiredResult;
            }
            set {
                parentEntity.DesiredResult = value;
            }
        }
        public bool IsSimpleProject {
            get {
                return parentEntity.IsSimpleProject;
            }
            set {
                if (value && Actions.Count > 1)
                    return;
                parentEntity.IsSimpleProject = value;
                foreach (var act in Actions) {
                    act.SetWLStatusUpdatedIfNeeded();
                }
                RaisePropertyChanged("IsSimpleProject");
            }
        }
        public int Id {
            get {
                return parentEntity.Id;
            }
        }
        public ObservableCollection<MyAction> Actions { get; set; }
    }




    [TestFixture]
    public class MyProjectTests {
        [Test]
        public void SetStatusDone() {
            //arrange
            MyProject pr = new MyProject(new Project());
            //act
            pr.Status = ProjectStatusEnum.Done;
            //assert
            Assert.AreNotEqual(null, pr.parentEntity.CompleteTime);
        }

        [Test]
        public void SetProjectIsNotInWork() {
            //arrange
            MyProject pr = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            myAction.WLId = "123";
            var myAction1 = new MyAction(new Action());
            pr.Actions.Add(myAction);
            pr.Actions.Add(myAction1);
            //act
            pr.Status = ProjectStatusEnum.Delayed;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, myAction.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);


        }
        [Test]
        public void SetProjectIsNotInWork_Done() {
            //arrange
            MyProject pr = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            myAction.WLId = "123";
            var myAction1 = new MyAction(new Action());
            pr.Actions.Add(myAction);
            pr.Actions.Add(myAction1);
            //act
            pr.Status = ProjectStatusEnum.Done;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, myAction.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);

            Assert.AreNotEqual(null, pr.parentEntity.CompleteTime);
        }

        [Test]
        public void AddActionToSimpleProject_ShouldNotSetWLStatusToNeedUpdate() {
            //arrange
            var proj = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            proj.Actions.Add(myAction);
            proj.IsSimpleProject = true;
            //act
            var act2 = new MyAction(new Action());
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(1, proj.Actions.Count);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
        }
        [Test]
        public void MakeActionsActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action());
            var act2 = new MyAction(new Action());
            var act3 = new MyAction(new Action());
            //act1
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            //assert1
            Assert.AreEqual(ActionsStatusEnum2.InWork, act1.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Delay, act2.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Delay, act2.Status2);
        }


        [Test]
        public void ShouldNotMakeCompletedActionActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.Delay };
            var act2 = new MyAction(new Action()) { Name = "act2", Status2 = ActionsStatusEnum2.Delay };
            var act3 = new MyAction(new Action()) { Name = "act3", Status2 = ActionsStatusEnum2.Delay };
            var act4 = new MyAction(new Action()) { Name = "act4", Status2 = ActionsStatusEnum2.Delay };
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            proj.AddAction(act4);
            //act2
            act2.Status2 = ActionsStatusEnum2.Done;
            act1.Status2 = ActionsStatusEnum2.Rejected;
            //assert2
            Assert.AreEqual(ActionsStatusEnum2.Rejected, act1.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Done, act2.Status2);
            Assert.AreEqual(ActionsStatusEnum2.InWork, act3.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Delay, act4.Status2);
        }
        [Test]
        public void AddActionMakeActionActiveIfThereAreNoOtherActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.Delay };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(ActionsStatusEnum2.InWork, act2.Status2);

        }
        [Test]
        public void AddActionNotMakeActionActiveIfThereAreActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.InWork };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(ActionsStatusEnum2.Delay, act2.Status2);

        }
    }
}
