using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public delegate void WLEventHandler(WLEventArgs e);
    public class WLEventArgs {
        public string Message { get; set; }
        public DateTime DTime { get; set; }
        public WLEventArgs(string st) {
            Message = st;
            DTime = DateTime.Now;
        }
    }
}
