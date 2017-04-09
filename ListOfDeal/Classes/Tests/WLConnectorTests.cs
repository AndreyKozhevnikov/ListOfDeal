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
    }
#endif
}
