using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ListOfDeal.Classes.Tests {
#if DebugTest
    [TestFixture]
    public class WLProcessorTest {
        Mock<IMainViewModel> mockMainVM;
        WLProcessor wlProc;
        Mock<IWLConnector> mockWlConnector;
        ObservableCollection<MyProject> projCollection;
        Mock<IListOfDealBaseEntities> mockGeneralEntity;
        Mock<IMainViewModelDataProvider> dataProviderEntity;
        List<WLTask> taskList;

        public void Initialize(MockBehavior behavior = MockBehavior.Strict, bool configureTaskList = false) {
            projCollection = new ObservableCollection<MyProject>();
            mockMainVM = new Mock<IMainViewModel>();
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);
            wlProc = new WLProcessor(mockMainVM.Object);
            mockWlConnector = new Mock<IWLConnector>(behavior);
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            wlProc.CreateWlConnector(mockWlConnector.Object);
            mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            mockWlConnector.Setup(x => x.GetNodesForTask(It.IsAny<string>())).Returns(new List<WLNote>());
            MainViewModel.DataProvider = dataProviderEntity.Object;
            taskList = new List<WLTask>();

            if (configureTaskList)
                ReturnTaskListfromMyListId2();
        }

        void ReturnTaskListfromMyListId2() {
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
        }

        [Test]
        public void CreateWlTasks() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act1", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = "123", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act3", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act4comment", StatusId = (int)ActionsStatusEnum2.InWork, Comment = "test comment", Project = new Project() { Name = "Pr1" } }));

            var nonActiveProject = new MyProject(new Project()) { Status = ProjectStatusEnum.Delayed };
            projCollection.Add(nonActiveProject);
            nonActiveProject.Actions.Add(new MyAction(new Action() { Name = "actFromNonActiveProject", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "NonActiveProject2" } }));
            nonActiveProject.Actions.Add(new MyAction(new Action() { Name = "actFromNonActiveProjectWithTime", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = DateTime.Today, Project = new Project() { Name = "NonActiveProject2" } }));

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "234" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act4comment", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "456" });
            mockWlConnector.Setup(x => x.CreateTask("NonActiveProject2 - actFromNonActiveProjectWithTime", It.IsAny<int>(), DateTime.Today, false)).Returns(new WLTask() { id = "567" });
            mockWlConnector.Setup(x => x.CreateNote("456", "test comment")).Returns(new WLNote());

            wlProc.UpdateData();
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act4comment", It.IsAny<int>(), null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("NonActiveProject2 - actFromNonActiveProjectWithTime", It.IsAny<int>(), DateTime.Today, false), Times.Once);
            
            Assert.AreEqual("234", firstProject.Actions[0].WLId);
            Assert.AreEqual("345", firstProject.Actions[2].WLId);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));
            mockWlConnector.Verify(x => x.CreateNote("456", "test comment"), Times.Once);

        }
        [Test]
        public void CreateWlTasks_NullAction() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            //act
            wlProc.CreateWlTasks();
            //assert
            //nothing should be done
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Once);
        }
        [Test]
        public void CreateWlTasks_Scheduled() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act1", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 8, 28), Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act2", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "Pr1" } }));
            projCollection.Add(firstProject);
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), It.IsAny<DateTime>(), false)).Returns(new WLTask() { id = "234", due_date = "2016-08-28" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act2", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());

            wlProc.UpdateData();
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", WLProcessor.MySchedId, new DateTime(2016, 8, 28), false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act2", WLProcessor.MyListId, null, false), Times.Once);
            Assert.AreEqual("234", firstProject.Actions[0].WLId);
            Assert.AreEqual("345", firstProject.Actions[1].WLId);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void CreateWlTasks_SetWLRevision() {
            //arrange
            Initialize();
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 8, 28), Project = new Project() { Name = "Pr1" } }));

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), It.IsAny<DateTime>(), false)).Returns(new WLTask() { id = "234", due_date = "2016-08-28", revision = 9 });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());

            wlProc.UpdateData();
            //act
            wlProc.CreateWlTasks();
            //assert
            Assert.AreEqual(9, proj.Actions[0].WLTaskRevision);
        }

        [Test]
        public void CreateWlTasks_Buy_v2() {
            //arrange
            Initialize();
            var parentProject = new Project() { Name = "Pr1" };
            var firstProject = new MyProject(parentProject) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            var a = new Action();
            a.Project = parentProject;
            a.StatusId = (int)ActionsStatusEnum2.InWork;
            a.Name = "act1";
            a.ToBuy = true;
            firstProject.Actions.Add(new MyAction(a));

            var a2 = new Action();
            a2.Name = "act2";
            a2.StatusId = (int)ActionsStatusEnum2.InWork;
            a2.Project = parentProject;
            firstProject.Actions.Add(new MyAction(a2));

            var a3 = new Action();
            a3.Name = "act3";
            a3.StatusId = (int)ActionsStatusEnum2.InWork;
            a3.Project = parentProject;
            var ma3 = new MyAction(a3);
            ma3.ToBuy = true;
            ma3.ScheduledTime = new DateTime(2017, 2, 2);
            firstProject.Actions.Add(ma3);

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "234" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act2", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), new DateTime(2017, 2, 2), false)).Returns(new WLTask() { id = "456" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());

            wlProc.UpdateData();
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", WLProcessor.MyBuyId, null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act2", WLProcessor.MyListId, null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act3", WLProcessor.MyBuyId, new DateTime(2017, 2, 2), false), Times.Once);
            Assert.AreEqual("234", firstProject.Actions[0].WLId);
            Assert.AreEqual("345", firstProject.Actions[1].WLId);
            Assert.AreEqual("456", firstProject.Actions[2].WLId);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void CreateWlTasks_AddProjectName_OnlyForNonSimple() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act1", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "MySimpleProject", IsSimpleProject = true } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act2", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() { Name = "MyNonSimpleProject", IsSimpleProject = false } }));

            mockWlConnector.Setup(x => x.CreateTask(It.IsAny<string>(), It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());

            wlProc.UpdateData();
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act1", WLProcessor.MyListId, null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("MyNonSimpleProject - act2", WLProcessor.MyListId, null, false), Times.Once);
        }

        [Test]
        public void HandleCompletedWLTasks() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });

            firstProject.Actions.Add(myAction1);
            firstProject.Actions.Add(myAction2);
            firstProject.Actions.Add(myAction3);

            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1" });
            taskList.Add(new WLTask() { id = "3" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTask("2")).Returns(new WLTask() { id = "2", completed_at = "2000-02-03T17:53:04.953Z" });
            mockWlConnector.Setup(x => x.GetNodesForTask("2")).Returns(new List<WLNote>() { new WLNote() { content = "content test node" } });
            wlProc.UpdateData();
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum2.Done, myAction2.Status2);
            Assert.AreEqual(new DateTime(2000, 2, 3), myAction2.CompleteTime);
            Assert.AreEqual("content test node", myAction2.Comment);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedWLTasks_shouldbenullWlId() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
            firstProject.Actions.Add(myAction1);
            mockWlConnector.Setup(x => x.GetTask(It.IsAny<string>())).Returns(new WLTask() { completed_at = "2000-02-03T17:53:04.953Z" });
            wlProc.UpdateData();
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum2.Done, myAction1.Status2);
            Assert.AreEqual(null, myAction1.WLId);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);

        }
        [Test]
        public void HandleCompletedWLTasks_Buy_Scheduled() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            for (int i = 0; i < 7; i++) {
                var a = new MyAction(new Action() { Name = "Action" + i, WLId = i.ToString(), StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
                firstProject.Actions.Add(a);
            }

            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "0" });
            taskList.Add(new WLTask() { id = "2" });

            var taskListSched = new List<WLTask>();
            taskListSched.Add(new WLTask() { id = "3" });

            var taskListBuy = new List<WLTask>();
            taskListBuy.Add(new WLTask() { id = "5" });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskListSched);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(taskListBuy);
            mockWlConnector.Setup(x => x.GetTask(It.IsAny<string>())).Returns(new WLTask() { completed_at = "2000-02-03T17:53:04.953Z" });
            wlProc.UpdateData();
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum2.Done, firstProject.Actions[1].Status2);
            Assert.AreEqual(ActionsStatusEnum2.Done, firstProject.Actions[4].Status2);
            Assert.AreEqual(ActionsStatusEnum2.Done, firstProject.Actions[6].Status2);
            Assert.AreEqual(ActionsStatusEnum2.InWork, firstProject.Actions[0].Status2);
            Assert.AreEqual(ActionsStatusEnum2.InWork, firstProject.Actions[2].Status2);
            Assert.AreEqual(ActionsStatusEnum2.InWork, firstProject.Actions[3].Status2);
            Assert.AreEqual(ActionsStatusEnum2.InWork, firstProject.Actions[5].Status2);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }

        [Test]
        public void HandleCompletedWLTasks_ShouldSkipActionsWithoutWLId() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", StatusId = (int)ActionsStatusEnum2.InWork, Project = new Project() });
            var myAction4 = new MyAction(new Action() { Name = "Action3", StatusId = (int)ActionsStatusEnum2.InWork });

            firstProject.Actions.Add(myAction1);
            firstProject.Actions.Add(myAction2);
            firstProject.Actions.Add(myAction3);
            firstProject.Actions.Add(myAction4);
            var taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1" });
            taskList.Add(new WLTask() { id = "3" });
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTask(It.IsAny<string>())).Returns(new WLTask() { completed_at = "2000-02-03T17:53:04.953Z" });
            wlProc.UpdateData();
            //act
            wlProc.HandleCompletedWLTasks();
            //assert
            Assert.AreEqual(ActionsStatusEnum2.Done, myAction2.Status2);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedActions() {
            //arrange
            Initialize();
            var p = new Project();
            var firstProject = new MyProject(p);
            projCollection.Add(firstProject);
            var act1 = new MyAction(new Action() { WLId = "123", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = p });
            var act2 = new MyAction(new Action { WLId = "234", WLTaskStatus = 2, StatusId = (int)ActionsStatusEnum2.Delay, Project = p });
            firstProject.Actions.Add(act1);
            firstProject.Actions.Add(act2);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);

            mockWlConnector.Setup(x => x.CompleteTask("234")).Returns(new WLTask());
            mockWlConnector.Setup(x => x.GetTasksForList(It.IsAny<int>())).Returns(new List<WLTask>());
            //act
            wlProc.HandleCompletedLODActions();
            //assert
            mockWlConnector.Verify(x => x.CompleteTask("234"), Times.Once);
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, firstProject.Actions[0].WLTaskStatus);
            Assert.AreEqual(null, firstProject.Actions[1].WLId);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, firstProject.Actions[1].WLTaskStatus);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }



        [Test]
        public void RaiseLog() {
            //arrange
            Initialize();
            var p = new Project();
            var mp = new MyProject(p);
            var a1 = new MyAction(new Action() { WLId = "123", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = p });
            var a2 = new MyAction(new Action() { WLId = "234", WLTaskStatus = 2, StatusId = (int)ActionsStatusEnum2.InWork, Project = p });
            mp.Actions.Add(a1);
            mp.Actions.Add(a2);

            projCollection.Add(mp);
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            mockWlConnector.Setup(x => x.CompleteTask("234")).Returns(new WLTask());
            //act
            wlProc.HandleCompletedLODActions();
            //assert
            Assert.AreEqual(4, logList.Count);

        }


        List<string> logList;
        private void Proc_Logged(string st) {
            logList.Add(st);
        }

        [Test]
        public void HandleChangedLODActions_Name() {
            //arrange 
            Initialize(MockBehavior.Default, true);

            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            MyAction myAction1 = new MyAction(new Action() { Name = "old name", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            myAction1.Name = "Newact1";
            proj.Actions.Add(myAction1);
            MyAction myAction2 = new MyAction(new Action() { Name = "act2", WLId = "2", StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            proj.Actions.Add(myAction2);

            var proj2 = new MyProject(new Project()) { Name = "NotSimpleProject", Status = ProjectStatusEnum.InWork };
            proj2.IsSimpleProject = false;
            proj2.Actions.Add(new MyAction(new Action() { Name = "act3", WLId = "3", StatusId = (int)ActionsStatusEnum2.InWork, Project = proj2.parentEntity }));
            MyAction myAction4 = new MyAction(new Action() { Name = "old name", WLId = "4", StatusId = (int)ActionsStatusEnum2.InWork, Project = proj2.parentEntity });
            myAction4.Name = "Newact4";
            proj2.Actions.Add(myAction4);
            projCollection.Add(proj);
            projCollection.Add(proj2);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 10 });
            taskList.Add(new WLTask() { id = "2", title = "act2" });
            taskList.Add(new WLTask() { id = "3", title = "act3" });
            taskList.Add(new WLTask() { id = "4", title = "act4", revision = 40 });

            mockWlConnector.Setup(x => x.ChangeTitleOfTask("1", "Newact1", 10)).Returns(new WLTask());
            mockWlConnector.Setup(x => x.ChangeTitleOfTask("4", "NotSimpleProject - Newact4", 40)).Returns(new WLTask());
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("1", "Newact1", 10), Times.Once);
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("4", "NotSimpleProject - Newact4", 40), Times.Once);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj2.Actions[1].WLTaskStatus);
        }
        [Test]
        public void HandleChangedLODActions_Name_severaltimes() {
            //arrange 
            Initialize(MockBehavior.Default, true);

            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            MyAction myAction1 = new MyAction(new Action() { Name = "old name", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            myAction1.Name = "Newact1";
            myAction1.Name = "Newact1(second time)";
            proj.Actions.Add(myAction1);
            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 10 });

            mockWlConnector.Setup(x => x.ChangeTitleOfTask("1", "Newact1(second time)", It.IsAny<int>())).Returns(new WLTask() { revision = 11, title = "Newact1(second time)", id = "1" });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("1", "Newact1(second time)", It.IsAny<int>()), Times.Once);
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("1", It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(11, myAction1.WLTaskRevision);
        }
        [Test]
        public void HandleChangedLODActions_Name_severaltimes_andbacktooriginal() {
            //arrange 
            Initialize(MockBehavior.Default, true);

            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            MyAction myAction1 = new MyAction(new Action() { Name = "act1", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            myAction1.Name = "Newact1";
            myAction1.Name = "act1";
            proj.Actions.Add(myAction1);
            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 10 });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeTitleOfTask("1", It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(10, myAction1.WLTaskRevision);
        }
        [Test]
        public void HandleChangedLODActions_Note() {
            //arrange
            Initialize(MockBehavior.Default, true);


            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //create
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, Comment = "old comment" });
            myAction1.Comment = "test comment";
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1" });
            mockWlConnector.Setup(x => x.GetNodesForTask("1")).Returns(new List<WLNote>());
            //change
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, Comment = "old comment2" });
            myAction2.Comment = "test comment2";
            proj.Actions.Add(myAction2);
            taskList.Add(new WLTask() { id = "2", title = "act2" });

            var noteLst2 = new List<WLNote>();
            noteLst2.Add(new WLNote() { id = "22", task_id = "2", content = "old content", revision = 5 });
            mockWlConnector.Setup(x => x.GetNodesForTask("2")).Returns(noteLst2);
            //clear
            MyAction myAction3 = new MyAction(new Action() { Name = "Newact3", WLId = "3", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, Comment = "old comment" });
            myAction3.Comment = null;
            proj.Actions.Add(myAction3);
            var noteLst3 = new List<WLNote>();
            noteLst3.Add(new WLNote() { id = "99", task_id = "3", content = "old content", revision = 5 });
            mockWlConnector.Setup(x => x.GetNodesForTask("3")).Returns(noteLst3);
            taskList.Add(new WLTask() { id = "3", title = "act3" });
            wlProc.UpdateData();
            mockWlConnector.Setup(x => x.GetTask(It.IsAny<string>())).Returns(new WLTask() { revision = 1 });
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.CreateNote("1", "test comment"), Times.Once);
            mockWlConnector.Verify(x => x.UpdateNoteContent("22", 5, "test comment2"), Times.Once);
            mockWlConnector.Verify(x => x.DeleteNote("99", 5), Times.Once);

        }

        [Test]
        public void HandleChangedWLTask_Note() {
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //create
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1" });
            mockWlConnector.Setup(x => x.GetNodesForTask("1")).Returns(new List<WLNote>() { new WLNote() { content = "new content" } });
            //change
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, Comment = "test comment2" });
            proj.Actions.Add(myAction2);
            taskList.Add(new WLTask() { id = "2", title = "act2" });

            var noteLst2 = new List<WLNote>();
            noteLst2.Add(new WLNote() { id = "22", task_id = "2", content = "new content2", revision = 5 });
            mockWlConnector.Setup(x => x.GetNodesForTask("2")).Returns(noteLst2);
            //clear
            MyAction myAction3 = new MyAction(new Action() { Name = "Newact3", WLId = "3", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, Comment = "test comment" });
            proj.Actions.Add(myAction3);
            mockWlConnector.Setup(x => x.GetNodesForTask("3")).Returns(new List<WLNote>());
            taskList.Add(new WLTask() { id = "3", title = "act3" });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual("new content", myAction1.Comment);
            Assert.AreEqual("new content2", myAction2.Comment);
            Assert.AreEqual(null, myAction3.Comment);
        }
        [Test]
        public void HandleChangedLODActions_IsMajor() {
            //arrange
            Initialize(MockBehavior.Default, true);
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //false-true
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, IsMajor = false });
            myAction1.IsMajor = true;
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 11 });
            //true-false 
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, IsMajor = true });
            myAction2.IsMajor = false;
            proj.Actions.Add(myAction2);
            taskList.Add(new WLTask() { id = "2", title = "act2", starred = true, revision = 22 });
            mockWlConnector.Setup(x => x.ChangeStarredOfTask("1", true, 11)).Returns(new WLTask() { revision = 12 });
            mockWlConnector.Setup(x => x.ChangeStarredOfTask("2", false, 22)).Returns(new WLTask() { revision = 23 });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //Assert
            mockWlConnector.Verify(x => x.ChangeStarredOfTask("1", true, 11), Times.Once);
            mockWlConnector.Verify(x => x.ChangeStarredOfTask("2", false, 22), Times.Once);
            Assert.AreEqual(12, myAction1.WLTaskRevision);
            Assert.AreEqual(23, myAction2.WLTaskRevision);
        }
        [Test]
        public void HandleChangedLODActions_ToBuy() {
            //arrange
            Initialize(MockBehavior.Default, true);
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //false-true
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = (int)WLTaskStatusEnum.UpdateNeeded, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            myAction1.ToBuy = true;
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 11, list_id = WLProcessor.MyListId });
            //true-false (shed)
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = (int)WLTaskStatusEnum.UpdateNeeded, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, ToBuy = true, ScheduledTime = new DateTime(2017, 2, 2) });
            myAction2.ToBuy = false;
            proj.Actions.Add(myAction2);
            taskList.Add(new WLTask() { id = "2", title = "act2", revision = 22, list_id = WLProcessor.MyBuyId });
            //true-false (nonshed)
            MyAction myAction3 = new MyAction(new Action() { Name = "Newact3", WLId = "3", WLTaskStatus = (int)WLTaskStatusEnum.UpdateNeeded, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, ToBuy = true });
            myAction3.ToBuy = false;
            proj.Actions.Add(myAction3);
            taskList.Add(new WLTask() { id = "3", title = "act3", revision = 33, list_id = WLProcessor.MyBuyId });


            mockWlConnector.Setup(x => x.ChangeListOfTask("1", WLProcessor.MyBuyId, 11)).Returns(new WLTask() { revision = 44 });
            mockWlConnector.Setup(x => x.ChangeListOfTask("2", WLProcessor.MySchedId, 22)).Returns(new WLTask() { revision = 55 });
            mockWlConnector.Setup(x => x.ChangeListOfTask("3", WLProcessor.MyListId, 33)).Returns(new WLTask() { revision = 66 });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //Assert
            mockWlConnector.Verify(x => x.ChangeListOfTask("1", WLProcessor.MyBuyId, 11), Times.Once);
            mockWlConnector.Verify(x => x.ChangeListOfTask("2", WLProcessor.MySchedId, 22), Times.Once);
            mockWlConnector.Verify(x => x.ChangeListOfTask("3", WLProcessor.MyListId, 33), Times.Once);
            Assert.AreEqual(44, myAction1.WLTaskRevision);
            Assert.AreEqual(55, myAction2.WLTaskRevision);
            Assert.AreEqual(66, myAction3.WLTaskRevision);
        }
        [Test]
        public void HandleChangedLODActions_Sequence_Name_ToBuy() {
            //arrange
            Initialize(MockBehavior.Default, true);
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //false-true
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity });
            myAction1.ToBuy = true;
            myAction1.Name = "newname11";
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 11, list_id = WLProcessor.MyListId });



            mockWlConnector.Setup(x => x.ChangeTitleOfTask("1", "newname11", 11)).Returns(new WLTask() { revision = 44, id = "1" });
            mockWlConnector.Setup(x => x.ChangeListOfTask("1", WLProcessor.MyBuyId, 44)).Returns(new WLTask() { revision = 89, id = "1" });

            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //Assert

            Assert.AreEqual(89, myAction1.WLTaskRevision);

        }
        [Test]
        public void HandleChangedWLTask_IsMajor() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //false-true
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, IsMajor = true });
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1", starred = false });
            //true-false 
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, StatusId = (int)ActionsStatusEnum2.InWork, Project = proj.parentEntity, IsMajor = false });
            proj.Actions.Add(myAction2);
            taskList.Add(new WLTask() { id = "2", title = "act2", starred = true });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedWLTask();
            //Assert
            Assert.AreEqual(false, myAction1.IsMajor);
            Assert.AreEqual(true, myAction2.IsMajor);

        }


        [Test]
        public void HandleChangedLODActions_SchouldUpdateWLRevision() {
            Initialize(configureTaskList: true);

            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            var myAction1 = new MyAction(new Action());
            myAction1.Name = "Newact1";
            myAction1.WLId = "1";
            myAction1.WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
            myAction1.Status2 = ActionsStatusEnum2.InWork;
            myAction1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(myAction1);

            projCollection.Add(proj);
            mockMainVM.Setup(x => x.Projects).Returns(projCollection);

            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 11 });

            mockWlConnector.Setup(x => x.ChangeTitleOfTask("1", "Newact1", 11)).Returns(new WLTask() { revision = 3 });
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //assert
            Assert.AreEqual(3, myAction1.WLTaskRevision);
        }

        [Test]
        public void HandleChangedLODActions_ScheduledTime() {
            //arrange 
            Initialize();

            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            proj.IsSimpleProject = true;
            var act1 = new MyAction(new Action() { Project = proj.parentEntity, Name = "act1", WLId = "1", StatusId = (int)ActionsStatusEnum2.InWork });
            act1.ScheduledTime = new DateTime(2016, 9, 11);

            var act2 = new MyAction(new Action() { Project = proj.parentEntity, Name = "act2", WLId = "2", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 9, 2) });

            proj.Actions.Add(act1);
            proj.Actions.Add(act2);
            var proj2 = new MyProject(new Project()) { Name = "NotSimpleProject", Status = ProjectStatusEnum.InWork };
            proj2.IsSimpleProject = false;

            var act3 = new MyAction(new Action() { Project = proj2.parentEntity, Name = "act3", WLId = "3", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 9, 3) });

            var act4 = new MyAction(new Action() { Project = proj2.parentEntity, Name = "act4", WLId = "4", StatusId = (int)ActionsStatusEnum2.InWork });
            act4.ScheduledTime = new DateTime(2016, 9, 14);
            proj2.Actions.Add(act3);
            proj2.Actions.Add(act4);
            projCollection.Add(proj);
            projCollection.Add(proj2);
            taskList = new List<WLTask>();
            taskList.Add(new WLTask() { id = "1", title = "act1", due_date = "2016-09-01", list_id = WLProcessor.MySchedId, revision = 11 });
            taskList.Add(new WLTask() { id = "2", title = "act2", due_date = "2016-09-02", list_id = WLProcessor.MySchedId });
            taskList.Add(new WLTask() { id = "3", title = "NotSimpleProject - act3", due_date = "2016-09-03", list_id = WLProcessor.MySchedId });
            taskList.Add(new WLTask() { id = "4", title = "NotSimpleProject - act4", due_date = "2016-09-04", list_id = WLProcessor.MySchedId, revision = 44 });
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.ChangeScheduledTime("1", "2016-09-11", 11)).Returns(new WLTask());
            mockWlConnector.Setup(x => x.ChangeScheduledTime("4", "2016-09-14", 44)).Returns(new WLTask());
            wlProc.UpdateData();
            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeScheduledTime("1", "2016-09-11", 11), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("4", "2016-09-14", 44), Times.Once);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj2.Actions[1].WLTaskStatus);
        }

        [Test]
        public void HandleChangedWLTask_Title() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            var act2 = new MyAction(new Action());
            act2.Name = "TestName2";
            act2.WLTaskRevision = 1;
            act2.WLId = "234";
            act2.Status2 = ActionsStatusEnum2.InWork;
            act2.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act2);
            projCollection.Add(proj);

            taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });
            taskList.Add(new WLTask() { id = "234", title = "TestName2", revision = 1 });

            wlProc.UpdateData();
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
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "Project1 - NewTestName1", revision = 2 });

            wlProc.UpdateData();
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual("NewTestName1", act1.Name);
            Assert.AreEqual(2, act1.WLTaskRevision);

        }

        [Test]
        public void HandleChangedWLTask_Title_notSimple_notValidName() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });

            wlProc.UpdateData();
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
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);

            wlProc.UpdateData();
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;

            //act
            try {
                wlProc.HandleChangedWLTask();
            }
            catch (Exception ex) {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
            //assert
            var b = logList[1].Contains("There are no tasks");
            Assert.AreEqual(true, b);
            //Assert.AreEqual(2, act1.WLTaskRevision);
        }
        [Test]
        public void HandleCompletedWLTask_Message() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action() { Id = 44 });
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            mockWlConnector.Setup(x => x.GetTask(It.IsAny<string>())).Returns(new WLTask() { completed_at = "2000-02-03T17:53:04.953Z" });
            wlProc.UpdateData();
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;

            //act

            wlProc.HandleCompletedWLTasks();
            //assert
            var st = string.Format("complete action - {0} {1}", "Project1 - TestName1", 44);

            Assert.AreEqual(true, logList[2].Contains("completed"));
        }


        [Test]
        public void HandleChangedWLTask_SchouldNotChangeActionsWLStatus() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "TestName1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);


            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "NewTestName1", revision = 2 });

            wlProc.UpdateData();
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
        }
        [Test]
        public void HandleChangedWLTask_ScheduledTime() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.ScheduledTime = new DateTime(2016, 9, 11);
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2, due_date = "2016-09-15" });

            wlProc.UpdateData();
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            //act
            wlProc.HandleChangedWLTask();

            //assert
            Assert.AreEqual(new DateTime(2016, 9, 15), act1.ScheduledTime);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
            Assert.AreEqual(4, logList.Count);
            Assert.AreEqual(true, logList[1].Contains("has old revision"));
            Assert.AreEqual(true, logList[2].Contains("changed time"));
        }

        [Test]
        public void HandleChangedWLTask_ScheduledTimeFromNullToNonNull() {
            //arrange
            Initialize();
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;

            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2, due_date = "2016-09-12" });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.UpdateData();
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(ActionsStatusEnum2.InWork, act1.Status2);
            Assert.AreEqual(new DateTime(2016, 9, 12), act1.ScheduledTime);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
            Assert.AreEqual(4, logList.Count);
            Assert.AreEqual(true, logList[2].Contains("2016-09-12"));
        }

        [Test]
        public void HandleChangedWLTask_ScheduledTimeFromNotNullToNull() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.Status2 = ActionsStatusEnum2.InWork;
            act1.ScheduledTime = new DateTime(2016, 9, 12);
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2, due_date = null });

            wlProc.UpdateData();
            logList = new List<string>();
            wlProc.Logged += Proc_Logged;
            //act
            wlProc.HandleChangedWLTask();
            //assert
            Assert.AreEqual(ActionsStatusEnum2.InWork, act1.Status2);
            Assert.AreEqual(null, act1.ScheduledTime);
            Assert.AreEqual(2, act1.WLTaskRevision);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act1.WLTaskStatus);
            Assert.AreEqual(4, logList.Count);
            Assert.AreEqual(true, logList[2].Contains("delete time"));
        }


        [Test]
        public void HandleChangedLodAction_StatusFromNonSchedToSched() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            proj.Name = "Project1";
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action() { Name = "act1", WLTaskRevision = 1, WLId = "123", StatusId = (int)ActionsStatusEnum2.InWork });

            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2 });

            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask("123", WLProcessor.MySchedId, 2)).Returns(new WLTask() { revision = 6, id = "123" });
            mockWlConnector.Setup(x => x.ChangeScheduledTime("123", "2016-09-11", 6)).Returns(new WLTask() { revision = 7, id = "123" });
            //act
            act1.ScheduledTime = new DateTime(2016, 9, 11);
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeListOfTask("123", WLProcessor.MySchedId, 2), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("123", "2016-09-11", 6), Times.Once);
            Assert.AreEqual(7, act1.WLTaskRevision);
        }

        [Test]
        public void HandleChangedLodAction_StatusFromSchedToNonSched() {
            //arrange
            Initialize(MockBehavior.Default);

            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.InWork;
            var act1 = new MyAction(new Action() { Name = "act1", WLTaskRevision = 1, WLId = "123", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 9, 11) });


            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "act1", revision = 2, due_date = "2016-09-11", list_id = WLProcessor.MySchedId });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask("123", WLProcessor.MyListId, 2)).Returns(new WLTask() { revision = 9, id = "123" });
            mockWlConnector.Setup(x => x.ChangeScheduledTime("123", "null", 9)).Returns(new WLTask() { revision = 10, id = "123" });

            //act
            act1.ScheduledTime = null;
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeListOfTask("123", WLProcessor.MyListId, 2), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("123", "null", 9), Times.Once);
            Assert.AreEqual(10, act1.WLTaskRevision);
        }

        [Test]
        public void CreateLogString() {
            //arrange
            var wlProc = new WLProcessor(null);
            string longTxt = "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglolonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglo";
            //act
            var res = wlProc.CreateLogString(longTxt, "test");
            var m = res.Split('|');


            //assert
            Assert.AreEqual(97, m[2].Length);
        }
        [Test]
        public void CreateLogStringwithCreateNewTaskText() {
            //arrange
            var wlProc = new WLProcessor(null);
           
            //act
            var res1 = wlProc.CreateLogString("subj1", "test","short value");
            var res2 = wlProc.CreateLogString("subj2", "test", "list id - MyList(263984253), new task's id=2695932312");
            
            var split1 = res1.Split(new char[] { '|' },StringSplitOptions.RemoveEmptyEntries);
            var split2 = res2.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);


            //assert
            Assert.AreEqual(split1[3].Length, split2[3].Length);
        }
        [Test]
        public void CopyDiaryToClipboard() {
            //arrange
            Initialize();
            List<WLTask> taskList = new List<WLTask>();
            taskList.Add(new WLTask() { title = "diar1" });
            taskList.Add(new WLTask() { title = "diar2" });
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyDiarId)).Returns(taskList);
            string res = null;
            wlProc.SetClipboardText = new Action<string>(x => res = x);
            //act
            wlProc.PasteDiaryEntries();
            //assert
            var sp = res.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, sp.Length);
            Assert.AreEqual("diar1", sp[0]);
        }
        [Test]
        public void HandleChangedLodAction_SheduledTimeOfActionInDelayedProject_create() {
            //arrange
            Initialize(MockBehavior.Default);

            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.Delayed;

            var act2 = new MyAction(new Action() { Name = "act2", WLTaskRevision = 1, StatusId = (int)ActionsStatusEnum2.InWork });
            act2.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act2);

            projCollection.Add(proj);


            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new WLTask());
            mockWlConnector.Setup(x => x.ChangeScheduledTime(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(new WLTask());

            //act
            act2.ScheduledTime = new DateTime(2017, 5, 5);
            wlProc.UpdateData();
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("act2", WLProcessor.MySchedId, It.IsAny<DateTime>(), false), Times.Once);
        }
        [Test]
        public void HandleChangedLodAction_SheduledTimeOfActionInDelayedProject_delete() {
            //arrange
            Initialize(MockBehavior.Default);

            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.Delayed;
            var act1 = new MyAction(new Action() { Name = "act1", WLTaskRevision = 1, WLId = "123", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 9, 11) });
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);
          

            projCollection.Add(proj);

            taskList.Add(new WLTask() { id = "123", title = "act1", revision = 2, due_date = "2016-09-11", list_id = WLProcessor.MySchedId });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new WLTask());
            mockWlConnector.Setup(x => x.ChangeScheduledTime(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(new WLTask());

            //act
            act1.ScheduledTime = null;
            wlProc.HandleCompletedLODActions();
            //assert
            mockWlConnector.Verify(x => x.CompleteTask("123"), Times.Once);
        }

        [Test]
        public void HandleChangedLodAction_SheduledTimeOfActionInDelayedProject_change() {
            //arrange
            Initialize(MockBehavior.Default);

            var proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            proj.Status = ProjectStatusEnum.Delayed;
            var act1 = new MyAction(new Action() { Name = "act1", WLTaskRevision = 1, WLId = "123", StatusId = (int)ActionsStatusEnum2.InWork, ScheduledTime = new DateTime(2016, 9, 11) });
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);


            projCollection.Add(proj);

            taskList.Add(new WLTask() { id = "123", title = "act1", revision = 2, due_date = "2016-09-11", list_id = WLProcessor.MySchedId });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new WLTask());
            mockWlConnector.Setup(x => x.ChangeScheduledTime(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(new WLTask());

            //act
            act1.ScheduledTime = new DateTime(1999,9,9);
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeScheduledTime("123","1999-09-09",2), Times.Once);
        }

    }
#endif
}
