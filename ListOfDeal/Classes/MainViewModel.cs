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
        Project _currentProject;
        public ICommand AddNewProjectCommand {
            get {
                if (_addNewProjectCommand == null)
                    _addNewProjectCommand = new DelegateCommand(AddNewProject);
                
                return _addNewProjectCommand; }
        }

    

        public ObservableCollection<Project> Projects { get; set; }
        public Project CurrentProject {
            get { return _currentProject; }
            set { _currentProject = value;
            RaisePropertyChanged("CurrentProject");
            }
        }
        public ObservableCollection<ProjectType> ProjectTypes { get; set; }
        
        void InitializeData() {
            ConnectToDataBase();
            Projects = new ObservableCollection<Project>(generalEntity.Projects);
            ProjectTypes = new ObservableCollection<ProjectType>(generalEntity.ProjectTypes);
            CreateNewProject();
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
            CurrentProject.IsWork = true;
        }
        private void AddNewProject() {
            CurrentProject.DateCreated = DateTime.Now;
            generalEntity.Projects.Add(CurrentProject);
            Projects.Add(CurrentProject);
            generalEntity.SaveChanges();
            CreateNewProject();
        }

        internal void Test() {
            generalEntity.SaveChanges();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
