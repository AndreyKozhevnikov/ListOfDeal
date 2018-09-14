
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WlConnectionLibrary;

namespace ListOfDeal {
    public class WunderListViewModel {

        public ObservableCollection<string> Logs { get; set; }
        StreamWriter logWriter;
        IMainViewModel parentViewModel;
        public WunderListViewModel(IMainViewModel _mainVM) {
            parentViewModel = _mainVM;
            Logs = new ObservableCollection<string>();
#if !DebugTest
            string fileName = string.Format("logs{0}.log", DateTime.Today.ToString("ddMMMyyyy"));
            logWriter = new StreamWriter(fileName, true);
#endif
        }
        ICommand _createProcessorCommand;
        ICommand _createTasksCommand;
        ICommand _handleCompletedWlTasks;
        ICommand _handleCompletedActionsCommand;
        ICommand _handleChangedLODActionsCommand;
        ICommand _handleChangedWLTaskCommand;
        ICommand _getDiaryEntriesCommand;
        ICommand _backupCommand;
        ICommand _deleteCompletedTasksCommand;
        public ICommand DeleteCompletedTasksCommand {
            get {
                if(_deleteCompletedTasksCommand == null)
                    _deleteCompletedTasksCommand = new DelegateCommand(DeleteCompletedTasks);
                return _deleteCompletedTasksCommand;
            }
        }
        public ICommand BackupCommand {
            get {
                if (_backupCommand == null)
                    _backupCommand = new DelegateCommand(Backup);
                return _backupCommand;
            }
        }
        public ICommand CreateProcessorCommand {
            get {
                if (_createProcessorCommand == null)
                    _createProcessorCommand = new DelegateCommand(CreateProcessor);
                return _createProcessorCommand;
            }
        }

        public ICommand TestCommand {
            get {
                if (_testCommand == null)
                    _testCommand = new DelegateCommand(Test);
                return _testCommand;
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
        public ICommand GetDiaryEntriesCommand {
            get {
                if (_getDiaryEntriesCommand == null)
                    _getDiaryEntriesCommand = new DelegateCommand(GetDiaryEntries);
                return _getDiaryEntriesCommand;
            }
        }

        private void GetDiaryEntries() {
            wlProcessor.PasteDiaryEntries();


        }

        private void HandleChangedLODActions() {
            wlProcessor.HandleChangedLODActions();
            CreateWlProcessor();
        }
        void HandleChangedWLTask() {
            wlProcessor.HandleChangedWLTask();
            CreateWlProcessor();
        }


        ICommand _testCommand;
        WLProcessor wlProcessor;

        void CreateWlProcessor() {
            wlProcessor = new WLProcessor(parentViewModel);
            wlProcessor.Logged += WlProcessor_Logged;
            wlProcessor.CreateWlConnector(new WLConnector());
            wlProcessor.SetClipboardText = ClipboardHelper.ClipboardSetText;

        }
        void Backup() {
            wlProcessor.Backup();
        }
        void DeleteCompletedTasks() {
            wlProcessor.DeleteCompletedTask();
        }


        void Test() {

            //var lst = wlProcessor.GetCompletedTasks(249220592);
            //foreach(var wl in lst) {
            //    wlProcessor.DeleteTaks(wl);
            //}
            //var lst = wlProcessor.Test();
            //var lst = MainViewModel.DataProvider.GetProjects().Where(x => x.TypeId == 10).ToList();
            //foreach(Project p in lst) {
            //    p.TypeId = 11;
            //    p.Name = "Купить - " + p.Name;
            //    foreach (Action a in p.Actions) {
            //        a.ToBuy = true;
            //        a.Name = "купить - " + a.Name;
            //    }
            //}
            //MainViewModel.SaveChanges();
            //for (int i = 0; i < 1000; i += 10) {
            //    string st = i.ToString() + new string('0', i);
            //    Logs.Add(st);
            //}
            CreateProcessor();
            wlProcessor.Test();
        }
        private void WlProcessor_Logged(string st) {
            Logs.Add(st);
#if !DebugTest
            logWriter.WriteLine(st);
            logWriter.Flush();
#endif
        }

        void CreateTasks() {

            wlProcessor.CreateWlTasks();
        }
        private void CreateProcessor() {
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
