
using DevExpress.Xpo;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.XPO {
    [Persistent("Projects")]
    public  class Project : XPLiteObject {
        public Project(Session session) : base(session) { }
        int fId;
        [Key(true)]
        public int Id {
            get { return fId; }
            set { SetPropertyValue<int>(nameof(Id), ref fId, value); }
        }
        string fName;
        [Size(SizeAttribute.Unlimited)]
        public string Name {
            get { return fName; }
            set { SetPropertyValue<string>(nameof(Name), ref fName, value); }
        }
        ProjectType fTypeId;
        [Association(@"ProjectsReferencesProjectTypes")]
        public ProjectType TypeId {
            get { return fTypeId; }
            set { SetPropertyValue<ProjectType>(nameof(TypeId), ref fTypeId, value); }
        }
        DateTime fDateCreated;
        public DateTime DateCreated {
            get { return fDateCreated; }
            set { SetPropertyValue<DateTime>(nameof(DateCreated), ref fDateCreated, value); }
        }
        int fStatusId;
        public int StatusId {
            get { return fStatusId; }
            set { SetPropertyValue<int>(nameof(StatusId), ref fStatusId, value); }
        }
        string fComment;
        [Size(SizeAttribute.Unlimited)]
        public string Comment {
            get { return fComment; }
            set { SetPropertyValue<string>(nameof(Comment), ref fComment, value); }
        }
        DateTime fCompleteTime;
        public DateTime CompleteTime {
            get { return fCompleteTime; }
            set { SetPropertyValue<DateTime>(nameof(CompleteTime), ref fCompleteTime, value); }
        }
        string fDesiredResult;
        [Size(SizeAttribute.Unlimited)]
        public string DesiredResult {
            get { return fDesiredResult; }
            set { SetPropertyValue<string>(nameof(DesiredResult), ref fDesiredResult, value); }
        }
        bool fIsSimpleProject;
        public bool IsSimpleProject {
            get { return fIsSimpleProject; }
            set { SetPropertyValue<bool>(nameof(IsSimpleProject), ref fIsSimpleProject, value); }
        }
        [Association(@"ActionsReferencesProjects")]
        public XPCollection<ActionXP> ActionsCollection { get { return GetCollection<ActionXP>(nameof(ActionsCollection)); } }
    }
}
