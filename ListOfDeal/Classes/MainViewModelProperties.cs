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
    public partial class MainViewModel :MyBindableBase {

        public static ListOfDealBaseEntities generalEntity;
        ICommand _addNewProjectCommand;
        ICommand _addActionCommand;
        ICommand _openEditProjectCommand;



        MyProject _currentProject;
        MyAction _currentAction;
        MyProject _selectedProject;



        public ICommand AddNewProjectCommand {
            get {
                if (_addNewProjectCommand == null)
                    _addNewProjectCommand = new DelegateCommand(AddNewProject);

                return _addNewProjectCommand;
            }
        }

        public ICommand AddActionCommand {
            get {
                if (_addActionCommand == null)
                    _addActionCommand = new DelegateCommand(AddAction);
                return _addActionCommand;
            }
        }
        public ICommand OpenEditProjectCommand {
            get {
                if (_openEditProjectCommand == null)
                    _openEditProjectCommand = new DelegateCommand(OpenEditProject);
                return _openEditProjectCommand;
            }
        }


        public ObservableCollection<MyProject> Projects { get; set; }
        public MyProject CurrentProject {
            get { return _currentProject; }
            set {
                _currentProject = value;
                RaisePropertyChanged("CurrentProject");
            }
        }
        public MyAction CurrentAction {
            get { return _currentAction; }
            set {
                _currentAction = value;
                RaisePropertyChanged("CurrentAction");
            }
        }
        public MyProject SelectedProject {
            get { return _selectedProject; }
            set {
                _selectedProject = value;
                RaisePropertyChanged("SelectedProject");
            }
        }
        public ObservableCollection<ProjectType> ProjectTypes { get; set; }
        public ObservableCollection<ProjectStatus> ProjectStatuses { get; set; }
        public ObservableCollection<ActionTrigger> ActionTriggers { get; set; }
        public ObservableCollection<ActionStatus> ActionStatuses { get; set; }
        public ObservableCollection<DelegatePerson> DelegatePersons { get; set; }



    }


}
