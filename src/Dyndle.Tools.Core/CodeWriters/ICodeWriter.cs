using System.Collections.Generic;
using Dyndle.Tools.Core.Models;

namespace Dyndle.Tools.Core.CodeWriters
{
    public interface ICodeWriter
    {
        IDictionary<string, string> WriteCode();
        string WriteHeader();
        string WriteFooter();
    }
}
