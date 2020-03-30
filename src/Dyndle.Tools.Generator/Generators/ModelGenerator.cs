using log4net;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dyndle.Tools.Core.ToolsModules;
using Tridion.ContentManager.CoreService.Client;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Generator.CodeWriters;
using Dyndle.Tools.Generator.Models;
using Dyndle.Tools.Generator.Registry;

namespace Dyndle.Tools.Generator.Generators
{
    public class ModelGenerator : GeneratorBase, IToolsModule
    {

        private ModelDefinition currentViewModel = null;

        public ModelGenerator(GeneratorConfiguration config) : base(config)
        {
            CodeWriter = new CsharpCodeWriter(config);
        }

        protected override string NuspecTemplate => "Dyndle.Tools.Generator.Resources.My.ViewModels.nuspec";

        public override string FileExtension => ".cs";

        protected override void CollectInput()
        {
            IEnumerable<SchemaData> schemas = new List<SchemaData>();
            IEnumerable<PageTemplateData> pageTemplates = new List<PageTemplateData>();
            if (!string.IsNullOrEmpty(Config.SchemaId))
            {
                schemas = SchemaCollector.FindSchema(Config.SchemaId);
            }
            else if (!string.IsNullOrEmpty(Config.TemplateId))
            {
                pageTemplates = TemplateCollector.FindTemplate(Config.TemplateId, true).Cast<PageTemplateData>();
            }
            else if (!string.IsNullOrEmpty(Config.PublicationId))
            {
                schemas = SchemaCollector.FindSchemasForPublication(Config.PublicationId);
                pageTemplates = TemplateCollector.FindTemplatesForPublication(Config.PublicationId, true).Cast<PageTemplateData>();
            }
            else if (!string.IsNullOrEmpty(Config.FolderId))
            {
                schemas = SchemaCollector.FindSchemasForFolder(Config.FolderId);
                pageTemplates = TemplateCollector.FindTemplatesForFolder(Config.FolderId, true).Cast<PageTemplateData>();
            }
            else
            {
                throw new Exception("You need to specify one of the following: schema (-s), publication (-p) or folder (-f)");
            }
            log.Info($"Found {schemas.Count()} schemas");
            ModelRegistry.GetInstance(Config).AddSchemas(schemas);
            log.Info($"Found {pageTemplates.Count()} page templates");
            ModelRegistry.GetInstance(Config).AddPageTemplates(pageTemplates);

        }

        protected override void Analyze()
        {
            Console.WriteLine("TcmUri;Title;RootElementName;TypeName;Purpose;PropertyDefinitions.Count");
            foreach (var model in ModelRegistry.GetInstance(Config).ViewModels)
            {
                Console.WriteLine($"{model.TcmUri};{model.Title};{model.RootElementName};{model.TypeName};{model.Purpose};{model.PropertyDefinitions.Count()}");
            }
        }

        public string Run()
        {
            log.Info("Started generator");

            CollectInput();

            if (Config.AnalyzeOnly)
            {
                Analyze();
                return "";
            }
            IDictionary<string, string> classes = CodeWriter.WriteCode();
            string path = Package(classes);
            log.Debug($"Models created in {path}");
            return path;
        }

        public bool RequiresCoreServiceClient
        {
            get { return true; }
        }
    }       
}
