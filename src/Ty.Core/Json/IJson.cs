using System;
using System.Collections.Generic;
using System.Text;
using Ty.Core.Generic;

namespace Ty.Core.Json
{
    public interface IJson
    {
        string ToJson(Func<string, object, object> replacer = null);
    }
}
