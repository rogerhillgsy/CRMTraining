using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Microsoft.Xrm.Sdk;

namespace CRMTrainingTests
{
    [TestClass]
    public class Module2LabA
    {
        [TestMethod]
        public void TestConnectToDiscovery()
        {
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;

           
        }
    }
}
