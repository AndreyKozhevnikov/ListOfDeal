using ListOfDeal.Views;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.Tests {
#if DebugTest
    [TestFixture]
    public class EnterNewProjectViewTests {
        [Test]
        public void GetScheduledActions_Test() {
            //arrange
            var listWithScheduled = new List<MyAction>() { new MyAction(new Action()) { ScheduledTime = DateTime.Today.AddDays(30) } };
            var listWithComing = new List<MyAction>() { new MyAction(new Action()) { ScheduledTime = DateTime.Today.AddDays(3) } };
            var listWithOutdated = new List<MyAction>() { new MyAction(new Action()) { ScheduledTime = DateTime.Today.AddDays(-3) } };

            //act
            GetScheduledActions func = new GetScheduledActions();
            var schduledResult = func.Evaluate(listWithScheduled);
            var comingResult = func.Evaluate(listWithComing);
            var outDatedresult = func.Evaluate(listWithOutdated);
            //Assert
            Assert.AreEqual(GetScheduledActionsResult.HasScheduledActions,schduledResult);
            Assert.AreEqual(GetScheduledActionsResult.HasComingScheduledActons, comingResult);
            Assert.AreEqual(GetScheduledActionsResult.HasOutDatedScheduledActons, outDatedresult);
        }

    }


#endif
}
