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
using Dyndle.Tools.Core.Configuration;
using Dyndle.Tools.Core.ToolsModules;
using Tridion.ContentManager.CoreService.Client;
using Dyndle.Tools.Generator.Models;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Generator.CodeWriters;
using Dyndle.Tools.Generator.Registry;

namespace Dyndle.Tools.Generator.Generators
{
    public class ViewGenerator : GeneratorBase, IToolsModule
    {

        public ViewGenerator(IGeneratorConfiguration config) : base(config)
        {
            CodeWriter = new RazorCodeWriter(config);
        }




        protected override string NuspecTemplate => "Dyndle.Tools.Generator.Resources.My.Views.nuspec";
        protected override void CollectInput()
        {
            IEnumerable<TemplateData> templates;
            if (!string.IsNullOrEmpty(Config.TemplateId))
            {
                templates = TemplateCollector.FindTemplate(Config.TemplateId);
            }
            else if (!string.IsNullOrEmpty(Config.PublicationId))
            {
                templates = TemplateCollector.FindTemplatesForPublication(Config.PublicationId);
            }
            else if (!string.IsNullOrEmpty(Config.FolderId))
            {
                templates = TemplateCollector.FindTemplatesForFolder(Config.FolderId);
            }
            else
            {
                throw new Exception("You need to specify one of the following: template (-t), publication (-p) or folder (-f)");
            }
            log.Info($"Found {templates.Count()} templates");
            ViewRegistry.GetInstance(Config).AddTemplates(templates);

        }

        public override string FileExtension => ".cshtml";

        protected override void Analyze()
        {
            Console.WriteLine("TcmUri;Title;ViewName;ModelTypeName;AssociatedModelDefinition.PropertyDefinitions.Count");
            foreach (var view in ViewRegistry.GetInstance(Config).Views)
            {
                Console.WriteLine($"{view.TcmUri};{view.Title};{view.ViewName};{view.ModelTypeName};{view.AssociatedModelDefinition.PropertyDefinitions.Count()}");
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
            log.Debug($"Views created in {path}");
            return path;
        }
    }
}
