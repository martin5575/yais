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
    public class CityFinderTest
    {
        private CityFinder _target = new CityFinder();

        [TestMethod]
        public void TestSimpleCity()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("64287 Darmstadt", out foundContent));
            Assert.AreEqual("Darmstadt", foundContent.Content);
            Assert.AreEqual(FoundContentType.City, foundContent.Type);
        }


        [TestMethod]
        public void TestCityWithMinusInName()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("63263 Neu-Isenburg", out foundContent));
            Assert.AreEqual("Neu-Isenburg", foundContent.Content);
        }

        [TestMethod]
        public void TestCityWithMultipleWords()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("60311 Frankfurt am Main", out foundContent));
            Assert.AreEqual("Frankfurt am Main", foundContent.Content);
        }


        [TestMethod]
        public void TestWithLeadingCountryLetterCode()
        {
            FoundContent foundContent;
            Assert.IsTrue(_target.TryFind("D-60311 Frankfurt a.M.", out foundContent));
            Assert.AreEqual("Frankfurt a.M.", foundContent.Content);
        }

        [TestMethod]
        public void TestNoMatchOnCityNameOnly()
        {
            FoundContent foundContent;
            Assert.IsFalse(_target.TryFind("Berlin", out foundContent));
        }

    }
}
