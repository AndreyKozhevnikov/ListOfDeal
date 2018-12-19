using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ListOfDeal {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            AssemblyResolverDll.AsseblyResolver.Attach("Dll181");
#if DebugTest
            Application.Current.Shutdown();
#endif
            CultureInfo newCI = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            newCI.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
            Thread.CurrentThread.CurrentCulture = newCI;
        }
    }
}
