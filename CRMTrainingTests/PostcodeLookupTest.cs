using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CRMTrainingTests
{
    [TestClass]
    public class PostcodeLookupTest
    {
        [TestMethod]
        public void TestIoLookup()
        {
            var postcode = "TW1 2HU";

            var request = WebRequest.Create($"https://api.postcodes.io/postcodes/{postcode}");
            request.Method = "GET";
            request.ContentType = "application/json";

            var response = request.GetResponse();
            var text = string.Empty;

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }
            Console.WriteLine($"Response: {text}");
            var result = JsonConvert.DeserializeObject<IOResponse>(text);

            var town = result.result.admin_ward;

            Console.WriteLine(town);
        }

        [TestMethod]
        public void TestIoLookupClass()
        {
            var town = IOLookup.IoLookupPostcode("TW118AX").result.admin_ward;

            Assert.AreEqual("Teddington", town);
        }
    }
    

}
