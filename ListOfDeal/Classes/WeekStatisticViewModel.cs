﻿
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
            WeekRecords = new ObservableCollection<WeekRecord>(MainViewModel.DataProvider.GetWeekRecords());
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
                if (act.StatusId == 4 && act.CompleteTime != null && act.CompleteTime.Value.Date <= endDt.Date) {
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



        public void CreateItems() {
            DateTime wkStartDate = DateTime.Today.AddDays(1);
            DateTime wkEndDate = wkStartDate.AddDays(6);
            var activeActions = MainViewModel.DataProvider.GetActions().Where(x => x.Project.StatusId == 1 && x.IsActive && x.StatusId != (int)ActionsStatusEnum.Completed && (x.ScheduledTime == null || x.ScheduledTime <= wkEndDate)).ToList();
            //var activeActions = MainViewModel.DataProvider.GetActions().Where(x => x.Project.StatusId == 1 && x.IsActive && x.StatusId != (int)ActionsStatusEnum.Completed ).ToList();
            var wRecords = MainViewModel.DataProvider.GetWeekRecords();
            foreach (var act in activeActions) {
                string weekId = wkStartDate.ToString("MMddyyyy");
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
            lst.Add(new Action() { Project = pr, IsActive = true, StatusId = (int)ActionsStatusEnum.Scheduled });
            lst.Add(new Action() { Project = pr, IsActive = true, StatusId = (int)ActionsStatusEnum.Scheduled ,ScheduledTime=DateTime.Today.AddDays(8)});
            lst.Add(new Action() { Project = pr, IsActive = true, StatusId = (int)ActionsStatusEnum.Scheduled, Id=1 });
            dataProviderEntity.Setup(x => x.GetActions()).Returns(lst);
            string wkId = DateTime.Today.AddDays(1).ToString("MMddyyyy");
            dataProviderEntity.Setup(x => x.GetWeekRecords()).Returns(new List<WeekRecord>() { new WeekRecord() { ActionId = 1, WeekId = wkId } });
            dataProviderEntity.Setup(x => x.CreateWeekRecord()).Returns(new WeekRecord());
            List<object> result = new List<object>();
            dataProviderEntity.Setup(x => x.AddWeekRecord(It.IsAny<WeekRecord>())).Callback(() => { result.Add("test"); });
            //act
            vm.CreateItems();
            //assert
            Assert.AreEqual(1, result.Count);
        }
    }

}
