using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.ToolsModules;
using Dyndle.Tools.Environments;
using Newtonsoft.Json;
using Environment = Dyndle.Tools.Core.Models.Environment;


namespace Dyndle.Tools.Environments
{
    public class ListEnvironments : IToolsModule
    {
        private IEnvironmentsConfiguration Configuration { get; set; }
        public ListEnvironments(IEnvironmentsConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string Run()
        {
            var environments = EnvironmentManager.GetAll();
            var sb = new StringBuilder();
            foreach (var env in environments)
            {
                string star = env.IsDefault ? "*" : "";
                sb.AppendLine($"[{env.Name}]{star} {env.CMSUrl}");
            }

            return sb.ToString();
        }
        public bool RequiresCoreServiceClient
        {
            get { return false; }
        }
    }
}
