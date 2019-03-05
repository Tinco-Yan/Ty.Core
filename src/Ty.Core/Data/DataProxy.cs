using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using Ty.Core.Builder;
using Ty.Core.Generic;
using Ty.Core.Options;

namespace Ty.Core.Data
{
    internal class DataProxy : IDataProxy
    {
        private IServiceProvider _provider;
        private TyCoreOptions _options;
        private IProvider<DbConnection> _dbConnectionProvider;
        private IProvider<DataProcessor> _dataProcessorProvider;

        public DataProxy(IServiceProvider provider, IOptions<TyCoreOptions> optionsAccessor)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _dbConnectionProvider = new DbConnectionProvider(_options);
            _dataProcessorProvider = _options.DataProcessorProvider ?? new DataProcessorProvider();
        }

        private DbParameter CloneParameter(DbParameter parameter)
        {
            try
            {
                if (parameter == null)
                {
                    return null;
                }

                Type tDbParameter = parameter.GetType();

                DbParameter clone = Activator.CreateInstance(tDbParameter) as DbParameter;

                PropertyInfo[] props = tDbParameter.GetProperties(BindingFlags.Public | BindingFlags.SetField | BindingFlags.Instance);

                foreach (var p in props)
                {
                    p.SetValue(clone, p.GetValue(parameter));
                }


                return clone;
            }
            catch (Exception)
            {
                return parameter;
            }
        }

        private string FormatMessage(string sql, IList<DbParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(sql);

            if(parameters != null && parameters?.Count > 0)
            {
                sb.Append("Parameter");
                if (parameters?.Count > 1)
                {
                    sb.Append("s");
                }
                sb.AppendLine();

                foreach (var p in parameters)
                {
                    sb.AppendLine(string.Format(
                        "ParameterName: {0}, DbType: {1}, Direction: {2}, Value: {3}",
                        p.ParameterName,
                        p.DbType,
                        p.Direction,
                        p.Value
                    ));
                }
            }

            return sb.ToString();
        }

        public virtual DbConnection Connection => _dbConnectionProvider.Get();

        protected virtual DataProcessor DataProcessor => _dataProcessorProvider.Get();

        public Dynamic Run(string sql) => Run(null, sql);

        public Dynamic Run(DbTransaction transaction, string sql) => Run(transaction, sql, CommandType.Text);

        public Dynamic Run(string sql, IList<DbParameter> parameters) => Run(null, sql, parameters);

        public Dynamic Run(string sql, CommandType type) => Run(null, sql, type);

        public Dynamic Run(string sql, CommandType type, IList<DbParameter> parameters) => Run(null, sql, type, parameters);

        public Dynamic Run(DbTransaction transaction, string sql, IList<DbParameter> parameters) => Run(transaction, sql, CommandType.Text, parameters);

        public Dynamic Run(DbTransaction transaction, string sql, CommandType type) => Run(transaction, sql, type, null);

        public Dynamic Run(DbTransaction transaction, string sql, CommandType type, IList<DbParameter> parameters)
        {
            DbConnection connection = null;
            DbCommand command = null;

            try
            {
                if (transaction == null)
                {
                    connection = Connection;
                    command = connection.CreateCommand();
                }
                else
                {
                    command = transaction?.Connection.CreateCommand();
                }

                command.CommandText = sql;
                command.CommandType = type;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        command.Parameters.Add(CloneParameter(p));
                    }
                }

                return DataProcessor.Read(command);
            }
            catch (Exception e)
            {
                return new Dynamic()
                {
                    ["Code"] = 1,
                    ["Message"] = e.Message,
                    ["Exception"] = new Dynamic(e),
                    ["Sql"] = FormatMessage(sql, parameters)
                };
            }
            finally
            {
                if(command != null)
                {
                    command.Dispose();
                    command = null;
                }

                if(connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }

                    connection.Dispose();
                    connection = null;
                }
            }
        }


        public Dynamic Export(string sql) => Export(null, sql);

        public Dynamic Export(DbTransaction transaction, string sql) => Export(transaction, sql, CommandType.Text);

        public Dynamic Export(string sql, IList<DbParameter> parameters) => Export(null, sql, parameters);

        public Dynamic Export(string sql, CommandType type) => Export(null, sql, type);

        public Dynamic Export(string sql, CommandType type, IList<DbParameter> parameters) => Export(null, sql, type, parameters);

        public Dynamic Export(DbTransaction transaction, string sql, IList<DbParameter> parameters) => Export(transaction, sql, CommandType.Text, parameters);

        public Dynamic Export(DbTransaction transaction, string sql, CommandType type) => Export(transaction, sql, type, null);

        public Dynamic Export(DbTransaction transaction, string sql, CommandType type, IList<DbParameter> parameters)
        {
            DbConnection connection = null;
            DbCommand command = null;

            try
            {
                if (transaction == null)
                {
                    connection = Connection;
                    command = connection.CreateCommand();
                }
                else
                {
                    command = transaction?.Connection.CreateCommand();
                }

                command.CommandText = sql;
                command.CommandType = type;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        command.Parameters.Add(CloneParameter(p));
                    }
                }

                return new ExcelDataProcessor(_provider).Read(command);
            }
            catch (Exception e)
            {
                return new Dynamic()
                {
                    ["Code"] = 1,
                    ["Message"] = e.Message,
                    ["Exception"] = new Dynamic(e),
                    ["Sql"] = FormatMessage(sql, parameters)
                };
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }

                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }

                    connection.Dispose();
                    connection = null;
                }
            }
        }
    }
}
