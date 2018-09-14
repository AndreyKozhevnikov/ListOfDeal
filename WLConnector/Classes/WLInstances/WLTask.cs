using System;
using System.Diagnostics;
using System.Linq;

namespace WlConnectionLibrary {
    //public class WLList {
    //}
    [DebuggerDisplay("Task. Id-{id} title-{title}")]
    public class WLTask {
        public string id { get; set; }
        public string created_at { get; set; }
        public int created_by_id { get; set; }
        public string created_by_request_id { get; set; }
        public bool completed { get; set; }
        public string completed_at { get; set; }
        public int completed_by_id { get; set; }
        public bool starred { get; set; }
        public int list_id { get; set; }
        public int revision { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string due_date { get; set; }
        public string recurrence_type { get; set; }
        public int? recurrence_count { get; set; }
        public override string ToString() {
            return string.Format("Task {0}", title);
        }
    }
}



