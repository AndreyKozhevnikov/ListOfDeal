using DevExpress.Xpo;
using ListOfDeal.Classes.XPO;
using System;

namespace ListOfDeal {
    public static class ConnectionHelper {
        static Type[] persistentTypes = new Type[] {
            typeof(ActionXP),
            typeof(Project),
            typeof(ProjectType),
            typeof(WeekRecord)
        };
        public static Type[] GetPersistentTypes() {
            Type[] copy = new Type[persistentTypes.Length];
            Array.Copy(persistentTypes, copy, persistentTypes.Length);
            return copy;
        }
        public static string ConnectionString { 
            get { 
                return System.Configuration.ConfigurationManager.ConnectionStrings["KOZHEVNIKOV-NBXListOfDealBase"].ConnectionString; 
            } 
        }
        public static void Connect(DevExpress.Xpo.DB.AutoCreateOption autoCreateOption, bool threadSafe = false) {
            if (threadSafe) {
                var provider = XpoDefault.GetConnectionProvider(ConnectionString, autoCreateOption);
                var dictionary = new DevExpress.Xpo.Metadata.ReflectionDictionary();
                dictionary.GetDataStoreSchema(persistentTypes);
                XpoDefault.DataLayer = new ThreadSafeDataLayer(dictionary, provider);
            } else {
                XpoDefault.DataLayer = XpoDefault.GetDataLayer(ConnectionString, autoCreateOption);
            }
            XpoDefault.Session = null;
        }
        public static DevExpress.Xpo.DB.IDataStore GetConnectionProvider(DevExpress.Xpo.DB.AutoCreateOption autoCreateOption) {
            return XpoDefault.GetConnectionProvider(ConnectionString, autoCreateOption);
        }
        public static DevExpress.Xpo.DB.IDataStore GetConnectionProvider(DevExpress.Xpo.DB.AutoCreateOption autoCreateOption, out IDisposable[] objectsToDisposeOnDisconnect) {
            return XpoDefault.GetConnectionProvider(ConnectionString, autoCreateOption, out objectsToDisposeOnDisconnect);
        }
        public static IDataLayer GetDataLayer(DevExpress.Xpo.DB.AutoCreateOption autoCreateOption) {
            return XpoDefault.GetDataLayer(ConnectionString, autoCreateOption);
        }
    }


}
