using System;
using System.Collections.Generic;
using System.Linq;

namespace WlConnectionLibrary {
    public class SubtaskPosition {
        public int id { get; set; }
        public int task_id { get; set; }
        public int revision { get; set; }
        public List<object> values { get; set; }
        public string type { get; set; }
    }
}



