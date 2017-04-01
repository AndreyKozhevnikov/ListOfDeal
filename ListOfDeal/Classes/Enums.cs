using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public enum ActionsStatusEnum2 {
        Delay =0,
        InWork=1,
        Done=2,
        Rejected=3
    }

    public enum ActionsStatusEnum {
        Waited = 1,
        Scheduled = 2,
        Completed = 4
    }
    public enum WLTaskStatusEnum {
        UpToDateWLTask = 0,
        UpdateNeeded = 1,
        DeletingNeeded = 2
    }
    public enum ProjectStatusEnum {
        InWork = 1,
        Delayed = 2,
        Done = 3
    }
}
