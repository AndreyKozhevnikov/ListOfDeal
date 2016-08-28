
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal {
    public class WunderListViewModel {
        IMainViewModel parentViewModel;
        public WunderListViewModel(IMainViewModel _mainVM) {
            parentViewModel = _mainVM;
          
        }
        ICommand _createProcessorCommand;
        ICommand _createTasksCommand;
        ICommand _handleCompletedWlTasks;
       ICommand _handleCompletedActionsCommand;
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

        WLProcessor wlProcessor;
   //     List<MyAction> lodActions;
        
        void CreateWlProcessor() {
            wlProcessor = new WLProcessor(parentViewModel);
            wlProcessor.CreateWlConnector(new WLConnector());
          //  wlProcessor.PopulateActions(lodActions);
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
