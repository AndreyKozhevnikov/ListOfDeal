using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public class WLProcessor {
        ObservableCollection<MyAction> allActions;
        IWLConnector wlConnector;
        public void PopulateActions(ObservableCollection<MyAction> _actions) {
            allActions = _actions;
        }
        public void CreateWlConnector(IWLConnector _conn) {
            wlConnector = _conn;
        }
        public void CreateWlTasks() {
            var emptyActions = allActions.Where(x => x.WLId == null);
            if (emptyActions.Count() == 0)
                return;
          //  emptyActions = emptyActions.Take(1);
            foreach (var act in emptyActions) {
                string title = act.Name;
                var wlTask = wlConnector.CreateTask(title);
                act.WLId = wlTask.id;
            }
            MainViewModel.generalEntity.SaveChanges();
        }
    }

    [TestFixture]
    public class WLProcessorTest {
    
    }
}
