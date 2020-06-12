using System.IO;

namespace Dyndle.Tools.Core.Configuration
{
    public interface ICoreConfiguration : IConfiguration
    {
        string LogFolder { get; set; }
        bool Verbose { get; set; }
    }
}
