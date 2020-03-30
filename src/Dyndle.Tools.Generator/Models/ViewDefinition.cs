using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Generator.Models
{
    public enum ViewPurpose { Page, Region, Entity }
    public class ViewDefinition
    {
        public ModelDefinition AssociatedModelDefinition { get; set; }
        public string Title { get; set; }
        public string ModelTypeName { get; set; }
        public string ViewName { get; set; }
        public string TcmUri { get; set; }
        public ViewPurpose ViewPurpose { get; set; }
    }
}
