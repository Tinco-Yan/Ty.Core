using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace Ty.Core.Options
{
    internal class TyCoreOptionsSetup : IConfigureOptions<TyCoreOptions>
    {
        public void Configure(TyCoreOptions options)
        {
            options.DbConnectionType = null;
            options.ConnectionString = null;
            options.DataProcessorProvider = null;
        }
    }
}
