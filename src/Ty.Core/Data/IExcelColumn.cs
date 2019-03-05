using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Data
{
    public interface IExcelColumn : IDisposable
    {
        void AutoFit();
        string SetFormat(Type dataType);
    }
}
