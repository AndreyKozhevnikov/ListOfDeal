using System;
using System.Linq;

namespace WlConnectionLibrary {
    public class Subtask {
        public int id { get; set; }
        public int task_id { get; set; }
        public bool completed { get; set; }
        public string completed_at { get; set; }
        public string created_at { get; set; }
        public int created_by_id { get; set; }
        public string created_by_request_id { get; set; }
        public int revision { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }
}



