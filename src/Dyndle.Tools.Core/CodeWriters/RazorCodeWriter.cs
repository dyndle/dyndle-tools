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
                sb.AppendLine("@model {0}", view.ModelTypeName);
                sb.AppendLine("<div class=\"dyndle-template-field\">");
                sb.Indent(false);
                sb.AppendLine("<table>");
                foreach (var propertyDefinition in view.AssociatedModelDefinition.PropertyDefinitions)
                {
                    sb.Indent(false);
                    sb.AppendLine("<tr>");
                    sb.Indent(false);
                    sb.AppendLine($"<td>{propertyDefinition.Name}</td>");
                    sb.AppendLine($"<td>@Model.{propertyDefinition.Name}</td>");
                    sb.Outdent(false);
                    sb.AppendLine("</tr>");
                    sb.Outdent(false);
                }
                sb.AppendLine("</table>");
                sb.Outdent(false);
                sb.AppendLine("</div>");
                var key = view.ViewPurpose.ToString() + "/" + view.ViewName;
                class2code.Add(key, sb.ToString());
                sb.Reset();
            }
            return class2code;
        }

        public override string WriteHeader()
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
