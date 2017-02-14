
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal.Classes {
    public class WeekStatisticViewModel: MyBindableBase {
        ICommand _createItemsCommand;
        public ICommand CreateItemsCommand {
            get {
                if (_createItemsCommand == null)
                    _createItemsCommand = new DelegateCommand(CreateItems);
                return _createItemsCommand;
            }
        }
        ICommand _getItemsCommand;
        public ICommand GetItemsCommand {
            get {
                if (_getItemsCommand == null)
                    _getItemsCommand = new DelegateCommand(GetItems);
                return _getItemsCommand;
            }
        }

        void GetItems() {
            WeekRecords =new ObservableCollection<WeekRecord>( MainViewModel.DataProvider.GetWeekRecords());
        }
        ObservableCollection<WeekRecord> weekRecords;
        public ObservableCollection<WeekRecord> WeekRecords {
            get {
                return weekRecords;
            }
            set {
                weekRecords = value;
                RaisePropertyChanged("WeekRecords");
            }
        }

        void CreateItems() {
            var activeActions = MainViewModel.DataProvider.GetActions().Where(x => x.Project.StatusId == 1 && x.IsActive && x.StatusId != 4).ToList();
            var wRecords = MainViewModel.DataProvider.GetWeekRecords();
            foreach (var act in activeActions) {
                string weekId= DateTime.Today.ToString("MMddyyyy"); ;
                var cnt = wRecords.Where(x => x.WeekId == weekId && x.ActionId == act.Id).Count();
                if (cnt > 0)
                    return;
                var wr = MainViewModel.DataProvider.CreateWeekRecord();
                wr.ActionId = act.Id;
                wr.WeekId = weekId;
                wr.DateAdd = DateTime.Now;
                MainViewModel.DataProvider.AddWeekRecord(wr);
            }
            MainViewModel.DataProvider.SaveChanges();
        }
        void SetIsCompleted() {

        }
    }
}
