using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DeBeers.Common.FluentEntity;
using DeBeers.Common.Utils;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Zero2Ten.Portal.WCF.API.Model;

namespace DeBeers.CRM.UnitTest.Plugins
{
    [TestClass]
    public class TestPIIEncryptionPlugin
    {
        private string _key = "[Kut~xPE(B\\9*.m*9W\"8@X!M\\a`+6tZv";

        // Test with live system as this is problematic with Fakes.
        [TestMethod]
        public void TestLiveMayDecrypt()
        {
            // Assumes that the test user has "View Confidential Information" privilege, and that the "CRM Service" user does not have this privilege.
            var service = TestUtilities.TestUserService;
            var piiProcessor = new PIIProcessing(_key);
            var myUserId = Guid.Empty;
            FluentSystemUser.CurrentUser(service)
                .WeakExtract((Guid id) => myUserId = id, "systemuserid").Execute();
            Assert.IsFalse(Guid.Empty.Equals(myUserId));

            var otherUserId = Guid.Empty;
            FluentSystemUser.SystemUser(service)
                .Trace(s => Debug.WriteLine(s))
                .Where("fullname").Equals("CRM Service").WeakExtract((Guid id) => otherUserId = id, "systemuserid")
                .Execute();
            Assert.IsFalse(Guid.Empty.Equals(otherUserId));

            var result = piiProcessor.MayDecrypt(myUserId, service);
            Assert.IsTrue(result);

            result = piiProcessor.MayDecrypt(otherUserId, service);
            Assert.IsFalse(result);
        }

        [DataRow("Create", 10, false)]
        [DataRow("Create", 20, true)]
        [DataRow("Create", 40, false)]
        [DataRow("Update", 10, false)]
        [DataRow("Update", 20, true)]
        [DataRow("Update", 40, false)]
        [DataTestMethod]
        public void TestEncryptionPluginCreateUpdateContact(string targetType, int stage, bool isEncryptionExpected)
        {
            var context = new XrmFakedContext();

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "123456",
                ["firstname"] = "Bob",
                //   ["birthdate"] = new DateTime(1964,8,16),
            };
            context.Initialize(target);

            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            var pluginResult = context.ExecutePluginWithTarget(plugin, target, targetType, stage);

            if (isEncryptionExpected)
            {
                Assert.IsTrue(target.Attributes.ContainsKey("deb_idnumber_encrypted"));
                Assert.IsFalse(String.IsNullOrWhiteSpace(target.GetAttributeValue<string>("deb_idnumber_encrypted")));
                Assert.AreEqual("******", target.GetAttributeValue<string>("deb_idnumber"));
            }
            else
            {
                Assert.IsFalse(target.Attributes.ContainsKey("deb_idnumber_encrypted"));
                Assert.AreEqual("123456", target.GetAttributeValue<string>("deb_idnumber"));
            }

            Assert.IsTrue(target.Attributes.ContainsKey("firstname"));
            Assert.AreEqual("Bob", target.GetAttributeValue<string>("firstname"));
        }

        [DataRow("Create", 10, "PersonalID.jpg", false)]
        [DataRow("Create", 20, "PersonalID.jpg", true)]
        [DataRow("Create", 20, "TestFile.jpg", false)]
        [DataRow("Create", 40, "PersonalID.jpg", false)]
        [DataRow("Update", 10, "PersonalID.jpg", false)]
        [DataRow("Update", 20, "PersonalID.jpg", true)]
        [DataRow("Update", 20, "TestFile.jpg", false)]
        [DataRow("Update", 40, "PersonalID.jpg", false)]
        [DataTestMethod]
        public void TestEncryptionPluginCreateUpdateAnnotation(string targetType, int stage, string filename,
            bool isEncryptionExpected)
        {
            var context = new XrmFakedContext();

            var target = new Entity("annotation")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "123456",
                ["documentbody"] = "This is the document body and it isn't very long",
                ["filename"] = filename,
            };
            context.Initialize(target);

            var original = target.Clone();

            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            var pluginResult = context.ExecutePluginWithTarget(plugin, target, targetType, stage);

            if (isEncryptionExpected)
            {
                Assert.AreNotEqual(original.GetAttributeValue<string>("documentbody"),
                    target.GetAttributeValue<string>("documentbody"));
            }
            else
            {
                Assert.AreEqual(original.GetAttributeValue<string>("documentbody"),
                    target.GetAttributeValue<string>("documentbody"));
            }

            Assert.IsFalse(target.Attributes.ContainsKey("documentbody_encrypted"));
            Assert.AreEqual("123456", target.GetAttributeValue<string>("deb_idnumber"));
            Assert.IsFalse(target.Attributes.ContainsKey("deb_idnumber_encrypted"));
            Assert.IsTrue(target.Attributes.ContainsKey("filename"));
            Assert.AreEqual(filename, target.GetAttributeValue<string>("filename"));
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestEncryptionPluginRetrieveContactCols(bool mayDecrypt)
        {
            var context = new XrmFakedContext();
            var stage = 20; // pre-operation¬
            var messageName = "Retrieve";
            var userId = Guid.NewGuid();

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "******",
                ["firstname"] = "Bob",
                ["birthdate"] = new DateTime(1900, 1, 1),
            };
            var mayDecryptTargets = fakedUser(userId, mayDecrypt);
            mayDecryptTargets.Add(target);
            context.Initialize(  mayDecryptTargets);
            var defaultPluginContext = context.GetDefaultPluginContext();
            defaultPluginContext.InputParameters.Add("Target", target.ToEntityReference());
            var colset = new ColumnSet((from k in target.Attributes where k.Key != "Id" select k.Key).ToArray());
            defaultPluginContext.InputParameters.Add("ColumnSet", colset);
            defaultPluginContext.OutputParameters.Add("BusinessEntity", target);
            defaultPluginContext.MessageName = messageName;
            defaultPluginContext.UserId = userId;
            defaultPluginContext.Stage = stage;

            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            // var pluginResult = context.ExecutePluginWithTargetReference( plugin, target.ToEntityReference(), messageName, stage);
            var pluginResult = context.ExecutePluginWith(defaultPluginContext, plugin);

            Assert.IsNotNull(pluginResult);

            Assert.IsTrue(defaultPluginContext.SharedVariables.ContainsKey("MayDecrypt"));
            if (mayDecrypt)
            {
                Assert.AreEqual(12, colset.Columns.Count);
                Assert.IsTrue(colset.Columns.Contains("deb_idnumber_encrypted"));
                Assert.IsTrue(colset.Columns.Contains("deb_birthdate_encrypted"));
                Assert.IsTrue((bool)defaultPluginContext.SharedVariables["MayDecrypt"]);
            }
            else
            {
                Assert.AreEqual(10, colset.Columns.Count);
                Assert.IsFalse(colset.Columns.Contains("deb_idnumber_encrypted"));
                Assert.IsFalse(colset.Columns.Contains("deb_birthdate_encrypted"));
                Assert.IsFalse((bool)defaultPluginContext.SharedVariables["MayDecrypt"]);
            }
        }

        [DataRow("Retrieve", 40, false)]
        [DataRow("Retrieve", 40, true)]
        [DataTestMethod]
        public void TestEncryptionPluginRetrieveContact(string messageName, int stage, bool isDecryptionExpected)
        {
            var context = new XrmFakedContext();

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "*****",
                ["firstname"] = "Bob",
                ["deb_idnumber_encrypted"] = "kDLHAu4bVF/wxGI1zNm3uQ==",
                ["birthdate"] = new DateTime(1900, 1, 1),
                ["deb_birthdate_encrypted"] = "a4R37idAYMfoM2/qHHn3hoLm/V1M/FHMa1PZWkd8+/U="
            };

            var userId = Guid.NewGuid();
            var entitites = fakedUser(userId, isDecryptionExpected);
            entitites.Add(target);
            context.Initialize(entitites);

            var defaultPluginContext = context.GetDefaultPluginContext();
            defaultPluginContext.InputParameters.Add("Target", target.ToEntityReference());
            var colset = new ColumnSet((from k in target.Attributes where k.Key != "Id" select k.Key).ToArray());
            defaultPluginContext.InputParameters.Add("ColumnSet", colset);
            defaultPluginContext.OutputParameters.Add("BusinessEntity", target);
            defaultPluginContext.MessageName = messageName;
            defaultPluginContext.Stage = stage;
            defaultPluginContext.UserId = userId;
            defaultPluginContext.SharedVariables["MayDecrypt"] = isDecryptionExpected;

            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            var pluginResult = context.ExecutePluginWith(defaultPluginContext, plugin);


            Assert.IsNotNull(pluginResult);

            Assert.IsTrue(target.Attributes.Contains("deb_idnumber"));
            Assert.IsTrue(target.Attributes.Contains("birthdate"));

            Assert.IsTrue(target.Attributes.ContainsKey("firstname"));
            Assert.AreEqual("Bob", target.GetAttributeValue<string>("firstname"));

            if (isDecryptionExpected)
            {
                Assert.AreEqual("123456", target.Attributes["deb_idnumber"]);
                Assert.AreEqual(new DateTime(1964, 8, 16), target.Attributes["birthdate"]);
            }
            else
            {
                Assert.AreEqual("*****", target.Attributes["deb_idnumber"]);
                Assert.AreEqual(new DateTime(1900, 1, 1), target.Attributes["birthdate"]);
            }
        }

        /// <summary>
        /// Create a faked user who may (or may not) be allowed to decrypt data.
        /// </summary>
        /// <param name="mayDecrypt"></param>
        /// <returns></returns>
        private List<Entity> fakedUser(Guid userId, bool mayDecrypt)
        {


            var user =
                new Entity("systemuser")
                {
                    Id = userId
                };

            var securityRole = new Entity("role")
            {
                Id = Guid.NewGuid(),
                ["name"] = "View Confidential Information"
            };


            var systemUserRole = new Entity("systemuserroles")
            {
                Id = Guid.NewGuid(),
                ["roleid"] = securityRole.Id,
                ["systemuserid"] = userId
            };

            if (mayDecrypt)
            {
                return new List<Entity> {user, securityRole, systemUserRole};
            }
            else
            {
                return new List<Entity> {user, securityRole};
            }
        }

        [TestMethod]
        public void TestRetrieveMultipleColset()
        {
            var context = new XrmFakedContext();
            var stage = 20; // pre-operation¬
            var messageName = "RetrieveMultiple";

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "******",
                ["firstname"] = "Bob",
                ["birthdate"] = new DateTime(1900, 1, 1),
            };
            var userId = Guid.NewGuid();
            var entitites = fakedUser(userId, true);
            entitites.Add(target);

            context.Initialize(entitites);

            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("deb_idnumber", "firstname", "birthdate"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "firstname",
                            EntityName = "contact",
                            Operator = ConditionOperator.Equal,
                            Values = {"firstname"}
                        }
                    }
                }
            };

            var defaultPluginContext = context.GetDefaultPluginContext();

            defaultPluginContext.InputParameters.Add("Query", query);
            defaultPluginContext.OutputParameters.Add("BusinessEntityCollection", target);
            defaultPluginContext.MessageName = messageName;
            defaultPluginContext.UserId = userId;
            defaultPluginContext.Stage = stage;


            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            // var pluginResult = context.ExecutePluginWithTargetReference( plugin, target.ToEntityReference(), messageName, stage);
            var pluginResult = context.ExecutePluginWith(defaultPluginContext, plugin);

            Assert.IsNotNull(pluginResult);

            var colset = query.ColumnSet;
            Assert.AreEqual(5, colset.Columns.Count);

            Assert.IsTrue(colset.Columns.Contains("deb_idnumber_encrypted"));
            Assert.IsTrue(colset.Columns.Contains("deb_birthdate_encrypted"));
            Assert.IsTrue(defaultPluginContext.SharedVariables.ContainsKey("MayDecrypt"));
            Assert.IsTrue((bool)defaultPluginContext.SharedVariables["MayDecrypt"]);
        }


        [TestMethod]
        public void TestRetrieveMultipleColsetWithFetchExpression()
        {
            var context = new XrmFakedContext();
            var stage = 20; // pre-operation¬
            var messageName = "RetrieveMultiple";

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "******",
                ["firstname"] = "Bob",
                ["birthdate"] = new DateTime(1900, 1, 1),
            };
            context.Initialize(target);

            var fetchQuery = new FetchExpression(@"<fetch top=""50"" >
  <entity name=""contact"" >
    <attribute name=""deb_idnumber"" alias=""deb_idnumber"" />
    <attribute name=""birthdate"" alias=""birthdate"" />
    <attribute name=""firstname"" alias=""firstname"" />
    <filter type=""and"" >
      <condition attribute=""firstname"" operator=""eq"" value=""Bob"" />
    </filter>
  </entity>
</fetch>");


            var defaultPluginContext = context.GetDefaultPluginContext();

            defaultPluginContext.InputParameters.Add("Query", fetchQuery);
            defaultPluginContext.OutputParameters.Add("BusinessEntityCollection", target);
            defaultPluginContext.MessageName = messageName;
            defaultPluginContext.Stage = stage;


            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            // var pluginResult = context.ExecutePluginWithTargetReference( plugin, target.ToEntityReference(), messageName, stage);
            var pluginResult = context.ExecutePluginWith(defaultPluginContext, plugin);

            Assert.IsNotNull(pluginResult);

            Debugger.Break();
            //var colset = fetchQuery. query.ColumnSet;
            //Assert.AreEqual(5, colset.Columns.Count);

            //Assert.IsTrue(colset.Columns.Contains("deb_idnumber_encrypted"));
            //Assert.IsTrue(colset.Columns.Contains("deb_birthdate_encrypted"));
        }

        [DataRow("RetrieveMultiple", 40, false)]
        [DataRow("RetrieveMultiple", 40, true)]
        [DataTestMethod]
        public void TestEncryptionPluginRetrieveMultipleContacts(string messageName, int stage,
            bool isDecryptionExpected)
        {
            var context = new XrmFakedContext();

            var target1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "*****",
                ["firstname"] = "Bob",
                ["deb_idnumber_encrypted"] = "kDLHAu4bVF/wxGI1zNm3uQ==",
                ["birthdate"] = new DateTime(1900, 1, 1),
                ["deb_birthdate_encrypted"] = "a4R37idAYMfoM2/qHHn3hoLm/V1M/FHMa1PZWkd8+/U="
            };

            var target2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Alan",
                ["birthdate"] = new DateTime(1900, 1, 1),
                ["deb_birthdate_encrypted"] = "a4R37idAYMfoM2/qHHn3hoLm/V1M/FHMa1PZWkd8+/U="
            };

            var target = new EntityCollection(new[] {target1, target2});
            var userId = Guid.NewGuid();
            var entitites = fakedUser(userId, isDecryptionExpected);
            entitites.AddRange(target.Entities);
            context.Initialize(entitites);

            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("deb_idnumber", "firstname", "birthdate"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "firstname",
                            EntityName = "contact",
                            Operator = ConditionOperator.Equal,
                            Values = {"firstname"}
                        }
                    }
                }
            };

            var defaultPluginContext = context.GetDefaultPluginContext();
            defaultPluginContext.InputParameters.Add("Query", query);
            defaultPluginContext.OutputParameters.Add("BusinessEntityCollection", target);
            defaultPluginContext.SharedVariables.Add("MayDecrypt", isDecryptionExpected);
            defaultPluginContext.MessageName = messageName;
            defaultPluginContext.Stage = stage;
            defaultPluginContext.UserId = userId;

            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            var pluginResult = context.ExecutePluginWith(defaultPluginContext, plugin);


            Assert.IsNotNull(pluginResult);

            Assert.IsTrue(target1.Attributes.Contains("deb_idnumber"));
            Assert.IsTrue(target1.Attributes.Contains("birthdate"));
            Assert.IsFalse(target2.Attributes.Contains("deb_idnumber"));
            Assert.IsTrue(target2.Attributes.Contains("birthdate"));

            Assert.IsTrue(target1.Attributes.ContainsKey("firstname"));
            Assert.AreEqual("Bob", target1.GetAttributeValue<string>("firstname"));
            Assert.IsTrue(target2.Attributes.ContainsKey("firstname"));
            Assert.AreEqual("Alan", target2.GetAttributeValue<string>("firstname"));

            if (isDecryptionExpected)
            {
                Assert.AreEqual("123456", target1.Attributes["deb_idnumber"]);
                Assert.AreEqual(new DateTime(1964, 8, 16), target1.Attributes["birthdate"]);
                Assert.AreEqual(new DateTime(1964, 8, 16), target2.Attributes["birthdate"]);
            }
            else
            {
                Assert.AreEqual("*****", target1.Attributes["deb_idnumber"]);
                Assert.AreEqual(new DateTime(1900, 1, 1), target1.Attributes["birthdate"]);
                Assert.AreEqual(new DateTime(1900, 1, 1), target2.Attributes["birthdate"]);
            }
        }

        [DataRow("RetrieveMultiple", 40, false)]
        [DataRow("RetrieveMultiple", 40, true)]
        [DataTestMethod]
        public void TestEncryptionPluginRetrieveMultipleContactsWithFetchExpression(string messageName, int stage,
            bool isDecryptionExpected)
        {
            var context = new XrmFakedContext();

            var target1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["deb_idnumber"] = "*****",
                ["firstname"] = "Bob",
                ["deb_idnumber_encrypted"] = "kDLHAu4bVF/wxGI1zNm3uQ==",
                ["birthdate"] = new DateTime(1900, 1, 1),
                ["deb_birthdate_encrypted"] = "a4R37idAYMfoM2/qHHn3hoLm/V1M/FHMa1PZWkd8+/U="
            };

            var target2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Alan",
                ["birthdate"] = new DateTime(1900, 1, 1),
                ["deb_birthdate_encrypted"] = "a4R37idAYMfoM2/qHHn3hoLm/V1M/FHMa1PZWkd8+/U="
            };

            var target = new EntityCollection(new[] {target1, target2});
            var userId = Guid.NewGuid();
            var entitites = fakedUser(userId, isDecryptionExpected);
            entitites.AddRange(target.Entities);
            context.Initialize(entitites);

            var fetchQuery = new FetchExpression(@"<fetch top=""50"" >
  <entity name=""contact"" >
    <attribute name=""deb_idnumber"" alias=""deb_idnumber"" />
    <attribute name=""birthdate"" alias=""birthdate"" />
    <attribute name=""firstname"" alias=""firstname"" />
    <filter type=""and"" >
      <condition attribute=""firstname"" operator=""eq"" value=""Bob"" />
    </filter>
  </entity>
</fetch>");

            var defaultPluginContext = context.GetDefaultPluginContext();
            defaultPluginContext.InputParameters.Add("Query", fetchQuery);
            defaultPluginContext.OutputParameters.Add("BusinessEntityCollection", target);
            defaultPluginContext.SharedVariables["MayDecrypt"] = isDecryptionExpected;
            defaultPluginContext.MessageName = messageName;
            defaultPluginContext.Stage = stage;
            defaultPluginContext.UserId = userId;

            var plugin = new CRM.Plugins.PIIEncryption("", _key);

            var pluginResult = context.ExecutePluginWith(defaultPluginContext, plugin);


            Assert.IsNotNull(pluginResult);

            Assert.IsTrue(target1.Attributes.Contains("deb_idnumber"));
            Assert.IsTrue(target1.Attributes.Contains("birthdate"));
            Assert.IsFalse(target2.Attributes.Contains("deb_idnumber"));
            Assert.IsTrue(target2.Attributes.Contains("birthdate"));

            Assert.IsTrue(target1.Attributes.ContainsKey("firstname"));
            Assert.AreEqual("Bob", target1.GetAttributeValue<string>("firstname"));
            Assert.IsTrue(target2.Attributes.ContainsKey("firstname"));
            Assert.AreEqual("Alan", target2.GetAttributeValue<string>("firstname"));

            if (isDecryptionExpected)
            {
                Assert.AreEqual("123456", target1.Attributes["deb_idnumber"]);
                Assert.AreEqual(new DateTime(1964, 8, 16), target1.Attributes["birthdate"]);
                Assert.AreEqual(new DateTime(1964, 8, 16), target2.Attributes["birthdate"]);
            }
            else
            {
                Assert.AreEqual("*****", target1.Attributes["deb_idnumber"]);
                Assert.AreEqual(new DateTime(1900, 1, 1), target1.Attributes["birthdate"]);
                Assert.AreEqual(new DateTime(1900, 1, 1), target2.Attributes["birthdate"]);
            }
        }


        ///// <summary>
        ///// Setup fake entities for security role that controls decryption.
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <param name="mayDecrypt"></param>
        ///// <returns></returns>
        //private List<Entity> MayDecryptSecurityRoles(Guid userId,  bool mayDecrypt)
        //{

        //    var user = new Entity("systemuser")
        //    {
        //        Id = userId
        //    };

        //    var securityRole = new Entity("role")
        //    {
        //        Id = Guid.NewGuid(),
        //        ["name"] = "View Confidential Information"
        //    };

        //    var systemUserRole = new Entity("systemuserroles")
        //    {
        //        Id = Guid.NewGuid(),
        //        ["roleid"] = securityRole.Id,
        //        ["systemuserid"] = userId
        //    };


        //    if (mayDecrypt)
        //    {
        //        return new List<Entity> {user, securityRole, systemUserRole};
        //    }
        //    else
        //    {
        //        return new List<Entity> {user, securityRole};

        //    }

        //}
    }

}
