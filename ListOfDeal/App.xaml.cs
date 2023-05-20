using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace ListOfDeal {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
       // string pathToSettingsFile = @"C:\MSSQLSettings.ini";
        public App() {
            //StreamReader sr = new StreamReader(pathToSettingsFile);
            //string st = sr.ReadToEnd();
            //sr.Close();
            //XElement xl = XElement.Parse(st);
            //var pathDropbox = xl.Element("Dropbox").Value;
            //var dllPath = Path.Combine(pathDropbox, @"deploy\Dll181");
            //AssemblyResolverDll.AsseblyResolver.Attach(dllPath);
            AssemblyResolverDll.AsseblyResolver.Attach(@"deploy\Dll2226");
#if DebugTest
            Application.Current.Shutdown();
#endif
            CultureInfo newCI = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            newCI.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
            Thread.CurrentThread.CurrentCulture = newCI;
        }
    }
}
