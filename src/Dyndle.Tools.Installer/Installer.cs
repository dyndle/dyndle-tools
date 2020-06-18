using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.ImportExport;
using Dyndle.Tools.Core.ToolsModules;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Installer.Configuration;
using Newtonsoft.Json;
using Tridion.ContentManager.CoreService.Client;
using Environment = Dyndle.Tools.Core.Models.Environment;
using ItemType = Dyndle.Tools.Core.ImportExport.ItemType;
using TcmItemType = Tridion.ContentManager.CoreService.Client.ItemType;


namespace Dyndle.Tools.Installer
{
    public class Installer : IToolsModule
    {
        public static readonly string ResourceRootPath = "Dyndle.Tools.Installer.Resources";
        public static readonly string DyndleTemplateResourceName = "Dyndle.Templates.merged.dll";
        public static readonly string InstallPackageResourceName = "dyndle-cm-package.zip";

        private SessionAwareCoreServiceClient Client;
        private FolderData DyndleFolder;
        private StructureGroupData DyndleStructureGroup;
        private static readonly ReadOptions DefaultReadOptions = new ReadOptions { LoadFlags = LoadFlags.WebDavUrls };

        private IInstallerConfiguration Configuration { get; set; }
        private List<Reference> mappings;
        public Installer(IInstallerConfiguration configuration)
        {
            Configuration = configuration;
            DefaultConfigurationSetter.ApplyDefaults(configuration);
            Client = CoreserviceClientFactory.GetClient();
            DyndleFolder = Client.Read(configuration.DyndleFolder, DefaultReadOptions) as FolderData;
            DyndleStructureGroup = Client.Read(configuration.DyndleStructureGroup, DefaultReadOptions) as StructureGroupData;
        }

        public string Run()
        {

            var env = EnvironmentManager.Get(Configuration.Environment);
            if (env == null)
            {
                return "you must create an environment before you can generate models or views - try dyndle help add-environment";
            }

            if (string.IsNullOrEmpty(Configuration.InstallPackagePath))
            {
                // no install package path configured, we will get the embedded package from the assembly

                ResourceUtils.StoreResourceOnDisk(ResourceRootPath + "." + InstallPackageResourceName, Configuration.WorkFolder, InstallPackageResourceName);
                var packageDir = Path.Combine(Configuration.WorkFolder.FullName, "package");
                Directory.CreateDirectory(packageDir);
                ZipFile.ExtractToDirectory(Path.Combine(Configuration.WorkFolder.FullName, InstallPackageResourceName), packageDir);
                StorageFactory.SetLocation(packageDir);
            }
            else
            {
                StorageFactory.SetLocation(Configuration.InstallPackagePath);
            }


            // todo: read zipped import package from embedded resource and unzip
            var importItems = StorageFactory.GetImportItems();
            var references = StorageFactory.GetReferences();

            mappings = new List<Reference>();

            int stopper = 10;
            while (mappings.Count() < importItems.Count() && stopper > 0)
            {
                var itemsWithoutDependencies = importItems.Where(i =>
                    (mappings.All(m => m.From != i.SourceId)) && references.All(r => r.From != i.SourceId));
                foreach (var importItem in itemsWithoutDependencies)
                {
                    var id = Import(importItem);
                    importItem.TargetId = id;
                    mappings.Add(new Reference(importItem.SourceId, importItem.TargetId));
                }
                var referencesToRemove = new List<Reference>();
                foreach (var reference in references.Where(r => mappings.Any(m => m.From == r.To)))
                {
                    // get 'from item'
                    var fromItem = importItems.FirstOrDefault(i => i.SourceId == reference.From && i.TargetId == null);
                    // get mapping of the 'to item'
                    var referencedMapping = mappings.FirstOrDefault(m => m.From == reference.To);
                    if (fromItem != null)
                    {
                        fromItem.Content = fromItem.Content.Replace(referencedMapping.From.ToPublicationId(fromItem.SourceId), referencedMapping.To.ToPublicationId(fromItem.SourceId));
                        if (!string.IsNullOrEmpty(fromItem.PageTemplateId) && fromItem.PageTemplateId.ToItemId() == referencedMapping.From.ToItemId())
                        {
                            fromItem.PageTemplateId = referencedMapping.To.ToPublicationId(referencedMapping.To);
                        }
                        if (!string.IsNullOrEmpty(fromItem.ParameterSchemaId) && fromItem.ParameterSchemaId.ToItemId() == referencedMapping.From.ToItemId())
                        {
                            fromItem.ParameterSchemaId = referencedMapping.To.ToPublicationId(referencedMapping.To);
                        }
                        referencesToRemove.Add(reference);
                    }
                }

                foreach (var reference in referencesToRemove)
                {
                    references.Remove(reference);
                }

                stopper--;
            }
            return $"Dyndle imported {mappings.Count()} items into Tridion. Please publish the pages in structure group {Configuration.DyndleStructureGroup}.";
        }

        private (TcmItemType tcmItemType, string) GetItemTypeData(ItemType itemType) 
        {
            switch (itemType) 
            {
                case ItemType.Component:
                    return (TcmItemType.Component, ".xml");
                case ItemType.ComponentTemplate:
                    return (TcmItemType.ComponentTemplate, ".tctcmp");
                case ItemType.Page:
                    return (TcmItemType.Page, ".tpg");
                case ItemType.PageTemplate:
                    return (TcmItemType.PageTemplate, ".tptcmp");
                case ItemType.Schema:
                    return (TcmItemType.Schema, ".xsd");
                case ItemType.TemplateBuildingBlock:
                    return (TcmItemType.TemplateBuildingBlock, ".tbbcmp");
                default:
                    return (TcmItemType.UnknownByClient, null);
            }
        }

        private T GetOrCreateNew<T>(ImportItem importItem) where T : VersionedItemData
        {
            var (tcmItemType, fileExtension) = GetItemTypeData(importItem.ItemType);
            if(importItem.IsDyndleMergedDll) 
            {
                fileExtension = ".tbbasm";
            }
            string basepath;
            string organizationalItemURI;
            if(ItemType.Page.Equals(importItem.ItemType)) 
            {
                basepath = DyndleStructureGroup.LocationInfo.WebDavUrl;
                organizationalItemURI = DyndleStructureGroup.Id;
            }
            else 
            {
                basepath = DyndleFolder.LocationInfo.WebDavUrl;
                organizationalItemURI = DyndleFolder.Id;
            }
            string webdavURL = $"{basepath}/{importItem.Name}{fileExtension}";

            T itemData = default(T);
            if(Client.IsExistingObject(webdavURL))
            {
                itemData = Client.CheckOut(webdavURL, true, DefaultReadOptions) as T;
            }
            else 
            {
                itemData = Client.GetDefaultData(tcmItemType, organizationalItemURI, DefaultReadOptions) as T;
            }
            return itemData;
        }

        private string Import(ImportItem importItem)
        {
            switch (importItem.ItemType)
            {
                case ItemType.Schema:
                    return ImportSchema(importItem);
                case ItemType.TemplateBuildingBlock:
                    return ImportTBB(importItem);
                case ItemType.PageTemplate:
                    return ImportPageTemplate(importItem);
                case ItemType.ComponentTemplate:
                    return ImportComponentTemplate(importItem);
                case ItemType.Page:
                    return ImportPage(importItem);
            }
            throw new Exception("unhandled item type " + importItem.ItemType);
        }

        private string ImportSchema(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);

            var schemaData = GetOrCreateNew<SchemaData>(importItem);

            schemaData.Title = importItem.Name;
            schemaData.NamespaceUri = importItem.Namespace;
            schemaData.RootElementName = importItem.RootElementName;
            schemaData.Xsd = importItem.Content;
            schemaData.Purpose = importItem.SchemaPurpose;
            schemaData.Description = importItem.Name;

            schemaData = (SchemaData)Client.Save(schemaData, new ReadOptions());
            schemaData = (SchemaData)Client.CheckIn(schemaData.Id, true, "Dyndle installer", new ReadOptions());

            return schemaData.Id;
        }

        private string ImportTBB(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);
            var templateBuildingBlockData = GetOrCreateNew<TemplateBuildingBlockData>(importItem);

            templateBuildingBlockData.Title = importItem.Name;
            templateBuildingBlockData.TemplateType = importItem.TemplateType;

            if (importItem.IsDyndleMergedDll)
            {
                // read the merged dll embedded resource and store it in the workfolder
                ResourceUtils.StoreResourceOnDisk(ResourceRootPath + "." + DyndleTemplateResourceName, Configuration.WorkFolder, DyndleTemplateResourceName);

                // get an access token (which is simply the current user
                var accessToken = Client.GetCurrentUser();

                // first, upload the merged DLL with the upload client
                string pathOnServer = null;
                using (var uploadClient = CoreserviceClientFactory.GetUploadClient())
                {
                    using (FileStream stream =
                        new FileStream(Path.Combine(Configuration.WorkFolder.FullName, DyndleTemplateResourceName),
                            FileMode.Open))
                    {
                        pathOnServer = uploadClient.UploadBinaryContent(accessToken, stream);
                    }
                }

                // if all went well, we now have the path of the DLL on the server

                if (pathOnServer == null)
                {
                    throw new Exception("unable to upload file");
                }

                templateBuildingBlockData.BinaryContent = new BinaryContentData();
                templateBuildingBlockData.BinaryContent.Filename = DyndleTemplateResourceName;
                templateBuildingBlockData.BinaryContent.UploadFromFile = pathOnServer;
            }
            else
            {
                templateBuildingBlockData.Content = importItem.Content;
            }

            if (!string.IsNullOrEmpty(importItem.ParameterSchemaId))
            {
                templateBuildingBlockData.ParameterSchema = new LinkToSchemaData() { IdRef = importItem.ParameterSchemaId };
            }

            templateBuildingBlockData = (TemplateBuildingBlockData)Client.Save(templateBuildingBlockData, new ReadOptions());
            templateBuildingBlockData = (TemplateBuildingBlockData)Client.CheckIn(templateBuildingBlockData.Id, true, "Dyndle installer", new ReadOptions());

            return templateBuildingBlockData.Id;
        }

        private string ImportPage(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleStructureGroup);
            var pageData = GetOrCreateNew<PageData>(importItem);

            pageData.Title = importItem.Name;           
            pageData.ComponentPresentations = JsonConvert.DeserializeObject<ComponentPresentationData[]>(importItem.Content);
            pageData.PageTemplate = new LinkToPageTemplateData() { IdRef = importItem.PageTemplateId };
            pageData.FileName = importItem.Filename;
            pageData.IsPageTemplateInherited = false;

            pageData = (PageData)Client.Save(pageData, new ReadOptions());
            pageData = (PageData)Client.CheckIn(pageData.Id, true, "Dyndle installer", new ReadOptions());

            return pageData.Id;
        }

        private string ImportPageTemplate(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);
            var pageTemplateData = GetOrCreateNew<PageTemplateData>(importItem);

            pageTemplateData.Title = importItem.Name;
            pageTemplateData.Content = importItem.Content;

            pageTemplateData = (PageTemplateData)Client.Save(pageTemplateData, new ReadOptions());
            pageTemplateData = (PageTemplateData)Client.CheckIn(pageTemplateData.Id, true, "Dyndle installer", new ReadOptions());

            return pageTemplateData.Id;
        }

        private string ImportComponentTemplate(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);

            var componentTemplateData = GetOrCreateNew<ComponentTemplateData>(importItem);

            componentTemplateData.Title = importItem.Name;
            componentTemplateData.Content = importItem.Content;

            componentTemplateData = (ComponentTemplateData)Client.Save(componentTemplateData, new ReadOptions());
            componentTemplateData = (ComponentTemplateData)Client.CheckIn(componentTemplateData.Id, true, "Dyndle installer", new ReadOptions());

            return componentTemplateData.Id;
        }
    }
}
