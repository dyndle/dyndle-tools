using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dyndle.Tools.Core.Models;
using Dyndle.Tools.Core.Utils;

namespace Dyndle.Tools.Core.CodeWriters
{
    public abstract class CodeWriterBase : ICodeWriter
    {
        protected IConfiguration Config { get; set; }
        protected ILog log;

        public CodeWriterBase(IConfiguration config)
        {
            Config = config;
            log = LogManager.GetLogger(GetType());
            Logger.Setup(config);
        }


        public class IndentedStringBuilder
        {
            int SpacesPerLevel { get; set; }
            int IndentLevel { get; set; }
            StringBuilder sb;
            bool atStartOfLine = true;
            public IndentedStringBuilder(int spacesPerLevel)
            {
                SpacesPerLevel = spacesPerLevel;
                IndentLevel = 0;
                sb = new StringBuilder();
            }
            public void Indent(bool addBracket = true)
            {
                if (addBracket)
                {
                    AppendIndenting();
                    sb.AppendLine("{");
                }
                IndentLevel++;
            }

            public void Outdent(bool addBracket = true)
            {
                if (IndentLevel > 0)
                {
                    IndentLevel--;
                }
                if (addBracket)
                {
                    AppendIndenting();
                    sb.AppendLine("}");
                }
            }
            public void Append(string s, params object[] p)
            {
                AppendIndenting();
                if (p.Length > 0)
                {
                    sb.Append(string.Format(s, p));
                }
                else
                {
                    sb.Append(s);
                }
                atStartOfLine = false;
            }
            public void AppendLine(string s, params object[] p)
            {
                AppendIndenting();
                if (p.Length > 0)
                {
                    sb.AppendLine(string.Format(s, p));
                }
                else
                {
                    sb.AppendLine(s);
                }
                atStartOfLine = true;
            }
            private void AppendIndenting()
            {
                if (!atStartOfLine)
                    return;

                for (int i = 0; i < IndentLevel; i++)
                {
                    if (SpacesPerLevel < 1)
                    {
                        sb.Append("\t");
                    }
                    else
                    {
                        for (int j = 0; j < SpacesPerLevel; j++)
                        {
                            sb.Append(" ");
                        }
                    }
                }

            }
            public override string ToString()
            {
                return sb.ToString();
            }
            public void Reset()
            {
                sb = new StringBuilder();
            }
        }

        public abstract IDictionary<string, string> WriteCode();
        public abstract string WriteHeader(string overrideNamespace = null);
        public abstract string WriteFooter();
    }
}
