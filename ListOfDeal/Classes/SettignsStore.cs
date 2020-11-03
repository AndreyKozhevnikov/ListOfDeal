using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ListOfDeal.Classes
{
    public class SettingsStore {
        const string filePath = @"c:\mssqlsettings.ini";
        public static string GetPropertyValue(string nameProperty) {

            StreamReader sr = new StreamReader(filePath);
            string st = sr.ReadToEnd();
            sr.Close();

            XElement xl = XElement.Parse(st);
            var dropBoxPath = xl.Element(nameProperty).Value;
            return dropBoxPath;
        }
    }
}
