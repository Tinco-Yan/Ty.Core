using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Ty.Core.Data;
using Ty.Core.Generic;

namespace Ty.Core.Options
{
    public class TyCoreOptions
    {
        public Type DbConnectionType { get; set; }

        public string ConnectionString { get; set; }

        public IProvider<DataProcessor> DataProcessorProvider { get; set; }
    }
}
