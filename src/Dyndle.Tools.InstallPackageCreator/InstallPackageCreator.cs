using System.Collections.Generic;
using System.Linq;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.ImportExport;
using Dyndle.Tools.Core.ToolsModules;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Installer.Configuration;
using Tridion.ContentManager.CoreService.Client;


namespace Dyndle.Tools.InstallPackageCreator
{
    public class InstallPackageCreator : IToolsModule
    {
        private IInstallerConfiguration Configuration { get; set; }
        private SessionAwareCoreServiceClient Client { get; set; }
        public InstallPackageCreator(IInstallerConfiguration configuration)
        {
            Configuration = configuration;
            var env = EnvironmentManager.Get(Configuration.Environment);
            CoreserviceClientFactory.SetEnvironment(env);
            Client = CoreserviceClientFactory.GetClient();
            DefaultConfigurationSetter.ApplyDefaults(configuration);
            StorageFactory.SetLocation(Configuration.InstallPackagePath);
        }

        public string Run()
        {
            if (string.IsNullOrEmpty(Configuration.DyndleFolder))
            {
                return "Please configure a dyndle folder with --dyndle-folder";
            }
            var references = new List<Reference>();

            var itemsInDyndleFolder = GetTridionItemsFromOrganizationalItem(Configuration.DyndleFolder);
            var itemsInSystemSG = GetTridionItemsFromOrganizationalItem(Configuration.DyndleStructureGroup);
            var itemsToExport =Enumerable.Concat(itemsInDyndleFolder, itemsInSystemSG).ToList();

            //var sourceIds = StorageFactory.GetItemsToExport();
            int counter = 0;
            foreach (var sourceId in itemsToExport)
            {
                var item = Client.Read(sourceId, new ReadOptions());
                var importItem = ModelFactory.CreateImportItem(item);
                counter++;
                StorageFactory.StoreImportItem(importItem);
                foreach (var childId in itemsToExport.Where(s => s != sourceId))
                {
                    if ((importItem.Content != null && importItem.Content.Contains(childId)) ||
                        importItem.PageTemplateId?.ParseId() == childId.ParseId() ||
                        importItem.ParameterSchemaId?.ParseId() == childId.ParseId()
                        )
                    {
                        references.Add(new Reference(sourceId, childId));
                    }
                }
            }
            StorageFactory.StoreReferences(references);
            return $"Exported {counter} items to {Configuration.InstallPackagePath}";
        }

        private IEnumerable<string> GetTridionItemsFromOrganizationalItem(string uri)
        {
            foreach (var item in Client.GetList(uri, new OrganizationalItemItemsFilterData()))
            {
                yield return item.Id;
            }
        }
    }
}
