using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dyndle.Tools.Generator.Models;
using Dyndle.Tools.Generator.Registry;
using Dyndle.Tools.Core.Utils;

namespace Dyndle.Tools.Generator.CodeWriters
{
    public class RazorCodeWriter : CodeWriterBase
    {
        public RazorCodeWriter (IGeneratorConfiguration config) : base(config)
        {
        }

        private static string ViewStartResourcePath => "Dyndle.Tools.Generator.Resources._ViewStart.cshtml";
        private static string LayoutResourcePath => "Dyndle.Tools.Generator.Resources._GeneratedLayout.cshtml";

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
                sb.AppendLine("@model {0}", view.ModelTypeName);
                foreach (var usingStatement in Config.UsingNamespaces)
                {
                    sb.AppendLine($"@using {usingStatement}");
                }
                sb.AppendLine($"@using {Config.ModelNamespace}.Pages");
                sb.AppendLine($"@using {Config.ModelNamespace}.Entities");
                sb.AppendLine("@using Dyndle.Modules.Core.Html");
                sb.AppendLine("<div class=\"container\">");
                sb.Indent(false);
                if (view.AssociatedModelDefinition != null)
                {
                    RazorForModel(view.AssociatedModelDefinition, view.ViewName, sb);
                }
                else
                {
                    sb.AppendLine("<div class=\"container\"><!--This view is not associated with a single concrete model. Please implement it yourself--></div>");
                }
                sb.Outdent(false);
                sb.AppendLine("</div>");

                var key = view.ViewPurpose.ToString() + "/" + view.ViewName;
                class2code.Add(key, sb.ToString());
                sb.Reset();
            }

            // include a layout and a viewstart
            var viewstart = ResourceUtils.GetResourceAsString(ViewStartResourcePath);
            var layout = ResourceUtils.GetResourceAsString(LayoutResourcePath);
            class2code.Add("Page/_ViewStart", viewstart);
            class2code.Add("Shared/_GeneratedLayout", layout);
            return class2code;
        }

        private void RazorForModel(ModelDefinition modelDefinition, string viewName, IndentedStringBuilder sb, string modelVariable = "Model")
        {            
            sb.AppendLine("<table class=\"table table-dark\">");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>Model Type</td>");
            sb.AppendLine("<td>");
            sb.AppendLine($"@{modelVariable}.GetType()");
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
            if (viewName != null)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>View Name</td>");
                sb.AppendLine("<td>");
                sb.AppendLine($"{viewName}");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
            }
            if (modelDefinition.Purpose == Tridion.ContentManager.CoreService.Client.SchemaPurpose.Multimedia)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>Multimedia</td>");
                sb.AppendLine("<td>");
                sb.AppendLine($"@if ({modelVariable}.Multimedia.MimeType == \"image/png\" || {modelVariable}.Multimedia.MimeType == \"image/jpeg\" || {modelVariable}.Multimedia.MimeType == \"image/gif\")");
                sb.Indent();
                sb.AppendLine($"<img src=\"@{modelVariable}.Multimedia.Url\" />");
                sb.Outdent();
                sb.AppendLine("else");
                sb.Indent();
                sb.AppendLine($"<a href=\"@{modelVariable}.Multimedia.Url\">@{modelVariable}.Multimedia.Url</a>");
                sb.Outdent();
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");


            }

            foreach (var propertyDefinition in modelDefinition.PropertyDefinitions)
            {
                sb.Indent(false);
                sb.AppendLine("<tr>");
                sb.Indent(false);
                sb.AppendLine($"<td>{propertyDefinition.Name}</td>");
                sb.AppendLine("<td>");

                var varName = propertyDefinition.IsMultipleValue ? propertyDefinition.Name?.GetSingular()?.LCFirst() : modelVariable + "." + propertyDefinition.Name;
                sb.AppendLine($"@if ({modelVariable}.{propertyDefinition.Name} != null)");
                sb.Indent();
                if (propertyDefinition.IsMultipleValue)
                {
                    if ((! propertyDefinition.IsComplexType) || propertyDefinition.FieldType == FieldType.Regions)
                    {
                        sb.AppendLine("<ul>");
                        sb.AppendLine($"@foreach (var {varName} in {modelVariable}.{propertyDefinition.Name})");
                    }
                    else
                    {
                        sb.AppendLine($"foreach (var {varName} in {modelVariable}.{propertyDefinition.Name})");
                    }
                    sb.Indent();
                }
                if (propertyDefinition.FieldType == FieldType.Entities)
                {
                    sb.AppendLine($"@Html.RenderEntity({varName})");
                }
                else if (propertyDefinition.FieldType == FieldType.Regions)
                {
                    sb.AppendLine($"<li>Region with name: @{varName}.Name and view @{varName}.ViewName, containing @{varName}.Entities.Count entities</li>");
                }
                else if (propertyDefinition.IsComplexType)
                {
                    if ((propertyDefinition.TargetSchemas == null || !propertyDefinition.TargetSchemas.Any()) && (propertyDefinition.FieldType == FieldType.ComponentLink || propertyDefinition.FieldType == FieldType.MultiMediaLink))
                    {
                        sb.AppendLine($"link without a specific target model<br/>Resolved URL: {varName}.Url");
                    }
                    else
                    {
                        // get the model definition for the embedded schema
                        var embeddedModel = ModelRegistry.GetInstance(Config).GetViewModelDefinition(propertyDefinition.TargetSchemas.FirstOrDefault()?.IdRef);
                        RazorForModel(embeddedModel, null, sb, varName);
                    }
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
                    if ((!propertyDefinition.IsComplexType) || propertyDefinition.FieldType == FieldType.Regions)
                    {
                        sb.AppendLine("</ul>");
                    }
                }
                sb.Outdent();
                sb.AppendLine("</td>");
                sb.Outdent(false);
                sb.AppendLine("</tr>");
                sb.Outdent(false);
            }
            sb.AppendLine("</table>");
        }

       


        public override string WriteHeader(string overrideNamespace = null)
        {
            return string.Empty;
        }

        public override string WriteFooter()
        {
            return string.Empty;
        }
     }
}
