
using DevExpress.Xpo;

namespace ListOfDeal.Classes.XPO {
    [Persistent("ProjectTypes")]
    public  class ProjectType : XPLiteObject {
        public ProjectType(Session session) : base(session) { }
        int fId;
        [Key(true)]
        public int Id {
            get { return fId; }
            set { SetPropertyValue<int>(nameof(Id), ref fId, value); }
        }
        string fName;
        [Size(50)]
        public string Name {
            get { return fName; }
            set { SetPropertyValue<string>(nameof(Name), ref fName, value); }
        }
        int fOrderNumber;
        public int OrderNumber {
            get { return fOrderNumber; }
            set { SetPropertyValue<int>(nameof(OrderNumber), ref fOrderNumber, value); }
        }
        [Association(@"ProjectsReferencesProjectTypes")]
        public XPCollection<Project> ProjectsCollection { get { return GetCollection<Project>(nameof(ProjectsCollection)); } }
    }
}
