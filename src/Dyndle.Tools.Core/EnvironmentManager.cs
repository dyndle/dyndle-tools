using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core.Models;
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

        public static void Delete(string name)
        {
            var environments = ReadEnvironments();

            var env = environments.FirstOrDefault(e => e.Name == name);
            if (env == null)
            {
                throw new InvalidEnvironmentException($"There is no environment named {name}");
            }
            bool setAnotherDefault = env.IsDefault;
            environments.Remove(env);
            if (setAnotherDefault && environments.Any())
            {
                environments.FirstOrDefault().IsDefault = true;
            }
            StoreEnvironments(environments);
        }

        public static void Update(string name, string cmsUrl, string username, string userDomain, string password, bool makeDefault)
        {
            var environments = ReadEnvironments();
            var env = environments.FirstOrDefault(e => e.Name == name);
            if (env == null)
            {
                throw new InvalidEnvironmentException($"There is no environment named {name}");
            }
            env.Username = username ?? env.Username;
            env.CMSUrl = cmsUrl ?? env.CMSUrl;
            env.UserDomain = userDomain ?? env.UserDomain;
            env.Password = password ?? env.Password;
            // behavior for 'default' is slightly more complicated because we cannot be sure if the user meant to set it to false, or to simply not specify it
            // we will assume the latter. If they want to remove the 'IsDefault' property, they simply need to make another one the default
            if (makeDefault)
            {
                env.IsDefault = true;
                foreach (var otherEnv in environments.Where(e => e.Name != env.Name))
                {
                    otherEnv.IsDefault = false;
                }
            }
            StoreEnvironments(environments);
        }

        public static Environment GetDefault()
        {
            var envs = EnvironmentManager.GetAll();
            return envs.FirstOrDefault(e => e.IsDefault) ?? envs.FirstOrDefault();
        }

        public static void Add(string name, string cmsUrl, string username, string userdomain, string password, bool isDefault)
        {
            var environments = ReadEnvironments();

            if (environments.Any(e => e.Name == name))
            {
                throw new InvalidEnvironmentException($"There is already an environment with the name {name}");
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
              
                foreach (var otherEnv in environments.Where(e => e.Name != env.Name))
                {
                    otherEnv.IsDefault = false;
                }
            }
            environments.Add(env);
            StoreEnvironments(environments);
        }

        private static List<Dyndle.Tools.Core.Models.Environment> ReadEnvironments()
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
            return environments;
        }
        private static void StoreEnvironments(List<Dyndle.Tools.Core.Models.Environment> environments)
        {
            var json = JsonConvert.SerializeObject(environments);
            File.WriteAllText(EnvironmentDataPath, json);
        }
    }
}
