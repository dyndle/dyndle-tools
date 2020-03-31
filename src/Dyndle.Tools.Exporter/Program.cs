using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.ImportExport;
using Dyndle.Tools.Installer.Test;
using Newtonsoft.Json;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Exporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = args.Length == 0 ? EnvironmentManager.GetDefault() : EnvironmentManager.Get(args[0]);
            CoreserviceClientFactory.SetEnvironment(env);
            var client = CoreserviceClientFactory.GetClient();
            var references = new List<Reference>();

            var sourceIds = StorageFactory.GetItemsToExport();
            foreach (var sourceId in sourceIds)
            {
                var item = client.Read(sourceId, new ReadOptions());
                var importItem = ModelFactory.CreateImportItem(item);
                StorageFactory.StoreImportItem(importItem);
                foreach (var childId in sourceIds.Where(s => s != sourceId))
                {
                    if (importItem.Content.Contains(childId))
                    {
                        references.Add(new Reference(sourceId, childId));
                    }
                }
            }
            StorageFactory.StoreReferences(references);

        }

    }
}
