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
using DevExpress.Xpf.Grid;

namespace ListOfDeal {
    public partial class MainViewModel :MyBindableBase, ISupportServices {

        public static ListOfDealBaseEntities generalEntity;
        ICommand _addNewProjectCommand;
        ICommand _addActionCommand;
        ICommand _openEditProjectCommand;
        ICommand _openNewInfoCommand;
        ICommand _provideActiveActionsCommand;
        ICommand _customRowFilterCommand;
        ICommand _exportWaitedGridCommand;
        ICommand _saveChangesCommand;



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
        public ICommand OpenNewInfoCommand {
            get {
                if (_openNewInfoCommand == null)
                    _openNewInfoCommand = new DelegateCommand<string>(OpenNewInfo);
                return _openNewInfoCommand; }
        }
        public ICommand ProvideActiveActionsCommand {
            get {
                if (_provideActiveActionsCommand == null)
                    _provideActiveActionsCommand = new DelegateCommand(ProvideActiveActions);
                return _provideActiveActionsCommand; }
        }
        public ICommand CustomRowFilterCommand {
            get {
                if (_customRowFilterCommand == null)
                    _customRowFilterCommand = new DelegateCommand<RowFilterEventArgs>(CustomRowFilter);
                return _customRowFilterCommand; }
        }
        public ICommand ExportWaitedGridCommand {
            get {
                if (_exportWaitedGridCommand == null)
                    _exportWaitedGridCommand = new DelegateCommand(ExportWaitedGrid);
                return _exportWaitedGridCommand; }
           
        }


        public ICommand SaveChangesCommand {
            get {
                if (_saveChangesCommand == null)
                    _saveChangesCommand = new DelegateCommand(SaveChanges);
                return _saveChangesCommand; }
        }
  
    
    
    
   

     
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
        public ObservableCollection<MyProject> Projects { get; set; }
        public ObservableCollection<ProjectType> ProjectTypes { get; set; }
        public ObservableCollection<ProjectStatus> ProjectStatuses { get; set; }
        public ObservableCollection<ActionTrigger> ActionTriggers { get; set; }
        public ObservableCollection<ActionStatus> ActionStatuses { get; set; }
        public ObservableCollection<DelegatePerson> DelegatePersons { get; set; }

        ObservableCollection<MyAction> _activeActions;

        public ObservableCollection<MyAction> ActiveActions {
            get { return _activeActions; }
            set { _activeActions = value;
            RaisePropertyChanged("ActiveActions");
            }
        }


        IServiceContainer serviceContainer = null;
        protected IServiceContainer ServiceContainer {
            get {
                if (serviceContainer == null)
                    serviceContainer = new ServiceContainer(this);
                return serviceContainer;
            }
        }
        IServiceContainer ISupportServices.ServiceContainer { get { return ServiceContainer; } }
        IExportToExcelService ExportToExcelService { get { return ServiceContainer.GetService<IExportToExcelService>(); } }
    }


}
