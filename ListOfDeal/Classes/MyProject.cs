using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public class MyProject : MyBindableBase {
        Project parentEntity;
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
                var ind = act.OrderNumber + 1;
                var targetAct = Actions.Where(x => x.OrderNumber == ind).FirstOrDefault();
                if (targetAct != null && !targetAct.IsActive) {
                    targetAct.IsActive = true;
                }
                if (this.IsSimpleProject) {
                    this.Status = ProjectStatusEnum.Done;
                }

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
                if ((ProjectStatusEnum)value == ProjectStatusEnum.Done) {
                    this.parentEntity.CompleteTime = DateTime.Now;
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
}
