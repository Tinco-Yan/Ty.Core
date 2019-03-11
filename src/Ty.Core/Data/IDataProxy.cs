using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Ty.Core.Generic;

namespace Ty.Core.Data
{
    public interface IDataProxy
    {
        Dynamic Run(string sql);

        Dynamic Run(DbTransaction transaction, string sql);

        Dynamic Run(string sql, IList<DbParameter> parameters);

        Dynamic Run(string sql, CommandType type);

        Dynamic Run(string sql, CommandType type, IList<DbParameter> parameters);

        Dynamic Run(DbTransaction transaction, string sql, IList<DbParameter> parameters);

        Dynamic Run(DbTransaction transaction, string sql, CommandType type);

        Dynamic Run(DbTransaction transaction, string sql, CommandType type, IList<DbParameter> parameters);
        Dynamic Export(string sql);

        Dynamic Export(DbTransaction transaction, string sql);

        Dynamic Export(string sql, IList<DbParameter> parameters);

        Dynamic Export(string sql, CommandType type);

        Dynamic Export(string sql, CommandType type, IList<DbParameter> parameters);

        Dynamic Export(DbTransaction transaction, string sql, IList<DbParameter> parameters);

        Dynamic Export(DbTransaction transaction, string sql, CommandType type);

        Dynamic Export(DbTransaction transaction, string sql, CommandType type, IList<DbParameter> parameters);
    }
}
