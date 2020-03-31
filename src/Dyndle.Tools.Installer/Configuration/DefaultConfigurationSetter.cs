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

            installerConfiguration.DyndleFolder = AddTcmPrefix(installerConfiguration.DyndleFolder);
            //installerConfiguration.RootStructureGroup = AddTcmPrefix(installerConfiguration.RootStructureGroup);
            installerConfiguration.DyndleStructureGroup = AddTcmPrefix(installerConfiguration.DyndleStructureGroup);


        }

        private static string AddTcmPrefix(string input)
        {
            return input == null || input.StartsWith("tcm:") ? input : "tcm:" + input;
        }
    }
}
