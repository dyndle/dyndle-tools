using Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.Environments
{
    public interface IAddOrUpdateEnvironmentsConfiguration : IEnvironmentsConfiguration
    {
        string CMSUrl { get; set; }
        string Username { get; set; }
        string UserDomain { get; set; }
        string Password { get; set; }
        bool MakeDefault { get; set; }

    }
}
