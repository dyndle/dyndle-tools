using Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.Environments
{
    public interface IEnvironmentsConfiguration : ICoreConfiguration
    {
        string Name { get; set; }

    }
}
