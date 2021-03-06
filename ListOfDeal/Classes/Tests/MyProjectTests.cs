﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.Tests {
#if DebugTest
    [TestFixture]
    public class MyProjectTests {
        [Test]
        public void SetStatusDone() {
            //arrange
            MyProject pr = new MyProject(new Project());
            //act
            pr.Status = ProjectStatusEnum.Done;
            //assert
            Assert.AreNotEqual(null, pr.parentEntity.CompleteTime);
        }
        [Test]
        public void SetStatusRejected() {
            //arrange
            MyProject pr = new MyProject(new Project());
            //act
            pr.Status = ProjectStatusEnum.Rejected;
            //assert
            Assert.AreNotEqual(null, pr.parentEntity.CompleteTime);
        }

        [Test]
        public void SetProjectIsNotInWork() {
            //arrange
            MyProject pr = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            myAction.WLId = "123";
            var myAction1 = new MyAction(new Action());
            pr.Actions.Add(myAction);
            pr.Actions.Add(myAction1);
            //act
            pr.Status = ProjectStatusEnum.Delayed;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, myAction.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);
        }
        [Test]
        public void SetProjectIsNotInWork_NullActionsScheduledTime() {
            //arrange
            MyProject pr = new MyProject(new Project());
            var myAction = new MyAction(new Action()) { ScheduledTime = DateTime.Today };
            myAction.WLId = "123";
            var myAction1 = new MyAction(new Action());
            pr.Actions.Add(myAction);
            pr.Actions.Add(myAction1);
            //act
            pr.Status = ProjectStatusEnum.Delayed;
            //assert
            Assert.AreEqual(DateTime.Today, myAction.ScheduledTime);
            


        }
        [Test]
        public void SetProjectIsNotInWork_Done() {
            //arrange
            MyProject pr = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            myAction.WLId = "123";
            var myAction1 = new MyAction(new Action());
            pr.Actions.Add(myAction);
            pr.Actions.Add(myAction1);
            //act
            pr.Status = ProjectStatusEnum.Done;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, myAction.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);

            Assert.AreNotEqual(null, pr.parentEntity.CompleteTime);
        }
        [Test]
        public void SetProjectIsNotInWork_Rejected() {
            //arrange
            MyProject pr = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            myAction.WLId = "123";
            var myAction1 = new MyAction(new Action());
            pr.Actions.Add(myAction);
            pr.Actions.Add(myAction1);
            //act
            pr.Status = ProjectStatusEnum.Rejected;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, myAction.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, myAction1.WLTaskStatus);

            Assert.AreNotEqual(null, pr.parentEntity.CompleteTime);
        }
        [Test]
        public void AddActionToSimpleProject_ShouldNotSetWLStatusToNeedUpdate() {
            //arrange
            var proj = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            proj.Actions.Add(myAction);
            proj.IsSimpleProject = true;
            //act
            var act2 = new MyAction(new Action());
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(1, proj.Actions.Count);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
        }

        [Test]
        public void AddActionToSimpleProject_DontreplaceIfHasComment() {
            //arrange
            var proj = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            myAction.Comment = "test commetn";
            proj.Actions.Add(myAction);
            proj.IsSimpleProject = true;
            //act
            var act2 = new MyAction(new Action());
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(2, proj.Actions.Count);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, proj.Actions[0].WLTaskStatus);
        }
        [Test]
        public void AddActionToSimpleProject_MakeFirstActionActive() {
            //arrange
            var proj = new MyProject(new Project());
            var myAction = new MyAction(new Action());
            proj.Actions.Add(myAction);
            proj.IsSimpleProject = true;
            //act
            var act2 = new MyAction(new Action());
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(1, proj.Actions.Count);
            Assert.AreEqual(ActionsStatusEnum.InWork, proj.Actions[0].Status);
        }
        [Test]
        public void MakeActionsActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action());
            var act2 = new MyAction(new Action());
            var act3 = new MyAction(new Action());
            //act1
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            //assert1
            Assert.AreEqual(ActionsStatusEnum.InWork, act1.Status);
            Assert.AreEqual(ActionsStatusEnum.Delay, act2.Status);
            Assert.AreEqual(ActionsStatusEnum.Delay, act2.Status);
        }
        [Test]
        public void MakeActionsActiveAfterPreviousDone() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Status = ActionsStatusEnum.InWork };
            var act2 = new MyAction(new Action());
            var act3 = new MyAction(new Action());
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            //act1
            act1.Status = ActionsStatusEnum.Delay;
       
            //assert1
            Assert.AreEqual(ActionsStatusEnum.InWork, act2.Status);
        }

        [Test]
        public void ShouldNotMakeCompletedActionActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status = ActionsStatusEnum.Delay };
            var act2 = new MyAction(new Action()) { Name = "act2", Status = ActionsStatusEnum.Delay };
            var act3 = new MyAction(new Action()) { Name = "act3", Status = ActionsStatusEnum.Done };
            var act4 = new MyAction(new Action()) { Name = "act4", Status = ActionsStatusEnum.Delay };
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            proj.AddAction(act4);
            //act2
            act2.Status = ActionsStatusEnum.Done;
            act1.Status = ActionsStatusEnum.Rejected;
            //assert2
            Assert.AreEqual(ActionsStatusEnum.Rejected, act1.Status);
            Assert.AreEqual(ActionsStatusEnum.Done, act2.Status);
            Assert.AreEqual(ActionsStatusEnum.Done, act3.Status);
            Assert.AreEqual(ActionsStatusEnum.InWork, act4.Status);
        }
        [Test]
        public void AddActionMakeActionActiveIfThereAreNoOtherActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status = ActionsStatusEnum.Delay };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(ActionsStatusEnum.InWork, act2.Status);

        }
        [Test]
        public void AddActionNotMakeActionActiveIfThereAreActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status = ActionsStatusEnum.InWork };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(ActionsStatusEnum.Delay, act2.Status);
        }
        [Test]
        public void CompleteActionInSimpleProject_completeProject_done() {
            //arrange
            var proj1 = new MyProject(new Project() { IsSimpleProject = true });
            var act1 = new MyAction(new Action()) { Name = "act1", Status = ActionsStatusEnum.InWork };
            proj1.AddAction(act1);
            //act
            act1.Status = ActionsStatusEnum.Done;
            //assert
            Assert.AreEqual(ProjectStatusEnum.Done, proj1.Status);
        }
        [Test]
        public void CompleteActionInSimpleProject_completeProject_rejected() {
            //arrange
            var proj2 = new MyProject(new Project() { IsSimpleProject = true });
            var act2 = new MyAction(new Action()) { Name = "act1", Status = ActionsStatusEnum.InWork };
            proj2.AddAction(act2);
            //act
            act2.Status = ActionsStatusEnum.Rejected;
            //assert
            Assert.AreEqual(ProjectStatusEnum.Rejected, proj2.Status);
        }
        [Test]
        public void DelayActionInSimpleProject_doNOTcompleteProject() {
            //arrange
            var proj2 = new MyProject(new Project() { IsSimpleProject = true, StatusId = (int)ProjectStatusEnum.InWork });
            var act2 = new MyAction(new Action()) { Name = "act1", Status = ActionsStatusEnum.InWork };
            proj2.AddAction(act2);
            //act
            act2.Status = ActionsStatusEnum.Delay;
            //assert
            Assert.AreEqual(ProjectStatusEnum.InWork, proj2.Status);
        }
        [Test]
        public void ActionRaiseScheduled_raiseProjectActions() {
            //arrange
            var lst = new List<string>();
            var proj = new MyProject(new Project());
            var act = new MyAction(new Action());
            proj.AddAction(act);
            proj.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { lst.Add(e.PropertyName); };
            //act
            act.Status = ActionsStatusEnum.Delay;
            act.ScheduledTime = DateTime.Now;
            //assert
            Assert.GreaterOrEqual(lst.Where(x => x == "Actions").Count(), 2);
        }

        [Test]
        public void CompletedRejectedTaskAlwaysOnTheTop() {
            //arrange
            var mp = new MyProject(new Project());
            var a1 = new MyAction(new Action());
            var a2 = new MyAction(new Action());
            mp.AddAction(a1);
            mp.AddAction(a2);
            //act
            a1.Status = ActionsStatusEnum.Rejected;
            a2.Status = ActionsStatusEnum.Done;
            //assert
            Assert.AreEqual(-1, a1.OrderNumber);
            Assert.AreEqual(-1, a2.OrderNumber);


        }

        [Test]
        public void SetTimeInAction_makeProjectActive() {
            //arrange
            var p = new MyProject(new Project());
            var a = new MyAction(new Action());
            p.AddAction(a);
            p.Status = ProjectStatusEnum.Delayed;
            //act
            a.ScheduledTime = DateTime.Now;
            //assert
            Assert.AreEqual(p.Status, ProjectStatusEnum.InWork);
        }
    }
#endif
}
