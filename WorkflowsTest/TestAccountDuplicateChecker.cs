using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeXrmEasy;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;
using System.Diagnostics;
using CRMTraining2.Workflows;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace WorkflowsTest
{
    [TestClass]
    public class TestAccountDuplicateChecker
    {
        string cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;

        [TestMethod]
        public void TestAccountDuplicates1()
        {
            using (var ctx = new CrmServiceClient(cnString))
            {

                var accountId = new Guid("DE005CDB-29E7-E811-A96E-0022480186C3");

                var codeActivity = new AccountDuplicateChecker2();
                var result =  codeActivity.DoActualWork(accountId,  ctx.OrganizationServiceProxy, s => Console.WriteLine(s));

            }
        }

        [TestMethod]
        public void TestAccountDuplicatesWithFakes()
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
            ctx.Initialize( new List<Entity>{ account1,account2,account3});
            var wfContext = ctx.GetDefaultWorkflowContext();
            wfContext.MessageName = "Create";

            var input = new Dictionary<string, object>(); 
            input.Add("AccountReference", new EntityReference("account", account1.Id));
            

            var codeActivity = new AccountDuplicateChecker();
            var result = ctx.ExecuteCodeActivity<AccountDuplicateChecker>(wfContext, input, codeActivity);

            Debugger.Break();
            ;

        }
    }
}
