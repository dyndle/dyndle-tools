using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Installer.Models
{
    public enum ItemType {  Page, Component, PageTemplate, ComponentTemplate, Folder, StructureGroup, Schema, TemplateBuildingBlock }
    public class ImportItem
    {
        public ItemType ItemType { get; set; }
        public string Name { get; set; }
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string Content { get; set; }
        public string Namespace { get; set; }
        public  SchemaPurpose SchemaPurpose { get; set; }
        public string RootElementName { get; set; }
        public string TemplateType { get; set; }
        public string Filename { get; set; }
        public IEnumerable<ImportItem> LinkedItems { get; set; }
    }
}
