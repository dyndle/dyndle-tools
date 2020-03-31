using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Installer.Models;
using Tridion.ContentManager.CoreService.Client;
using ItemType = Dyndle.Tools.Installer.Models.ItemType;

namespace Dyndle.Tools.Installer.Test
{
    public static class ModelFactory
    {

        public static ImportItem CreateImportItem(IdentifiableObjectData ioData)
        {
            switch (ioData)
            {
                case SchemaData data:
                    return CreateSchema(data);
                case TemplateBuildingBlockData data:
                    return CreateTBB(data);
                case PageTemplateData data:
                    return CreatePageTemplate(data);
                case ComponentTemplateData data:
                    return CreateComponentTemplate(data);
                default:
                    return null;
            }
        }

        public static ImportItem CreateSchema(SchemaData schemaData)
        {
            var importItem = new ImportItem()
            {
                ItemType = ItemType.Schema,
                Name = schemaData.Title,
                Namespace = schemaData.NamespaceUri,
                Content = schemaData.Xsd,
                RootElementName =  schemaData.RootElementName,
                SchemaPurpose = schemaData.Purpose.Value,
                SourceId = schemaData.Id

            };
            return importItem;
        }

        public static ImportItem CreateTBB(TemplateBuildingBlockData templateBuildingBlockData)
        {
            var importItem = new ImportItem()
            {
                ItemType = ItemType.TemplateBuildingBlock,
                Name = templateBuildingBlockData.Title,
                Content = templateBuildingBlockData.Content,
                TemplateType = templateBuildingBlockData.TemplateType,
                SourceId = templateBuildingBlockData.Id
            };
            return importItem;
        }

        public static ImportItem CreatePageTemplate(PageTemplateData pageTemplateData)
        {
            var importItem = new ImportItem()
            {
                ItemType = ItemType.PageTemplate,
                Name = pageTemplateData.Title,
                Content = pageTemplateData.Content,
                SourceId = pageTemplateData.Id

            };
            return importItem;
        }
        public static ImportItem CreateComponentTemplate(ComponentTemplateData componentTemplateData)
        {
            var importItem = new ImportItem()
            {
                ItemType = ItemType.ComponentTemplate,
                Name = componentTemplateData.Title,
                Content = componentTemplateData.Content,
                SourceId = componentTemplateData.Id
            };
            return importItem;
        }

    }
}
