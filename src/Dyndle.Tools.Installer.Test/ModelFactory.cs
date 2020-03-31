using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Installer.Models;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Installer.Test
{
    public static class ModelFactory
    {
        public static readonly string ResourceRootPath = "Dyndle.Tools.Installer.Test.Resources";
        public static void LoadResource(ImportItem importItem)
        {
            if (importItem.ContentResourceName != null)
            {
                importItem.Content = ResourceUtils.GetResourceAsString(importItem.ContentResourceName);
            }
        }

        public static ImportItem CreateSchema(SchemaData schemaData)
    }
}
