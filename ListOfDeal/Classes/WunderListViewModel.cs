
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
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
        ICommand _getActionsCommand;
        ICommand _createTasksCommand;
        public ICommand GetActionsCommand {
            get {
                if (_getActionsCommand == null)
                    _getActionsCommand = new DelegateCommand(GetActions);
                return _getActionsCommand;
            }
        }

     
        public ICommand CreateTasksCommand {
            get {
                if (_createTasksCommand == null)
                    _createTasksCommand = new DelegateCommand(CreateTasks);
                return _createTasksCommand;
            }

         
        }
        WLProcessor wlProcessor;
        void CreateWlProcessor() {
            wlProcessor = new WLProcessor();
          
            
            //  wlProcessor.PopulateActions(parentViewModel.);
            
            wlProcessor.CreateWlConnector(new WLConnector());
        }
        void CreateTasks() {
            wlProcessor.CreateWlTasks();
        }
        private void GetActions() {
         
        }
    }
}
