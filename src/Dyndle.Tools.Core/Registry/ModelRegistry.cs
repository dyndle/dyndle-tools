using Dyndle.Tools.Core.ItemCollectors;
using Dyndle.Tools.Core.Models;
using Dyndle.Tools.Core.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tridion.ContentManager.CoreService.Client;

namespace Dyndle.Tools.Core.Registry
{
    public class ModelRegistry
    {
        public static readonly int MaxTries = 20; // maximum number of parsings (needed for schemas linking to other schemas)
        private static readonly Regex reRemoveGarbage = new Regex("[^a-zA-Z0-9_]");
        private static ModelRegistry _modelRegistry;

        private int dynamicModelCounter = 0;
        private ILog _log;
        private ModelDefinition currentViewModel;
        private SchemaCollector schemaCollector;
        private ViewRegistry ViewRegistry
        {
            get
            {
                return ViewRegistry.GetInstance(Config);
            }
        }

        private SessionAwareCoreServiceClient Client { get; set; }
        private IConfiguration Config { get; set; }

        private ModelRegistry(IConfiguration configuration)
        {
            _log = LogManager.GetLogger(typeof(ModelRegistry));
            currentViewModel = null;
            ViewModels = new List<ModelDefinition>();
            Client = CoreserviceClientFactory.GetClient();
            Config = configuration;
            schemaCollector = new SchemaCollector(Config);
        }

        public static ModelRegistry GetInstance(IConfiguration configuration)
        {
            if (_modelRegistry == null)
            {
                _modelRegistry = new ModelRegistry(configuration);
            }
            return _modelRegistry;
        }

        public IList<ModelDefinition> ViewModels { get; }

        public IList<ModelDefinition> UniqueViewModels
        {
            get
            {
                return ViewModels.UniqueBy(a => a.RootElementName).ToList();
            }
        }

        public void AddPageTemplates(IEnumerable<PageTemplateData> pageTemplates)
        {
            foreach (var pageTemplate in pageTemplates)
            {
                ConvertPageTemplate(pageTemplate);
            }
        }

        public void AddPageSchemas(IEnumerable<SchemaData> schemas)
        {
        }

        public void AddSchemas(IEnumerable<SchemaData> schemas)
        {
            currentViewModel = null;
            List<SchemaData> convertibleSchemas = new List<SchemaData>();
            convertibleSchemas.AddRange(schemas);

            int nrTries = 0;
            while (convertibleSchemas.Count > 0 && nrTries++ < MaxTries)
            {
                _log.DebugFormat("trying to convert {0} schemas to viewmodel definitions, starting parsing round #{1}", convertibleSchemas.Count, nrTries);
                List<SchemaData> extraSchemas = new List<SchemaData>();
                foreach (var schema in convertibleSchemas)
                {
                    _log.Debug("starting converting schema " + schema.Title);
                    ConvertSchema(schema);
                }
                convertibleSchemas = convertibleSchemas.Where(a => ViewModels.Any(b => b.TcmUri == a.Id && b.HasUnresolvedProperties)).ToList();
                if (extraSchemas.Count > 0)
                {
                    _log.DebugFormat("adding {0} extra schemas (found during property parsing)", extraSchemas.Count);
                    convertibleSchemas.AddRange(extraSchemas);
                }
                _log.DebugFormat("after parsing round #{0} there are still {1} schemas to be converted", nrTries, convertibleSchemas.Count);
            }
            // after adding all schemas, we need to tell the ViewRegistry to add all component templates linked to the current set of schemas
            // this will trigger the ModelRegistry to create merged models in case a CT is linked to multiple schemas
            UsingItemsFilterData filter = new UsingItemsFilterData()
            {
                ItemTypes = new[] { ItemType.ComponentTemplate }
            };

            foreach (var schema in convertibleSchemas.Where(s => s.Purpose == SchemaPurpose.Component))
            {
                var associatedCTs = Client.GetList(schema.Id, filter).Cast<ComponentTemplateData>();
                associatedCTs?.Select(s => ViewRegistry.GetViewModelDefinition(s.Id)); // we don't care about the return value but we want this
            }

        }

        public bool ContainsModel(string tcmUri)
        {
            return ViewModels.Any(m => m.TcmUri == tcmUri);
        }
        public ModelDefinition GetViewModelDefinition(string tcmUri)
        {
            var model = ViewModels.FirstOrDefault(vm => vm.TcmUri == tcmUri);
            if (model == null)
            {



                var item = Client.Read(tcmUri, new ReadOptions());
                if (item == null)
                {
                    _log.Warn($"unable to retrieve model definition for {tcmUri}: it does not exist");
                }
                else if (item is SchemaData)
                {
                    var expandedSchemas = schemaCollector.FindSchema(tcmUri);
                    var allSchemas = new List<SchemaData>();
                    allSchemas.Add(item as SchemaData);
                    allSchemas.AddRange(expandedSchemas);
                    AddSchemas(allSchemas);
                }
                else if (item is PageTemplateData)
                {
                    AddPageTemplates(new[] { item as PageTemplateData });
                }
                else
                {
                    _log.Warn($"unable to retrieve model definition for {tcmUri}: it has an unexpected item type");
                }
            }
            return ViewModels.First(vm => vm.TcmUri == tcmUri);

        }

        private void ConvertPageTemplate(PageTemplateData pageTemplate)
        {
            if (this.ContainsModel(pageTemplate.Id))
            {
                _log.Debug($"page template '{pageTemplate.Title}' is already in the ModelRegistry (type = {this.GetViewModelDefinition(pageTemplate.Id).TypeName}");
                return;
            }
            _log.Debug($"Converting page template {pageTemplate.Title}");
            ModelDefinition pageModel;
            if (this.ContainsModel(pageTemplate.Id))
            {
                _log.Debug($"model {pageTemplate.Title} was already in the model registry");
                pageModel = ViewModels.First(m => m.TcmUri == pageTemplate.Id);
            }
            else
            {
                _log.Debug($"creating new model definition for page template {pageTemplate.Title}");
                pageModel = new ModelDefinition();
                pageModel.Title = pageTemplate.Title;
                pageModel.TcmUri = pageTemplate.Id;
                pageModel.RootElementName = pageTemplate.Title;
                pageModel.TypeName = MakeCsName(pageModel.RootElementName, true);
                pageModel.Purpose = SchemaPurpose.Region;
                pageModel.PropertyDefinitions = new List<PropertyDefinition>();
            }
            currentViewModel = pageModel;

            // Note: this code does not work because page templates are not schemas
            // For Tridion 9+ we need to create models based on the region schema instead
            //var schemaFieldSets = Client.ReadSchemaFields(pageTemplate.Id, true, new ReadOptions());
            //if (schemaFieldSets.MetadataFields != null)
            //{
            //    AddFields(pageModel.TypeName, pageModel.PropertyDefinitions, schemaFieldSets.MetadataFields, true);
            //}


            PropertyDefinition entitiesPropertyDefinition = new PropertyDefinition(Config)
            {
                FieldType = FieldType.Entities,
                Name = "Entities",
                IsMultipleValue = true,
                
            };
            PropertyDefinition regionsPropertyDefinition = new PropertyDefinition(Config)
            {
                FieldType = FieldType.Regions,
                Name = "Regions",
                IsMultipleValue = true,
            };
            PropertyDefinition pageTitlePropertyDefinition = new PropertyDefinition(Config)
            {
                FieldType = FieldType.PageTitle,
                Name = "PageTitle",
                IsMultipleValue = false,
            };
            PropertyDefinition pageIdPropertyDefinition = new PropertyDefinition(Config)
            {
                FieldType = FieldType.PageId,
                Name = "PageId",
                IsMultipleValue = false,
            };

            pageModel.PropertyDefinitions.Add(pageTitlePropertyDefinition);
            pageModel.PropertyDefinitions.Add(pageIdPropertyDefinition);
            pageModel.PropertyDefinitions.Add(regionsPropertyDefinition);
            pageModel.PropertyDefinitions.Add(entitiesPropertyDefinition);

            // schema was converted into a ViewModelDefinition successfully (possibly with some unresolved fields, but we'll deal with them later!)
            // we can now add a mapping to the registry
            ViewModels.Add(pageModel);
        }

        private void ConvertSchema(SchemaData schema)
        {
            if (this.ContainsModel(schema.Id))
            {
                var modelDefinition = GetViewModelDefinition(schema.Id);
                _log.Debug($"schema '{schema.Title}' is already in the ModelRegistry (type = {modelDefinition.TypeName})");
                if (modelDefinition.HasUnresolvedProperties)
                {
                    _log.Debug($"schema '{schema.Title}' is already in the ModelRegistry but one or more of its properties are unresolved (type = {modelDefinition.TypeName}");
                    foreach (var propertyDefinition in modelDefinition.PropertyDefinitions.Where(p => !p.IsResolved))
                    {
                        ResolvePropertyDefinition(propertyDefinition);
                    }
                    if (modelDefinition.HasUnresolvedProperties)
                    {
                        _log.Debug($"after attempting to resolve the properties of the schema '{schema.Title}', there are still unresolved properties (they will be picked up later)");
                    }
                    else
                    {
                        _log.Debug($"after attempting to resolve the properties of the schema '{schema.Title}', there are no more unresolved properties");
                    }
                }
                else
                {
                    _log.Debug($"schema '{schema.Title}' is already in the ModelRegistry and is fully resolved, so there is nothing to do");
                }
                return;
            }
            _log.Debug($"Converting schema {schema.Title}");
            ModelDefinition schemaModel;
            if (this.ContainsModel(schema.Id))
            {
                _log.Debug($"schema {schema.Title} was already in the model registry");
                schemaModel = ViewModels.First(m => m.TcmUri == schema.Id);
            }
            else
            {
                _log.Debug($"creating new ViewModelDefinition for schema {schema.Title}");
                schemaModel = new ModelDefinition();
                schemaModel.Title = schema.Title;
                schemaModel.TcmUri = schema.Id;
                schemaModel.RootElementName = Config.IdentifyModelsBySchemaTitle ? schema.Title : (string.IsNullOrWhiteSpace(schema.RootElementName) ? schema.Title : schema.RootElementName);
                schemaModel.TypeName = MakeCsName(schemaModel.RootElementName, true);
                schemaModel.Purpose = schema.Purpose.Value;
                schemaModel.PropertyDefinitions = new List<PropertyDefinition>();
            }
            currentViewModel = schemaModel;

            var schemaFieldSets = Client.ReadSchemaFields(schema.Id, true, new ReadOptions());
            if (schemaFieldSets.Fields != null)
            {
                AddFields(schemaModel.TypeName, schemaModel.PropertyDefinitions, schemaFieldSets.Fields, false);
            }
            if (schemaFieldSets.MetadataFields != null)
            {
                AddFields(schemaModel.TypeName, schemaModel.PropertyDefinitions, schemaFieldSets.MetadataFields, true);
            }

            // schema was converted into a ViewModelDefinition successfully (possibly with some unresolved fields, but we'll deal with them later!)
            // we can now add a mapping to the registry
            ViewModels.Add(schemaModel);
        }

        public ModelDefinition MergeModelDefinitions(IEnumerable<ModelDefinition> modelDefinitions)
        {
            IEnumerable<PropertyDefinition> commonPropertyDefinitions = new List<PropertyDefinition>();
            commonPropertyDefinitions = modelDefinitions.FirstOrDefault()?.PropertyDefinitions;
            foreach (var modelDefinition in modelDefinitions.Skip(1))
            {
                commonPropertyDefinitions = commonPropertyDefinitions.Intersect(modelDefinition.PropertyDefinitions);
                if (!commonPropertyDefinitions.Any())
                {
                    break;
                }
            }
            ModelDefinition mergedModelDefinition = new ModelDefinition()
            {
                Purpose = modelDefinitions.FirstOrDefault().Purpose,
                TypeName = string.Join("", modelDefinitions.Take(3).Select(md => md.TypeName)) + "Base",
                Title = string.Join("", modelDefinitions.Take(3).Select(md => md.Title)) + "Base",
                TcmUri = "dynamic-" + dynamicModelCounter++,
                PropertyDefinitions = commonPropertyDefinitions.ToList()
            };
            ViewModels.Add(mergedModelDefinition);
            return mergedModelDefinition;
        }

        private void AddFields(string schemaTypeName, IList<PropertyDefinition> propertyDefinitions, ItemFieldDefinitionData[] fields, bool isMetadata)
        {
            foreach (var fieldDef in fields)
            {
                string fieldName = MakeCsName(fieldDef.Name, true);
                PropertyDefinition def = propertyDefinitions.FirstOrDefault(a => a.Name == fieldName);
                if (def == null)
                {
                    def = new PropertyDefinition(Config);
                    propertyDefinitions.Add(def);
                }
                if (def.IsResolved)
                {
                    _log.Debug($"field {def.Name} was already processed successfully");
                    continue;
                }
                def.Name = fieldName;
                _log.Debug($"processing field {def.Name}");
                def.FieldType = GetFieldType(fieldDef);
                def.IsMandatory = fieldDef.MinOccurs > 0;
                def.IsMultipleValue = fieldDef.MaxOccurs != 1; // note: MaxOccurs == -1 means the field is multiple value, but it is also possible to enter a custom value

                if (def.FieldType == FieldType.ComponentLink)
                {
                    def.TargetSchemas = ((ComponentLinkFieldDefinitionData)fieldDef).AllowedTargetSchemas.ToList();
                }
                if (def.FieldType == FieldType.MultiMediaLink)
                {
                    def.TargetSchemas = ((MultimediaLinkFieldDefinitionData)fieldDef).AllowedTargetSchemas.ToList();
                }
                if (def.FieldType == FieldType.Embedded)
                {
                    def.TargetSchemas = new List<LinkToSchemaData>() { ((EmbeddedSchemaFieldDefinitionData)fieldDef).EmbeddedSchema };
                }
                if (def.FieldType == FieldType.Keyword)
                {
                    CategoryData cat = (CategoryData)Client.Read(((KeywordFieldDefinitionData)fieldDef).Category.IdRef, new ReadOptions());
                    if (cat.KeywordMetadataSchema != null && cat.KeywordMetadataSchema.IdRef != "tcm:0-0-0")
                    {
                        def.TargetSchemas = new List<LinkToSchemaData>() { cat.KeywordMetadataSchema };
                    }
                }
                ResolvePropertyDefinition(def);
                def.IsMetadata = isMetadata;
            }
        }

        private void ResolvePropertyDefinition(PropertyDefinition def)
        {
            try
            {
                if (def.FieldType == FieldType.ComponentLink)
                {
                    def.TargetModels = GetTargetModelTypesForLinks(def.TargetSchemas, false);
                }
                if (def.FieldType == FieldType.MultiMediaLink)
                {
                    def.TargetModels = GetTargetModelTypesForLinks(def.TargetSchemas, true);
                }
                if (def.FieldType == FieldType.Embedded)
                {
                    def.TargetModels = GetTargetModelTypes(def.TargetSchemas);
                }
                if (def.FieldType == FieldType.Keyword && def.TargetSchemas != null)
                {
                    def.TargetModels = GetTargetModelTypes(def.TargetSchemas);
                }
                def.IsResolved = true;
            }
            catch (TargetModelNotFoundException e)
            {
                _log.Debug($"Failed to generate property {def.Name}; target model {e.TargetSchema} not found");
            }
        }

        private FieldType GetFieldType(ItemFieldDefinitionData fieldDef)
        {
            if (fieldDef is SingleLineTextFieldDefinitionData)
                return FieldType.Text;
            if (fieldDef is MultiLineTextFieldDefinitionData)
                return FieldType.Text;
            if (fieldDef is DateFieldDefinitionData)
                return FieldType.Date;
            if (fieldDef is ComponentLinkFieldDefinitionData)
                return FieldType.ComponentLink;
            if (fieldDef is MultimediaLinkFieldDefinitionData)
                return FieldType.MultiMediaLink;
            if (fieldDef is XhtmlFieldDefinitionData)
                return FieldType.Xhtml;
            if (fieldDef is EmbeddedSchemaFieldDefinitionData)
                return FieldType.Embedded;
            if (fieldDef is KeywordFieldDefinitionData)
                return FieldType.Keyword;
            if (fieldDef is NumberFieldDefinitionData)
                return FieldType.Number;
            return FieldType.Text;
        }

        private List<string> GetTargetModelTypesForLinks(IEnumerable<LinkToSchemaData> linkedSchemas, bool isMultimediaLink)
        {
            List<string> targetTypes = new List<string>();
            if (! linkedSchemas.Any())
            {
                if (isMultimediaLink && Config.BaseClassesForMultimedia != null && Config.BaseClassesForMultimedia.Any())
                {
                    targetTypes.Add(Config.BaseClassesForMultimedia.FirstOrDefault());
                }
                if ((!isMultimediaLink) && Config.BaseClasses != null && Config.BaseClasses.Any())
                {
                    targetTypes.Add(Config.BaseClasses.FirstOrDefault());
                }
                targetTypes.Add("EntityModel");
                return targetTypes;
            }
            return GetTargetModelTypes(linkedSchemas);
        }

        private List<string> GetTargetModelTypes(IEnumerable<LinkToSchemaData> linkedSchemas)
        {
            List<string> targetTypes = new List<string>();
            if (!linkedSchemas.Any())
            {
                targetTypes.Add("EntityModel");
                return targetTypes;
            }
            foreach (var targetSchema in linkedSchemas)
            {
                if (ContainsModel(targetSchema.IdRef))
                {
                    targetTypes.Add(GetViewModelDefinition(targetSchema.IdRef).TypeName);
                }
                else if (targetSchema.IdRef == currentViewModel.TcmUri)
                {
                    targetTypes.Add(currentViewModel.TypeName);
                }
                else
                {
                    throw new TargetModelNotFoundException() { TargetSchema = targetSchema.Title };
                }
            }
            return targetTypes;
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
