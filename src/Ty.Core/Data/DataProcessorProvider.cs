using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Data
{
    internal class DataProcessorProvider : IProvider<DataProcessor>
    {
        public DataProcessor Get()
        {
            return new DefaultDataProcessor();
        }
    }
}
