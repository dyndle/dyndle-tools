using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.Core.Models
{
    public class InvalidEnvironmentException : Exception
    {
        public InvalidEnvironmentException(string message) : base(message) { }
        public InvalidEnvironmentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
