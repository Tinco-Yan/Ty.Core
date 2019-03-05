using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Data
{
    public interface IExcelSheet : IDisposable
    {
        IExcelRange Range(int row, int col);

        IExcelColumn Column(int col);
    }
}
