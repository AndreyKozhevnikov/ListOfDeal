using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal {
    public class MainViewModel {
        public MainViewModel() {
            InitializeData();
        }
        ListOfDealBaseEntities generalEntity;
        ICommand _addNewProjectCommand;

        public ICommand AddNewProjectCommand {
            get {
                if (_addNewProjectCommand == null)
                    _addNewProjectCommand = new DelegateCommand(AddNewProject);
                
                return _addNewProjectCommand; }
        }

    

        public ObservableCollection<Project> Projects { get; set; }
        public Project CurrentProject { get; set; }
    
        
        void InitializeData() {
            generalEntity = new ListOfDealBaseEntities();
            Projects = new ObservableCollection<Project>(generalEntity.Projects.ToList());
            CreateNewProject();
        }

        private void CreateNewProject() {
            CurrentProject = generalEntity.Projects.Create();
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
    }
}
