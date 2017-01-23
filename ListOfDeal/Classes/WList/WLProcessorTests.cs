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
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = "123", IsActive = true, Project = new Project() { Name = "Pr1" } }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act3", IsActive = true, Project = new Project() { Name = "Pr1" } }));

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null)).Returns(new WLTask() { id = "234" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;

            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null), Times.Once);
            Assert.AreEqual("234", proj.Actions[0].WLId);
            Assert.AreEqual("345", proj.Actions[2].WLId);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));


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
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>(MockBehavior.Strict);
            mockGeneralEntity.Setup(x => x.SaveChanges()).Returns(0);
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            //nothing should be done
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Once);
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

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), It.IsAny<DateTime>())).Returns(new WLTask() { id = "234", due_date = "2016-08-28" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act2", It.IsAny<int>(), null)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", WLProcessor.MySchedId, new DateTime(2016, 8, 28)), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act2", WLProcessor.MyListId, null), Times.Once);
            Assert.AreEqual("234", proj.Actions[0].WLId);
            Assert.AreEqual("345", proj.Actions[1].WLId);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));


        }
        [Test]
        public void CreateWlTasks_SetWLRevision() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, StatusId = 2, ScheduledTime = new DateTime(2016, 8, 28), Project = new Project() { Name = "Pr1" } }));


            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), It.IsAny<DateTime>())).Returns(new WLTask() { id = "234", due_date = "2016-08-28", revision = 9 });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            Assert.AreEqual(9, proj.Actions[0].WLTaskRevision);



        }
    
        [Test]
        public void CreateWlTasks_Buy_v2() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var a = new Action();
            a.Project = new Project() { TypeId = 1, Name = "Pr1" };
            a.IsActive = true;
            a.Name = "act1";
            a.ToBuy = true;
            proj.Actions.Add(new MyAction(a));

            var a2 = new Action();
            a2.Name = "act2";
            a2.IsActive = true;
            a2.Project = new Project() { Name = "Pr1" };
            proj.Actions.Add(new MyAction(a2));

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null)).Returns(new WLTask() { id = "234" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act2", It.IsAny<int>(), null)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", WLProcessor.MyBuyId, null), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act2", WLProcessor.MyListId, null), Times.Once);
            Assert.AreEqual("234", proj.Actions[0].WLId);
            Assert.AreEqual("345", proj.Actions[1].WLId);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));


        }




        [Test]
        public void CreateWlTasks_AddProjectName_OnlyForNonSimple() {
            //arrange
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, Project = new Project() { Name = "MySimpleProject", IsSimpleProject = true } }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", IsActive = true, Project = new Project() { Name = "MyNonSimpleProject", IsSimpleProject = false } }));

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();

            mockWlConnector.Setup(x => x.CreateTask(It.IsAny<string>(), It.IsAny<int>(), null)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            // wlProc.PopulateActions(actList);

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act1", WLProcessor.MyListId, null), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("MyNonSimpleProject - act2", WLProcessor.MyListId, null), Times.Once);



        }

        [Test]
        public void HandleCompletedWLTasks() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true,Project=new Project() });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", IsActive = true, Project = new Project() });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", IsActive = true, Project = new Project() });


            proj.Actions.Add(myAction1);
            proj.Actions.Add(myAction2);
            proj.Actions.Add(myAction3);

            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1" });
            taskList.Add(new WLTask() { id = "3" });
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
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedWLTasks_shouldbenullWlId() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true,Project=new Project() });

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
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);

        }
        [Test]
        public void HandleCompletedWLTasks_Buy_Scheduled() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            for (int i = 0; i < 7; i++) {
                var a = new MyAction(new Action() { Name = "Action" + i, WLId = i.ToString(), IsActive = true,Project=new Project() }) { Status = ActionsStatusEnum.Waited };
                proj.Actions.Add(a);
            }

            //var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true });
            //var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", IsActive = true });
            //var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", IsActive = true });
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
            taskList.Add(new WLTask() { id = "0" });
            taskList.Add(new WLTask() { id = "2" });

            var taskListSched = new List<WLTask>();
            taskListSched.Add(new WLTask() { id = "3" });

            var taskListBuy = new List<WLTask>();
            taskListBuy.Add(new WLTask() { id = "5" });

            var mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskListSched);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(taskListBuy);
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
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }

        [Test]
        public void HandleCompletedWLTasks_ShouldSkipActionsWithoutWLId() {
            //arrange

            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;

            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true, Project = new Project() });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", IsActive = true, Project = new Project() });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", IsActive = true, Project = new Project() });
            var myAction4 = new MyAction(new Action() { Name = "Action3", IsActive = true });

            proj.Actions.Add(myAction1);
            proj.Actions.Add(myAction2);
            proj.Actions.Add(myAction3);
            proj.Actions.Add(myAction4);
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1" });
            taskList.Add(new WLTask() { id = "3" });
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
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedActions() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var projLst = new ObservableCollection<MyProject>();
            var p = new Project();
            var proj = new MyProject(p);
            var act1 = new MyAction(new Action() { WLId = "123", WLTaskStatus = 1, IsActive = true,Project=p });
            var act2 = new MyAction(new Action { WLId = "234", WLTaskStatus = 2, IsActive = false,Project=p });
            proj.Actions.Add(act1);
            proj.Actions.Add(act2);
            projLst.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projLst);
            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            //var lstMock = new Mock<IDbSet<Action>>();
            //var lstAct = new List<Action>();
            //lstAct.Add(new Action() { WLId = "123", WLTaskStatus = 1, IsActive = true });
            //lstAct.Add(new Action() { WLId = "234", WLTaskStatus = 2, IsActive = true });

            //var querAct = lstAct.AsQueryable();
            //lstMock.Setup(m => m.Provider).Returns(querAct.Provider);
            //lstMock.Setup(m => m.Expression).Returns(querAct.Expression);
            //lstMock.Setup(m => m.ElementType).Returns(querAct.ElementType);
            //lstMock.Setup(m => m.GetEnumerator()).Returns(querAct.GetEnumerator());

            //mockGeneralEntity.Setup(x => x.Actions).Returns(lstMock.Object);
            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            mockWlConnector.Setup(x => x.CompleteTask("234")).Returns(new WLTask());
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);

            //act
            wlProc.HandleCompletedLODActions();
            //assert
            mockWlConnector.Verify(x => x.CompleteTask("234"), Times.Once); 
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(null, proj.Actions[1].WLId);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[1].WLTaskStatus);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }



        [Test]
        public void RaiseLog() {
            //arrange
            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            var mockMainVM = new Mock<IMainViewModel>();
            var p = new Project();
            var mp = new MyProject(p);
            var a1=new MyAction(new Action() { WLId = "123", WLTaskStatus = 1, IsActive = true,Project=p });
            var a2 = new MyAction(new Action() { WLId = "234", WLTaskStatus = 2, IsActive = true,Project=p });
            mp.Actions.Add(a1);
            mp.Actions.Add(a2);
            var lst = new ObservableCollection<MyProject>();
            lst.Add(mp);
            mockMainVM.Setup(x => x.Projects).Returns(lst);
            WLProcessor proc = new WLProcessor(mockMainVM.Object);
            logList = new List<string>();
            proc.Logged += Proc_Logged;
            var mockWlConnector = new Mock<IWLConnector>();
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            proc.CreateWlConnector(mockWlConnector.Object);
            //act
            proc.HandleCompletedLODActions();
            //assert
            Assert.AreEqual(3, logList.Count);

        }
        [Test]
        public void RaiseLog_2() {
            //arrange

            WLProcessor proc = new WLProcessor(null);
            logList = new List<string>();
            proc.Logged += Proc_Logged;
            //act
            proc.RaiseLog("test");
            proc.RaiseLog("test1 {0}", "test2");
            //assert
            Assert.AreEqual(2, logList.Count);
            Assert.AreEqual("test", logList[0]);
            Assert.AreEqual("test1 test2", logList[1]);

        }
        List<string> logList;
        private void Proc_Logged(WLEventArgs e) {
            logList.Add(e.Message);
        }

        [Test]
        public void HandleChangedLODActions_Name() {
            //arrange 
            //var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            //MainViewModel.generalEntity = mockGeneralEntity.Object;


            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            proj.Actions.Add(new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = "2", IsActive = true, Project = proj.parentEntity }));

            var proj2 = new MyProject(new Project()) { Name = "NotSimpleProject", Status = ProjectStatusEnum.InWork };
            proj2.IsSimpleProject = false;
            proj2.Actions.Add(new MyAction(new Action() { Name = "act3", WLId = "3", IsActive = true, Project = proj2.parentEntity }));
            proj2.Actions.Add(new MyAction(new Action() { Name = "Newact4", WLId = "4", WLTaskStatus = 1, IsActive = true, Project = proj2.parentEntity }));
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            projCollection.Add(proj);
            projCollection.Add(proj2);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);

            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1", title = "act1" });
            taskList.Add(new WLTask() { id = "2", title = "act2" });
            taskList.Add(new WLTask() { id = "3", title = "act3" });
            taskList.Add(new WLTask() { id = "4", title = "act4" });
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("1", "Newact1"), Times.Once);
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("4", "NotSimpleProject - Newact4"), Times.Once);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj2.Actions[1].WLTaskStatus);
        }


        [Test]
        public void HandleChangedLODActions_SchouldUpdateWLRevision() {
            //arrange 
            //var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            //MainViewModel.generalEntity = mockGeneralEntity.Object;


            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            var myAction1 = new MyAction(new Action());
            myAction1.Name = "Newact1";
            myAction1.WLId = "1";
            myAction1.WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
            myAction1.IsActive = true;
            myAction1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(myAction1);

            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);

            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1", title = "act1" });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.ChangeTitleOfTask("1", "Newact1")).Returns(new WLTask() { revision = 3 });
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedLODActions();
            //assert
            Assert.AreEqual(3, myAction1.WLTaskRevision);
        }

        [Test]
        public void HandleChangedLODActions_ScheduledTime() {
            //arrange 
            //var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            //MainViewModel.generalEntity = mockGeneralEntity.Object;


            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            var act1 = new MyAction(new Action() { Project = proj.parentEntity });
            act1.Name = "act1";
            act1.WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
            act1.WLId = "1";
            act1.IsActive = true;
            act1.Status = ActionsStatusEnum.Scheduled;
            act1.ScheduledTime = new DateTime(2016, 9, 11);

            var act2 = new MyAction(new Action() { Project = proj.parentEntity, });
            act2.Name = "act2";
            act2.WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
            act2.WLId = "2";
            act2.IsActive = true;
            act2.Status = ActionsStatusEnum.Scheduled;
            act2.ScheduledTime = new DateTime(2016, 9, 2);

            proj.Actions.Add(act1);
            proj.Actions.Add(act2);
            var proj2 = new MyProject(new Project()) { Name = "NotSimpleProject", Status = ProjectStatusEnum.InWork };
            proj2.IsSimpleProject = false;

            var act3 = new MyAction(new Action() { Project = proj2.parentEntity });
            act3.Name = "act3";
            act3.WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
            act3.WLId = "3";
            act3.IsActive = true;
            act3.Status = ActionsStatusEnum.Scheduled;
            act3.ScheduledTime = new DateTime(2016, 9, 3);

            var act4 = new MyAction(new Action() { Project = proj2.parentEntity });
            act4.Name = "act4";
            act4.WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
            act4.WLId = "4";
            act4.IsActive = true;
            act4.Status = ActionsStatusEnum.Scheduled;
            act4.ScheduledTime = new DateTime(2016, 9, 14);
            proj2.Actions.Add(act3);
            proj2.Actions.Add(act4);
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            projCollection.Add(proj);
            projCollection.Add(proj2);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);

            var mockWlConnector = new Mock<IWLConnector>(MockBehavior.Strict);
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1", title = "act1", due_date = "2016-09-01", list_id = WLProcessor.MySchedId });
            taskList.Add(new WLTask() { id = "2", title = "act2", due_date = "2016-09-02", list_id = WLProcessor.MySchedId });
            taskList.Add(new WLTask() { id = "3", title = "NotSimpleProject - act3", due_date = "2016-09-03", list_id = WLProcessor.MySchedId });
            taskList.Add(new WLTask() { id = "4", title = "NotSimpleProject - act4", due_date = "2016-09-04", list_id = WLProcessor.MySchedId });
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.ChangeScheduledTime("1", "2016-09-11")).Returns(new WLTask());
            mockWlConnector.Setup(x => x.ChangeScheduledTime("4", "2016-09-14")).Returns(new WLTask());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeScheduledTime("1", "2016-09-11"), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("4", "2016-09-14"), Times.Once);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj2.Actions[1].WLTaskStatus);
        }


        [Test]
        public void HandleChangedWLTask_Title() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            var act2 = new MyAction(new Action());
            act2.Name = "TestName2";
            act2.WLTaskRevision = 1;
            act2.WLId = "234";
            act2.IsActive = true;
            act2.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act2);
            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });
            taskList.Add(new WLTask() { id = "234", title = "TestName2", revision = 1 });
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual("NewTestName1", act1.Name);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual("TestName2", act2.Name);
            Assert.AreEqual(1, act2.WLTaskRevision);
        }
        [Test]
        public void HandleChangedWLTask_Title_notSimple() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "Project1 - NewTestName1", revision = 2 });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual("NewTestName1", act1.Name);
            Assert.AreEqual(2, act1.WLTaskRevision);

        }

        [Test]
        public void HandleChangedWLTask_Title_notSimple_notValidName() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual("NewTestName1", act1.Name);
            Assert.AreEqual(2, act1.WLTaskRevision);
            //maybe rename wltask?
        }

        [Test]
        public void HandleChangedWLTask_ThereIsNotTaskForAction() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
        //    taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
               
            //act
            try {
                wlProc.HandleChangedWLTask();
            }
            catch(Exception ex) {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
            //assert
            var b = logList[1].Contains("There is no task for");
            Assert.AreEqual(true, b);
            //Assert.AreEqual(2, act1.WLTaskRevision);
        }
        [Test]
        public void HandleCompletedWLTask_Message() {
            //arrange
        
            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            var dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            MainViewModel.DataProvider = dataProviderEntity.Object;
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project() );
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action() { Id = 44 });
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
        //        taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;

            //act
        
                wlProc.HandleCompletedWLTasks();
            //assert
            var st = string.Format("complete action - {0} {1}", "Project1 - TestName1", 44);
            
            Assert.AreEqual(logList[2], st);
            //Assert.AreEqual(2, act1.WLTaskRevision);
        }


        [Test]
        public void HandleChangedWLTask_SchouldNotChangeActionsWLStatus() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);


            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
        }
        [Test]
        public void HandleChangedWLTask_ScheduledTime() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.ScheduledTime = new DateTime(2016, 9, 11);
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2, due_date = "2016-09-15" });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(new DateTime(2016, 9, 15), act1.ScheduledTime);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
            Assert.AreEqual(4, logList.Count);
            Assert.AreEqual("act has old revision Project1 - act1", logList[1]);
            Assert.AreEqual("Project1 - act1 is changing time from 2016-09-11 to 2016-09-15", logList[2]);
        }

        [Test]
        public void HandleChangedWLTask_ScheduledTimeFromNullToNonNull() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.Status = ActionsStatusEnum.Waited;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2, due_date = "2016-09-12" });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(ActionsStatusEnum.Scheduled, act1.Status);
            Assert.AreEqual(new DateTime(2016, 9, 12), act1.ScheduledTime);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
            Assert.AreEqual(4, logList.Count);
            Assert.AreEqual("Project1 - act1 -set time to 2016-09-12", logList[2]);
        }

        [Test]
        public void HandleChangedWLTask_ScheduledTimeFromNotNullToNull() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.Status = ActionsStatusEnum.Scheduled;
            act1.ScheduledTime = new DateTime(2016, 9, 12);
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2, due_date = null });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(ActionsStatusEnum.Waited, act1.Status);
            Assert.AreEqual(null, act1.ScheduledTime);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
            Assert.AreEqual(4, logList.Count);
            Assert.AreEqual("Project1 - act1 -delete time", logList[2]);
        }


        [Test]
        public void HandleChangedLodAction_StatusFromNonSchedToSched() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.ScheduledTime = new DateTime(2016, 9, 11);
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2 });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            int k = 5;
            mockWlConnector.Setup(x => x.ChangeListOfTask("123", WLProcessor.MySchedId)).Returns(new WLTask() { revision = ++k });
            mockWlConnector.Setup(x => x.ChangeScheduledTime("123", "2016-09-11")).Returns(new WLTask() { revision = ++k });
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeListOfTask("123", WLProcessor.MySchedId), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("123", "2016-09-11"), Times.Once);
            Assert.AreEqual(k, act1.WLTaskRevision);
            //Assert.AreEqual(new DateTime(2016, 9, 15), act1.ScheduledTime);
            //Assert.AreEqual(2, act1.WLTaskRevision);
        }

        [Test]
        public void HandleChangedLodAction_StatusFromSchedToNonSched() {
            //arrange
            var mockMainVM = new Mock<IMainViewModel>();
            var projCollection = new ObservableCollection<MyProject>();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.ScheduledTime = new DateTime(2016, 9, 11);
            act1.Status = ActionsStatusEnum.Waited;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            WLProcessor wlProc = new WLProcessor(mockMainVM.Object);
            var mockWlConnector = new Mock<IWLConnector>();
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "123", title = "act1", revision = 2, due_date = "2016-09-11", list_id = WLProcessor.MySchedId });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            int k = 9;
            mockWlConnector.Setup(x => x.ChangeListOfTask("123", WLProcessor.MyListId)).Returns(new WLTask() { revision = ++k });
            mockWlConnector.Setup(x => x.ChangeScheduledTime("123", "null")).Returns(new WLTask() { revision = ++k });

            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeListOfTask("123", WLProcessor.MyListId), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("123", "null"), Times.Once);
            Assert.AreEqual(k, act1.WLTaskRevision);
            //Assert.AreEqual(new DateTime(2016, 9, 15), act1.ScheduledTime);
            //Assert.AreEqual(2, act1.WLTaskRevision);
        }
    }

}
