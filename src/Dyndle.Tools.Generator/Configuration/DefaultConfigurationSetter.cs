using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.Generator.Configuration
{
    public static class DefaultConfigurationSetter
    {
        public static readonly string DefaultPathModels = "/Generated/Models";
        public static readonly string DefaultPathViews = "/Areas/Core/Views";
        public static readonly string DefaultMinDd4TCoreVersion = "2.5";
        public static readonly string DefaultMinDd4TMvcVersion = "2.5";


        public static void ApplyDefaults(IGeneratorConfiguration generatorConfiguration)
        {
            if (generatorConfiguration.WorkFolder == null)
            {
                var workFolderPath = Path.Combine(Path.GetTempPath(), "dyndle_" + DateTime.Now.Ticks);
                if (Directory.Exists(workFolderPath))
                {
                    throw new Exception($"unexpected error: temporary workfolder {workFolderPath} already exists");
                }
                generatorConfiguration.WorkFolder = Directory.CreateDirectory(workFolderPath);
            }

            if (generatorConfiguration.FolderInProject == null)
            {
                generatorConfiguration.FolderInProject = generatorConfiguration.ExportType == ExportType.Models ? DefaultPathModels : DefaultPathViews;
            }

            if (generatorConfiguration.MinDD4TCoreVersion == null)
            {
                generatorConfiguration.MinDD4TCoreVersion = DefaultMinDd4TCoreVersion;
            }
            if (generatorConfiguration.MinDD4TMvcVersion == null)
            {
                generatorConfiguration.MinDD4TMvcVersion = DefaultMinDd4TMvcVersion;
            }

            if (generatorConfiguration.PackageName == null)
            {
                generatorConfiguration.PackageName = generatorConfiguration.ExportType == ExportType.Models ? "MyModels" : "MyViews";
            }

            // if one of the TCM uris is present and it does NOT start with 'tcm:', add this prefix 
            generatorConfiguration.PublicationId = generatorConfiguration.PublicationId?.StartsWith("tcm:") ?? true
                ? generatorConfiguration.PublicationId
                : "tcm:" + generatorConfiguration.PublicationId;
            generatorConfiguration.FolderId = generatorConfiguration.FolderId?.StartsWith("tcm:") ?? true
                ? generatorConfiguration.FolderId
                : "tcm:" + generatorConfiguration.FolderId;
            generatorConfiguration.SchemaId = generatorConfiguration.SchemaId?.StartsWith("tcm:") ?? true
                ? generatorConfiguration.SchemaId
                : "tcm:" + generatorConfiguration.SchemaId;
            generatorConfiguration.TemplateId = generatorConfiguration.TemplateId?.StartsWith("tcm:") ?? true
                ? generatorConfiguration.TemplateId
                : "tcm:" + generatorConfiguration.TemplateId;


        }
    }
}
