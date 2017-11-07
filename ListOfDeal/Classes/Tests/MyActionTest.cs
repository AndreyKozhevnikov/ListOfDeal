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
    public class MyActionTest {
        [Test]
        public void CompleteAction() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.WLId = "123";
            act.Status = ActionsStatusEnum.InWork;
            MyAction act2 = new MyAction(new Action());
            act2.WLId = "123";
            act2.Status = ActionsStatusEnum.InWork;
            //act
            act.Status = ActionsStatusEnum.Done;
            act2.Status = ActionsStatusEnum.Done;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act.WLTaskStatus);
            Assert.AreNotEqual(null, act.parentEntity.CompleteTime);

            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act2.WLTaskStatus);
            Assert.AreNotEqual(null, act2.parentEntity.CompleteTime);
        }

        [Test]
        public void CompleteAction_withoutWLId() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status = ActionsStatusEnum.InWork;
            MyAction act2 = new MyAction(new Action());
            act2.Status = ActionsStatusEnum.InWork;
            //act
            act.Status = ActionsStatusEnum.Done;
            act.Status = ActionsStatusEnum.Rejected;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act2.WLTaskStatus);

        }


        [Test]
        public void SetWLStatusWhenNameIsChanged() {
            //arrange
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "1";
            //act
            act.Name = "NewName1";
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenNameIsChanged_andThereIsNoWlId() {
            //arrange
            MyAction act = new MyAction(new Action() { Name = "Name1" });

            //act
            act.Name = "NewName1";
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenScheduledDateIsChanged() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.WLId = "1";
            act.Status = ActionsStatusEnum.InWork;
            act.ScheduledTime = new DateTime(2016, 1, 1);
            //act
            act.ScheduledTime = new DateTime(2016, 1, 2);
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenScheduledDateIsChanged_andThereIsNoWlTask() {
            //arrange
            MyAction act = new MyAction(new Action());

            act.Status = ActionsStatusEnum.InWork;
            act.ScheduledTime = new DateTime(2016, 1, 1);
            //act
            act.ScheduledTime = new DateTime(2016, 1, 2);
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpToDateWLTask, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenIsSimpePropertyIsChanged() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "2";
            proj.Actions.Add(act);
            //act
            proj.IsSimpleProject = false;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void PreventSettingIsSimpleTrueWhenThereAreManyAcitons() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = false;
            MyAction act1 = new MyAction(new Action());
            proj.Actions.Add(act1);
            MyAction act2 = new MyAction(new Action());
            proj.Actions.Add(act2);
            //act
            proj.IsSimpleProject = true;
            //assert
            Assert.AreEqual(false, proj.IsSimpleProject);
        }
        [Test]
        public void SetWLStatusWhenCommentPropertyIsChanged() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "2";
            proj.Actions.Add(act);
            //act
            act.Comment = "new comment";
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetWLStatusWhenIsMajorPropertyIsChanged() {
            //arrange
            MyProject proj = new MyProject(new Project());
            proj.IsSimpleProject = true;
            MyAction act = new MyAction(new Action() { Name = "Name1" });
            act.WLId = "2";
            proj.Actions.Add(act);
            //act
            act.IsMajor = true;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetScheduledTimeSetStatusToScheduled() {
            //arrange
            MyAction act = new MyAction(new Action());
            //act
            act.ScheduledTime = new DateTime(2016, 9, 10);
            //assert
            Assert.AreEqual(ActionsStatusEnum.InWork, act.Status);
        }

        [Test]
        public void SetStatusToNonScheduledShoulNullScheduledTime() {
            //arrange
            MyAction act = new MyAction(new Action() { Project = new Project() });
            act.Status = ActionsStatusEnum.InWork;
            act.ScheduledTime = new DateTime(2016, 9, 10);
            //act
            act.Status = ActionsStatusEnum.Delay;
            //assert
            Assert.AreEqual(false, act.ScheduledTime.HasValue);
        }
        [Test]
        public void SetStatusToNonScheduledShoulNotChangeScheduledTimeIfItIsAlreadyNull() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status = ActionsStatusEnum.InWork;
            act.WLId = "12";
            //act
            act.Status = ActionsStatusEnum.Delay;
            //assert
            Assert.AreNotEqual(WLTaskStatusEnum.UpdateNeeded, act.WLTaskStatus);
        }
        [Test]
        public void SetStatusToSameShouldNotChangeStatus() {
            //arrange
            MyAction act = new MyAction(new Action());
            act.Status = ActionsStatusEnum.InWork;
            string tmpSting = null;
            act.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "Status")
                    tmpSting = "IsSet";
            };
            //act
            act.Status = ActionsStatusEnum.InWork;
            //assert
            Assert.AreEqual(null, tmpSting);
        }


        [Test]
        public void SetStatusToActiveAgainShouldClearCompletedTime() {
            //arrange
            MyAction act = new MyAction(new Action());
            MyAction act2 = new MyAction(new Action());
            //act
            act.Status = ActionsStatusEnum.Done;
            act.Status = ActionsStatusEnum.InWork;
            act2.Status = ActionsStatusEnum.Rejected;
            act2.Status = ActionsStatusEnum.InWork;
            //assert
            Assert.AreEqual(null, act.CompleteTime);
            Assert.AreEqual(null, act2.CompleteTime);
        }

        [Test]
        public void ChangingPropertiesSeveralTimesdoesntaddthisPropertyToCollection() {
            //arrange
            var act1 = new MyAction(new Action());
            //act
            act1.Name = "name1";
            act1.Name = "name2";
            //assert
            Assert.AreEqual(1, act1.changedProperties.Count);
        }

        [Test]
        public void ActionWithWlTaskShouldCompleteTaskIfSetDelay() {
            //arrange
            var act1 = new MyAction(new Action());
            act1.Status = ActionsStatusEnum.InWork;
            act1.WLId = "1";
            //act
            act1.Status = ActionsStatusEnum.Delay;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act1.WLTaskStatus);
        }

        [Test]
        public void CompleteTaskIfScheduledActionCompleted() {
            //arrange
            var act1 = new MyAction(new Action());
            act1.Status = ActionsStatusEnum.InWork;
            act1.WLId = "1";
            act1.ScheduledTime = DateTime.Today;
            //act
            act1.Status = ActionsStatusEnum.Done;
            //assert
            Assert.AreEqual(WLTaskStatusEnum.DeletingNeeded, act1.WLTaskStatus);
        }
    }
#endif
}
