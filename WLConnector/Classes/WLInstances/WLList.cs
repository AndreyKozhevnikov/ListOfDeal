using System;
using System.Diagnostics;
using System.Linq;

namespace WlConnectionLibrary {
    //public class WLList {

    //}
    [DebuggerDisplay("List. Id-{id} title-{title}")]
    public class WLList {
        public int id { get; set; }
        public string title { get; set; }
        public string owner_type { get; set; }
        public int owner_id { get; set; }
        public string list_type { get; set; }
        public bool @public { get; set; }
        public int revision { get; set; }
        public string created_at { get; set; }
        public string type { get; set; }
        public string created_by_request_id { get; set; }
    }
}



