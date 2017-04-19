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

            var lst = new List<Action>();
            var pr = new Project() { StatusId = (int)ProjectStatusEnum.InWork };
            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.InWork }); //should be added
            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = DateTime.Today.AddDays(3) }); //should be added

            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.InWork, Id = 1 }); //already exist
            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = DateTime.Today.AddDays(8) }); //out of date action
            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.Done }); //wrong status
            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.Rejected }); //wrong status
            lst.Add(new Action() { Project = pr, StatusId = (int)ActionsStatusEnum2.Delay }); //wrong status
            var pr2 = new Project() { StatusId = (int)ProjectStatusEnum.Done };
            lst.Add(new Action() { Project = pr2, StatusId = (int)ActionsStatusEnum2.InWork }); //project inactive
            dataProviderEntity.Setup(x => x.GetActions()).Returns(lst);
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

            var lst = new List<WeekRecord>();
            var w1 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum2.Done, CompleteTime = new DateTime(2017, 2, 28) }, WeekId = "02272017" };
            var w2 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum2.InWork }, WeekId = "02272017" };
            var w3 = new WeekRecord() { Action = new Action() { StatusId = (int)ActionsStatusEnum2.Done, CompleteTime = new DateTime(2017, 3, 28) }, WeekId = "02272017" };
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
#endif
}
