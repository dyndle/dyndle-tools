using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.Installer.Configuration
{
    public static class DefaultConfigurationSetter
    {

        public static void ApplyDefaults(IInstallerConfiguration installerConfiguration)
        {
            if (installerConfiguration.WorkFolder == null)
            {
                var workFolderPath = Path.Combine(Path.GetTempPath(), "dyndle_" + DateTime.Now.Ticks);
                if (Directory.Exists(workFolderPath))
                {
                    throw new Exception($"unexpected error: temporary workfolder {workFolderPath} already exists");
                }
                installerConfiguration.WorkFolder = Directory.CreateDirectory(workFolderPath);
            }

            installerConfiguration.MainPublication = AddTcmPrefix(installerConfiguration.MainPublication);
            installerConfiguration.PagePublication = AddTcmPrefix(installerConfiguration.PagePublication);
            installerConfiguration.PageTemplateFolder = AddTcmPrefix(installerConfiguration.PageTemplateFolder);
            installerConfiguration.ComponentTemplateFolder = AddTcmPrefix(installerConfiguration.ComponentTemplateFolder);
            installerConfiguration.SystemSG = AddTcmPrefix(installerConfiguration.SystemSG);
            installerConfiguration.TBBFolder = AddTcmPrefix(installerConfiguration.TBBFolder);


        }

        private static string AddTcmPrefix(string input)
        {
            return input == null || input.StartsWith("tcm:") ? input : "tcm:" + input;
        }
    }
}
