using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using CRMTraining2.Plugins;

namespace CRMTrainingTests
{
    [TestClass]
    public class Module5PostcodeLookupTest
    {
        [DataRow("TW12HU", "200")]
        [DataRow("TW19HU", "404")]
        [DataTestMethod]
        public void TestPostcodeLookupTest2(string postcode, string expectedStatus)
        {
            var httpClient = System.Net.HttpWebRequest.Create($"https://api.postcodes.io/postcodes/{postcode}");


            var response = httpClient.GetResponse();

            var reader = new StreamReader(response.GetResponseStream());
            var json = reader.ReadToEnd();

            var statusRe = new Regex("\"status\":\\s*(\\d+)");
            var result = statusRe.Match(json);
            Assert.IsTrue(result.Success);
            var status = result.Groups[1].Value;
            Assert.AreEqual(expectedStatus, status);

        }

        [TestMethod]
        public void TestPostcodeLookup3()
        {
            var plugin = new ValidatePostcode("","");

            var result = plugin.IsValidPostcode("TW12HU", s => Debug.WriteLine(s));

            Assert.IsTrue(result);

            result = plugin.IsValidPostcode("TW19HU", s => Debug.WriteLine(s));

            Assert.IsFalse(result);

        }

    }
}
