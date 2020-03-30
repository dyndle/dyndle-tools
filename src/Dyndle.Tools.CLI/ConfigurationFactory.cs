using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.CLI
{
    public static class ConfigurationFactory
    {
        public static T ToConfiguration<T>(Options options) where T : IConfiguration, new()
        {
            var optionProperties = options.GetType().GetProperties(BindingFlags.FlattenHierarchy |
                                                                   BindingFlags.Instance |
                                                                   BindingFlags.Public).ToList();
            T configuration = new T();
            foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public))
            {
                var optionProperty = optionProperties.FirstOrDefault(o => o.Name == property.Name);
                if (optionProperty != null)
                {
                    property.SetValue(configuration, optionProperty.GetValue(options));
                }
                else
                {
                    var oops = "oops";
                }
            }

            return configuration;
        }

    }
}
