using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
   public class HistoryActionItem {
       public MyAction Action { get; set; }
       public DateTime? FinalDate { get; set; }
       public bool IsCompleted{get;set;}

    }
}
