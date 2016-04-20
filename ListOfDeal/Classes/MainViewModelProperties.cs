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
using DevExpress.Data;

namespace ListOfDeal {
    public partial class MainViewModel :MyBindableBase, ISupportServices {

        public static ListOfDealBaseEntities generalEntity;
        ICommand _addNewProjectCommand;
        ICommand _addActionCommand;
        ICommand _openEditProjectCommand;
        ICommand _openNewInfoCommand;
        ICommand _provideActiveActionsCommand;
        ICommand _customRowFilterCommand;
        ICommand _exportGridsCommand;
        ICommand _saveChangesCommand;
        ICommand _previewKeyProjectsCommand;
        ICommand _previewKeyActionsCommand;
        ICommand _customSummaryCommand;
        ICommand _goToParentProjectCommand;
        ICommand _getChartDataCommand;

        public ICommand GetChartDataCommand {
            get {
                if (_getChartDataCommand == null)
                    _getChartDataCommand = new DelegateCommand(GetChartData);
                return _getChartDataCommand; }
        }

     
  
     
    

        MyProject _currentProject;
        MyAction _currentAction;
        MyProject _selectedProject;

        MyAction _selectedAction;

    

        int _selectedTabIndex;

    


        public ICommand AddNewProjectCommand {
            get {
                if (_addNewProjectCommand == null)
                    _addNewProjectCommand = new DelegateCommand(AddProject);

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
        public ICommand ExportGridsCommand {
            get {
                if (_exportGridsCommand == null)
                    _exportGridsCommand = new DelegateCommand(ExportGrids);
                return _exportGridsCommand; }
           
        }


        public ICommand SaveChangesCommand {
            get {
                if (_saveChangesCommand == null)
                    _saveChangesCommand = new DelegateCommand(SaveChanges);
                return _saveChangesCommand; }
        }
        public ICommand PreviewKeyProjectsCommand {
            get {
                if (_previewKeyProjectsCommand == null)
                    _previewKeyProjectsCommand = new DelegateCommand<KeyEventArgs>(PreviewKeyProjects);
                return _previewKeyProjectsCommand;
            }
        }
        public ICommand PreviewKeyActionsCommand {
            get {
                if (_previewKeyActionsCommand == null)
                    _previewKeyActionsCommand = new DelegateCommand<KeyEventArgs>(PreviewKeyActions);
                return _previewKeyActionsCommand;
            }
        }
        public ICommand CustomSummaryCommand {
            get {
                if (_customSummaryCommand == null)
                    _customSummaryCommand = new DelegateCommand<CustomSummaryEventArgs>(CustomSummary);
                return _customSummaryCommand;

            }
        }
        public ICommand GoToParentProjectCommand {
            get {
                if (_goToParentProjectCommand == null)
                    _goToParentProjectCommand = new DelegateCommand<MyAction>(GoToParentProject);
                return _goToParentProjectCommand; }
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

        public MyAction SelectedAction {
            get { return _selectedAction; }
            set {
                _selectedAction = value;
                RaisePropertyChanged("SelectedAction");
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

        public int SelectedTabIndex {
            get { return _selectedTabIndex; }
            set {
                _selectedTabIndex = value;
                RaisePropertyChanged("SelectedTabIndex");
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
        IGridControlManagerService GridControlManagerService { get { return serviceContainer.GetService<IGridControlManagerService>(); } }
    }


}
