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
            foreach (var a in parentEntity.Actions) {
                Actions.Add(new MyAction(a));
            }
        }
        public void AddAction(MyAction act) {
            Actions.Add(act);
            parentEntity.Actions.Add(act.parentEntity);
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
