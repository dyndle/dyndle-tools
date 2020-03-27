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
    public class RazorCodeWriter : CodeWriterBase
    {
        public RazorCodeWriter (IConfiguration config) : base(config)
        {
        }

        public override IDictionary<string,string> WriteCode()
        {
            IDictionary<string, string> class2code = new Dictionary<string, string>();
            IndentedStringBuilder sb = new IndentedStringBuilder(Config.IndentNrOfSpaces);
            ViewRegistry viewRegistry = ViewRegistry.GetInstance(Config);

            log.DebugFormat("Started GenerateCode with {0} view models", viewRegistry.Views.Count);
            foreach (var view in viewRegistry.Views)
            {
                sb.AppendLine("@{");
                sb.AppendLine("///");
                sb.AppendLine("/// View is auto-generated from Tridion template {0} ({1})", view.Title, view.TcmUri);
                sb.AppendLine("/// Date: {0}", DateTime.Now);
                sb.AppendLine("}");
                sb.AppendLine("@model {0}", view.AssociatedModelDefinition.TypeName);
                sb.AppendLine("<div class=\"dyndle-template-field\">");
                sb.Indent(false);
                RazorForModel(view.AssociatedModelDefinition, sb);
                sb.Outdent(false);
                sb.AppendLine("</div>");

                var key = view.ViewPurpose.ToString() + "/" + view.ViewName;
                class2code.Add(key, sb.ToString());
                sb.Reset();
            }
            return class2code;
        }

        private void RazorForModel(ModelDefinition modelDefinition, IndentedStringBuilder sb, string modelVariable = "Model")
        {
            sb.AppendLine("<table>");
            foreach (var propertyDefinition in modelDefinition.PropertyDefinitions)
            {
                sb.Indent(false);
                sb.AppendLine("<tr>");
                sb.Indent(false);
                sb.AppendLine($"<td>{propertyDefinition.Name}</td>");
                sb.AppendLine("<td>");

                var varName = propertyDefinition.IsMultipleValue ? GetSingular(propertyDefinition.Name) : modelVariable + "." + propertyDefinition.Name;
                if (propertyDefinition.IsMultipleValue)
                {
                    if (! propertyDefinition.IsComplexType)
                    {
                        sb.AppendLine("<ul>");
                    }

                    sb.AppendLine($"@foreach (var {varName} in {modelVariable}.{propertyDefinition.Name})");
                    sb.Indent();
                }
                if (propertyDefinition.IsComplexType)
                {
                    // get the model definition for the embedded schema
                    var embeddedModel = ModelRegistry.GetInstance(Config).GetViewModelDefinition(propertyDefinition.TargetSchemas.FirstOrDefault()?.IdRef);
                    RazorForModel(embeddedModel, sb, varName);
                }
                else
                {
                    if (propertyDefinition.IsMultipleValue)
                    {
                        sb.AppendLine($"<li>@{varName}</li>");
                    }
                    else
                    {
                        sb.AppendLine($"@{varName}");
                    }
                }
                if (propertyDefinition.IsMultipleValue)
                {
                    sb.Outdent();
                    if (!propertyDefinition.IsComplexType)
                    {
                        sb.AppendLine("</ul>");
                    }
                }
                sb.AppendLine("</td>");
                sb.Outdent(false);
                sb.AppendLine("</tr>");
                sb.Outdent(false);
            }
            sb.AppendLine("</table>");
        }

        private static string GetSingular(string name)
        {
            return (name.EndsWith("s") ? name.Substring(0, name.Length - 1) : name).LCFirst();
        }


        public override string WriteHeader(string overrideNamespace = null)
        {
            IndentedStringBuilder sb = new IndentedStringBuilder(Config.IndentNrOfSpaces);
            foreach (var usingStatement in Config.UsingNamespaces)
            {
                sb.AppendLine($"using {usingStatement};");
            }
            return sb.ToString();
        }
        public override string WriteFooter()
        {
            return string.Empty;
        }
     }
}
