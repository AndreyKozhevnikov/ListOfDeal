using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    [TestFixture]
    public class WLProcessorTest {
        [Test]
        public void CreateWlTasks() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, Project = new Project() { Name = "Pr1" } }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = 123, IsActive = true, Project = new Project() { Name = "Pr1" } }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act3", IsActive = true, Project = new Project() { Name = "Pr1" } }));

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null)).Returns(new WLTask() { id = 234 });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null)).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null), Times.Once);
            Assert.AreEqual(234, proj.Actions[0].WLId);
            Assert.AreEqual(345, proj.Actions[2].WLId);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));


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
            mockGeneralEntity.Setup(x => x.SaveChanges()).Returns(0);
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            //nothing should be done
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Once);
        }
        [Test]
        public void CreateWlTasks_Scheduled() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, StatusId = 2, ScheduledTime = new DateTime(2016, 8, 28), Project = new Project() { Name = "Pr1" } }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", IsActive = true, Project = new Project() { Name = "Pr1" } }));


            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), It.IsAny<DateTime>())).Returns(new WLTask() { id = 234, due_date = "2016-08-28" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act2", It.IsAny<int>(), null)).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", wlProc.MySchedId, new DateTime(2016, 8, 28)), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act2", wlProc.MyListId, null), Times.Once);
            Assert.AreEqual(234, proj.Actions[0].WLId);
            Assert.AreEqual(345, proj.Actions[1].WLId);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));


        }

        [Test]
        public void CreateWlTasks_Buy() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var a = new Action();
            a.Project = new Project() { TypeId = 10, Name = "Pr1" };
            a.IsActive = true;
            a.Name = "act1";
            proj.Actions.Add(new MyAction(a));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", IsActive = true, Project = new Project() { Name = "Pr1" } }));


            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null)).Returns(new WLTask() { id = 234 });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act2", It.IsAny<int>(), null)).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", wlProc.MyBuyId, null), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act2", wlProc.MyListId, null), Times.Once);
            Assert.AreEqual(234, proj.Actions[0].WLId);
            Assert.AreEqual(345, proj.Actions[1].WLId);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));


        }

   



             [Test]
        public void CreateWlTasks_AddProjectName_OnlyForNonSimple() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, Project = new Project() { Name = "MySimpleProject",IsSimpleProject=true } }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", IsActive = true, Project = new Project() { Name = "MyNonSimpleProject", IsSimpleProject = false } }));

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask(It.IsAny<string>(), It.IsAny<int>(), null)).Returns(new WLTask() { id = 345 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act1", wlProc.MyListId, null), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("MyNonSimpleProject - act2", wlProc.MyListId, null), Times.Once);



        }

        [Test]
        public void HandleCompletedWLTasks() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = 1, IsActive = true });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = 2, IsActive = true });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = 3, IsActive = true });


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
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedWLTasks_shouldbenullWlId() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = 1, IsActive = true });

            proj.Actions.Add(myAction1);

            var taskList = new List<WLTask>();
            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(taskList);
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum.Completed, myAction1.Status);
            Assert.AreEqual(null, myAction1.WLId);
            Assert.AreEqual(0, myAction1.parentEntity.WLTaskStatus);

        }
        [Test]
        public void HandleCompletedWLTasks_Buy_Scheduled() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            for (int i = 0; i < 7; i++) {
                var a = new MyAction(new Action() { Name = "Action" + i, WLId = i, IsActive = true }) { Status = ActionsStatusEnum.Waited };
                proj.Actions.Add(a);
            }

            //var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = 1, IsActive = true });
            //var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = 2, IsActive = true });
            //var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = 3, IsActive = true });
            //var myAction4 = new MyAction(new Action() { Name = "Action4", WLId = 4, IsActive = true });
            //var myAction5 = new MyAction(new Action() { Name = "Action5", WLId = 5, IsActive = true });
            //var myAction6 = new MyAction(new Action() { Name = "Action6", WLId = 6, IsActive = true });
            //var myAction7 = new MyAction(new Action() { Name = "Action7", WLId = 7, IsActive = true });

            //proj.Actions.Add(myAction1);
            //proj.Actions.Add(myAction2);
            //proj.Actions.Add(myAction3);
            //proj.Actions.Add(myAction4);
            //proj.Actions.Add(myAction5);
            //proj.Actions.Add(myAction6);
            //proj.Actions.Add(myAction7);

            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = 0 });
            taskList.Add(new WLTask() { id = 2 });

            var taskListSched = new List<WLTask>();
            taskListSched.Add(new WLTask() { id = 3 });

            var taskListBuy = new List<WLTask>();
            taskListBuy.Add(new WLTask() { id = 5 });

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.GetTasksForList(wlProc.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(wlProc.MySchedId)).Returns(taskListSched);
            mockWlConnector.Setup(x => x.GetTasksForList(wlProc.MyBuyId)).Returns(taskListBuy);
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum.Completed, proj.Actions[1].Status);
            Assert.AreEqual(ActionsStatusEnum.Completed, proj.Actions[4].Status);
            Assert.AreEqual(ActionsStatusEnum.Completed, proj.Actions[6].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, proj.Actions[0].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, proj.Actions[2].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, proj.Actions[3].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, proj.Actions[5].Status);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }

        [Test]
        public void HandleCompletedWLTasks_ShouldSkipActionsWithoutWLId() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = 1, IsActive = true });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = 2, IsActive = true });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = 3, IsActive = true });
            var myAction4 = new MyAction(new Action() { Name = "Action3", IsActive = true });

            proj.Actions.Add(myAction1);
            proj.Actions.Add(myAction2);
            proj.Actions.Add(myAction3);
            proj.Actions.Add(myAction4);
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
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedActions() {
            //arrange
            WLProcessor wlProc = new WLProcessor(null);
            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            var lstMock = new Mock<IDbSet<Action>>();
            var lstAct = new List<Action>();
            lstAct.Add(new Action() { WLId = 123, WLTaskStatus = 1, IsActive = true });
            lstAct.Add(new Action() { WLId = 234, WLTaskStatus = 2, IsActive = true });

            var querAct = lstAct.AsQueryable();
            lstMock.Setup(m => m.Provider).Returns(querAct.Provider);
            lstMock.Setup(m => m.Expression).Returns(querAct.Expression);
            lstMock.Setup(m => m.ElementType).Returns(querAct.ElementType);
            lstMock.Setup(m => m.GetEnumerator()).Returns(querAct.GetEnumerator());

            mockGeneralEntity.Setup(x => x.Actions).Returns(lstMock.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.CompleteTask(234)).Returns(new WLTask());

            wlProc.CreateWlConnector(mockWlConnector.Object);

            //act
            wlProc.HandleCompletedLODActions();
            //assert
            mockWlConnector.Verify(x => x.CompleteTask(234), Times.Once);
            Assert.AreEqual(1, lstAct[0].WLTaskStatus);
            Assert.AreEqual(null, lstAct[1].WLId);
            Assert.AreEqual(0, lstAct[1].WLTaskStatus);
            mockGeneralEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }



        [Test]
        public void RaiseLog() {
            //arrange
            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;

            var lstMock = new Mock<IDbSet<Action>>();
            var lstAct = new List<Action>();
            //lstAct.Add(new Action() { WLId = 123, WLTaskStatus = 1, IsActive = true });
            //lstAct.Add(new Action() { WLId = 234, WLTaskStatus = 2, IsActive = true });

            var querAct = lstAct.AsQueryable();
            lstMock.Setup(m => m.Provider).Returns(querAct.Provider);
            lstMock.Setup(m => m.Expression).Returns(querAct.Expression);
            lstMock.Setup(m => m.ElementType).Returns(querAct.ElementType);
            lstMock.Setup(m => m.GetEnumerator()).Returns(querAct.GetEnumerator());

            mockGeneralEntity.Setup(x => x.Actions).Returns(lstMock.Object);

            WLProcessor proc = new WLProcessor(null);
            logList = new List<string>();
            proc.Logged += Proc_Logged;
            //act
            proc.HandleCompletedLODActions();
            //assert
            Assert.AreEqual(2, logList.Count);

        }
        List<string> logList;
        private void Proc_Logged(WLEventArgs e) {
            logList.Add(e.Message);
        }
    }

}
