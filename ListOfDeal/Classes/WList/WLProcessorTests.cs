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

namespace ListOfDeal {
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
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = "123", IsActive = true, Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act3", IsActive = true, Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act4comment", IsActive = true, Comment = "test comment", Project = new Project() { Name = "Pr1" } }));

            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "234" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "345" });
            mockWlConnector.Setup(x => x.CreateTask("Pr1 - act4comment", It.IsAny<int>(), null, false)).Returns(new WLTask() { id = "456" });
            mockWlConnector.Setup(x => x.CreateNote("456", "test comment")).Returns(new WLNote());

            wlProc.UpdateData();
            //act
            wlProc.CreateWlTasks();
            //assert
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act1", It.IsAny<int>(), null, false), Times.Once);
            mockWlConnector.Verify(x => x.CreateTask("Pr1 - act3", It.IsAny<int>(), null, false), Times.Once);

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
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, StatusId = 2, ScheduledTime = new DateTime(2016, 8, 28), Project = new Project() { Name = "Pr1" } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act2", IsActive = true, Project = new Project() { Name = "Pr1" } }));
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
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, StatusId = 2, ScheduledTime = new DateTime(2016, 8, 28), Project = new Project() { Name = "Pr1" } }));

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
            a.IsActive = true;
            a.Name = "act1";
            a.ToBuy = true;
            firstProject.Actions.Add(new MyAction(a));

            var a2 = new Action();
            a2.Name = "act2";
            a2.IsActive = true;
            a2.Project = parentProject;
            firstProject.Actions.Add(new MyAction(a2));

            var a3 = new Action();
            a3.Name = "act3";
            a3.IsActive = true;
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
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true, Project = new Project() { Name = "MySimpleProject", IsSimpleProject = true } }));
            firstProject.Actions.Add(new MyAction(new Action() { Name = "act2", IsActive = true, Project = new Project() { Name = "MyNonSimpleProject", IsSimpleProject = false } }));

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
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true, Project = new Project() });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", IsActive = true, Project = new Project() });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", IsActive = true, Project = new Project() });

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
            Assert.AreEqual(ActionsStatusEnum.Completed, myAction2.Status);
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
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true, Project = new Project() });
            firstProject.Actions.Add(myAction1);
            mockWlConnector.Setup(x => x.GetTask(It.IsAny<string>())).Returns(new WLTask() { completed_at = "2000-02-03T17:53:04.953Z" });
            wlProc.UpdateData();
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
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            for (int i = 0; i < 7; i++) {
                var a = new MyAction(new Action() { Name = "Action" + i, WLId = i.ToString(), IsActive = true, Project = new Project() }) { Status = ActionsStatusEnum.Waited };
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
            Assert.AreEqual(ActionsStatusEnum.Completed, firstProject.Actions[1].Status);
            Assert.AreEqual(ActionsStatusEnum.Completed, firstProject.Actions[4].Status);
            Assert.AreEqual(ActionsStatusEnum.Completed, firstProject.Actions[6].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, firstProject.Actions[0].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, firstProject.Actions[2].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, firstProject.Actions[3].Status);
            Assert.AreEqual(ActionsStatusEnum.Waited, firstProject.Actions[5].Status);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }

        [Test]
        public void HandleCompletedWLTasks_ShouldSkipActionsWithoutWLId() {
            //arrange
            Initialize();
            var firstProject = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(firstProject);
            var myAction1 = new MyAction(new Action() { Name = "Action1", WLId = "1", IsActive = true, Project = new Project() });
            var myAction2 = new MyAction(new Action() { Name = "Action2", WLId = "2", IsActive = true, Project = new Project() });
            var myAction3 = new MyAction(new Action() { Name = "Action3", WLId = "3", IsActive = true, Project = new Project() });
            var myAction4 = new MyAction(new Action() { Name = "Action3", IsActive = true });

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
            Assert.AreEqual(ActionsStatusEnum.Completed, myAction2.Status);
            dataProviderEntity.Verify(x => x.SaveChanges(), Times.Exactly(2));

        }


        [Test]
        public void HandleCompletedActions() {
            //arrange
            Initialize();
            var p = new Project();
            var firstProject = new MyProject(p);
            projCollection.Add(firstProject);
            var act1 = new MyAction(new Action() { WLId = "123", WLTaskStatus = 1, IsActive = true, Project = p });
            var act2 = new MyAction(new Action { WLId = "234", WLTaskStatus = 2, IsActive = false, Project = p });
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
            var a1 = new MyAction(new Action() { WLId = "123", WLTaskStatus = 1, IsActive = true, Project = p });
            var a2 = new MyAction(new Action() { WLId = "234", WLTaskStatus = 2, IsActive = true, Project = p });
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
            proj.Actions.Add(new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = "2", IsActive = true, Project = proj.parentEntity }));

            var proj2 = new MyProject(new Project()) { Name = "NotSimpleProject", Status = ProjectStatusEnum.InWork };
            proj2.IsSimpleProject = false;
            proj2.Actions.Add(new MyAction(new Action() { Name = "act3", WLId = "3", IsActive = true, Project = proj2.parentEntity }));
            proj2.Actions.Add(new MyAction(new Action() { Name = "Newact4", WLId = "4", WLTaskStatus = 1, IsActive = true, Project = proj2.parentEntity }));
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
        public void HandleChangedLODActions_Note() {
            //arrange
            Initialize(MockBehavior.Default, true);


            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //create
            proj.Actions.Add(new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, Comment = "test comment" }));
            taskList.Add(new WLTask() { id = "1", title = "act1" });
            mockWlConnector.Setup(x => x.GetNodesForTask("1")).Returns(new List<WLNote>());
            //change
            proj.Actions.Add(new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, Comment = "test comment2" }));
            taskList.Add(new WLTask() { id = "2", title = "act2" });

            var noteLst2 = new List<WLNote>();
            noteLst2.Add(new WLNote() { id = "22", task_id = "2", content = "old content", revision = 5 });
            mockWlConnector.Setup(x => x.GetNodesForTask("2")).Returns(noteLst2);
            //clear
            proj.Actions.Add(new MyAction(new Action() { Name = "Newact3", WLId = "3", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity }));
            var noteLst3 = new List<WLNote>();
            noteLst3.Add(new WLNote() { id = "99", task_id = "3", content = "old content", revision = 5 });
            mockWlConnector.Setup(x => x.GetNodesForTask("3")).Returns(noteLst3);
            taskList.Add(new WLTask() { id = "3", title = "act3" });
            wlProc.UpdateData();
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
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity });
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1" });
            mockWlConnector.Setup(x => x.GetNodesForTask("1")).Returns(new List<WLNote>() { new WLNote() { content = "new content" } });
            //change
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, Comment = "test comment2" });
            proj.Actions.Add(myAction2);
            taskList.Add(new WLTask() { id = "2", title = "act2" });

            var noteLst2 = new List<WLNote>();
            noteLst2.Add(new WLNote() { id = "22", task_id = "2", content = "new content2", revision = 5 });
            mockWlConnector.Setup(x => x.GetNodesForTask("2")).Returns(noteLst2);
            //clear
            MyAction myAction3 = new MyAction(new Action() { Name = "Newact3", WLId = "3", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, Comment = "test comment" });
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
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, IsMajor = true });
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1", revision = 11 });
            //true-false 
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, IsMajor = false });
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
        public void HandleChangedWLTask_IsMajor() {
            //arrange
            Initialize(configureTaskList: true);
            var proj = new MyProject(new Project()) { Status = ProjectStatusEnum.InWork };
            projCollection.Add(proj);
            proj.IsSimpleProject = true;
            //false-true
            MyAction myAction1 = new MyAction(new Action() { Name = "Newact1", WLId = "1", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, IsMajor = true });
            proj.Actions.Add(myAction1);
            taskList.Add(new WLTask() { id = "1", title = "act1", starred = false });
            //true-false 
            MyAction myAction2 = new MyAction(new Action() { Name = "Newact2", WLId = "2", WLTaskStatus = 1, IsActive = true, Project = proj.parentEntity, IsMajor = false });
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
            myAction1.IsActive = true;
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
            act1.IsActive = true;
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
            act1.IsActive = true;
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
            act1.IsActive = true;
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
            act1.IsActive = true;
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

            Assert.AreEqual(true,logList[2].Contains("completed"));
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
            act1.IsActive = true;
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
            act1.IsActive = true;
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
            act1.IsActive = true;
            act1.Status = ActionsStatusEnum.Waited;
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
            Assert.AreEqual(ActionsStatusEnum.Scheduled, act1.Status);
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
            act1.IsActive = true;
            act1.Status = ActionsStatusEnum.Scheduled;
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
            Assert.AreEqual(ActionsStatusEnum.Waited, act1.Status);
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
            var act1 = new MyAction(new Action());
            act1.Name = "act1";
            act1.WLTaskRevision = 1;
            act1.WLId = "123";
            act1.IsActive = true;
            act1.ScheduledTime = new DateTime(2016, 9, 11);
            act1.parentEntity.Project = proj.parentEntity;
            proj.Actions.Add(act1);

            projCollection.Add(proj);
            taskList.Add(new WLTask() { id = "123", title = "Project1 - act1", revision = 2 });

            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask("123", WLProcessor.MySchedId, 2)).Returns(new WLTask() { revision = 6 });
            mockWlConnector.Setup(x => x.ChangeScheduledTime("123", "2016-09-11", 6)).Returns(new WLTask() { revision = 7 });
            //act
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
            taskList.Add(new WLTask() { id = "123", title = "act1", revision = 2, due_date = "2016-09-11", list_id = WLProcessor.MySchedId });

            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyListId)).Returns(new List<WLTask>());
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MySchedId)).Returns(taskList);
            mockWlConnector.Setup(x => x.GetTasksForList(WLProcessor.MyBuyId)).Returns(new List<WLTask>());
            wlProc.UpdateData();

            mockWlConnector.Setup(x => x.ChangeListOfTask("123", WLProcessor.MyListId, 2)).Returns(new WLTask() { revision = 9 });
            mockWlConnector.Setup(x => x.ChangeScheduledTime("123", "null", 9)).Returns(new WLTask() { revision = 10 });

            //act
            wlProc.HandleChangedLODActions();
            //assert
            mockWlConnector.Verify(x => x.ChangeListOfTask("123", WLProcessor.MyListId, 2), Times.Once);
            mockWlConnector.Verify(x => x.ChangeScheduledTime("123", "null", 9), Times.Once);
            Assert.AreEqual(10, act1.WLTaskRevision);
        }
    }

}
