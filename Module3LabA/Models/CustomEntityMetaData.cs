namespace Module3LabA.Models
{
    public class CustomEntityMetaData
    {

        // Entity MetaData
        public string EntityName { get; set;}
        public string EntityDisplayName { get; set; }
        public string EntityDisplayCollectionName { get; set; }
        public string EntityDescription { get; set; }
        
        // The Metatdata for the Entity's Primary Attribute
        public string PrimaryAttributeSchemaName { get; set; }
        public string PrimaryAttributeDisplayName { get; set; }
        public string PrimaryAttributeDescription { get; set;}
    }
}