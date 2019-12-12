using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dyndle.Tools.Core.Models;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Core.Registry;

namespace Dyndle.Tools.Core.CodeWriters
{
    public class CsharpCodeWriter : CodeWriterBase
    {

        public CsharpCodeWriter(IConfiguration config) : base(config)
        {
        }

        public override IDictionary<string,string> WriteCode()
        {
            IDictionary<string, string> class2code = new Dictionary<string, string>();
            IndentedStringBuilder sb = new IndentedStringBuilder(Config.IndentNrOfSpaces);
            ModelRegistry modelRegistry = ModelRegistry.GetInstance(Config);

            log.DebugFormat("Started GenerateCode with {0} models", modelRegistry.ViewModels.Count);
            var uniqueViewModels = modelRegistry.UniqueViewModels;
            log.DebugFormat("Unique view models: {0}", uniqueViewModels.Count);
            sb.Indent(false);
            var modelAttributeName = Config.ModelAttributeName;
            foreach (var modelDefinition in uniqueViewModels)
            {
                //var implRenderableViewModel = viewModelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Component ? ", IRenderableViewModel" : "";
                sb.AppendLine("///<summary>");
                sb.AppendLine("/// Class is auto-generated from Tridion schema {0} ({1})", modelDefinition.Title, modelDefinition.TcmUri);
                sb.AppendLine("/// Date: {0}", DateTime.Now);
                sb.AppendLine("/// </summary>");
                sb.AppendLine("[{0}(\"{1}\", {2})]",
                    modelAttributeName, string.IsNullOrEmpty(modelDefinition.RootElementName) ? modelDefinition.Title : modelDefinition.RootElementName,
                    modelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Embedded ? "false" : "true");
                if (modelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Multimedia)
                {
                    sb.AppendLine("public partial class {0} : {1}", modelDefinition.TypeName, string.Join(",", Config.BaseClassesForMultimedia));
                }
                else if (modelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Embedded)
                {
                    sb.AppendLine("public partial class {0} : {1}", modelDefinition.TypeName, string.Join(",", Config.BaseClassesForEmbedded));
                }
                else
                {
                    sb.AppendLine("public partial class {0} : {1}", modelDefinition.TypeName, string.Join(",", Config.BaseClasses));
                }
                log.Debug("added partial class");
                sb.Indent();
                foreach (var propertyDefinition in modelDefinition.PropertyDefinitions)
                { 
                    sb.AppendLine("[{0}]", propertyDefinition.GetPropertyAttribute(modelDefinition.TypeName));
                    sb.Append("public virtual {0} {1} ",
                        propertyDefinition.IsMultipleValue ? "List<" + propertyDefinition.PropertyType + ">" : propertyDefinition.PropertyType,
                        modelDefinition.TypeName == propertyDefinition.Name ? propertyDefinition.Name + "Field" : propertyDefinition.Name);
                    sb.AppendLine("{ get; set; }");
                }
                if (modelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Multimedia)
                {
                    sb.AppendLine("[Multimedia]");
                    sb.AppendLine("public IMultimedia Multimedia { get; set; }");
                }

                if (!Config.NoRenderData)
                {
                    sb.AppendLine("[RenderData]");
                    sb.AppendLine("public IRenderData RenderData { get; set; }");
                }
                sb.Outdent();
                sb.AppendLine("");

                var key = modelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Region ? "Pages/" + modelDefinition.TypeName : "Entities/" + modelDefinition.TypeName;
                class2code.Add(key, sb.ToString());
                sb.Reset();
            }
            return class2code;
        }

        public override string WriteHeader()
        {
            IndentedStringBuilder sb = new IndentedStringBuilder(Config.IndentNrOfSpaces);
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using DD4T.ContentModel;");
            sb.AppendLine("using DD4T.Core.Contracts.ViewModels;");
            sb.AppendLine("using DD4T.Mvc.ViewModels.Attributes;");
            sb.AppendLine("using DD4T.ViewModels.Attributes;");
            sb.AppendLine("using DD4T.ViewModels.Base;");
            foreach (var usingStatement in Config.UsingNamespaces)
            {
                sb.AppendLine($"using {usingStatement};");
            }

            sb.AppendLine("");
            sb.AppendLine("namespace {0}", Config.ModelNamespace);
            sb.Indent();
            return sb.ToString();
        }
        public override string WriteFooter()
        {
            IndentedStringBuilder sb = new IndentedStringBuilder(Config.IndentNrOfSpaces);
            sb.Outdent();
            return sb.ToString();
        }

    }
}
