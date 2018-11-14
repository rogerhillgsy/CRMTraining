using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows.Forms;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;

namespace Module2LabA
{
    public partial class Form1 : Form
    {
        private ClientCredentials userCred;
        private ClientCredentials deviceCred;

        public Form1()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            userCred = new ClientCredentials();
            userCred.UserName.UserName = "roger@rbh2.onmicrosoft.com";
            userCred.UserName.Password = "Password1";

            deviceCred = new ClientCredentials();
            deviceCred.UserName.UserName = "11CRMTrainingPC";
            deviceCred.UserName.Password = "Password1";

            var discoUrl = "https://disco.crm11.dynamics.com/XRMServices/2011/Discovery.svc";

            using(var serviceProxy = new DiscoveryServiceProxy(new Uri(discoUrl), null, userCred, deviceCred))
            {
                var orgProxy = GetInfo(serviceProxy);

                MessageBox.Show(orgProxy.ClientCredentials.UserName.UserName);

                var crmServiceClient = CreateOrgServiceClient();
                MessageBox.Show(crmServiceClient.OrganizationServiceProxy.ClientCredentials.UserName.UserName);
                crmServiceClient.Dispose();
            }
        }

        private OrganizationServiceProxy GetInfo(DiscoveryServiceProxy serviceProxy)
        {
            var orgsRequest = new RetrieveOrganizationsRequest
            {
                AccessType = EndpointAccessType.Default,
                Release = OrganizationRelease.Current
            };
            var orgs = (RetrieveOrganizationsResponse)serviceProxy.Execute(orgsRequest);

            foreach (var endpoint in  orgs.Details[0].Endpoints)
            {
                listBox1.Items.Add($"  Name: {endpoint.Key}");
                listBox1.Items.Add($"  URL: {endpoint.Value}");

                if (endpoint.Key == EndpointType.OrganizationService)
                {
                    var orgProxy = new OrganizationServiceProxy(new Uri(endpoint.Value), null, userCred, deviceCred );
                    return orgProxy;
                }
            }

            return null;

        }

        private CrmServiceClient CreateOrgServiceClient()
        {
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            return new CrmServiceClient(cnString);
        }
    }
}
