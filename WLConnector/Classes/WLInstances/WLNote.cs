using System;
using System.Linq;

namespace WlConnectionLibrary {
    public class WLNote {
        public string id { get; set; }
        public int revision { get; set; }
        public string content { get; set; }
        public string type { get; set; }
        public string task_id { get; set; }
        public string created_by_request_id { get; set; }
    }
}



