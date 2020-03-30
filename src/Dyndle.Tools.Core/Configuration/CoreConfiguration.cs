using System.IO;

namespace Dyndle.Tools.Core.Configuration
{
    public abstract class CoreConfiguration : IConfiguration
    {
        public string LogFolder { get; set; }
    }
}
