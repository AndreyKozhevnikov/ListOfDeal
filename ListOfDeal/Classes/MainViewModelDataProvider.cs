using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using ListOfDeal.Classes.XPO;

using System.Collections.Generic;
using System.Linq;

namespace ListOfDeal {
    public class MainViewModelDataProvider : IMainViewModelDataProvider {
        public MainViewModelDataProvider() {
            ConnectToDataBase();

        }
        UnitOfWork uow;
        private void ConnectToDataBase() {
   
            ConnectionHelper.Connect(DevExpress.Xpo.DB.AutoCreateOption.None);
            uow=new UnitOfWork();
        }

        public IEnumerable<ProjectType> GetProjectTypes() {
            return new XPCollection<ProjectType>(uow);
        }

        public IEnumerable<Project> GetProjects() {
            return new XPCollection<Project>(uow);
            
        }
        public IEnumerable<Project> GetActiveProejcts() {
            return new XPCollection<Project>(uow,CriteriaOperator.FromLambda< Project>(x=>x.StatusId==(int)ProjectStatusEnum.InWork));
        }
        public IEnumerable<WeekRecord> GetWeekRecords() {
            return new XPCollection<WeekRecord>(uow);
            
        }
        public Project CreateProject() {
            return new Project(uow);
        }

        //public void AddProject(Project p) {
        //    GeneralEntity.Projects.Add(p);
        //}
        //public void AddWeekRecord(WeekRecord wr) {
        //    GeneralEntity.WeekRecords.Add(wr);
            
        //}
        //public void AddWeekRecords(IEnumerable<WeekRecord> wrs) {
        //    GeneralEntity.WeekRecords.AddRange(wrs);
        //}
        public IEnumerable<ActionXP> GetActions() {
            return new XPCollection<ActionXP>();
        }
        public ProjectType CreateProjectType() {
            return new ProjectType(uow);
        }
        //public void AddProjectType(ProjectType projectType) {
        //    GeneralEntity.ProjectTypes.Add(projectType);
        //}
        public void SaveChanges() {
            uow.CommitChanges();
        }
        public ActionXP CreateAction() {
            return new ActionXP(uow);
        }
        public WeekRecord CreateWeekRecord() {
            return new WeekRecord(uow);
        }

        public ProjectType GetProjectTypeById(int projectId) {
            return uow.GetObjectByKey<ProjectType> (projectId);
        }

        public ActionXP GetActionById(int id) {
            return uow.GetObjectByKey<ActionXP>(id);
        }
    }


}
