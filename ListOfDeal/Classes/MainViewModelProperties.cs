﻿using DevExpress.Mvvm;
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
using ListOfDeal.Classes.XPO;

namespace ListOfDeal {
    public partial class MainViewModel : MyBindableBase, ISupportServices {

        //public static IListOfDealBaseEntities generalEntity {
        //    get {
        //        return DataProvider.GeneralEntity;
        //    }
        //}
        public static IMainViewModelDataProvider DataProvider;








        MyProject _selectedProject;

        MyAction _selectedAction;



        int _selectedTabIndex;
        int _chartMinValue;

        public int ChartMinValue {
            get { return _chartMinValue; }
            set {
                _chartMinValue = value;
                RaisePropertyChanged("ChartMinValue");
            }
        }

        #region Commands
        ICommand _addNewProjectCommand;
        ICommand _addActionCommand;
        ICommand _openEditProjectCommand;

        ICommand _provideActionsCommand;
        ICommand _exportGridsCommand;
        ICommand _saveChangesCommand;
        ICommand _previewKeyProjectsCommand;
        ICommand _previewKeyActionsCommand;
        ICommand _customSummaryCommand;
        ICommand _goToParentProjectCommand;
        ICommand _getChartDataCommand;
        ICommand _getActionsHistoryCommand;
        ICommand _validateColumnCommand;
        ICommand _customColumnSortCommand;
        ICommand _onFocusedRowHandleChangedCommand;
        public ICommand CustomColumnSortCommand {
            get {
                if (_customColumnSortCommand == null)
                    _customColumnSortCommand = new DelegateCommand<CustomColumnSortEventArgs>(CustomColumnSort);
                return _customColumnSortCommand;
            }
        }

        public ICommand ValidateColumnCommand {
            get {
                if (_validateColumnCommand == null)
                    _validateColumnCommand = new DelegateCommand<GridRowValidationEventArgs>(ValidateColumn);
                return _validateColumnCommand;
            }

        }


        public ICommand GetActionsHistoryCommand {
            get {
                if (_getActionsHistoryCommand == null)
                    _getActionsHistoryCommand = new DelegateCommand(GetActionsHistory);
                return _getActionsHistoryCommand;
            }
        }


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

        public ICommand ProvideActionsCommand {
            get {
                if (_provideActionsCommand == null)
                    _provideActionsCommand = new DelegateCommand(ProvideActions);
                return _provideActionsCommand;
            }
        }

        public ICommand ExportGridsCommand {
            get {
                if (_exportGridsCommand == null)
                    _exportGridsCommand = new DelegateCommand(ExportGrids);
                return _exportGridsCommand;
            }

        }


        public ICommand SaveChangesCommand {
            get {
                if (_saveChangesCommand == null)
                    _saveChangesCommand = new DelegateCommand(SaveChanges);
                return _saveChangesCommand;
            }
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
                return _goToParentProjectCommand;
            }
        }
        public ICommand GetChartDataCommand {
            get {
                if (_getChartDataCommand == null)
                    _getChartDataCommand = new DelegateCommand(GetChartData);
                return _getChartDataCommand;
            }
        }

        public ICommand OnFocusedRowHandleChangedCommand {
            get {
                if (_onFocusedRowHandleChangedCommand == null)
                    _onFocusedRowHandleChangedCommand = new DelegateCommand<ProjectType>(OnFocusedRowHandleChanged);
                return _onFocusedRowHandleChangedCommand;
            }
        }
        #endregion




        public string NewProjectName {
            get => newProjectName; set {
                newProjectName = value;
                RaisePropertyChanged("NewProjectName");
            }
        }
        public string NewActionName {
            get => newActionName; set {
                newActionName = value;
                RaisePropertyChanged("NewActionName");
            }
        }
        public ActionsStatusEnum NewActionStatus { get => newActionStatus; set {
                newActionStatus = value;
                RaisePropertyChanged("NewActionStatus");
            }
        }
        public ProjectType SelectedProjectType {
            get => selectedProjectType; set {
                selectedProjectType = value;
                RaisePropertyChanged("SelectedProjectType");
            }
        }
        //public MyProject CurrentProject {
        //    get { return _currentProject; }
        //    set {
        //        _currentProject = value;
        //        RaisePropertyChanged("CurrentProject");
        //    }
        //}
        //public MyAction CurrentAction {
        //    get { return _currentAction; }
        //    set {
        //        _currentAction = value;
        //        RaisePropertyChanged("CurrentAction");
        //    }
        //}
        public MyProject SelectedProject {
            get { return _selectedProject; }
            set {
                _selectedProject = value;
                RaisePropertyChanged("SelectedProject");
                OnSelectedProjectChanged();

            }
        }



        public MyAction SelectedAction {
            get { return _selectedAction; }
            set {
                _selectedAction = value;
                RaisePropertyChanged("SelectedAction");
                OnSelectedActionChanged();
            }
        }




        public ObservableCollection<MyProject> Projects { get; set; }
        public ObservableCollection<ProjectType> ProjectTypes { get; set; }
        ObservableCollection<DayData> _allDayData;
        ObservableCollection<MyAction> _waitedActions;
        ObservableCollection<MyAction> _scheduledActions;
        ObservableCollection<HistoryActionItem> _actionsHistoryCollection;

        public ObservableCollection<HistoryActionItem> ActionsHistoryCollection {
            get { return _actionsHistoryCollection; }
            set {
                _actionsHistoryCollection = value;
                RaisePropertyChanged("ActionsHistoryCollection");
            }
        }
        public ObservableCollection<DayData> AllDayData {
            get { return _allDayData; }
            set {
                _allDayData = value;
                RaisePropertyChanged("AllDayData");
            }
        }

        public ObservableCollection<MyAction> WaitedActions {
            get { return _waitedActions; }
            set {
                _waitedActions = value;
                RaisePropertyChanged("WaitedActions");
            }
        }
        public ObservableCollection<MyAction> ScheduledActions {
            get { return _scheduledActions; }
            set {
                _scheduledActions = value;
                RaisePropertyChanged("ScheduledActions");
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
        IGridControlManagerService _gridControlManagerService;
        private string newActionName;
        private string newProjectName;
        private ProjectType selectedProjectType;
        private ActionsStatusEnum newActionStatus;

        public IGridControlManagerService GridControlManagerService {
            get {
                if (_gridControlManagerService == null)
                    _gridControlManagerService = serviceContainer.GetService<IGridControlManagerService>();
                return _gridControlManagerService;
            }
            set { _gridControlManagerService = value; }
        }


    }


}
