using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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


namespace Dyndle.Tools.Installer
{
    public class Installer : IToolsModule
    {
        public static readonly string ResourceRootPath = "Dyndle.Tools.Installer.Resources";
        public static readonly string DyndleTemplateResourceName = "Dyndle.Templates.merged.dll";
        public static readonly string InstallPackageResourceName = "dyndle-cm-package.zip";
        public static readonly string TcmUploadAssemblyResourceName = "TcmUploadAssembly.exe";
        private SessionAwareCoreServiceClient Client;

        private IInstallerConfiguration Configuration { get; set; }
        private List<Reference> mappings;
        public Installer(IInstallerConfiguration configuration)
        {
            Configuration = configuration;
            DefaultConfigurationSetter.ApplyDefaults(configuration);
            Client = CoreserviceClientFactory.GetClient();

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
                        if (!string.IsNullOrEmpty(fromItem.PageTemplateId))
                        {
                            fromItem.PageTemplateId = referencedMapping.To.ToPublicationId(fromItem.SourceId);
                        }
                        if (!string.IsNullOrEmpty(fromItem.ParameterSchemaId))
                        {
                            fromItem.ParameterSchemaId = referencedMapping.To.ToPublicationId(fromItem.SourceId);
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
            var schemaData = (SchemaData)Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.Schema,
                Configuration.DyndleFolder, new ReadOptions());

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
            var templateBuildingBlockData = (TemplateBuildingBlockData)Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.TemplateBuildingBlock,
                Configuration.DyndleFolder, new ReadOptions());

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
                var mapping = mappings.FirstOrDefault(m => m.From == importItem.ParameterSchemaId);
                if (mapping != null)
                {
                    templateBuildingBlockData.ParameterSchema = new LinkToSchemaData() { IdRef = mapping.To };
                }
            }

            templateBuildingBlockData = (TemplateBuildingBlockData)Client.Save(templateBuildingBlockData, new ReadOptions());
            templateBuildingBlockData = (TemplateBuildingBlockData)Client.CheckIn(templateBuildingBlockData.Id, true, "Dyndle installer", new ReadOptions());

            return templateBuildingBlockData.Id;
        }

        private string ImportPage(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);
            var sgUri = Configuration.DyndleStructureGroup;
            var pageData = (PageData)Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.Page, sgUri, new ReadOptions());
            pageData.Title = importItem.Name;
           
            pageData.ComponentPresentations = JsonConvert.DeserializeObject<ComponentPresentationData[]>(importItem.Content);
            pageData.PageTemplate = new LinkToPageTemplateData() { IdRef = importItem.PageTemplateId.ToPublicationId(sgUri) };
            pageData.FileName = importItem.Filename;
            pageData.IsPageTemplateInherited = false;

            pageData = (PageData)Client.Save(pageData, new ReadOptions());
            pageData = (PageData)Client.CheckIn(pageData.Id, true, "Dyndle installer", new ReadOptions());

            return pageData.Id;
        }

        private string ImportPageTemplate(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);
            var pageTemplateData = (PageTemplateData)Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.PageTemplate,
                Configuration.DyndleFolder,
                new ReadOptions());

            pageTemplateData.Title = importItem.Name;
            pageTemplateData.Content = importItem.Content;

            pageTemplateData = (PageTemplateData)Client.Save(pageTemplateData, new ReadOptions());
            pageTemplateData = (PageTemplateData)Client.CheckIn(pageTemplateData.Id, true, "Dyndle installer", new ReadOptions());

            return pageTemplateData.Id;
        }

        private string ImportComponentTemplate(ImportItem importItem)
        {
            importItem.FixPublicationContext(Configuration.DyndleFolder);
            var componentTemplateData = (ComponentTemplateData)Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.ComponentTemplate,
                Configuration.DyndleFolder,
                new ReadOptions());

            componentTemplateData.Title = importItem.Name;
            componentTemplateData.Content = importItem.Content;

            componentTemplateData = (ComponentTemplateData)Client.Save(componentTemplateData, new ReadOptions());
            componentTemplateData = (ComponentTemplateData)Client.CheckIn(componentTemplateData.Id, true, "Dyndle installer", new ReadOptions());

            return componentTemplateData.Id;
        }


    
    }
}
