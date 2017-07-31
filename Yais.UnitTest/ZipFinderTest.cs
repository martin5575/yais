using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yais.Model.Search.ContentFinder;

namespace Yias.UnitTest
{
    [TestClass]
    public class ZipFinderTest
    {
        private ZipCodeFinder _target = new ZipCodeFinder();

        [TestMethod]
        public void TestSimpleCity()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("64287 Darmstadt", out foundContent));
            Assert.AreEqual("64287", foundContent.Content);
            Assert.AreEqual(FoundContentType.ZipCode, foundContent.Type);
        }


        [TestMethod]
        public void TestCityWithMinusInName()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("63263 Neu-Isenburg", out foundContent));
            Assert.AreEqual("63263", foundContent.Content);
        }

        [TestMethod]
        public void TestCityWithMultipleWords()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("60311 Frankfurt am Main", out foundContent));
            Assert.AreEqual("60311", foundContent.Content);
        }


        [TestMethod]
        public void TestWithLeadingCountryLetterCode()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("D-60311 Frankfurt am Main", out foundContent));
            Assert.AreEqual("60311", foundContent.Content);
        }

        [TestMethod]
        public void TestNoMatchOnNumberOnly()
        {
            FoundContent foundContent;
            Assert.IsFalse(_target.TryFind("60311", out foundContent));
        }


        [TestMethod]
        public void TestNoMatchOnTooShortNumber()
        {
            FoundContent foundContent;
            Assert.IsFalse(_target.TryFind("6078", out foundContent));
        }

        [TestMethod]
        public void TestNoMatchOnTooLongNumber()
        {
            FoundContent foundContent;
            Assert.IsFalse(_target.TryFind("123456", out foundContent));
        }
    }
}
