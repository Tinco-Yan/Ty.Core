using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Data
{
    public abstract class ExcelPackage : IDisposable
    {
        public ExcelPackage()
        {

        }

        public virtual IExcelSheet AddSheet(string name)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
        }
    }
}
