
using DevExpress.Mvvm;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal.Classes {
    public class WeekStatisticViewModel : MyBindableBase {
        public WeekStatisticViewModel() {
            WeekDataList = new ObservableCollection<WeekData>();
        }
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
        public ObservableCollection<WeekData> WeekDataList { get; set; }

        void GetItems() {
            WeekRecords = new ObservableCollection<WeekRecord>(MainViewModel.DataProvider.GetWeekRecords());

            var lst = WeekRecords.GroupBy(x => x.WeekId).Select(d => new { dt = d.Key, all = d.Count(), completed = d.Sum(y => y.IsCompletedInWeek ? 1 : 0) });
            var lst2 = lst.Select(x => new WeekData(x.dt, x.all, x.completed)).ToList();
            foreach (var w in lst2)
                WeekDataList.Add(w);
        }
        ICommand _markItemsCompleteCommand;
        public ICommand MarkItemsCompleteCommand {
            get {
                if (_markItemsCompleteCommand == null)
                    _markItemsCompleteCommand = new DelegateCommand(MarkItemsComplete);
                return _markItemsCompleteCommand;
            }
        }
        void MarkItemsComplete() {
            var stDate = DateTime.Today.AddDays(-21);
            var wRecords = MainViewModel.DataProvider.GetWeekRecords().Where(x => x.DateAdd > stDate && !x.IsCompletedInWeek);

            var idList = wRecords.GroupBy(x => x.WeekId).Select(x => new { wId = x.Key, records = x });
            foreach (var week in idList) {
                var weekId = week.wId;
                int m = int.Parse(weekId.Substring(0, 2));
                int d = int.Parse(weekId.Substring(2, 2));
                int y = int.Parse(weekId.Substring(4, 4));
                DateTime wDt = new DateTime(y, m, d);
                DateTime endDt = wDt.AddDays(6);
                foreach (var wr in week.records) {
                    var act = wr.Action;
                    if (act.CompleteTime.HasValue && act.CompleteTime.Value.Date <= endDt.Date) {
                        wr.IsCompletedInWeek = true;
                    }
                }
            }
            MainViewModel.DataProvider.SaveChanges();
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
            DateTime wkStartDate = DateTime.Today.AddDays(1);
            DateTime wkEndDate = wkStartDate.AddDays(6);
            var activeActions = WLProcessor.ReturnActiveActionsFromProjectList(MainViewModel.DataProvider.GetActiveProejcts().Select(x => new MyProject(x)));
            var filteredActions = activeActions.Where(x => !x.ScheduledTime.HasValue || x.ScheduledTime <= wkEndDate);
            string weekId = wkStartDate.ToString("MMddyyyy");
            var wRecords = MainViewModel.DataProvider.GetWeekRecords().Where(x => x.WeekId == weekId);
            foreach (var act in filteredActions) {
                var cnt = wRecords.Where(x => x.ActionId == act.Id).Count();
                if (cnt > 0)
                    continue;
                var wr = MainViewModel.DataProvider.CreateWeekRecord();
                wr.ActionId = act.Id;
                wr.WeekId = weekId;
                wr.DateAdd = DateTime.Now;
                MainViewModel.DataProvider.AddWeekRecord(wr);
            }
            MainViewModel.DataProvider.SaveChanges();
        }

    }
    public class WeekData {
        public WeekData(string dt, int all, int completed) {
            Id = dt;
            AllInActions = all;
            PercentComplete = all > 0 ? (double)completed / all : 0;
        }
        public string Id { get; set; }
        public double PercentComplete { get; set; }
        public int AllInActions { get; set; }
    }


}
