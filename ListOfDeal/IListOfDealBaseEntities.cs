using System.Data.Entity;

namespace ListOfDeal {
    public interface IListOfDealBaseEntities {
        IDbSet<Action> Actions { get; set; }
        DbSet<WeekRecord> WeekRecords { get; set; }
        DbSet<Project> Projects { get; set; }
        DbSet<ProjectType> ProjectTypes { get; set; }
        int SaveChanges();
    }
}