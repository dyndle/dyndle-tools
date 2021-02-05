using System;
using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Generator.Models
{
    public enum FieldType { Text, MultiLineText, Xhtml, Keyword, Embedded, MultiMediaLink, ComponentLink, ExternalLink, Number, Date, Entities, Regions, PageTitle, PageId }

    public class PropertyDefinition
    {

        private IDictionary<FieldType, string> field2PropertyTypeMapping = new Dictionary<FieldType, string>();
        private IDictionary<FieldType, string> field2PropertyAttributeMapping = new Dictionary<FieldType, string>();
        public PropertyDefinition(IGeneratorConfiguration configuration)
        {
            field2PropertyTypeMapping.Add(FieldType.Text, "string");
            field2PropertyTypeMapping.Add(FieldType.MultiLineText, "string");
            field2PropertyTypeMapping.Add(FieldType.Xhtml, "System.Web.Mvc.MvcHtmlString");
            field2PropertyTypeMapping.Add(FieldType.Date, "DateTime");
            field2PropertyTypeMapping.Add(FieldType.Number, "double");
            field2PropertyTypeMapping.Add(FieldType.ExternalLink, "string");
            field2PropertyTypeMapping.Add(FieldType.Keyword, "string");
            field2PropertyTypeMapping.Add(FieldType.ComponentLink, configuration.BaseClasses.Any() ? configuration.BaseClasses.FirstOrDefault() : "EntityModel");
            field2PropertyTypeMapping.Add(FieldType.MultiMediaLink, configuration.BaseClassesForMultimedia.Any() ? configuration.BaseClassesForMultimedia.FirstOrDefault() : "EntityModel");
            field2PropertyTypeMapping.Add(FieldType.Entities, "IEntityModel");
            field2PropertyTypeMapping.Add(FieldType.Regions, "IRegionModel");
            field2PropertyTypeMapping.Add(FieldType.PageTitle, "string");
            field2PropertyTypeMapping.Add(FieldType.PageId, "TcmUri");


            field2PropertyAttributeMapping.Add(FieldType.Text, "TextField");
            field2PropertyAttributeMapping.Add(FieldType.MultiLineText, "TextField");
            field2PropertyAttributeMapping.Add(FieldType.Xhtml, "RichTextField");
            field2PropertyAttributeMapping.Add(FieldType.Date, "DateField");
            field2PropertyAttributeMapping.Add(FieldType.Number, "NumberField");
            field2PropertyAttributeMapping.Add(FieldType.ExternalLink, "TextField");
            field2PropertyAttributeMapping.Add(FieldType.Keyword, "KeywordTitleField");
            field2PropertyAttributeMapping.Add(FieldType.ComponentLink, "LinkedComponentField");
            field2PropertyAttributeMapping.Add(FieldType.MultiMediaLink, "LinkedComponentField");
            field2PropertyAttributeMapping.Add(FieldType.Embedded, "EmbeddedSchemaField");
            field2PropertyAttributeMapping.Add(FieldType.Entities, "ComponentPresentations");
            field2PropertyAttributeMapping.Add(FieldType.Regions, "Regions");
            field2PropertyAttributeMapping.Add(FieldType.PageTitle, "PageTitle");
            field2PropertyAttributeMapping.Add(FieldType.PageId, "PageId");
        }

        public bool IsResolved { get; set; }
        public string Name { get; set; }
        public FieldType FieldType { get; set; }
        public string PropertyType
        {
            get
            {
                if (TargetModels != null && TargetModels.Count > 0)
                {
                    return TargetModels.Count > 1 ? field2PropertyTypeMapping[FieldType] : TargetModels[0]; // NOTE: if you have more than one allowed target schema, the property will get the generic 'EntityModel' type 
                }
                if (field2PropertyTypeMapping.ContainsKey(FieldType))
                {
                    return field2PropertyTypeMapping[FieldType];
                }
                return null;
            }
        }

        public bool IsComplexType => FieldType == FieldType.ComponentLink || FieldType == FieldType.MultiMediaLink || FieldType == FieldType.Embedded || FieldType == FieldType.Entities || FieldType == FieldType.Regions;
      

        public string GetPropertyAttribute(string className)
        {
            var propertyAttribute = PropertyAttribute;
            if (className != Name)
            {
                return propertyAttribute;
            }
            if (this.FieldType == FieldType.Entities || this.FieldType == FieldType.Regions)
            {
                return propertyAttribute;
            }
            var fieldNameIdentifier = $"FieldName=\"{Name}\"";

            if (propertyAttribute.EndsWith(")"))
            {
                return propertyAttribute.Substring(0,propertyAttribute.Length - 1) + ", " + fieldNameIdentifier + ")";
            }
            return $"{propertyAttribute}({fieldNameIdentifier})";
        }

        public string PropertyAttribute
        {
            get
            {

                if (field2PropertyAttributeMapping.ContainsKey(FieldType))
                {   
                    string propAttr = field2PropertyAttributeMapping[FieldType];
                    if (FieldType == FieldType.Embedded)
                    {
                        return string.Format("{0}(EmbeddedModelType = typeof({1}){2})", propAttr, this.TargetModels[0], this.IsMetadata ? ",IsMetadata = true" : "");
                    }
                    if (FieldType == FieldType.Keyword)
                    {
                        if (this.TargetModels != null && this.TargetModels.Count > 0)
                        {
                            return "KeywordField" + (this.IsMetadata ? "(IsMetadata = true)" : "");
                        }
                    }
                    if (FieldType == FieldType.ComponentLink)
                    {
                        if (this.TargetModels != null && this.TargetModels.Count > 0)
                        {
                            string targetSchemaTypes = "";
                            foreach (string s in TargetModels)
                            {
                                if (targetSchemaTypes != "")
                                {
                                    targetSchemaTypes += ", ";
                                }
                                targetSchemaTypes = targetSchemaTypes + string.Format("typeof({0})", s);
                            }
                            targetSchemaTypes = "{ " + targetSchemaTypes + " }";
                            return string.Format("{0}(LinkedComponentTypes = new [] {1}{2})", propAttr, targetSchemaTypes, this.IsMetadata ? ",IsMetadata = true" : "");
                        }
                    }
                    return propAttr + (this.IsMetadata ? "(IsMetadata = true)" : "");
                }
                return null;
            }
        }
        public bool IsMandatory { get; set; }
        public bool IsMetadata { get; set; }
        public bool IsMultipleValue { get; set; }
        public List<string> TargetModels { get; set; }
        public List<LinkToSchemaData> TargetSchemas { get; set; }


        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (Name + "/" + PropertyType).GetHashCode();
        }

    }

}
