using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Data
{
    public interface IProvider<T>
    {
        T Get();
    }
}
