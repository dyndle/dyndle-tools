using Dyndle.Tools.Core.ItemCollectors;
using Dyndle.Tools.Core.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Core.Registry
{
    public class ViewRegistry
    {
        public static readonly int MaxTries = 20; // maximum number of parsings (needed for schemas linking to other schemas)
        private static readonly Regex reRemoveGarbage = new Regex("[^a-zA-Z0-9_]");
        private static ViewRegistry _viewRegistry;



        private ILog _log;
        private ViewDefinition currentView;
        private SessionAwareCoreServiceClient Client { get; set; }
        private IConfiguration Config { get; set; }
        private ModelRegistry ModelRegistry
        {
            get
            {
                return ModelRegistry.GetInstance(Config);
            }
        }

        private ViewRegistry(IConfiguration configuration)
        {
            _log = LogManager.GetLogger(this.GetType());
            currentView = null;
            Views = new List<ViewDefinition>();
            Client = CoreserviceClientFactory.GetClient();
            Config = configuration;
        }

        public static ViewRegistry GetInstance(IConfiguration configuration)
        {
            if (_viewRegistry == null)
            {
                _viewRegistry = new ViewRegistry(configuration);
            }
            return _viewRegistry;
        }


        public IList<ViewDefinition> Views { get; }

        public void AddTemplates(IEnumerable<TemplateData> templates)
        {
            foreach (var template in templates)
            {
                ConvertTemplate(template);
                _log.Debug($"converted template {template.Title}");
            }
        }

        public bool ContainsView(string tcmUri)
        {
            return Views.Any(m => m.TcmUri == tcmUri);
        }

        public ViewDefinition GetViewModelDefinition(string tcmUri)
        {
            return Views.First(vm => vm.TcmUri == tcmUri);
        }


        private void ConvertTemplate(TemplateData template)
        {
            _log.Debug($"Converting template {template.Title}");
            ViewDefinition viewDefinition;
            if (this.ContainsView(template.Id))
            {
                _log.Debug($"schema {template.Title} was already in the model registry");
                viewDefinition = Views.First(m => m.TcmUri == template.Id);
            }
            else
            {
                _log.Debug($"creating new ViewDefinition for template {template.Title}");
                viewDefinition = new ViewDefinition();
                viewDefinition.Title = template.Title;
                viewDefinition.TcmUri = template.Id;
                if (string.IsNullOrEmpty(template.Metadata))
                {
                    viewDefinition.ViewName = MakeCsName(template.Title, true);
                }
                else
                {
                    viewDefinition.ViewName = GetFieldValue(template.Metadata, "view");
                }

                if (template is ComponentTemplateData)
                {
                    viewDefinition.ViewPurpose = ViewPurpose.Entity;
                    UsedItemsFilterData filter = new UsedItemsFilterData()
                    {
                        ItemTypes = new[] { ItemType.Schema }
                    };

                    var associatedSchemas = Client.GetList(template.Id, filter).Cast<SchemaData>();

                    var allAssociatedModels = associatedSchemas?.Select(s => ModelRegistry.GetViewModelDefinition(s.Id)).Where(a => a.Purpose == SchemaPurpose.Component);

                    if (allAssociatedModels.Count() > 1)
                    {
                        _log.Info($"Template {template.Title} is linked to more than one schema. We need to merge models before we can continue");
                        var mergedModel = ModelRegistry.MergeModelDefinitions(allAssociatedModels);
                        viewDefinition.AssociatedModelDefinition = mergedModel;
                        viewDefinition.ModelTypeName = mergedModel.TypeName;
                    }
                    else if (allAssociatedModels.Count() == 0)
                    {
                        _log.Warn($"found component template {template.Title} ({template.Id}) without any associated schema. We will use the base class {Config.BaseClasses.FirstOrDefault()} instead");
                        viewDefinition.ModelTypeName = Config.BaseClasses.FirstOrDefault();
                    }
                    else
                    {
                        viewDefinition.AssociatedModelDefinition = ModelRegistry.GetViewModelDefinition(associatedSchemas.First().Id);
                        viewDefinition.ModelTypeName = viewDefinition.AssociatedModelDefinition.TypeName;
                    }

                }
                else // this is a page template, so it has its own associated model definition
                {
                    viewDefinition.AssociatedModelDefinition = ModelRegistry.GetViewModelDefinition(template.Id);
                    viewDefinition.ModelTypeName = viewDefinition.AssociatedModelDefinition.TypeName;
                    viewDefinition.ViewPurpose = ViewPurpose.Page;
                }
            }
            
            currentView = viewDefinition;

            if (this.ContainsView(template.Id))
            {
                Views.Add(viewDefinition);
            }
            else
            {
                Views.Add(viewDefinition);
            }
        }


        private static string GetFieldValue(string metadata, string fieldName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(metadata);
            var field = doc.SelectSingleNode("//*[local-name()='" + fieldName + "']");
            return field.InnerText;
        }


        private static string MakeCsName(string input, bool ucFirst)
        {
            string safeName = reRemoveGarbage.Replace(input, "");
            string result;
            if (safeName.Where(a => Char.IsUpper(a)).Count() > safeName.Length / 2) // true if more than half of the characters are upper case, in that case convert everything to lower except the first letter 
            {
                result = safeName.Substring(0, 1).ToUpper() + safeName.Substring(1).ToLower();
            }
            else
            {
                result = safeName.Substring(0, 1).ToUpper() + safeName.Substring(1);
            }
            return result;
        }
    }
}
