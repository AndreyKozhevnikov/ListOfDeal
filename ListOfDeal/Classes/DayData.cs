using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    [DebuggerDisplay("{TDate}, {CountIn}, {CountOut},{Delta},{Summary}")]
    public class DayData {
        public DateTime TDate { get; set; }
        public int CountIn { get; set; }
        public int CountOut { get; set; }
        public int Delta { get; set; }
        public int Summary { get; set; }
    }
}
