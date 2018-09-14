using System;
using System.Linq;

namespace WlConnectionLibrary {
    //public class WLList {
    //}
    public class Reminder {
        public int id { get; set; }
        public string date { get; set; }
        public int task_id { get; set; }
        public int revision { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string created_by_request_id { get; set; }
        public string type { get; set; }
    }
}



