using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Installer.Models;
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

            DirectoryInfo exportDir = Directory.Exists("export") ? new DirectoryInfo("export") : Directory.CreateDirectory("export");

            var references = new List<Reference>();

            var sourceIds = File.ReadAllLines("items-to-export.txt");
            foreach (var sourceId in sourceIds)
            {
                var item = client.Read(sourceId, new ReadOptions());
                var importItem = ModelFactory.CreateImportItem(item);
                var json = JsonConvert.SerializeObject(importItem);
                File.WriteAllText(Path.Combine(exportDir.FullName, item.Id.Replace("tcm:", "")), json);
                foreach (var childId in sourceIds.Where(s => s != sourceId))
                {
                    if (json.Contains(childId))
                    {
                        references.Add(new Reference(sourceId, childId));
                    }
                }
            }
            File.WriteAllText(Path.Combine(exportDir.FullName, "references.json"), JsonConvert.SerializeObject(references));

        }

    }
}
