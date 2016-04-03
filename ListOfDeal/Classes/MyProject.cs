using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public class MyProject :MyBindableBase {
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
            var listAction = parentEntity.Actions.OrderBy(x => x.OrderNumber);
            foreach (var a in listAction) {
                MyAction act = new MyAction(a);
                act.PropertyChanged+=act_PropertyChanged;
                Actions.Add(act);
            }
        }
        public void AddAction(MyAction act) {
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
        }

        void act_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "StatusId") {
                MyAction act = sender as MyAction;
                if (act.StatusId!=4)
                    return;
                var ind = act.OrderNumber + 1;
                var targetAct = Actions.Where(x => x.OrderNumber == ind).FirstOrDefault();
                if (targetAct != null&& !targetAct.IsActive) {
                    targetAct.IsActive = true;
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
            MainViewModel.generalEntity.SaveChanges();
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
            }
        }
        public int StatusId {
            get {
                return parentEntity.StatusId;
            }
            set {
                parentEntity.StatusId = value;
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

        public ObservableCollection<MyAction> Actions { get; set; }
    }
}
