using System;
using System.Collections.Generic;
using System.Diagnostics;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CRMTrainingTests
{
    [TestClass]
    public class FakeXrmEasyTest
    {
        [TestMethod]
        public void TestFakeXrmEasy()
        {
            #region "setup"
            var account1 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account1",
                ["name2"] = "Account name 2",
                ["name3"] = "name3",
                ["phone1"] = "123456",
                ["address1_country"] = "UK",
                ["statecode"] = 0
            };

            var account2 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account2",
                ["name2"] = "",
                ["phone1"] = "654321",
                ["address1_country"] = "UK",
                ["statecode"] = 0
            };
            var contact1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["parentcustomerid"] = account1.ToEntityReference(),
                ["telephone1"] = "12345677",
                ["telephone2"] = "23456789",
                ["phone"] = "776543212"
            };
#endregion

            var context = new XrmFakedContext();
            context.Initialize(new List<Entity> {account1, account2, contact1});

            IOrganizationService orgService = context.GetOrganizationService();

            var queryByName = new QueryByAttribute("account");
            queryByName.ColumnSet = new ColumnSet("firstname","lastname", "phone1");
            queryByName.Attributes.AddRange("name");
            queryByName.Values.AddRange("Account1");

            var result = orgService.RetrieveMultiple(queryByName);

            Assert.IsNotNull(result);

            foreach (var account in result.Entities)
            {
                Assert.IsTrue(account.Attributes.ContainsKey("name"));
                Assert.AreEqual("Account1", account.GetAttributeValue<string>("name"));

                Debugger.Break();
            }

        }
    }
}
