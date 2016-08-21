using System.Data.Entity;

namespace ListOfDeal {
    public interface IListOfDealBaseEntities {
        DbSet<Action> Actions { get; set; }
        DbSet<ActionTrigger> ActionTriggers { get; set; }
        DbSet<DelegatePerson> DelegatePersons { get; set; }
        DbSet<Project> Projects { get; set; }
        DbSet<ProjectType> ProjectTypes { get; set; }
        int SaveChanges();
    }
}