using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public class WLProcessor {
        List<MyAction> allActions;
        List<WLTask> allTasks;
        IWLConnector wlConnector;
        const int MyListId = 262335124;
        public void PopulateActions(List<MyAction> _actions) {
            allActions = _actions;
        }
        public void CreateWlConnector(IWLConnector _conn) {
            wlConnector = _conn;
            (wlConnector as WLConnector).Start();
        }
        public void CreateWlTasks() {
            var emptyActions = allActions.Where(x => x.WLId == null);
            if (emptyActions.Count() == 0)
                return;
            //   emptyActions = emptyActions.Take(1);
            foreach (var act in emptyActions) {
                string title = act.Name;
                var wlTask = wlConnector.CreateTask(title,MyListId);
                act.WLId = wlTask.id;
            }
            MainViewModel.SaveChanges();
        }
     
        public void HandleCompletedWLTasks() {
            allTasks = GetAllActiveTasks();
            var lstwlIdinLod = allActions.Select(x =>(int) x.WLId);
            var lstwlIdInWL = allTasks.Select(x => x.id);
            var diff = lstwlIdinLod.Except(lstwlIdInWL);
      
            foreach (int tskId in diff) {
                Debug.Print(tskId.ToString());
                allActions.Where(x => x.WLId == tskId).First().Status = ActionsStatusEnum.Completed;

            }
            
        }
        List<WLTask> GetAllActiveTasks() {
            return wlConnector.GetTasksForList(MyListId);
        }
        public void HandleCompletedLODActions() {

        }

    }

    [TestFixture]
    public class WLProcessorTest {
        [Test]
        public void CreateWlTasks() {
            //arrange
            var actList = new List<MyAction>();
            actList.Add(new MyAction(new Action() { Name = "act1" }));
            actList.Add(new MyAction(new Action() { Name = "act2",WLId=123 }));
            actList.Add(new MyAction(new Action() { Name = "act3" }));
            WLProcessor wlProc = new WLProcessor();
            var mockWlConnector = new Mock<IWLConnector>();
          
            mockWlConnector.Setup(x => x.CreateTask("act1",It.IsAny<int>())).Returns(new WLTask() { id = 234 });
            mockWlConnector.Setup(x => x.CreateTask("act3", It.IsAny<int>())).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act1", It.IsAny<int>()), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("act3", It.IsAny<int>()), Times.Once);
            Assert.AreEqual(234, actList[0].WLId);
            Assert.AreEqual(345, actList[2].WLId);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Once);


        }
        [Test]
        public void CreateWlTasks_NullAction() {
            //arrange
            var actList = new List<MyAction>();

            WLProcessor wlProc = new WLProcessor();
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            wlProc.CreateWlConnector(mockWlConnector.Object);
            wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>(MockBehavior.Strict);
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            //nothing should be done
        }

        [Test]
        public void HandleCompletedWLTasks() {
            //arrange
            var actList = new List<MyAction>();
            var myAction = new MyAction(new Action() { Name = "Action1", WLId = 1 });
            var myAction1 = new MyAction(new Action() { Name = "Action2", WLId = 2 });
            var myAction2 = new MyAction(new Action() { Name = "Action3", WLId = 3 });

            actList.Add(myAction);
            actList.Add(myAction1);
            actList.Add(myAction2);

            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = 1 });
            taskList.Add(new WLTask() { id = 3 });
            WLProcessor wlProc = new WLProcessor();
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(taskList);
            wlProc.CreateWlConnector(mockWlConnector.Object);
            wlProc.PopulateActions(actList);
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum.Completed, myAction1.Status);


        }
    }
}
