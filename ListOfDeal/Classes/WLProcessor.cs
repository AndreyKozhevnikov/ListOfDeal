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

        //public void PopulateActions(List<MyAction> _actions) {
        //    allActions = _actions;
        //}
        IMainViewModel parentVM;
        public WLProcessor(IMainViewModel _parentVM) {
            parentVM = _parentVM;
        }
        public void CreateWlConnector(IWLConnector _conn) {
            wlConnector = _conn;
            //  (wlConnector as WLConnector).Start();
        }
        public void CreateWlTasks() {
            allActions = GetActiveActions();
            var emptyActions = allActions.Where(x => x.WLId == null);
            if (emptyActions.Count() == 0)
                return;
            //   emptyActions = emptyActions.Take(1);
            foreach (var act in emptyActions) {
                string title = act.Name;
                var wlTask = wlConnector.CreateTask(title, MyListId);
                act.WLId = wlTask.id;
            }
            MainViewModel.SaveChanges();
        }

        public void HandleCompletedWLTasks() {
            allTasks = GetAllActiveTasks();
            allActions = GetActiveActions();
            var lstwlIdinLod = allActions.Select(x => (int)x.WLId);
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
        List<MyAction> GetActiveActions() {
            var lst = parentVM.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.IsActive).ToList();
            return lst;
        }


        public void HandleCompletedLODActions() {

        }


    }

    [TestFixture]
    public class WLProcessorTest {
        [Test]
        public void CreateWlTasks() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1",IsActive=true }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = 123,IsActive=true }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act3",IsActive=true }));
            
            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("act1", It.IsAny<int>())).Returns(new WLTask() { id = 234 });
            mockWlConnector.Setup(x => x.CreateTask("act3", It.IsAny<int>())).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act1", It.IsAny<int>()), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("act3", It.IsAny<int>()), Times.Once);
            Assert.AreEqual(234, proj.Actions[0].WLId);
            Assert.AreEqual(345, proj.Actions[2].WLId);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Once);


        }
        [Test]
        public void CreateWlTasks_NullAction() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

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
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = 1,IsActive=true });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = 2,IsActive=true });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = 3,IsActive=true });

         
            proj.Actions.Add(myAction1);
            proj.Actions.Add(myAction2);
            proj.Actions.Add(myAction3);

            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = 1 });
            taskList.Add(new WLTask() { id = 3 });
            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(taskList);
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum.Completed, myAction2.Status);


        }
    }
}
