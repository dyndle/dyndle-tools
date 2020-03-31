using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.ToolsModules;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Environments;
using Dyndle.Tools.Installer.Configuration;
using Dyndle.Tools.Installer.Models;
using Newtonsoft.Json;
using Tridion.ContentManager.CoreService.Client;
using Environment = Dyndle.Tools.Core.Models.Environment;
using ItemType = Dyndle.Tools.Installer.Models.ItemType;


namespace Dyndle.Tools.Environments
{
    public class Installer : IToolsModule
    {
        public static readonly string ResourceRootPath = "Dyndle.Tools.Installer.Resources";
        public static readonly string DyndleTemplateResourceName = "Dyndle.Templates.merged.dll";
        public static readonly string TcmUploadAssemblyResourceName = "TcmUploadAssembly.exe";
        private SessionAwareCoreServiceClient Client;

        private IInstallerConfiguration Configuration { get; set; }
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

            //// copy resources to the work folder
            //ResourceUtils.StoreResourceOnDisk(ResourceRootPath + "." + DyndleTemplateResourceName, Configuration.WorkFolder, DyndleTemplateResourceName);
            //ResourceUtils.StoreResourceOnDisk(ResourceRootPath + "." + TcmUploadAssemblyResourceName, Configuration.WorkFolder, TcmUploadAssemblyResourceName);

            //var installcmd =
            //    $"/C \"{Configuration.WorkFolder.FullName}\\{TcmUploadAssemblyResourceName}\" /folder:tcm:5-50-2 /targeturl:http://live.machine /username:.\\Administrator /password:Tr1v1d3nt /uploadpdb:false \"{Configuration.WorkFolder.FullName}\\{DyndleTemplateResourceName}\"";

            ////System.Diagnostics.Process process = new System.Diagnostics.Process();
            //var process = Process.Start("cmd.exe", installcmd);
            //process.WaitForExit();

            ////System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            ////{
            ////    WorkingDirectory = @"C:\Windows\System32"
            ////};
            //////startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            ////startInfo.FileName = "cmd.exe";
            ////startInfo.Arguments = ;
            ////process.StartInfo = startInfo;
            ////process.Start();
            ////process.WaitForExit();


            //if (process.HasExited)
            //{
            //    return "successfully installed Dyndle in your content manager";
            //}
            //else
            //{
            //    return "failed to install Dyndle";
            //}


            // read all items to import

            // todo: read zipped import package from embedded resource and unzip
            var importItems = new List<ImportItem>();
            List<Reference> references = null;
            foreach (var file in Directory.EnumerateFiles("importpackage"))
            {
                var json = File.ReadAllText(file);
                if (file.EndsWith("references.json"))
                {
                    references = JsonConvert.DeserializeObject<List<Reference>>(json);
                }
                else
                {
                    importItems.Add(JsonConvert.DeserializeObject<ImportItem>(json));
                }
            }


            List<Reference> mappings = new List<Reference>();


            int stopper = 10;
            while (references.Any() && stopper > 0)
            {
                foreach (var importItem in importItems.Where(i => !references.Any(r => r.From == i.SourceId)))
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
                        fromItem.Content.Replace(referencedMapping.From, referencedMapping.To);
                        referencesToRemove.Add(reference);
                    }
                }

                foreach (var reference in referencesToRemove)
                {
                    references.Remove(reference);
                }

                stopper++;
            }


            return $"found {importItems.Count} items to import";

        }


        private string Import(ImportItem importItem)
        {
            switch (importItem.ItemType)
            {
                case ItemType.Schema:
                    return ImportSchema(importItem);
                case ItemType.TemplateBuildingBlock:
                    return ImportTBB(importItem);
            }
            throw new Exception("unhandled item type " + importItem.ItemType);
        }

        private string ImportSchema(ImportItem importItem)
        {

            var schemaData = (SchemaData) Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.Schema,
                Configuration.TBBFolder, new ReadOptions());

            schemaData.Title = importItem.Name;
            schemaData.NamespaceUri = importItem.Namespace;
            schemaData.RootElementName = importItem.RootElementName;
            schemaData.Xsd = importItem.Content;
            schemaData.Purpose = importItem.SchemaPurpose;
            schemaData.Description = importItem.Name;

            schemaData = (SchemaData) Client.Save(schemaData, new ReadOptions());
            schemaData = (SchemaData)Client.CheckIn(schemaData.Id,true,"Dyndle installer", new ReadOptions());

            return schemaData.Id;
        }

        private string ImportTBB(ImportItem importItem)
        {
            var templateBuildingBlockData = (TemplateBuildingBlockData)Client.GetDefaultData(Tridion.ContentManager.CoreService.Client.ItemType.TemplateBuildingBlock,
                Configuration.TBBFolder, new ReadOptions());

            templateBuildingBlockData.Title = importItem.Name;
            templateBuildingBlockData.TemplateType = importItem.TemplateType;
            templateBuildingBlockData.Content = importItem.Content;

            templateBuildingBlockData = (TemplateBuildingBlockData)Client.Save(templateBuildingBlockData, new ReadOptions());
            templateBuildingBlockData = (TemplateBuildingBlockData)Client.CheckIn(templateBuildingBlockData.Id, true, "Dyndle installer", new ReadOptions());

            return templateBuildingBlockData.Id;


        }


        public bool RequiresCoreServiceClient
        {
            get { return true; }
        }
    }
}
