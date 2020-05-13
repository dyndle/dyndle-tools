using System;
using System.Runtime.Serialization;

namespace Dyndle.Tools.Generator.Models
{
    [Serializable]
    internal class TargetModelNotFoundException : Exception
    {
        public TargetModelNotFoundException()
        {
        }

        public TargetModelNotFoundException(string message) : base(message)
        {
        }

        public TargetModelNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TargetModelNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string TargetSchema { get; set; }
    }
}