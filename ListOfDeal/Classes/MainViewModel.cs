using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListOfDeal {
    public class MainViewModel:INotifyPropertyChanged {
        public MainViewModel() {
            InitializeData();
        }
        ListOfDealBaseEntities generalEntity;
        ICommand _addNewProjectCommand;
        ICommand _addActionCommand;

     
        Project _currentProject;
        Action _currentAction;
        Project _selectedProject;

  
  
        public ICommand AddNewProjectCommand {
            get {
                if (_addNewProjectCommand == null)
                    _addNewProjectCommand = new DelegateCommand(AddNewProject);
                
                return _addNewProjectCommand; }
        }

        public ICommand AddActionCommand {
            get {
                if (_addActionCommand == null)
                    _addActionCommand = new DelegateCommand(AddAction);
                return _addActionCommand; }
        }

   

        public ObservableCollection<Project> Projects { get; set; }
        public Project CurrentProject {
            get { return _currentProject; }
            set { _currentProject = value;
            RaisePropertyChanged("CurrentProject");
            }
        }
        public Action CurrentAction {
            get { return _currentAction; }
            set { _currentAction = value;
            RaisePropertyChanged("CurrentAction");
            }
        }
        public Project SelectedProject {
            get { return _selectedProject; }
            set { _selectedProject = value;
            RaisePropertyChanged("SelectedProject");
            }
        }
        public ObservableCollection<ProjectType> ProjectTypes { get; set; }
        public ObservableCollection<ProjectStatus> ProjectStatuses { get; set; }
        public ObservableCollection<ActionTrigger> ActionTriggers { get; set; }
        public ObservableCollection<ActionStatus> ActionStatuses { get; set; }
        public ObservableCollection<DelegatePerson> DelegatePersons { get; set; }
        
        void InitializeData() {
            ConnectToDataBase();
            Projects = new ObservableCollection<Project>(generalEntity.Projects);
            ProjectTypes = new ObservableCollection<ProjectType>(generalEntity.ProjectTypes);
            ProjectStatuses = new ObservableCollection<ProjectStatus>(generalEntity.ProjectStatuses);
            ActionTriggers = new ObservableCollection<ActionTrigger>(generalEntity.ActionTriggers);
            ActionStatuses = new ObservableCollection<ActionStatus>(generalEntity.ActionStatuses);
            DelegatePersons = new ObservableCollection<DelegatePerson>(generalEntity.DelegatePersons);
            CreateNewProject();
            CreateNewAction();

        }

     

        private void ConnectToDataBase() {
           
            string machineName = System.Environment.MachineName;
            if (machineName == "KOZHEVNIKOV-W8") {
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesWork");
            }
            else {
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesHome");
            }
        }

        private void CreateNewProject() {
            CurrentProject = generalEntity.Projects.Create();
            CurrentProject.TypeId = 1;
            CurrentProject.StatusId = 1;
        }
        private void CreateNewAction() {
            CurrentAction = generalEntity.Actions.Create();
            CurrentAction.StatusId = 1;
            CurrentAction.IsActive = false;
            

        }
        private void AddNewProject() {
            CurrentProject.DateCreated = DateTime.Now;
            generalEntity.Projects.Add(CurrentProject);
            Projects.Add(CurrentProject);
            generalEntity.SaveChanges();
            CreateNewProject();
        }
        private void AddAction() {
            CurrentAction.DateCreated = DateTime.Now;
            SelectedProject.Actions.Add(CurrentAction);
           
            generalEntity.SaveChanges();
            SelectedProject.RaisePropertyChanged("ActionsList");
            CreateNewAction();
        }
        internal void Test() {
            var a = generalEntity.Actions.Create();
            a.Name = "testaction";
            a.DateCreated = DateTime.Now;
            a.StatusId = 1;
            a.TriggerId = 1;
            Projects[0].Actions.Add(a);

            generalEntity.SaveChanges();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public partial class Project:INotifyPropertyChanged {
        public ObservableCollection<Action> ActionsList {
            get {
                return new ObservableCollection<Action>(this.Actions);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
