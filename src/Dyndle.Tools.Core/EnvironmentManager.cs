using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Environment = Dyndle.Tools.Core.Models.Environment;


namespace Dyndle.Tools.Core
{
    public static class EnvironmentManager
    {
        private static string EnvironmentDataPath { get; set; } = ".\\environments.json";

        public static List<Environment> GetAll()
        {
            if (!File.Exists(EnvironmentDataPath))
            {
                return new List<Environment>();
            }

            var environments = JsonConvert.DeserializeObject<List<Environment>>(File.ReadAllText(EnvironmentDataPath));
            return environments;
        }

        public static Environment Get(string name)
        {
            var envs = EnvironmentManager.GetAll();

            var env = envs.FirstOrDefault(e => string.IsNullOrEmpty(name) ? e.IsDefault : e.Name == name);
            if (env == null)
            {
                return envs.FirstOrDefault();
            }
            return env;
        }

        public static void Add(string name, string cmsUrl, string username, string userdomain, string password, bool isDefault)
        {
            List<Dyndle.Tools.Core.Models.Environment> environments = null;
            if (!File.Exists(EnvironmentDataPath))
            {
                environments = new List<Environment>();
            }
            else
            {
                environments = JsonConvert.DeserializeObject<List<Environment>>(File.ReadAllText(EnvironmentDataPath));
            }

            if (environments.Any(e => e.Name == name))
            {
                throw new Exception("There is already an environment with the name " + name);
            }

            var env = new Environment()
            {
                Name = name,
                CMSUrl = cmsUrl,
                Username = username,
                Password = password,
                UserDomain = userdomain
            };

            if (isDefault || environments.Count() == 0)
            {
                env.IsDefault = true;
            }
            environments.Add(env);
            var json = JsonConvert.SerializeObject(environments);
            File.WriteAllText(EnvironmentDataPath, json);
        }
    }
}
