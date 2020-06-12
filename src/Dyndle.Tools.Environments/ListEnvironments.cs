using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.Configuration;
using Dyndle.Tools.Core.ToolsModules;
using Dyndle.Tools.Environments;
using Newtonsoft.Json;
using Environment = Dyndle.Tools.Core.Models.Environment;


namespace Dyndle.Tools.Environments
{
    public class ListEnvironments : IToolsModule
    {
        private ICoreConfiguration Configuration { get; set; }
        public ListEnvironments(ICoreConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string Run()
        {
            var environments = EnvironmentManager.GetAll();
            var sb = new StringBuilder();
            foreach (var env in environments)
            {
                if (Configuration.Verbose)
                {
                    sb.AppendLine($"Name:        {env.Name}");
                    sb.AppendLine($"URL:         {env.CMSUrl}");
                    sb.AppendLine($"User name:   {env.Username}");
                    sb.AppendLine($"User domain: {env.UserDomain}");
                    sb.AppendLine($"Password:    ***");
                    sb.AppendLine($"Default:     {env.IsDefault}");
                    sb.AppendLine();
                }
                else
                {
                    string star = env.IsDefault ? "*" : "";
                    sb.AppendLine($"[{env.Name}]{star} {env.CMSUrl}");
                }
            }

            return sb.ToString();
        }
    }
}
