using Moq;
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
         //   emptyActions = emptyActions.Take(1);
            foreach (var act in emptyActions) {
                string title = act.Name;
                var wlTask = wlConnector.CreateTask(title);
                act.WLId = wlTask.id;
            }
            MainViewModel.SaveChanges();
        }
    }

    [TestFixture]
    public class WLProcessorTest {
        [Test]
        public void CreateWlTasks() {
            //arrange
            var actList = new ObservableCollection<MyAction>();
            actList.Add(new MyAction(new Action() { Name = "act1" }));
            actList.Add(new MyAction(new Action() { Name = "act2",WLId=123 }));
            actList.Add(new MyAction(new Action() { Name = "act3" }));
            WLProcessor wlProc = new WLProcessor();
            var mockWlConnector = new Mock<IWLConnector>();
          
            mockWlConnector.Setup(x => x.CreateTask("act1")).Returns(new WLTask() { id = 234 });
            mockWlConnector.Setup(x => x.CreateTask("act3")).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act1"), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("act3"), Times.Once);
            Assert.AreEqual(234, actList[0].WLId);
            Assert.AreEqual(345, actList[2].WLId);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Once);


        }
    }
}
