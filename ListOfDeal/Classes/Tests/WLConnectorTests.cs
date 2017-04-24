using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.Tests {
    #if DebugTest
    [TestFixture]
    public class JsonCreatorTests {
        [Test]
        public void CreateJson() {
            //arrange
            JsonCreator.Add("revision", 123);
            JsonCreator.Add("title", "NewTestTitle3");
            JsonCreator.Add("completed", true);
            JsonCreator.Add("note_id", "678");
            //act
            string st = JsonCreator.GetString();
            //assert
            string json = "{\"revision\":" + 123 + "," +
               "\"title\":\"NewTestTitle3\"," +
               "\"completed\":true," +
               "\"note_id\":678" +
               "}";
            Assert.AreEqual(json, st);

            string st2 = JsonCreator.GetString();
            Assert.AreEqual("{}", st2);

        }

        [Test]
        public void NormalizeString_test() {
            //arrange
            WLConnector wlConn = new WLConnector();
            var title = "NewTestTitle3 \"test\"";
            var title2 = @"NewTestTitle3 \test";
            var content = "test"+Environment.NewLine+"test2";
            //act
            string st = wlConn.NormalizeString(title);
            string st2 = wlConn.NormalizeString(title2);
            string content2 = wlConn.NormalizeString(content);
            //assert
            string need = "NewTestTitle3 \\\"test\\\"";
            string need2 = "NewTestTitle3 \\\\test";
            string needContent = "test\\r\\ntest2";
            Assert.AreEqual(need, st);
            Assert.AreEqual(need2, st2);
            Assert.AreEqual(needContent, content2);
        }
    }
#endif
}
