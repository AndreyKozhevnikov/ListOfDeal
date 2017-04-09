using NUnit.Framework;
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
            Assert.AreEqual(ActionsStatusEnum2.InWork, act1.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Delay, act2.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Delay, act2.Status2);
        }


        [Test]
        public void ShouldNotMakeCompletedActionActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.Delay };
            var act2 = new MyAction(new Action()) { Name = "act2", Status2 = ActionsStatusEnum2.Delay };
            var act3 = new MyAction(new Action()) { Name = "act3", Status2 = ActionsStatusEnum2.Done };
            var act4 = new MyAction(new Action()) { Name = "act4", Status2 = ActionsStatusEnum2.Delay };
            proj.AddAction(act1);
            proj.AddAction(act2);
            proj.AddAction(act3);
            proj.AddAction(act4);
            //act2
            act2.Status2 = ActionsStatusEnum2.Done;
            act1.Status2 = ActionsStatusEnum2.Rejected;
            //assert2
            Assert.AreEqual(ActionsStatusEnum2.Rejected, act1.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Done, act2.Status2);
            Assert.AreEqual(ActionsStatusEnum2.Done, act3.Status2);
            Assert.AreEqual(ActionsStatusEnum2.InWork, act4.Status2);
        }
        [Test]
        public void AddActionMakeActionActiveIfThereAreNoOtherActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.Delay };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(ActionsStatusEnum2.InWork, act2.Status2);

        }
        [Test]
        public void AddActionNotMakeActionActiveIfThereAreActive() {
            //arrange
            var proj = new MyProject(new Project());
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.InWork };
            proj.Actions.Add(act1);
            //act
            var act2 = new MyAction(new Action()) { Name = "act2" };
            proj.AddAction(act2);
            //assert
            Assert.AreEqual(ActionsStatusEnum2.Delay, act2.Status2);
        }
        [Test]
        public void CompleteActionInSimpleProject_completeProject_done() {
            //arrange
            var proj1 = new MyProject(new Project() { IsSimpleProject = true });
            var act1 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.InWork };
            proj1.AddAction(act1);
            //act
            act1.Status2 = ActionsStatusEnum2.Done;
            //assert
            Assert.AreEqual(ProjectStatusEnum.Done, proj1.Status);
        }
        [Test]
        public void CompleteActionInSimpleProject_completeProject_rejected() {
            //arrange
            var proj2 = new MyProject(new Project() { IsSimpleProject = true });
            var act2 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.InWork };
            proj2.AddAction(act2);
            //act
            act2.Status2 = ActionsStatusEnum2.Rejected;
            //assert
            Assert.AreEqual(ProjectStatusEnum.Rejected, proj2.Status);
        }
        [Test]
        public void DelayActionInSimpleProject_doNOTcompleteProject() {
            //arrange
            var proj2 = new MyProject(new Project() { IsSimpleProject = true, StatusId = (int)ProjectStatusEnum.InWork });
            var act2 = new MyAction(new Action()) { Name = "act1", Status2 = ActionsStatusEnum2.InWork };
            proj2.AddAction(act2);
            //act
            act2.Status2 = ActionsStatusEnum2.Delay;
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
            act.Status2 = ActionsStatusEnum2.Delay;
            act.ScheduledTime = DateTime.Now;
            //assert
            Assert.GreaterOrEqual(lst.Where(x => x == "Actions").Count(), 2);


        }
    }
#endif
}
