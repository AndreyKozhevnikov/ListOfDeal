using System;
using System.Collections.Generic;
using System.Linq;

namespace WlConnectionLibrary {
    public static class JsonCreator {
        static List<Tuple<string, object>> tList = new List<Tuple<string, object>>();
        public static void Add(string st, object o) {
            tList.Add(new Tuple<string, object>(st, o));
        }

        public static string GetString() {
            var lstTuple = tList.Select(x => GetTupleString(x));
            string st = string.Join(",", lstTuple);
            st = "{" + st + "}";
            tList.Clear();
            return st;
        }
        static string GetTupleString(Tuple<string, object> t) {
            if(t.Item1.Contains("id"))
                return string.Format("\"{0}\":{1}", t.Item1, t.Item2);
            if(t.Item2 is string)
                return string.Format("\"{0}\":\"{1}\"", t.Item1, t.Item2);
            if(t.Item2 is bool)
                return string.Format("\"{0}\":{1}", t.Item1, t.Item2.ToString().ToLower());
            return string.Format("\"{0}\":{1}", t.Item1, t.Item2);
        }

    }
}



