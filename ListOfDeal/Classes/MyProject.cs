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
            parentEntity = MainViewModel.generalEntity.Projects.Create();
            InitiateProject();
        }

        private void InitiateProject() {
            Actions = new ObservableCollection<MyAction>();
            var listAction = parentEntity.Actions.Where(x => x.StatusId != 4).OrderBy(x => x.OrderNumber);
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
                targetAction.IsActive = true;
                IsSimpleProject = false;
            }
            else {
                if (Actions.Count == 0) {
                    act.OrderNumber = 0;
                    act.IsActive = true;
                }
                else {
                    var maxOrderNumber = Actions.Max(x => x.OrderNumber);
                    act.OrderNumber = maxOrderNumber + 1;
                    if (Actions.Where(x => x.IsActive).Count() == 0)
                        act.IsActive = true;
                }
                act.PropertyChanged += act_PropertyChanged;
                Actions.Add(act);
                parentEntity.Actions.Add(act.parentEntity);
                RaisePropertyChanged("Actions");
            }
        }

        void act_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Status") {
                MyAction act = sender as MyAction;
                if (act.Status != ActionsStatusEnum.Completed)
                    return;
                bool needSetIsActive = Actions.Where(x => x.IsActive).Count() == 0;
                if (!needSetIsActive)
                    return;
                var targetAct = Actions.Where(x => x.Status != ActionsStatusEnum.Completed && !x.IsActive).OrderBy(x => x.OrderNumber).FirstOrDefault();
                if (targetAct != null) {
                    targetAct.IsActive = true;
                }
                if (this.IsSimpleProject) {
                    this.Status = ProjectStatusEnum.Done;
                }
                RaisePropertyChanged("Actions");
            }
        }
        public void DeleteAction(MyAction act) {
            Actions.Remove(act);
            parentEntity.Actions.Remove(act.parentEntity);
        }

        public void Save() {
            if (parentEntity.Id <= 0)
                MainViewModel.generalEntity.Projects.Add(parentEntity);
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


    public enum ProjectStatusEnum {
        InWork = 1, Delayed = 2, Done = 3
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
            Assert.AreEqual(true, act1.IsActive);
            Assert.AreEqual(false, act2.IsActive);
            Assert.AreEqual(false, act2.IsActive);
        }


        [Test]
        public void ShouldNotMakeCompletedActionActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1" };
            var act2 = new MyAction(new Action()) { Name = "act2" };
            var act3 = new MyAction(new Action()) { Name = "act3" };
            var act4 = new MyAction(new Action()) { Name = "act4" };
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            proj.AddAction(act4);
            //act2
            act2.Status = ActionsStatusEnum.Completed;
            act1.Status = ActionsStatusEnum.Completed;
            //assert2
            Assert.AreEqual(false, act1.IsActive);
            Assert.AreEqual(false, act2.IsActive);
            Assert.AreEqual(true, act3.IsActive);
            Assert.AreEqual(false, act4.IsActive);
        }
        [Test]
        public void AddActionMakeActionActiveIfThereAreNoOtherActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", IsActive = false };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(true, act2.IsActive);

        }
        [Test]
        public void AddActionNotMakeActionActiveIfThereAreActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", IsActive = true };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(false, act2.IsActive);

        }
    }
}
