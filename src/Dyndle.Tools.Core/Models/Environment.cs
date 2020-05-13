namespace Dyndle.Tools.Core.Models
{
    public class Environment
    {
        public string Name { get; set; }
        public string CMSUrl { get; set; }
        public string Username { get; set; }
        public string UserDomain { get; set; }
        public string Password { get; set; }
        public bool IsDefault { get; set; }
    }
}
