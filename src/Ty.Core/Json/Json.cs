using System;
using System.Collections.Generic;
using System.Text;
using Ty.Core.Generic;

namespace Ty.Core.Json
{
    internal class Json
    {
        public static Dynamic Parse(string text, Func<string, Dynamic, Dynamic> reviver = null)
        {
            return new JsonParser(text, reviver).Parse();
        }

        public static string Serialize(object value, Func<string, object, object> replacer = null)
        {
            return new JsonSerializer(value, replacer).Serialize();
        }
    }
}
