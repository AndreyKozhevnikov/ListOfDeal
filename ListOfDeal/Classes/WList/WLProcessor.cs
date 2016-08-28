using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
        public event WLEventHandler Logged;
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
            RaiseLog("Start creating tasks");
            MainViewModel.SaveChanges();
            allActions = GetActiveActions();
            var emptyActions = allActions.Where(x => x.WLId == null);
            var v = emptyActions.Count();
            RaiseLog(string.Format("new task count={0}", v));
            if (v == 0)
                return;
            //   emptyActions = emptyActions.Take(1);
            foreach (var act in emptyActions) {
                string title = act.Name;
                var wlTask = wlConnector.CreateTask(title, MyListId);
                act.WLId = wlTask.id;
                string message = string.Format("title={0},new task's id={1}", wlTask.title, wlTask.id);
            }
            MainViewModel.SaveChanges();
        }

        public void HandleCompletedWLTasks() {
            RaiseLog("Start handle completed  WLtasks");
            MainViewModel.SaveChanges();
            allTasks = GetAllActiveTasks();
            allActions = GetActiveActions();
            var lstwlIdinLod = allActions.Select(x => (int)x.WLId);
            var lstwlIdInWL = allTasks.Select(x => x.id);
            var diff = lstwlIdinLod.Except(lstwlIdInWL);
            RaiseLog(string.Format("wlId in LOD - {0}, wlId in WL-{1}",lstwlIdinLod.Count(),lstwlIdInWL.Count()));
            foreach (int tskId in diff) {
                Debug.Print(tskId.ToString());
                var act = allActions.Where(x => x.WLId == tskId).First();
                act.Status = ActionsStatusEnum.Completed;
                RaiseLog(string.Format("complete action - {0} {1}",act.Name,act.parentEntity.Id));
            }
            MainViewModel.SaveChanges();

        }
        List<WLTask> GetAllActiveTasks() {
            return wlConnector.GetTasksForList(MyListId);
        }
        List<MyAction> GetActiveActions() {
            var lst = parentVM.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.IsActive).ToList();
            return lst;
        }


        public void HandleCompletedLODActions() {
            RaiseLog("Start handle completed  LODActions");
            MainViewModel.SaveChanges();
            var lst = MainViewModel.generalEntity.Actions.Where(x => x.WLTaskStatus == 2).ToList();
            RaiseLog(string.Format("amount actions - {0}", lst.Count));
            foreach (var act in lst) {
                var wlId = (int)act.WLId;
                wlConnector.CompleteTask(wlId); 
                act.WLId = null;
                act.WLTaskStatus = 0;
                RaiseLog(string.Format("complete task of actions {0} {1}", act.Name, act.Id));
            }
            MainViewModel.SaveChanges();
        }

        void RaiseLog(string st) {
            if (Logged != null)
                Logged(new WLEventArgs( st));
            
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
            proj.Actions.Add(new MyAction(new Action() { Name = "act1", IsActive = true }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act2", WLId = 123, IsActive = true }));
            proj.Actions.Add(new MyAction(new Action() { Name = "act3", IsActive = true }));

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
        public void HandleCompletedActions() {
            //arrange
            WLProcessor wlProc = new WLProcessor(null);
            var mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            MainViewModel.generalEntity = mockGeneralEntity.Object;
            var lstMock = new Mock<IDbSet<Action>>();
            var lstAct = new List<Action>();
            lstAct.Add(new Action() { WLId = 123, WLTaskStatus = 1 ,IsActive=true});
            lstAct.Add(new Action() { WLId = 234, WLTaskStatus = 2 ,IsActive=true});

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
