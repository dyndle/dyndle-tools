using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Dyndle.Tools.Core.ImportExport
{
    public static class StorageFactory
    {
        private static DirectoryInfo exportDir;

        static StorageFactory()
        {
            exportDir = Directory.Exists("export") ? new DirectoryInfo("export") : Directory.CreateDirectory("export");
        }

        public static void SetLocation(string location)
        {
            exportDir = Directory.Exists(location) ? new DirectoryInfo(location) : Directory.CreateDirectory(location);
        }


        public static IEnumerable<string> GetItemsToExport()
        {
            return File.ReadAllLines("items-to-export.txt");
        }

        public static IEnumerable<ImportItem> GetImportItems()
        {
            List<Reference> references = null;
            var importItems = new List<ImportItem>();
            foreach (var file in exportDir.EnumerateFiles())
            {
                var json = File.ReadAllText(file.FullName);
                if (!file.FullName.EndsWith("references.json"))
                {
                    importItems.Add(JsonConvert.DeserializeObject<ImportItem>(json));
                }
            }

            return importItems;
        }

        public static List<Reference> GetReferences()
        {
            var file = Path.Combine(exportDir.FullName, "references.json");
            var json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<List<Reference>>(json);
        }



        public static void StoreImportItem(ImportItem importItem)
        {
            var json = JsonConvert.SerializeObject(importItem);
            File.WriteAllText(Path.Combine(exportDir.FullName, importItem.SourceId.Replace("tcm:", "")), json);
        }

        public static void StoreReferences(List<Reference> references)
        {
            File.WriteAllText(Path.Combine(exportDir.FullName, "references.json"),
                JsonConvert.SerializeObject(references));
        }

    }
}
