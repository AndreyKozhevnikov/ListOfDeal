using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    [DebuggerDisplay("Name = {Name}")]
    public class MyAction:MyBindableBase {
      public  Action parentEntity;
        public MyAction(Action _parentEntity) {
            parentEntity = _parentEntity;
        }

        public MyAction() {
            var v = MainViewModel.generalEntity.Actions.Create();
            parentEntity = v;
            DateCreated = DateTime.Now;
        }

        public int StatusId {
            get {
                return parentEntity.StatusId;
            }
            set {
                parentEntity.StatusId = value;
                if (value == 4) {
                    this.parentEntity.CompleteTime = DateTime.Now;
                }
                RaisePropertyChanged("StatusId");
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
            }
        }
        public string Name {
            get {
                return parentEntity.Name;
            }
            set {
                parentEntity.Name = value;
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

    }
}
