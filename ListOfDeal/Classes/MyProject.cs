using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            var listAction = parentEntity.Actions.OrderBy(x => x.OrderNumber);
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
                if (Actions.Count == 0) {
                    act.OrderNumber = 0;
                    act.Status = ActionsStatusEnum.InWork;
                }
                else {
                    var maxOrderNumber = Actions.Max(x => x.OrderNumber);
                    act.OrderNumber = maxOrderNumber + 1;
                    if (GetIsThereIsNoActiveActions())
                        act.Status = ActionsStatusEnum.InWork;
                }
                act.PropertyChanged += act_PropertyChanged;
                Actions.Add(act);
                parentEntity.Actions.Add(act.parentEntity);
                RaisePropertyChanged("Actions");
            }
        }
        bool GetIsThereIsNoActiveActions() {
            return Actions.Where(x => x.Status == ActionsStatusEnum.InWork).Count() == 0;
        }
        void act_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            MyAction act = sender as MyAction;
            if (e.PropertyName == "Status") {
                if (!(act.Status == ActionsStatusEnum.InWork)) {
                    bool isThereIsNoActiveActions = GetIsThereIsNoActiveActions();
                    if (isThereIsNoActiveActions) {
                        var targetAct = Actions.Where(x => x.Status == ActionsStatusEnum.InWork || x.Status == ActionsStatusEnum.Delay).OrderBy(x => x.OrderNumber).FirstOrDefault();
                        if (targetAct != null) {
                            targetAct.Status = ActionsStatusEnum.InWork;
                        }
                        if (this.IsSimpleProject) {
                            if (act.Status == ActionsStatusEnum.Done)
                                this.Status = ProjectStatusEnum.Done;
                            if (act.Status == ActionsStatusEnum.Rejected)
                                this.Status = ProjectStatusEnum.Rejected;
                        }
                    }
                }
                RaisePropertyChanged("Actions");
            }
            if (e.PropertyName == "ScheduledTime") {
                if (act.ScheduledTime.HasValue)
                    this.Status = ProjectStatusEnum.InWork;
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
                    if ((ProjectStatusEnum)value == ProjectStatusEnum.Done || (ProjectStatusEnum)value == ProjectStatusEnum.Rejected) {
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





}
