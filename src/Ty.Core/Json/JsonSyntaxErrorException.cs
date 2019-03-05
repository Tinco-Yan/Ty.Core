using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Json
{
    public class JsonSyntaxErrorException : Exception
    {
        protected JsonSyntaxErrorException()
        {

        }
        protected JsonSyntaxErrorException(string message, Exception innerException = null)
        {

        }

        public JsonSyntaxErrorException(string message, int at, string text, Exception innerException = null) : base(message, innerException)
        {
            this.At = at;
            this.Text = text;
        }

        public string Name { get; } = "SyntaxError";

        public int At { get; set; }

        public string Text { get; set; }
    }
}
