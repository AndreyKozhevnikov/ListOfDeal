using ListOfDeal.Classes.XPO;

using System.Collections.Generic;

namespace ListOfDeal {
    public interface IMainViewModelDataProvider {
        IEnumerable<ProjectType> GetProjectTypes();
        ProjectType GetProjectTypeById(int projectId);
        IEnumerable<Project> GetProjects();
        IEnumerable<Project> GetActiveProejcts();
        Project CreateProject();
      //  void AddProject(Project p);
      //  void AddWeekRecord(WeekRecord wr);
       // void AddWeekRecords(IEnumerable<WeekRecord> wrs);
        IEnumerable<ActionXP> GetActions();
        IEnumerable<WeekRecord> GetWeekRecords();
        ProjectType CreateProjectType();
      //  void AddProjectType(ProjectType projectType);
        void SaveChanges();
        ActionXP CreateAction();
        WeekRecord CreateWeekRecord();
        ActionXP GetActionById(int id);
    }


}
