using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Ty.Core.Options;

namespace Ty.Core.Data
{
    internal class DbConnectionProvider : IProvider<DbConnection>
    {
        TyCoreOptions _options;
        public DbConnectionProvider(TyCoreOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public DbConnection Get()
        {
            Type type = _options.DbConnectionType ?? throw new NullReferenceException(nameof(_options.DbConnectionType));
            string connectionString = _options.ConnectionString ?? throw new NullReferenceException(nameof(_options.ConnectionString));
            
            return Activator.CreateInstance(type, new object[] { connectionString }) as DbConnection;
        }
    }
}
