using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices;
using System.Net;
using CRMTraining2.Workflows;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;

namespace TestWorkflow
{
    [TestClass]
    public class TestDuplicateWorkflow
    {
        string cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;

        [TestMethod]
        public void TestCodeActivity()
        {
             ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var ctx = new CrmServiceClient(cnString))
            {
                var codeActivity = new DuplicateChecker();
                var result = codeActivity.NumberOfDuplicates(ctx.OrganizationServiceProxy, "Name1");

                Assert.IsTrue( result > 0);

            }
        }

        [TestMethod]
        public void TestAccountDuplicatesWithFakes1()
        {
            var account1 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account One",
            };
            var account2 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account Two",
            };
            var account3 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account Three",
            };
            var ctx = new XrmFakedContext();
            ctx.Initialize(new List<Entity> { account1, account2, account3 });
            var wfContext = ctx.GetDefaultWorkflowContext();
            wfContext.MessageName = "Create";

            var input = new Dictionary<string, object>();
            input.Add("AccountReference", new EntityReference("account", account1.Id));


            var codeActivity = new DuplicateChecker();
            var result = codeActivity.NumberOfDuplicates(ctx.GetOrganizationService(), "Name1");

            Assert.AreEqual( 0, result);

            result = codeActivity.NumberOfDuplicates(ctx.GetOrganizationService(), "Account One");
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestAccountDuplicatesWithFakes2()
        {
            var account1 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account One",
            };
            var account2 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account Two",
            };
            var account3 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account Three",
            };
            var ctx = new XrmFakedContext();
            ctx.Initialize(new List<Entity> { account1, account2, account3 });
            var wfContext = ctx.GetDefaultWorkflowContext();
            wfContext.MessageName = "Create";
            wfContext.PrimaryEntityId = account1.Id;
            wfContext.PreEntityImages.Add("account",account1);

            var input = new Dictionary<string, object>();
//            input.Add("AccountReference", new EntityReference("account", account1.Id));


            var codeActivity = new DuplicateChecker();
            var result = ctx.ExecuteCodeActivity<DuplicateChecker>(wfContext, input, codeActivity);

            Assert.IsTrue( result.ContainsKey("PossibleMatch"));
            Assert.IsTrue((bool)result["PossibleMatch"]);

            //            Debugger.Break();         
        }
    }
}
