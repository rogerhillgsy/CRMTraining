using System;
using System.Collections.Generic;
using System.Diagnostics;
using DeBeers.Common.Utils;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DeBeers.CRM.UnitTest.Regression
{
    [TestClass]
    public class TestPIIENcryption
    {
        private string _key = "[Kut~xPE(B\\9*.m*9W\"8@X!M\\a`+6tZv";

        [TestMethod]
        public void TestMayDecrypt()
        {
            var piiProcessor = new PIIProcessing(_key);
            var userId = Guid.NewGuid();
            
            #region "Setup fake"
            var user = new Entity("systemuser")
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

            var context = new XrmFakedContext();
            context.Initialize(new List<Entity>() { user, securityRole, systemUserRole });
            #endregion

            var result = piiProcessor.MayDecrypt(userId, context.GetOrganizationService());
            Assert.IsTrue(result);

            result = piiProcessor.MayDecrypt(Guid.NewGuid(), context.GetOrganizationService());
            Assert.IsFalse(result);

        }

        [TestMethod]
        public void TestNotEncryptedEntities()
        {
            var piiProcessor = new PIIProcessing(_key);

            var target = new Entity("nonencrypted")
            {
                Id = Guid.NewGuid(),
                ["name"] = "The name",
                ["deb_idnumber"] = "The id",
                ["documentbody"] = "The document body",
                ["date"] = new DateTime(2019,1,25),
                ["name_encrypted"] = "The name encrypted",
                ["deb_idnumber_encrypted"] = "The ID encrypted",
                ["documentbody_encrypted"] = "The docbody encrypted",
                ["date_encrypted"] = new DateTime(1900,1,1),
            };

            var original = target.Clone();
            Entity preImage = null;
            var encrypted = piiProcessor.EncryptEntity(target, preImage, s => Debug.WriteLine(s));

            Assert.IsTrue( original.Id.Equals(encrypted.Id));

            foreach (var attr in original.Attributes)
            {
                Assert.IsTrue(encrypted.Attributes.ContainsKey(attr.Key), $"Missing {attr.Key}");
                Assert.IsTrue( attr.Value.Equals(encrypted.Attributes[attr.Key]));
            }
        }

        [TestMethod]
        public void TestContactEncryption()
        {
            var piiProcessor = new PIIProcessing(_key);

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["name"] = "The name",
                ["deb_idnumber"] = "The id",
                ["documentbody"] = "The document body",
                // ["birthdate"] = new DateTime(2019,1,25),
                ["name_encrypted"] = "The name encrypted",
                ["deb_idnumber_encrypted"] = "The ID encrypted",
                ["documentbody_encrypted"] = "The docbody encrypted",
                ["date_encrypted"] = "",
            };

            var original = target.Clone();
            var preImage = new Entity("contact");
            var encrypted = piiProcessor.EncryptEntity(target, preImage, s => Debug.WriteLine(s));

            Assert.IsTrue( original.Id.Equals(encrypted.Id));

            foreach (var attr in new[] {"name", "documentbody", "name_encrypted", "documentbody_encrypted"})
            {
                Debug.WriteLine($"Attribute: {attr}");
                Assert.IsTrue(encrypted.Attributes.ContainsKey(attr), $"Missing {attr}");
                Assert.IsTrue( original.Attributes[attr].Equals(encrypted.Attributes[attr]));
            }

            Assert.AreEqual("******", encrypted.Attributes["deb_idnumber"]);
            Assert.AreNotEqual("The ID Encrypted", encrypted.Attributes["deb_idnumber_encrypted"]);

            // Now decrypt the encrypted entity.
            var decrypted = piiProcessor.DecryptEntity(target, s => Debug.WriteLine(s));

            // Assert that deb_idnumber (and other fields) should be as they were.
            foreach (var attr in new[] {"name", "documentbody", "name_encrypted", "documentbody_encrypted", "deb_idnumber"})
            {
                Debug.WriteLine($"Attribute: {attr}");
                Assert.IsTrue(encrypted.Attributes.ContainsKey(attr), $"Missing {attr}");
                Assert.IsTrue( original.Attributes[attr].Equals(encrypted.Attributes[attr]));
            }
            Assert.AreNotEqual("The ID Encrypted", encrypted.Attributes["deb_idnumber_encrypted"]);
        }

        [TestMethod]
        public void TestAnnotationEncryption()
        {
            var piiProcessor = new PIIProcessing(_key);

            var target = new Entity("annotation")
            {
                Id = Guid.NewGuid(),
                ["name"] = "The name",
                ["deb_idnumber"] = "The id",
                ["documentbody"] = "The document body can be very very long",
                // ["birthdate"] = new DateTime(2019,1,25),
                ["name_encrypted"] = "The name encrypted",
                ["deb_idnumber_encrypted"] = "The ID encrypted",
                ["documentbody_encrypted"] = "The docbody encrypted",
                ["date_encrypted"] = "",
            };

            var original = target.Clone();
            var preImage = new Entity("annotation")
            {
                ["filename"] = "PersonalID.jpg"
            };
            var encrypted = piiProcessor.EncryptEntity(target, preImage, s => Debug.WriteLine(s));

            Assert.IsTrue( original.Id.Equals(encrypted.Id));

            foreach (var attr in new[] {"name", "deb_idnumber", "name_encrypted", "documentbody_encrypted", "deb_idnumber_encrypted"})
            {
                Debug.WriteLine($"Attribute: {attr}");
                Assert.IsTrue(encrypted.Attributes.ContainsKey(attr), $"Missing {attr}");
                Assert.IsTrue( original.Attributes[attr].Equals(encrypted.Attributes[attr]));
            }

            Assert.AreNotEqual(original.Attributes["documentbody"], encrypted.Attributes["documentbody"]);

            // Now decrypt the encrypted entity.
            var decrypted = piiProcessor.DecryptEntity(target, s => Debug.WriteLine(s));

            // Assert that deb_idnumber (and other fields) should be as they were.
            foreach (var attr in original.Attributes.Keys)
            {
                Debug.WriteLine($"Attribute: {attr}");
                Assert.IsTrue(encrypted.Attributes.ContainsKey(attr), $"Missing {attr}");
                Assert.IsTrue( original.Attributes[attr].Equals(encrypted.Attributes[attr]));
            }

        }

        [TestMethod]
        public void TestDateEncryption()
        {
                        var piiProcessor = new PIIProcessing(_key);

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["name"] = "The name",
                ["deb_idnumber"] = "The id",
                ["documentbody"] = "The document body",
                ["birthdate"] = new DateTime(2019,1,25),
                ["name_encrypted"] = "The name encrypted",
                ["deb_idnumber_encrypted"] = "The ID encrypted",
                ["documentbody_encrypted"] = "The docbody encrypted",
                ["date_encrypted"] = "",
            };

            var original = target.Clone();
            var encrypted = piiProcessor.EncryptEntity(target, null, s => Debug.WriteLine(s));

            Assert.IsTrue( original.Id.Equals(encrypted.Id));

            foreach (var attr in new[] {"name", "documentbody", "name_encrypted", "documentbody_encrypted"})
            {
                Debug.WriteLine($"Attribute: {attr}");
                Assert.IsTrue(encrypted.Attributes.ContainsKey(attr), $"Missing {attr}");
                Assert.IsTrue( original.Attributes[attr].Equals(encrypted.Attributes[attr]));
            }

            Assert.AreEqual("******", encrypted.Attributes["deb_idnumber"]);
            Assert.AreNotEqual("The ID Encrypted", encrypted.Attributes["deb_idnumber_encrypted"]);
            Assert.AreEqual(new DateTime(1900,1,1), encrypted.Attributes["birthdate"]);
            Assert.AreNotEqual("The ID Encrypted", encrypted.Attributes["deb_birthdate_encrypted"]);
            Debug.WriteLine(encrypted.GetAttributeValue<string>("deb_birthdate_encrypted"));

            // Now decrypt the encrypted entity.
            var decrypted = piiProcessor.DecryptEntity(target, s => Debug.WriteLine(s));

            // Assert that deb_idnumber (and other fields) should be as they were.
            foreach (var attr in new[] {"name", "documentbody", "name_encrypted", "documentbody_encrypted", "deb_idnumber", "birthdate"})
            {
                Debug.WriteLine($"Attribute: {attr}");
                Assert.IsTrue(decrypted.Attributes.ContainsKey(attr), $"Missing {attr}");
                if (!original.Attributes[attr].Equals(decrypted.Attributes[attr]))
                {
                    Assert.IsTrue(original.Attributes[attr].Equals(decrypted.Attributes[attr]));
                }
            }
            Assert.AreNotEqual("The ID Encrypted", encrypted.Attributes["deb_birthdate_encrypted"]);
            Assert.AreEqual(decrypted.Attributes["deb_birthdate_encrypted"], encrypted.Attributes["deb_birthdate_encrypted"]);
        }

        [DataRow("PersonalID.jpg", true)]
        [DataRow("PersonalID.png", true)]
        [DataRow("PersonalID.jpeg", true)]
        [DataRow("Personal ID.jpg", false)]
        [DataRow("Primary identification - (798234372647283).jpeg", true)]
        [DataRow("A Primary identification.doc", false)]
        [DataRow("Primary ID.jpg", true)]
        [DataRow("Photo ID.jpg", true)]
        [DataTestMethod]
        public void TestAnnotationFilenameSelection( string filename, bool expectedResult)
        {
            var piiProcessor = new PIIProcessing(_key);

            var target = new Entity("annotation")
            {
                Id = Guid.NewGuid(),
                ["name"] = "The name",
                ["deb_idnumber"] = "The id",
                ["documentbody"] = "The document body can be very very long",
                // ["birthdate"] = new DateTime(2019,1,25),
                ["name_encrypted"] = "The name encrypted",
                ["deb_idnumber_encrypted"] = "The ID encrypted",
                ["documentbody_encrypted"] = "The docbody encrypted",
                ["date_encrypted"] = "",
                ["filename"] = filename
            };

            var original = target.Clone();
            Entity preImage = null;
            var encrypted = piiProcessor.EncryptEntity(target, preImage, s => Debug.WriteLine(s));

            Assert.IsTrue( original.Id.Equals(encrypted.Id));

            if (expectedResult)
                // Documentbody should encrypted
                Assert.IsTrue( original["documentbody"] != encrypted["documentbody"]);
            else
                // Documentbody should be un-encrypted
                Assert.IsFalse( original["documentbody"] == encrypted["documentbody"]);
        }

        [TestMethod]
        public void TestSetupColumnSet()
        {
            var piiProcessor = new PIIProcessing(_key);

            var target = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["name"] = "The name",
                ["deb_idnumber"] = "The id",
                ["documentbody"] = "The document body",
                ["birthdate"] = new DateTime(2019, 1, 25),
                ["name_encrypted"] = "The name encrypted",
                ["deb_idnumber_encrypted"] = "The ID encrypted",
                ["documentbody_encrypted"] = "The docbody encrypted",
                ["date_encrypted"] = "",
            };
            var target2 = new Entity("annotation")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Note",
                ["documentbody"] = "The document body" 
            };

            ColumnSetTest(piiProcessor, target.ToEntityReference(), new[] {"deb_idnumber"}, new[] {"deb_idnumber_encrypted"});
            ColumnSetTest(piiProcessor, target2.ToEntityReference(), new[] {"documentbody"} );
            ColumnSetTest(piiProcessor, target.ToEntityReference(), new[] {"deb_idnumber", "birthdate"}, new[] {"deb_idnumber_encrypted", "deb_birthdate_encrypted"});
            ColumnSetTest(piiProcessor, target2.ToEntityReference(), new []{"name"}, null);

        }

        private void ColumnSetTest( PIIProcessing processor, EntityReference target, string[] initial,  string[] added = null)
        {
            var columnSet = new ColumnSet(initial);
            var expectedColumnSet = new HashSet<string>(initial);
            if (added != null)
            {
                added.ForEach( (a,i) => expectedColumnSet.Add((string) a.GetValue(i)));
            }
            processor.AddAdditionalColumns(target, columnSet, s => Debug.WriteLine(s));
            Assert.AreEqual(expectedColumnSet.Count, columnSet.Columns.Count);

            foreach (var s in expectedColumnSet)
            {
                Assert.IsTrue(columnSet.Columns.Contains(s), $"Columnset did not contain expected value {s}");
            }
            foreach (var s in columnSet.Columns)
            {
                Assert.IsTrue(expectedColumnSet.Contains(s), $"Columnset contained unexpected value {s}");
            }
        }
    }
}
