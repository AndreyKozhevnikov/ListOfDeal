using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public partial class ListOfDealBaseEntities : IListOfDealBaseEntities {
        public ListOfDealBaseEntities(string connectionName)
              : base(connectionName) {
        }
    }
}
