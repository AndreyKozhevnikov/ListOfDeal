using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    [DebuggerDisplay("Name = {Name}")]
    public class MyAction : MyBindableBase {
        public Action parentEntity;
        public MyAction(Action _parentEntity) {
            parentEntity = _parentEntity;
            changedProperties = new List<string>();
        }
        public List<string> changedProperties;
        void HandlePropertyChanges(string st) {
            changedProperties.Add(st);
            SetWLStatusUpdatedIfNeeded();
            RaisePropertyChanged(st);
        }
        public MyAction() {
            var v = MainViewModel.DataProvider.CreateAction();
            parentEntity = v;
            DateCreated = DateTime.Now;
            changedProperties = new List<string>();
        }
        public string Name {
            get {
                return parentEntity.Name;
            }
            set {
                parentEntity.Name = value;
                HandlePropertyChanges("Name");
            }
        }
        public ActionsStatusEnum2 Status2 {
            get {
                return (ActionsStatusEnum2)parentEntity.StatusId2;
            }
            set {
                if (parentEntity.StatusId2 == (int)value)
                    return;
                parentEntity.StatusId2 = (int)value;
                if (value == ActionsStatusEnum2.Done || value == ActionsStatusEnum2.Rejected) {
                    this.CompleteTime = DateTime.Now;
                    SetDeleteTaskIfNeeded();
                }
                else {
                    this.CompleteTime = null;
                }
                if (value != ActionsStatusEnum2.InWork && this.ScheduledTime != null) {
                    this.ScheduledTime = null;
                }
            }
        }
        //public ActionsStatusEnum Status {
        //    get {
        //        return (ActionsStatusEnum)parentEntity.StatusId;
        //    }
        //    set {
        //        var intVal = (int)value;
        //        if (parentEntity.StatusId == intVal)
        //            return;
        //        parentEntity.StatusId = intVal;
        //        if (value != ActionsStatusEnum.Scheduled && this.ScheduledTime != null) {
        //            this.ScheduledTime = null;
        //        }
        //        if (value == ActionsStatusEnum.Completed) {
        //            this.CompleteTime = DateTime.Now;
        //            this.IsActive = false;
        //            SetDeleteTaskIfNeeded();
        //        }
        //        else {
        //            this.CompleteTime = null;
        //        }
        //        RaisePropertyChanged("Status");
        //    }
        //}
        //public bool IsActive {
        //    get {
        //        return parentEntity.IsActive;
        //    }
        //    set {
        //        parentEntity.IsActive = value;
        //        if (!value) {
        //            SetDeleteTaskIfNeeded();
        //        }
        //        RaisePropertyChanged("IsActive");
        //    }
        //}

        public DateTime DateCreated {
            get {
                return parentEntity.DateCreated;
            }
            set {
                parentEntity.DateCreated = value;
                RaisePropertyChanged("DateCreated");
            }
        }

        public DateTime? ScheduledTime {
            get {
                return parentEntity.ScheduledTime;
            }
            set {
                if (value == parentEntity.ScheduledTime)
                    return;
                parentEntity.ScheduledTime = value;
                if (value.HasValue) { //!!! && statuse!=inwork?
                    this.Status2 = ActionsStatusEnum2.InWork;
                }
                HandlePropertyChanges("ScheduledTime");
            }
        }

        public void SetWLStatusUpdatedIfNeeded() {
            if (WLId != null)
                WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
        }


        public string Comment {
            get {
                return parentEntity.Comment;
            }
            set {
                parentEntity.Comment = value;
                HandlePropertyChanges("Comment");
            }
        }
        public int OrderNumber {
            get {
                return parentEntity.OrderNumber;
            }
            set {
                parentEntity.OrderNumber = value;
                RaisePropertyChanged("OrderNumber");
            }
        }
        public bool ToBuy {
            get {
                return parentEntity.ToBuy;
            }
            set {
                parentEntity.ToBuy = value;
                HandlePropertyChanges("ToBuy");
            }
        }

        public string ProjectName {
            get {
                return parentEntity.Project.Name;
            }
        }
        public int ProjectType {
            get {
                return parentEntity.Project.TypeId;
            }
        }
        public int ProjectId {
            get {
                return parentEntity.Project.Id;
            }
        }


        public DateTime? CompleteTime {
            get {
                return parentEntity.CompleteTime;
            }
            set {
                parentEntity.CompleteTime = value;
            }
        }
        public int? WLTaskRevision {
            get {
                return parentEntity.WLTaskRevision;
            }
            set {
                parentEntity.WLTaskRevision = value;
            }
        }
        public string WLId {
            get {
                return parentEntity.WLId;
            }
            set {
                parentEntity.WLId = value;
            }
        }
        public WLTaskStatusEnum WLTaskStatus {
            get {
                return (WLTaskStatusEnum)parentEntity.WLTaskStatus;
            }
            set {
                if (WLId != null)
                    parentEntity.WLTaskStatus = (int)value;
            }
        }
        public bool IsMajor {
            get {
                return parentEntity.IsMajor;
            }
            set {
                parentEntity.IsMajor = value;
                HandlePropertyChanges("IsMajor");
            }
        }
        public bool IsActive2 {
            get {
                return Status2 != ActionsStatusEnum2.Done && Status2 != ActionsStatusEnum2.Rejected;
            }
        }

        public void SetDeleteTaskIfNeeded() {
            if (WLId != null)
                this.WLTaskStatus = WLTaskStatusEnum.DeletingNeeded;
        }
        internal void CopyProperties(MyAction act) {
            this.Name = act.Name;
            this.Status2 = act.Status2;
            this.ScheduledTime = act.ScheduledTime;
            this.Comment = act.Comment;
            this.DateCreated = act.DateCreated;
        }

        internal string GetWLTitle() {
            string title;
            if (parentEntity.Project.IsSimpleProject)
                title = this.Name;

            else
                title = string.Format("{0} - {1}", this.ProjectName, this.Name);
            return title;
        }

        public override string ToString() {
            return string.Format("Action {0} {1}", GetWLTitle(), parentEntity.Id);
        }
    }


    [TestFixture]
    public class MyActionTest {
        [Test]
        public void CompleteAction() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.WLId = "123";
            act.Status2 = ActionsStatusEnum2.InWork;
            MyAction act2 = new MyAction(new Action());
            act2.WLId = "123";
            act2.Status2 = ActionsStatusEnum2.InWork;
            //act
            act.Status2 = ActionsStatusEnum2.Done;
            act2.Status2 = ActionsStatusEnum2.Done;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act.WLTaskStatus);
            Assert.AreNotEqual(null, act.parentEntity.CompleteTime);

            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act2.WLTaskStatus);
            Assert.AreNotEqual(null, act2.parentEntity.CompleteTime);
        }

        [Test]
        public void CompleteAction_withoutWLId() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status2 = ActionsStatusEnum2.InWork;
            MyAction act2 = new MyAction(new Action());
            act2.Status2 = ActionsStatusEnum2.InWork;
            //act
            act.Status2 = ActionsStatusEnum2.Done;
            act.Status2 = ActionsStatusEnum2.Rejected;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act2.WLTaskStatus);

        }


        [Test]
        public void SetWLStatusWhenNameIsChanged() {
            //arrange
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "1";
            //act
            act.Name = "NewName1";
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenNameIsChanged_andThereIsNoWlId() {
            //arrange
            MyAction act = new MyAction(new Action() { Name = "Name1" });

            //act
            act.Name = "NewName1";
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenScheduledDateIsChanged() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.WLId = "1";
            act.Status2 = ActionsStatusEnum2.InWork;
            act.ScheduledTime = new DateTime(2016, 1, 1);
            //act
            act.ScheduledTime = new DateTime(2016, 1, 2);
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenScheduledDateIsChanged_andThereIsNoWlTask() {
            //arrange
            MyAction act = new MyAction(new Action());

            act.Status2 = ActionsStatusEnum2.InWork;
            act.ScheduledTime = new DateTime(2016, 1, 1);
            //act
            act.ScheduledTime = new DateTime(2016, 1, 2);
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenIsSimpePropertyIsChanged() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "2";
            proj.Actions.Add(act);
            //act
            proj.IsSimpleProject = false;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void PreventSettingIsSimpleTrueWhenThereAreManyAcitons() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            MyAction act1 = new MyAction(new Action());
            proj.Actions.Add(act1);
            MyAction act2 = new MyAction(new Action());
            proj.Actions.Add(act2);
            //act
            proj.IsSimpleProject = true;
            //assert
            Assert.AreEqual(false, proj.IsSimpleProject);
        }
        [Test]
        public void SetWLStatusWhenCommentPropertyIsChanged() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "2";
            proj.Actions.Add(act);
            //act
            act.Comment = "new comment";
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenIsMajorPropertyIsChanged() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "2";
            proj.Actions.Add(act);
            //act
            act.IsMajor = true;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetScheduledTimeSetStatusToScheduled() {
            //arrange
            MyAction act = new MyAction(new Action());
            //act
            act.ScheduledTime = new DateTime(2016, 9, 10);
            //assert
            Assert.AreEqual(ActionsStatusEnum2.InWork, act.Status2);
        }

        [Test]
        public void SetStatusToNonScheduledShoulNullScheduledTime() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status2 = ActionsStatusEnum2.InWork;
            act.ScheduledTime = new DateTime(2016, 9, 10);
            //act
            act.Status2 = ActionsStatusEnum2.Delay;
            //assert
            Assert.AreEqual(false, act.ScheduledTime.HasValue);
        }
        [Test]
        public void SetStatusToNonScheduledShoulNotChangeScheduledTimeIfItIsAlreadyNull() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status2 = ActionsStatusEnum2.InWork;
            act.WLId = "12";
            //act
            act.Status2 = ActionsStatusEnum2.Delay;
            //assert
            Assert.AreNotEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetStatusToSameShouldNotChangeStatus() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status2 = ActionsStatusEnum2.InWork;
            string tmpSting = null;
            act.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "Status")
                    tmpSting = "IsSet";
            };
            //act
            act.Status2 = ActionsStatusEnum2.InWork;
            //assert
            Assert.AreEqual(null, tmpSting);
        }


        [Test]
        public void SetStatusToActiveAgainShouldClearCompletedTime() {
            //arrange
            MyAction act = new MyAction(new Action());
            MyAction act2 = new MyAction(new Action());
            //act
            act.Status2 = ActionsStatusEnum2.Done;
            act.Status2 = ActionsStatusEnum2.InWork;
            act2.Status2 = ActionsStatusEnum2.Rejected;
            act2.Status2 = ActionsStatusEnum2.InWork;
            //assert
            Assert.AreEqual(null, act.CompleteTime);
            Assert.AreEqual(null, act2.CompleteTime);

        }
    }

}
