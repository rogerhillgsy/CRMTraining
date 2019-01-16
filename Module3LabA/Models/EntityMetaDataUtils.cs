using System.Configuration;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.Connector.Model;
using emd = Microsoft.Xrm.Sdk.Metadata;

namespace Module3LabA.Models
{
    public class EntityMetaDataUtils
    {

        string cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;

        const string SchemaPrefix = "new_";
        private emd.EntityMetadata BankAccountEntity;

        public void CreateEntity(CustomEntityMetaData newEntityMetaData)
        {

            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var CreateRequest = new CreateEntityRequest
                {
                    Entity = new emd.EntityMetadata
                    {
                        SchemaName = (SchemaPrefix + newEntityMetaData.PrimaryAttributeSchemaName).ToLower(),
                        DisplayName = new Label(newEntityMetaData.EntityDisplayName, 1033),
                        DisplayCollectionName = new Label(newEntityMetaData.EntityDisplayCollectionName, 1033),
                        Description = new Label("New entity created via code", 1033),
                        OwnershipType = emd.OwnershipTypes.UserOwned,
                        IsActivity = false,
                    },
                    PrimaryAttribute = new emd.StringAttributeMetadata
                    {
                        SchemaName = (SchemaPrefix + newEntityMetaData.PrimaryAttributeSchemaName).ToLower(),
                        RequiredLevel = new emd.AttributeRequiredLevelManagedProperty(emd.AttributeRequiredLevel.None),
                        MaxLength = 100,
                        FormatName = emd.StringFormatName.Text,
                        DisplayName = new Label(newEntityMetaData.PrimaryAttributeDisplayName, 1033),
                        Description = new Label("The primary created via code", 1033)
                    }
                };

                var result = crmSvc.OrganizationServiceProxy.Execute(CreateRequest);

              
            }




        }


        public void UpdateEntity(string entityName, AttributeMetaData attributeMetaData)
        {
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = (SchemaPrefix + entityName).ToLower(),
                    Attribute = new emd.StringAttributeMetadata
                    {
                        SchemaName = (SchemaPrefix + attributeMetaData.SchemaName).ToLower(),
                        RequiredLevel = new emd.AttributeRequiredLevelManagedProperty(emd.AttributeRequiredLevel.None),
                        MaxLength = 100,
                        FormatName =  emd.StringFormatName.Text,
                        DisplayName = new Label(attributeMetaData.DisplayName, 1033),
                        Description =  new Label(attributeMetaData.Description, 1033)
                    }
                };

                var result = crmSvc.OrganizationServiceProxy.Execute(createAttributeRequest);

                var pub = new PublishAllXmlRequest();
                crmSvc.OrganizationServiceProxy.Execute(pub);
            }
        }
            
       
    }
}