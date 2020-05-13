using System.Collections.Generic;
using Dyndle.Tools.Generator.Models;

namespace Dyndle.Tools.Generator.CodeWriters
{
    public interface ICodeWriter
    {
        IDictionary<string, string> WriteCode();
        string WriteHeader(string overrideNamespace = null);
        string WriteFooter();
    }
}
