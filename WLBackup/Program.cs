using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WlConnectionLibrary;

namespace WLBackup {
    class Program {
        static void Main(string[] args) {
            var wlConnector = new WLConnector();
            wlConnector.CreateBackup();
            Console.WriteLine("WLBackup created");
        }
    }
}
