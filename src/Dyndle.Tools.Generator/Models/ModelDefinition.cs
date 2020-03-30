using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Generator.Models
{
    public class ModelDefinition
    {
        public string RootElementName { get; set; }
        public string Title { get; set; }
        public string TypeName { get; set; }
        public SchemaPurpose Purpose { get; set; }
        public IList<PropertyDefinition> PropertyDefinitions { get; set; }
        public string TcmUri { get; set; }
        public bool IsPageModel => Purpose == SchemaPurpose.Region;
        public bool HasUnresolvedProperties
        {
            get
            {
                return PropertyDefinitions.Any(a => a.IsResolved == false);
            }
        }
    }
}
