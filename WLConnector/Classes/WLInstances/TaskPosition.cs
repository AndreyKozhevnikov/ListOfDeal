using System;
using System.Collections.Generic;
using System.Linq;

namespace WlConnectionLibrary {
    public class TaskPosition {
        public int id { get; set; }
        public int list_id { get; set; }
        public int revision { get; set; }
        public List<int> values { get; set; }
        public string type { get; set; }
    }
}



