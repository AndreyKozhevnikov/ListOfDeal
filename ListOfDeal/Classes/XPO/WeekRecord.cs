using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.XPO {
    [Persistent("WeekRecords")]
    public  class WeekRecord : XPLiteObject {
        public WeekRecord(Session session) : base(session) { }
        int fId;
        [Key(true)]
        public int Id {
            get { return fId; }
            set { SetPropertyValue<int>(nameof(Id), ref fId, value); }
        }
        ActionXP fActionId;
        [Association(@"WeekRecordsReferencesActions")]
        public ActionXP ActionId {
            get { return fActionId; }
            set { SetPropertyValue<ActionXP>(nameof(ActionId), ref fActionId, value); }
        }
        DateTime fDateAdd;
        public DateTime DateAdd {
            get { return fDateAdd; }
            set { SetPropertyValue<DateTime>(nameof(DateAdd), ref fDateAdd, value); }
        }
        string fComment;
        [Size(SizeAttribute.Unlimited)]
        public string Comment {
            get { return fComment; }
            set { SetPropertyValue<string>(nameof(Comment), ref fComment, value); }
        }
        string fWeekId;
        [Size(50)]
        public string WeekId {
            get { return fWeekId; }
            set { SetPropertyValue<string>(nameof(WeekId), ref fWeekId, value); }
        }
        bool fIsCompletedInWeek;
        public bool IsCompletedInWeek {
            get { return fIsCompletedInWeek; }
            set { SetPropertyValue<bool>(nameof(IsCompletedInWeek), ref fIsCompletedInWeek, value); }
        }
    }
}
