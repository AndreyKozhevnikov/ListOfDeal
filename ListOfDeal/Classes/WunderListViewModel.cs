﻿
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
        ICommand _getActionsCommand;
        ICommand _createTasksCommand;
        ICommand _handleCompletedWlTasks;
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

        public ICommand HandleCompletedWlTasks {
            get {
                if (_handleCompletedWlTasks == null)
                    _handleCompletedWlTasks = new DelegateCommand(HandleCompletedLODActions);
                return _handleCompletedWlTasks;
            }
        }

        WLProcessor wlProcessor;
        List<MyAction> lodActions;
        
        void CreateWlProcessor() {
            wlProcessor = new WLProcessor();
            wlProcessor.CreateWlConnector(new WLConnector());
            wlProcessor.PopulateActions(lodActions);
        }
        void CreateTasks() {

            wlProcessor.CreateWlTasks();
        }
        private void GetActions() {
            var lst = parentViewModel.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.IsActive);
            lodActions = lst.ToList();
            CreateWlProcessor();
        }
        void HandleCompletedLODActions() {
            wlProcessor.HandleCompletedWLTasks();
        }
    }
}
