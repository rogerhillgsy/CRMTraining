using System;
using System.Configuration;
using System.Linq;
using DynamicsLinq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace CRMTrainingTests
{
    [TestClass]
    public class LinqUpdateTest
    {
        [TestMethod]
        public void TestLinqUpdate()
        {
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmSvc);

                var acc = (from a in ctx.AccountSet where a.Name == "Aardvark Ltd" select a).ToList();

                var account = new Account();
                account.Id = acc.First().Id;
                account.Address1_City = "Manchester";

                ctx.Attach(account);
                
                var response = ctx.SaveChanges();

                account.Address1_City = "Leeds";
                response = ctx.SaveChanges();

                Assert.IsNotNull(response);


            }
        }
    }
}
