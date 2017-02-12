
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal.Classes {
    public class WeekStatisticViewModel {
        ICommand _createItemsCommand;
        public ICommand CreateItemsCommand {
            get {
                if (_createItemsCommand == null)
                    _createItemsCommand = new DelegateCommand(CreateItems);
                return _createItemsCommand;
            }
        }
     


        void CreateItems() {
            var activeActions = MainViewModel.DataProvider.GetActions().Where(x => x.Project.StatusId == 1 && x.IsActive && x.StatusId != 4).ToList();
            foreach (var act in activeActions) {
                var wr = MainViewModel.DataProvider.CreateWeekRecord();
                wr.ActionId = act.Id;
                wr.WeekId = DateTime.Today.ToString("MMddyyyy");
                wr.DateAdd = DateTime.Now;
                MainViewModel.DataProvider.AddWeekRecord(wr);
            }
            MainViewModel.DataProvider.SaveChanges();
        }
    }
}
