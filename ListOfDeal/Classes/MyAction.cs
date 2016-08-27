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
    public class MyAction:MyBindableBase,IDataErrorInfo {
      public  Action parentEntity;
        public MyAction(Action _parentEntity) {
            parentEntity = _parentEntity;
        }

        public MyAction() {
            var v = MainViewModel.generalEntity.Actions.Create();
            parentEntity = v;
            DateCreated = DateTime.Now;
        }
        public string Name {
            get {
                return parentEntity.Name;
            }
            set {
                parentEntity.Name = value;
                RaisePropertyChanged("Name");
            }
        }
        public ActionsStatusEnum Status {
            get {
                return (ActionsStatusEnum)parentEntity.StatusId;
            }
            set {
                parentEntity.StatusId =(int) value;
                if (value == ActionsStatusEnum.Completed) {
                    this.parentEntity.CompleteTime = DateTime.Now;
                    this.parentEntity.WLTaskStatus = 2;
                    this.IsActive = false;
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
        public int? TriggerId {
            get {
                return parentEntity.TriggerId;
            }
            set {
                parentEntity.TriggerId = value;
            }
        }
        public DateTime? ScheduledTime {
            get {
                return parentEntity.ScheduledTime;
            }
            set {
                parentEntity.ScheduledTime = value;
            }
        }
        public int? DelegatedTo {
            get {
                return parentEntity.DelegatedTo;
            }
            set {
                parentEntity.DelegatedTo = value;
            }
        }
        public string Comment {
            get {
                return parentEntity.Comment;
            }
            set {
                parentEntity.Comment = value;
                RaisePropertyChanged("Comment");
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
                if (Status == ActionsStatusEnum.Delegated && DelegatedTo == null)
                    return "delegated required";
                return null;
            }
        }

        public string this[string columnName] {
            get {
                if (columnName == "Status") {
                    if (Status == ActionsStatusEnum.Scheduled && ScheduledTime == null)
                        return "scheduled required";
                    if (Status == ActionsStatusEnum.Delegated && DelegatedTo == null)
                        return "delegated required";
                }
                return null;
            }
        }
        public DateTime? CompleteTime {
            get {
                return parentEntity.CompleteTime;
            }
        }
        public int? WLId {
            get {
                return parentEntity.WLId;
            }
            set {
                parentEntity.WLId = value;
            }
        }
        //public WLTaskStatusEnum WLTaskStatus {
        //    get {
        //        return (WLTaskStatusEnum) parentEntity.WLTaskStatus;
        //    }
        //    set {
        //        parentEntity.WLTaskStatus = (int)value;
        //    }
        //}
        internal void CopyProperties(MyAction act) {
            this.Name = act.Name;
            this.Status = act.Status;
            this.TriggerId = act.TriggerId;
            this.DelegatedTo = act.DelegatedTo;
            this.ScheduledTime = act.ScheduledTime;
            this.IsActive = act.IsActive;
            this.Comment = act.Comment;
            this.DateCreated = act.DateCreated;
        }
    }

    public enum ActionsStatusEnum {
        Waited=1,
        Scheduled=2,
        Delegated=3,
        Completed=4
    }
    //public enum WLTaskStatusEnum {
    //    NoWLTask=0,
    //    WLTaskActive=1,
    //    WLTaskNeedToDelete=2
    //}
    [TestFixture] //todo -case action become active and then become inactive
    public class MyActionTest {
        [Test]
        public void CompleteAction() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.IsActive = true;
            //act
            act.Status = ActionsStatusEnum.Completed;
            //assert
            Assert.AreEqual(2, act.parentEntity.WLTaskStatus);
            Assert.AreNotEqual(null, act.parentEntity.CompleteTime);
            Assert.AreEqual(false, act.parentEntity.IsActive);
        }
    }
}
