using System;
using Dyndle.Tools.Core.ToolsModules;
using Tridion.CoreService.Tools;

namespace Dyndle.Tools.Environments
{
    public class UpdateEnvironment : IToolsModule
    {
        private IAddOrUpdateEnvironmentsConfiguration Configuration { get; set; }
        public UpdateEnvironment(IAddOrUpdateEnvironmentsConfiguration configuration)
        {
            Configuration = configuration;
        }
        public string Run()
        {
            if (string.IsNullOrEmpty(Configuration.Name))
            {
                return
                    "Failed - you need to supply a name. Type dyndle help environment for help.";
            }

            try
            {
                EnvironmentManager.Update(Configuration.Name, Configuration.CMSUrl, Configuration.Username,
                    Configuration.UserDomain, Configuration.Password, Configuration.MakeDefault);
            }
            catch (Exception e)
            {
                return "Failed - " + e.Message;
            }
            return "Successfully updated environment";
        }
    }
}
