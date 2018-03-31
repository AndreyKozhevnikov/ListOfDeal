using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal.Classes.Tests {
#if DebugTest
    [TestFixture]
    public class SpecialDateBorderStyleConverterTest {
        [Test]
        public void ConvertSaturday() {
            //arrange
            var conv = new SpecialDateBorderStyleConverter();
            conv.ToDayToTest = new DateTime(2018, 3, 31);
            var prevSun = new DateTime(2018, 4, 1);
            var targetMon = new DateTime(2018, 4, 2);
            var targetSun = new DateTime(2018, 4, 8);
            var followMon= new DateTime(2018, 4, 9);
            //act
            var resPrevSun = conv.Convert(prevSun,null,null,null);
            var resTargetMon = conv.Convert(targetMon, null, null, null);
            var resTargetSun = conv.Convert(targetSun, null, null, null);
            var resFollowSun = conv.Convert(followMon, null, null, null);
            //assert
            Assert.AreEqual(false, resPrevSun);
            Assert.AreEqual(true, resTargetMon);
            Assert.AreEqual(true, resTargetSun);
            Assert.AreEqual(false, resFollowSun);

        }
        [Test]
        public void ConvertSanday() {
            //arrange
            var conv = new SpecialDateBorderStyleConverter();
            conv.ToDayToTest = new DateTime(2018, 4, 1);
            var prevSun = new DateTime(2018, 4, 1);
            var targetMon = new DateTime(2018, 4, 2);
            var targetSun = new DateTime(2018, 4, 8);
            var followMon = new DateTime(2018, 4, 9);
            //act
            var resPrevSun = conv.Convert(prevSun, null, null, null);
            var resTargetMon = conv.Convert(targetMon, null, null, null);
            var resTargetSun = conv.Convert(targetSun, null, null, null);
            var resFollowSun = conv.Convert(followMon, null, null, null);
            //assert
            Assert.AreEqual(false, resPrevSun);
            Assert.AreEqual(true, resTargetMon);
            Assert.AreEqual(true, resTargetSun);
            Assert.AreEqual(false, resFollowSun);

        }
        [Test]
        public void ConvertTuesday() {
            //arrange
            var conv = new SpecialDateBorderStyleConverter();
            conv.ToDayToTest = new DateTime(2018, 4, 3);
            var prevSun = new DateTime(2018, 4, 1);
            var targetMon = new DateTime(2018, 4, 2);
            var targetSun = new DateTime(2018, 4, 8);
            var followMon = new DateTime(2018, 4, 9);
            //act
            var resPrevSun = conv.Convert(prevSun, null, null, null);
            var resTargetMon = conv.Convert(targetMon, null, null, null);
            var resTargetSun = conv.Convert(targetSun, null, null, null);
            var resFollowSun = conv.Convert(followMon, null, null, null);
            //assert
            Assert.AreEqual(false, resPrevSun);
            Assert.AreEqual(true, resTargetMon);
            Assert.AreEqual(true, resTargetSun);
            Assert.AreEqual(false, resFollowSun);

        }
    }

#endif
}
