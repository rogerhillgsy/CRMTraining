using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.IO;
using System.Net;
using CRMTraining2.Plugins;

namespace AddressResolver
{
    public class AddressFromZipCodePlugin : PluginBase
    {

        public AddressFromZipCodePlugin(string unsecure, string secure) : base(typeof(AddressFromZipCodePlugin))
        {

        }


        protected override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            var context = localcontext.PluginExecutionContext;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity contact = (Entity)context.InputParameters["Target"];


                // Verify that the target entity represents a contact
                // If not, this plug-in was not registered correctly.
                if (contact.LogicalName != "contact")
                {
                    return;
                }
                else
                {
                    ResolveAddressIfNecessary(contact, localcontext);

                }


            }
        }

        private void ResolveAddressIfNecessary(Entity contact, LocalPluginContext localcontext)
        {

            if (contact.Attributes.Contains("address1_postalcode")
                            && !contact.Attributes.Contains("address1_city"))
            {

                string strZipcode = contact["address1_postalcode"].ToString();

                try
                {


                    var httpClient = HttpWebRequest.Create("http://webservicex.net/uszip.asmx/GetInfoByZIP?USZip="
                                                                + strZipcode);
                    var response = httpClient.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string xmlData = reader.ReadToEnd();
                    int beginLocation = xmlData.IndexOf("<CITY>");
                    int endLocation = xmlData.IndexOf("</CITY>");
                    string city = xmlData.Substring(beginLocation + 6, endLocation - beginLocation - 6);
                    beginLocation = xmlData.IndexOf("<STATE>");
                    endLocation = xmlData.IndexOf("</STATE>");
                    string state = xmlData.Substring(beginLocation + 7, endLocation - beginLocation - 7);


                    contact["address1_city"] = city;
                    contact["address1_stateorprovince"] = state;
                    localcontext.OrganizationService.Update(contact);

                }
                catch (Exception ex)
                {
                    localcontext.TracingService.Trace("Exception {0}", ex.Message);
                    localcontext.TracingService.Trace("Exception all : {0}", ex.ToString());
                }
            }
        }



    }
}