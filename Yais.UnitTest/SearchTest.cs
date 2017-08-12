using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yais.Model;
using Yais.Model.Search.ContentFinder;

namespace Yias.UnitTest
{
    [TestClass]
    public class SearchTest
    {
#warning TODO: repair nuget pakages
        private static readonly HtmlDocument _html = new HtmlDocument();

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            var assembly = typeof(SearchTest).Assembly;
            var stream = assembly.GetManifestResourceStream("Yais.UnitTest.TestData.TestPage.html");
            _html.Load(stream);
        }

        [TestMethod]
        public void SearchZipFinderTest()
        {
            var finders = new IContentFinder[] { new ZipCodeFinder() };
            var foundContent = SearchEngine.Parse(_html, finders).ToList();
            Assert.AreEqual(7, foundContent.Count);
            Assert.IsTrue(foundContent.All(x => x.Type == FoundContentType.ZipCode));
        }

        [TestMethod]
        public void SearchCityFinderTest()
        {
            var finders = new IContentFinder[] { new CityFinder() };
            var foundContent = SearchEngine.Parse(_html, finders).ToList();
            Assert.AreEqual(7, foundContent.Count);
            Assert.IsTrue(foundContent.All(x => x.Type == FoundContentType.City));
        }

        [TestMethod]
        public void SearchEMailFinderTest()
        {
            var finders = new IContentFinder[] { new EMailAddressFinder() };
            var foundContent = SearchEngine.Parse(_html, finders).ToList();
            Assert.AreEqual(1, foundContent.Count);
            Assert.AreEqual(FoundContentType.EMailAdress, foundContent[0].Type);
            Assert.AreEqual("Publikumsservice@mdr.de", foundContent[0].Content);
        }

        [TestMethod]
        public void SearchPhoneFinderTest()
        {
            var finders = new IContentFinder[] { new PhoneNumberFinder() };
            var foundContent = SearchEngine.Parse(_html, finders).ToList();
            Assert.AreEqual(1, foundContent.Count);
            Assert.AreEqual(FoundContentType.PhoneNumber, foundContent[0].Type);
            Assert.AreEqual("0341-3000", foundContent[0].Content);
        }

        [TestMethod]
        public void SearchNameFinderTest()
        {
            var finders = new IContentFinder[] { new NameFinder() };
            var foundContent = SearchEngine.Parse(_html, finders).ToList();
            Assert.AreEqual(1, foundContent.Count);
            Assert.AreEqual(FoundContentType.Name, foundContent[0].Type);
        }

        [TestMethod]
        public void SearchTaxNumberFinderTest()
        {
            var finders = new IContentFinder[] { new TaxNumberFinder() };
            var foundContent = SearchEngine.Parse(_html, finders).ToList();
            Assert.AreEqual(1, foundContent.Count);
            Assert.AreEqual(FoundContentType.TaxIdentifiactionNumber, foundContent[0].Type);
            Assert.AreEqual("DE141510836", foundContent[0].Content);
        }

    }
}
