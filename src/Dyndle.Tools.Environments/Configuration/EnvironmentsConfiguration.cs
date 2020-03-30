using Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.Environments
{
    public class EnvironmentsConfiguration : CoreConfiguration
    {
        public string Name { get; set; }
        public string CMSUrl { get; set; }
        public string Username { get; set; }
        public string UserDomain { get; set; }
        public string Password { get; set; }
        public bool MakeDefault { get; set; }

    }
}
