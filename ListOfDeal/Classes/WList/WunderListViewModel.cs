
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal {
    public class WunderListViewModel {

      public  ObservableCollection<string> Logs { get; set; }
        StreamWriter logWriter;
        IMainViewModel parentViewModel;
        public WunderListViewModel(IMainViewModel _mainVM) {
            parentViewModel = _mainVM;
            Logs = new ObservableCollection<string>();
            string fileName = string.Format("logs{0}.log", DateTime.Today.ToString("ddMMMyyyy"));
            logWriter = new StreamWriter(fileName, true);
        }
        ICommand _createProcessorCommand;
        ICommand _createTasksCommand;
        ICommand _handleCompletedWlTasks;
        ICommand _handleCompletedActionsCommand;
        ICommand _handleChangedLODActionsCommand;
        ICommand _handleChangedWLTaskCommand;
        public ICommand CreateProcessorCommand {
            get {
                if (_createProcessorCommand == null)
                    _createProcessorCommand = new DelegateCommand(CreateProcessor);
                return _createProcessorCommand;
            }
        }


        public ICommand CreateTasksCommand {
            get {
                if (_createTasksCommand == null)
                    _createTasksCommand = new DelegateCommand(CreateTasks);
                return _createTasksCommand;
            }


        }

        public ICommand HandleCompletedWlTasksCommand {
            get {
                if (_handleCompletedWlTasks == null)
                    _handleCompletedWlTasks = new DelegateCommand(HandleCompletedWLTasks);
                return _handleCompletedWlTasks;
            }
        }

        public ICommand HandleCompletedActionsCommand {
            get {
                if (_handleCompletedActionsCommand == null)
                    _handleCompletedActionsCommand = new DelegateCommand(HandleCompletedActions);
                return _handleCompletedActionsCommand;
            }
        }

        public ICommand HandleChangedActionsCommand {
            get {
                if (_handleChangedLODActionsCommand == null)
                    _handleChangedLODActionsCommand = new DelegateCommand(HandleChangedLODActions);
                return _handleChangedLODActionsCommand;
            }
        }
        public ICommand HandleChangedWLTaskCommand {
            get {
                if (_handleChangedWLTaskCommand == null)
                    _handleChangedWLTaskCommand = new DelegateCommand(HandleChangedWLTask);
                return _handleChangedWLTaskCommand;
            }
        }
        private void HandleChangedLODActions() {
            wlProcessor.HandleChangedLODActions();
        }
        void HandleChangedWLTask() {
            wlProcessor.HandleChangedWLTask();
        }


        WLProcessor wlProcessor;
        //     List<MyAction> lodActions;

        void CreateWlProcessor() {
            wlProcessor = new WLProcessor(parentViewModel);
            wlProcessor.CreateWlConnector(new WLConnector());
            wlProcessor.Logged += WlProcessor_Logged;
            WlProcessor_Logged(new WLEventArgs( "===== WLProcessorCreated ====="));
            //  wlProcessor.PopulateActions(lodActions);
        }

        private void WlProcessor_Logged(WLEventArgs e) {
            string st = string.Format("{0}  --  {1}", e.DTime, e.Message);
            Logs.Add(st);
            if (logWriter != null) {
                logWriter.WriteLine(st);
                logWriter.Flush();
            }
        }

        void CreateTasks() {

            wlProcessor.CreateWlTasks();
        }
        private void CreateProcessor() {
            //var lst = parentViewModel.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.IsActive);
            //lodActions = lst.ToList();
            CreateWlProcessor();
        }
        void HandleCompletedWLTasks() {
            wlProcessor.HandleCompletedWLTasks();
        }
        void HandleCompletedActions() {
            wlProcessor.HandleCompletedLODActions();
        }
    }
}
