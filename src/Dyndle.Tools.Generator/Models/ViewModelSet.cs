using System.Collections.Generic;

namespace Dyndle.Tools.Generator.Models
{
    public class ViewModelSet
    {
        public string Namespace { get; set; }
        public IList<ModelDefinition> ViewModelDefinitions { get; set; }
    }
}
