using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Data
{
    public interface IExcelRange: IDisposable
    {
        object Value { get; set; }

        string SetFormat(Type dataType);
    }
}
