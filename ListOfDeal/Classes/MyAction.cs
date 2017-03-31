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
    public class MyAction : MyBindableBase, IDataErrorInfo {
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
        public ActionsStatusEnum Status {
            get {
                return (ActionsStatusEnum)parentEntity.StatusId;
            }
            set {
                var intVal = (int)value;
                if (parentEntity.StatusId == intVal)
                    return;
                parentEntity.StatusId = intVal;
                if (value != ActionsStatusEnum.Scheduled && this.ScheduledTime != null) {
                    this.ScheduledTime = null;
                }
                if (value == ActionsStatusEnum.Completed) {
                    this.CompleteTime = DateTime.Now;
                    this.IsActive = false;
                    SetDeleteTaskIfNeeded();
                }
                else {
                    this.CompleteTime = null;
                }
                RaisePropertyChanged("Status");
            }
        }
        public bool IsActive {
            get {
                return parentEntity.IsActive;
            }
            set {
                parentEntity.IsActive = value;
                if (!value) {
                    SetDeleteTaskIfNeeded();
                }
                RaisePropertyChanged("IsActive");
            }
        }

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
                if (value.HasValue) {
                    this.Status = ActionsStatusEnum.Scheduled;
                }
                HandlePropertyChanges("ScheduledTime");
                this.IsActive = true;
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

        public string Error {
            get {
                if (Status == ActionsStatusEnum.Scheduled && ScheduledTime == null)
                    return "scheduled required";

                return null;
            }
        }

        public string this[string columnName] {
            get {
                if (columnName == "Status") {
                    if (Status == ActionsStatusEnum.Scheduled && ScheduledTime == null)
                        return "scheduled required";

                }
                return null;
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

        public void SetDeleteTaskIfNeeded() {
            if (WLId != null)
                this.WLTaskStatus = WLTaskStatusEnum.DeletingNeeded;
        }
        internal void CopyProperties(MyAction act) {
            this.Name = act.Name;
            this.Status = act.Status;
            this.ScheduledTime = act.ScheduledTime;
            this.IsActive = act.IsActive;
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
            act.IsActive = true;
            //act
            act.Status = ActionsStatusEnum.Completed;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act.WLTaskStatus);
            Assert.AreNotEqual(null, act.parentEntity.CompleteTime);
            Assert.AreEqual(false, act.parentEntity.IsActive);
        }
        [Test]
        public void CompleteAction_withoutWLId() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.IsActive = true;
            //act
            act.Status = ActionsStatusEnum.Completed;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);

        }

        [Test]
        public void SetIsActiveSetWLStatus() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.IsActive = true;
            act.parentEntity.WLId = "123";
            //act
            act.IsActive = false;
            //asssert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act.WLTaskStatus);
        }

        [Test]
        public void SetIsActiveSetWLStatus_3() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.IsActive = true;
            //act
            act.IsActive = false;
            act.IsActive = true;
            //asssert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
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
            act.Status = ActionsStatusEnum.Scheduled;
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

            act.Status = ActionsStatusEnum.Scheduled;
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
            Assert.AreEqual(ActionsStatusEnum.Scheduled, act.Status);
        }
        [Test]
        public void SetStatusToNonScheduledShoulNullScheduledTime() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status = ActionsStatusEnum.Scheduled;
            act.ScheduledTime = new DateTime(2016, 9, 10);
            //act
            act.Status = ActionsStatusEnum.Waited;
            //assert
            Assert.AreEqual(false, act.ScheduledTime.HasValue);
        }
        [Test]
        public void SetStatusToNonScheduledShoulNotChangeScheduledTimeIfItIsAlreadyNull() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status = ActionsStatusEnum.Waited;
            act.WLId = "12";
            //act
            act.Status = ActionsStatusEnum.Waited;
            //assert
            Assert.AreNotEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetStatusToSameShouldNotChangeStatus() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status = ActionsStatusEnum.Scheduled;
            string tmpSting = null;
            act.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "Status")
                    tmpSting = "IsSet";
            };
            //act
            act.Status = ActionsStatusEnum.Scheduled;
            //assert
            Assert.AreEqual(null, tmpSting);
        }

        [Test]
        public void SetTimeShouldSetIsActive() {
            //arrange
            MyAction act = new MyAction(new Action());
            //act
            act.ScheduledTime = DateTime.Now;
            //assert
            Assert.AreEqual(true, act.IsActive);
        }
        [Test]
        public void SetStatusToActiveAgainShouldClearCompletedTime() {
            //arrange
            MyAction act = new MyAction(new Action());
            //act
            act.Status = ActionsStatusEnum.Completed;
            act.Status = ActionsStatusEnum.Waited;
            //assert
            Assert.AreEqual(null, act.CompleteTime);

        }
    }

}
