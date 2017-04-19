using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.Tests {
#if DebugTest
    [TestFixture]
    public class MainViewModelTests {
        Mock<IListOfDealBaseEntities> mockGeneralEntity;
        Mock<IMainViewModelDataProvider> dataProviderEntity;
        MainViewModel vm;
        void Initialize() {
            mockGeneralEntity = new Mock<IListOfDealBaseEntities>();
            dataProviderEntity = new Mock<IMainViewModelDataProvider>();
            dataProviderEntity.Setup(x => x.GeneralEntity).Returns(mockGeneralEntity.Object);
            dataProviderEntity.Setup(x => x.GetProjects()).Returns(new List<Project>());
            dataProviderEntity.Setup(x => x.GetProjectTypes()).Returns(new List<ProjectType>());
            dataProviderEntity.Setup(x => x.CreateProject()).Returns(new Project());
            dataProviderEntity.Setup(x => x.CreateAction()).Returns(new Action());
            MainViewModel.DataProvider = dataProviderEntity.Object;
            vm = new MainViewModel();
            vm.GridControlManagerService = new Mock<IGridControlManagerService>().Object;
        }
        [Test]
        public void NewProjectHasTypeOfPrevious() {
            //arrange
            Initialize();
            vm.CurrentProject.Name = "testproject";
            //act
            vm.CurrentProject.TypeId = (int)ProjectStatusEnum.Delayed;
            vm.AddNewProjectCommand.Execute(null);
            //Assert
            Assert.AreEqual((int)ProjectStatusEnum.Delayed, vm.CurrentProject.TypeId);
        }

        [Test]
        public void Initialize_GetOnlyActiveProject() {
            //arrange
            Initialize();
            var lst = new List<Project>();
            lst.Add(new Project() { StatusId = (int)ProjectStatusEnum.Delayed });
            lst.Add(new Project() { StatusId = (int)ProjectStatusEnum.Done });
            lst.Add(new Project() { StatusId = (int)ProjectStatusEnum.InWork });
            lst.Add(new Project() { StatusId = (int)ProjectStatusEnum.Rejected });
            dataProviderEntity.Setup(x => x.GetProjects()).Returns(lst);
            //act
            var vmN = new MainViewModel();
            //Assert
            Assert.AreEqual(2, vmN.Projects.Count);
            Assert.AreEqual(1, vmN.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).Count());
            Assert.AreEqual(1, vmN.Projects.Where(x => x.Status == ProjectStatusEnum.Delayed).Count());
        }

        [Test]
        public void IsNewStatusIsValid() {
            //arrange
            Initialize();

            MyProject dontCan1 = new MyProject(new Project());
            dontCan1.Actions.Add(new MyAction(new Action() { StatusId = (int)ActionsStatusEnum.InWork }));
            MyProject dontCan2 = new MyProject(new Project());
            dontCan2.Actions.Add(new MyAction(new Action() { StatusId = (int)ActionsStatusEnum.Delay }));
            MyProject Can1 = new MyProject(new Project());
            Can1.Actions.Add(new MyAction(new Action() { StatusId = (int)ActionsStatusEnum.Rejected }));
            MyProject Can2 = new MyProject(new Project());
            Can2.Actions.Add(new MyAction(new Action() { StatusId = (int)ActionsStatusEnum.Done }));
            //act
            var dontCan1_Delay = vm.IsNewStatusIsValid(dontCan1, ProjectStatusEnum.Delayed);
            var dontCan1_InWork = vm.IsNewStatusIsValid(dontCan1, ProjectStatusEnum.InWork);
            var dontCan1_Done = vm.IsNewStatusIsValid(dontCan1, ProjectStatusEnum.Done);
            var dontCan1_Rejected = vm.IsNewStatusIsValid(dontCan1, ProjectStatusEnum.Rejected);

            var dontCan2_Delay = vm.IsNewStatusIsValid(dontCan2, ProjectStatusEnum.Delayed);
            var dontCan2_InWork = vm.IsNewStatusIsValid(dontCan2, ProjectStatusEnum.InWork);
            var dontCan2_Done = vm.IsNewStatusIsValid(dontCan2, ProjectStatusEnum.Done);
            var dontCan2_Rejected = vm.IsNewStatusIsValid(dontCan2, ProjectStatusEnum.Rejected);

            var Can1_Delay = vm.IsNewStatusIsValid(Can1, ProjectStatusEnum.Delayed);
            var Can1_InWork = vm.IsNewStatusIsValid(Can1, ProjectStatusEnum.InWork);
            var Can1_Done = vm.IsNewStatusIsValid(Can1, ProjectStatusEnum.Done);
            var Can1_Rejected = vm.IsNewStatusIsValid(Can1, ProjectStatusEnum.Rejected);

            var Can2_Delay = vm.IsNewStatusIsValid(Can2, ProjectStatusEnum.Delayed);
            var Can2_InWork = vm.IsNewStatusIsValid(Can2, ProjectStatusEnum.InWork);
            var Can2_Done = vm.IsNewStatusIsValid(Can2, ProjectStatusEnum.Done);
            var Can2_Rejected = vm.IsNewStatusIsValid(Can2, ProjectStatusEnum.Rejected);

            //assert
            Assert.AreEqual(true, dontCan1_Delay);
            Assert.AreEqual(true, dontCan1_InWork);
            Assert.AreEqual(false, dontCan1_Done);
            Assert.AreEqual(false, dontCan1_Rejected);

            Assert.AreEqual(true, dontCan2_Delay);
            Assert.AreEqual(true, dontCan2_InWork);
            Assert.AreEqual(false, dontCan2_Done);
            Assert.AreEqual(false, dontCan2_Rejected);

            Assert.AreEqual(true, Can1_Delay);
            Assert.AreEqual(true, Can1_InWork);
            Assert.AreEqual(true, Can1_Done);
            Assert.AreEqual(true, Can1_Rejected);

            Assert.AreEqual(true, Can2_Delay);
            Assert.AreEqual(true, Can2_InWork);
            Assert.AreEqual(true, Can2_Done);
            Assert.AreEqual(true, Can2_Rejected);

        }
    }
#endif
}
