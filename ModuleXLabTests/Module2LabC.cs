using System;
using System.Configuration;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace ModuleXLabTests
{
    [TestClass]
    public class Module2LabC
    {
        [DataRow("Name1","Line1", "City1")]
        [DataTestMethod]
        public void Module2LabCAsUnitTest(string name, string line1, string city)
        {
            var  cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            var crmServiceClient = new CrmServiceClient(cnString);

            var account = new Entity("account");
            account["name"] = name;
            account["address1_line1"] = line1;
            account["address1_city"] = city;

            var newAccountGuid = crmServiceClient.Create(account);

            Debug.WriteLine($"New account id: {newAccountGuid}");

            crmServiceClient.Dispose();
        }
    }
}
