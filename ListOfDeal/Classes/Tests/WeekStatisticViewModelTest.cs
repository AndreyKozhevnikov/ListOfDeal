using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.Tests {
#if DebugTest
    [TestFixture]
    public class WeekStatisticViewModelTest {
        [Test]
        public void CreateItems() {
            //arrange
            WeekStatisticViewModel vm = new WeekStatisticViewModel();

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var lst = new List<Project>();
            var pr = new Project() { StatusId = (int)ProjectStatusEnum.InWork };
            lst.Add(pr);
            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.InWork }); //should be added
            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.InWork, ScheduledTime = DateTime.Today.AddDays(3) }); //should be added

            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.InWork, Id = 1 }); //already exist
            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.InWork, ScheduledTime = DateTime.Today.AddDays(8) }); //out of date action
            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.Done }); //wrong status
            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.Rejected }); //wrong status
            pr.Actions.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum.Delay }); //wrong status
            var pr2 = new Project() { StatusId = (int)ProjectStatusEnum.Done };
            lst.Add(pr2);
            pr2.Actions.Add(new Action() { Project = pr2, StatusId = (int)ActionsStatusEnum.InWork }); //project inactive
            var pr3 = new Project() { StatusId = (int)ProjectStatusEnum.Delayed };
            lst.Add(pr3);
            pr3.Actions.Add(new Action() { Project = pr3, StatusId = (int)ActionsStatusEnum.InWork, ScheduledTime = DateTime.Today.AddDays(8) });//out of date action
            dataProviderEntity.Setup(x => x.GetProjects()).Returns(lst);
            string wkId = DateTime.Today.AddDays(1).ToString("MMddyyyy");
            dataProviderEntity.Setup(x => x.GetWeekRecords()).Returns(new List<WeekRecord>() { new WeekRecord() { ActionId = 1, WeekId = wkId } });
            dataProviderEntity.Setup(x => x.CreateWeekRecord()).Returns(new WeekRecord());
            List<object> result = new List<object>();
            dataProviderEntity.Setup(x => x.AddWeekRecord(It.IsAny<WeekRecord>())).Callback(() => { result.Add("test"); });
            //act
            vm.CreateItemsCommand.Execute(null);
            //assert
            Assert.AreEqual(2, result.Count);
        }
        [Test]
        public void MarkItemsComplete() {
            //arrange
            WeekStatisticViewModel vm = new WeekStatisticViewModel();

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            MainViewModel.DataProvider = dataProviderEntity.Object;
            var stDate = DateTime.Today.AddDays(-7);
            var lst = new List<WeekRecord>();
            var w1 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum.Done, CompleteTime = new DateTime(2017, 2, 28) }, WeekId = "02272017",DateAdd= stDate };
            var w2 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum.Rejected, CompleteTime = new DateTime(2017, 2, 28) }, WeekId = "02272017",DateAdd= stDate };
            var w3 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum.Rejected, CompleteTime = new DateTime(2017, 2, 23) }, WeekId = "02202017", DateAdd = stDate };
            var w4 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum.InWork }, WeekId = "02272017", DateAdd = stDate };
            var w5 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum.Done, CompleteTime = new DateTime(2017, 3, 28) }, WeekId = "02272017", DateAdd = stDate };
            var w6 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum.Done, CompleteTime = new DateTime(2017, 2, 28) }, WeekId = "01152017",DateAdd= stDate.AddDays(-20) };
            lst.Add(w1);
            lst.Add(w2);
            lst.Add(w3);
            lst.Add(w4);
            dataProviderEntity.Setup(x => x.GetWeekRecords()).Returns(lst);
            //act
            vm.MarkItemsCompleteCommand.Execute(null);
            //assert
            var cnt = lst.Where(x => x.IsCompletedInWeek).Count();
            Assert.AreEqual(3, cnt);
        }
    }
#endif
}
