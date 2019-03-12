using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DeBeers.Common.FluentEntity;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using WebGrease.Css.Extensions;

namespace DeBeers.Common.Utils
{
    public class PIIProcessing
    {
        private byte[] _encryptionKey = {};
        // Prefix to be added to attribute names if not already present
        private const string EncryptedAttributePrefix = "deb_"; 
        private const string EncryptedAttributeSuffix = "_encrypted";
        private const string SecurityRoleToDecrypt = "View Confidential Information";
        private static string _salt = "aEVk9L,?`Qb$;8cs";
        private Regex filesToEncryptRegex = null;

        public PIIProcessing(string encryptionKey)
        {
            if (!string.IsNullOrEmpty(encryptionKey))
            {
                _encryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
            }

            filesToEncryptRegex = new Regex( string.Join(  "|" ,annotationFilesToEncrypt));
        }

        /// <summary>
        /// Config data  - a dictionary of  { [entity logical name], { { [Attribute Name], [Encrypted Attribute Name] }...}...
        /// Alternative to encrypt in place is to encrypt to a separate attribute with an _encrypted suffix
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> config = new Dictionary<string, Dictionary<string,string>>
        {
            {"contact", new Dictionary<string, string>
            {
                {"deb_idnumber", "deb_idnumber_encrypted"},
                {"birthdate", "deb_birthdate_encrypted"}
            }},
            {
                "annotation", new Dictionary<string, string>
                {
                    {"documentbody", "documentbody" },
                    {"filename", "filename"}  // We don't actually encrypt the filename, but doing this ensures that the value of filename is
                                                    // available in the target when we try to decrypt.
                }
            }
        };

        private HashSet<string> annotationFilesToEncrypt = new HashSet<string>
        {
            "^PersonalID\\..*",
            "^Primary Identification.*",
            "^Primary ID.*",
            "^Photo ID.*"
        };

        /// <summary>
        /// Attempt to encrypt the fields in the entity that may require encryption
        /// </summary>
        /// <param name="target">Target entity</param>
        /// <param name="trace">tracing function</param>
        /// <returns></returns>
        public Entity EncryptEntity(Entity target, Entity preImage, Action<string> trace)
        {
            if (config.ContainsKey(target.LogicalName))
            {
                trace("Attempting to encrypt entity");
                var attributesToEncrypt = config[target.LogicalName];
                attributesToEncrypt.Where(s => s.Key != "filename").ForEach((a, i) =>
                {
                    trace($"Attempting to encrypt attribute {a.Key} to {a.Value}");
                    // Check that entity contains the attribute to be encrypted
                    if (target.Attributes.ContainsKey(a.Key))
                    {
                        // Check that if this is encrypt-in-place that the filename is one that needs encryption.               
                        if ( a.Key == a.Value)
                        {
                            string filename = target.Attributes.ContainsKey("filename") ? target.GetAttributeValue<string>("filename") :
                                               ( (preImage?.Attributes.ContainsKey("filename") ?? false )? preImage.GetAttributeValue<string>("filename") : string.Empty);
                            // Encrypt in place
                            if (!string.IsNullOrWhiteSpace(filename))
                            {
                                if (filesToEncryptRegex.IsMatch(filename))
                                {
                                    if (!IsEncrypted(target[a.Key], trace)) {
                                        trace($"Filename {filename} Encrypting in-place attribute {a.Key}");
                                        var cipherText = Encrypt(target[a.Key]);
                                        target[a.Key] = cipherText;
                                    }
                                    else
                                    {
                                        trace($"Attribute {a.Key} is already encrypted - not re-encrypting.");
                                    }
                                }
                                else
                                {
                                    trace($"Filename {filename} doe not require encryption");
                                }
                            }
                            else
                            {
                                trace($"Did not find source attribute {a.Key}");
                            }
                        }
                        else
                        {
                            trace($"Encrypting attribute {a.Key}");
                            var cipherText = Encrypt(target[a.Key]);

                            // Save to encrypted attribute
                            var encryptedAttribute = a.Value;
                            trace($"Writing encrypted value to: {encryptedAttribute}");
                            target[encryptedAttribute] = cipherText;
                            target[a.Key] = GetBlankString(target[a.Key].GetType());
                        }
                    }
                });
            }

            return target;
        }

        /// <summary>
        /// Ensure that when we try to read encrypted columns we read both the clear text and ciphertext.
        /// (We need to add the ciphertext to the list of columns to retreive, so that when we try to decrypt it late rin the pipeline, it is available.)x
        /// </summary>
        /// <param name="target"></param>
        /// <param name="colSet"></param>
        /// <param name="trace"></param>
        public void AddAdditionalColumns(EntityReference target, ColumnSet colSet, Action<string> trace)
        {
            if (config.ContainsKey(target.LogicalName))
            {
                config[target.LogicalName].ForEach((a, i) =>
                {
                    if (colSet.Columns.Contains(a.Key) && a.Key != a.Value)
                    {
                        colSet.AddColumn(a.Value);
                    }
                });
            }
        }

        public Entity DecryptEntity(Entity target, Action<string> trace)
        {
            if (config.ContainsKey(target.LogicalName))
            {
                trace("Attempting to decrypt entity");
                var attributesToDecrypt = config[target.LogicalName];
                attributesToDecrypt.ForEach((a, i) =>
                    {
                        // ( determine if it is encrypt in place or separate attribute
                        var sourceAttribute = a.Value;

                        if (target.Attributes.ContainsKey(sourceAttribute))
                        {
                            trace($"Target entity contains source attribute: {sourceAttribute}");
                            if (IsEncrypted(target[sourceAttribute], trace))
                            {
                                trace($"Decrypting attribute {a.Key}");
                                var clearText = Decrypt(target.GetAttributeValue<string>(sourceAttribute));

                                // Write decrypted value back to target
                                if (target[a.Key] is DateTime )
                                {
                                    target[a.Key] = DateTime.Parse(clearText);
                                }
                                else
                                {
                                    target[a.Key] = clearText;
                                }

                                target["wasEncrypted"] = true; // Let end user know that there was encrypted data  - used to check on encryption of data.
                            }
                            else
                            {
                                trace($"Target entity is not encrypted - skipping");
                            }
                        }
                        else
                        {
                            trace($"Target did not contain attribute to be decrypted: {sourceAttribute}");
                        }
                    }
                );
            }

            return target;
        }

        private bool IsEncrypted(object a, Action<string> trace)
        {
            var isEncrypted = false;
            try
            {
                var cipherText = a.ToString();
                var cipherTextBytes = Convert.FromBase64String(cipherText);
                var clearText = cipherTextBytes.DecryptString(_salt, _encryptionKey);
                isEncrypted = true;
            }
            catch (FormatException e)
            {
                // trace($"Caught failed decryption : {e.Message}");
            }
            catch (CryptographicException ex)
            {

            }

            return isEncrypted;
        }

        private string Encrypt(object o)
        {
            var clearText = o.ToString();
            var cipherTextBytes = clearText.EncryptString(_salt, _encryptionKey);
            return Convert.ToBase64String(cipherTextBytes);
        }

        private string Decrypt(string a)
        {
            var cipherText = (string) a;
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var clearText = cipherTextBytes.DecryptString(_salt, _encryptionKey);
            return clearText;
        }

        /// <summary>
        ///  Determine if the user has the required "View Confidential Information" security role
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool MayDecrypt(Guid userId, IOrganizationService service, Action<string> trace = null)
        {
            var mayDecrypt = false;
            try
            {
                FluentSecurityRole.SecurityRole(service)
                    .Trace((s) => Debug.WriteLine(s))
                    // .TraceFetchXML()
                    .Where("name").Equals(SecurityRoleToDecrypt)
                    .Join<FluentSystemUserRoles>(sur => sur.Join<FluentSystemUser>(
                        su => su.WeakExtract((Guid id) =>
                        {
                            if (userId.Equals(id))
                            {
                                mayDecrypt = true;
                            }
                        }, "systemuserid"))
                    )
                    .Execute();
            }
            catch (Exception ex)
            {
                trace?.Invoke($"Exception getting security role info: {ex.Message}");
            }

            return mayDecrypt;
        }

        private object GetBlankString(Type t)
        {
            if (t.Name == "String")
            {
                return "******";
            }

            if (t.Name == "DateTime")
            {
                return new DateTime(1900,1,1);
            }

            return null;
        }
    }
}
