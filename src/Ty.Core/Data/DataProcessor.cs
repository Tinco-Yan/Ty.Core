using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Ty.Core.Generic;

namespace Ty.Core.Data
{
    public abstract class DataProcessor
    {
        protected DbConnection _connection;
        protected DbCommand _command;
        protected ConnectionState _state;
        protected IDataReader _reader;
                
        private int? _fieldCount;
        public DataProcessor()
        {
        }
        private void Initialize()
        {
            _connection = _command.Connection;
            _state = _connection.State;

            if(_state == ConnectionState.Closed)
            {
                _connection.Open();
            }

            _reader = _command.ExecuteReader();
        }

        private void CloseReader()
        {
            if(_reader != null)
            {
                if (!_reader.IsClosed)
                {
                    _reader.Close();
                }

                _reader.Dispose();
                _reader = null;
            }
        }

        private void ResetConnection()
        {
            if(_state == ConnectionState.Closed && _connection?.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        protected bool ReadLine() => _reader?.Read() ?? false;

        protected bool ReadNextResult()
        {
            _fieldCount = null;
            return _reader?.NextResult() ?? false;
        }

        protected int FieldCount => _fieldCount ?? (_fieldCount = _reader?.FieldCount ?? 0).Value;


        protected virtual string ConvertFieldName(string name) => name;

        protected string GetFieldName(int i) => ConvertFieldName(_reader?.GetName(i));

        protected object GetFieldValue(int i) => _reader?.GetValue(i);

        protected Dynamic GetDynamicValue(int i) => new Dynamic(GetFieldValue(i));

        protected Type GetFieldType(int i) => _reader?.GetFieldType(i);

        protected virtual bool ReadParameter => true;

        protected virtual Dynamic Process()
        {
            throw new NotImplementedException();
        }

        private void ProcessParameter(Dynamic data)
        {
            if(data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if(_command?.Parameters != null)
            {
                foreach(IDbDataParameter p in _command.Parameters)
                {
                    if(p.Direction == ParameterDirection.InputOutput
                        || p.Direction == ParameterDirection.Output)
                    {
                        data["Output"][ConvertFieldName(p.ParameterName)] = new Dynamic(p.Value);
                    }
                    else if(p.Direction == ParameterDirection.ReturnValue)
                    {
                        data["ReturnValue"] = new Dynamic(p.Value);
                    }
                }
            }
        }

        public Dynamic Read(DbCommand command)
        {
            try
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));

                Initialize();

                var data = Process();

                if (ReadParameter)
                {
                    ProcessParameter(data);
                }

                return data;
            }
            finally
            {
                CloseReader();
                ResetConnection();
            }
        }
    }
}
