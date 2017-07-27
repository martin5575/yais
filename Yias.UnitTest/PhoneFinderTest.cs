using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yais.Model.Search.ContentFinder;

namespace Yias.UnitTest
{
    [TestClass]
    public class PhoneFinderTest
    {
        private PhoneNumberFinder _target = new PhoneNumberFinder();

        [TestMethod]
        public void TestEasyNumber()
        {
            FoundContent found;
            const string number = "069112233";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }

        [TestMethod]
        public void TestNumberWithBlank()
        {
            FoundContent found;
            const string number = "069 112233";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }

        [TestMethod]
        public void TestNumberWithBlanks()
        {
            FoundContent found;
            const string number = "069 11 22 33";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }

        [TestMethod]
        public void TestNumberWithMinus()
        {
            FoundContent found;
            const string number = "069-112233";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }

        [TestMethod]
        public void TestNumberWithBracketsAtStart()
        {
            FoundContent found;
            const string number = "(069) 112233";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }

        [TestMethod]
        public void TestNumberWithBracketsInMiddle()
        {
            FoundContent found;
            const string number = "069-1122(0)33";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }

        [TestMethod]
        public void TestNumberWithCountryCode()
        {
            FoundContent found;
            const string number = "+4969112233";
            Assert.IsTrue(_target.TryFind(number, out found));
            Assert.AreEqual(number, found.Content);
        }


        [TestMethod]
        public void TestFailure()
        {
            FoundContent found;
            Assert.IsFalse(_target.TryFind("112", out found));
        }

        [TestMethod]
        public void TestFailure2()
        {
            FoundContent found;
            Assert.IsFalse(_target.TryFind("DE123456789", out found));
        }

        [TestMethod]
        public void TestFailure3()
        {
            FoundContent found;
            Assert.IsFalse(_target.TryFind("Karl 12", out found));
        }
    }
}
