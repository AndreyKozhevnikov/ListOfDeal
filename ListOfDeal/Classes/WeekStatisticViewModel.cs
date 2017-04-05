
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
            var wRecords = MainViewModel.DataProvider.GetWeekRecords();
            foreach (var wr in wRecords) {
                string wId = wr.WeekId;

                int m = int.Parse(wId.Substring(0, 2));
                int d = int.Parse(wId.Substring(2, 2));
                int y = int.Parse(wId.Substring(4, 4));
                DateTime wDt = new DateTime(y, m, d);
                DateTime endDt = wDt.AddDays(6);
                var act = wr.Action;
                if (act.StatusId2 == (int)ActionsStatusEnum2.Done && act.CompleteTime != null && act.CompleteTime.Value.Date <= endDt.Date) {
                    wr.IsCompletedInWeek = true;
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
            var activeActions = MainViewModel.DataProvider.GetActions().Where(x => x.Project.StatusId == (int)ProjectStatusEnum.InWork && (x.StatusId2 == (int)ActionsStatusEnum2.Delay || x.StatusId2 == (int)ActionsStatusEnum2.InWork) && (x.StatusId2 != (int)ActionsStatusEnum2.Done && x.StatusId2 != (int)ActionsStatusEnum2.Rejected) && (x.ScheduledTime == null || x.ScheduledTime <= wkEndDate)).ToList();
            //var activeActions = MainViewModel.DataProvider.GetActions().Where(x => x.Project.StatusId == 1 && x.IsActive && x.StatusId != (int)ActionsStatusEnum.Completed ).ToList();
            var wRecords = MainViewModel.DataProvider.GetWeekRecords();
            foreach (var act in activeActions) {
                string weekId = wkStartDate.ToString("MMddyyyy");
                var cnt = wRecords.Where(x => x.WeekId == weekId && x.ActionId == act.Id).Count();
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
    [TestFixture]
    public class WeekStatisticViewModelTest {


        [Test]
        public void CreateItems() {
            //arrange
            WeekStatisticViewModel vm = new WeekStatisticViewModel();

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var lst = new List<Action>();
            var pr = new Project() { StatusId = 1 };
            lst.Add(new Action() { Project = pr, StatusId2 = (int)ActionsStatusEnum2.InWork, Id = 1 });
            lst.Add(new Action() { Project = pr, StatusId2 = (int)ActionsStatusEnum2.InWork });
            lst.Add(new Action() { Project = pr, StatusId2 = (int)ActionsStatusEnum2.InWork, ScheduledTime = DateTime.Today.AddDays(8) }); //out of date action
            dataProviderEntity.Setup(x => x.GetActions()).Returns(lst);
            string wkId = DateTime.Today.AddDays(1).ToString("MMddyyyy");
            dataProviderEntity.Setup(x => x.GetWeekRecords()).Returns(new List<WeekRecord>() { new WeekRecord() { ActionId = 1, WeekId = wkId } });
            dataProviderEntity.Setup(x => x.CreateWeekRecord()).Returns(new WeekRecord());
            List<object> result = new List<object>();
            dataProviderEntity.Setup(x => x.AddWeekRecord(It.IsAny<WeekRecord>())).Callback(() => { result.Add("test"); });
            //act
            vm.CreateItemsCommand.Execute(null);
            //assert
            Assert.AreEqual(1, result.Count);
        }
        [Test]
        public void MarkItemsComplete() {
            //arrange
            WeekStatisticViewModel vm = new WeekStatisticViewModel();

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var lst = new List<WeekRecord>();
            var w1 = new WeekRecord() { Action = new Action() { StatusId2 = (int)ActionsStatusEnum2.Done, CompleteTime = new DateTime(2017, 2, 28) }, WeekId = "02272017" };
            var w2 = new WeekRecord() { Action = new Action() { StatusId2 = (int)ActionsStatusEnum2.InWork }, WeekId = "02272017" };
            var w3 = new WeekRecord() { Action = new Action() { StatusId2 = (int)ActionsStatusEnum2.Done, CompleteTime = new DateTime(2017, 3, 28) }, WeekId = "02272017" };
            lst.Add(w1);
            lst.Add(w2);
            lst.Add(w3);
            dataProviderEntity.Setup(x => x.GetWeekRecords()).Returns(lst);
            //act
            vm.MarkItemsCompleteCommand.Execute(null);
            //assert
            var cnt = lst.Where(x => x.IsCompletedInWeek).Count();
            Assert.AreEqual(1, cnt);
        }
    }

}
