using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Security;
using System.Text;
using Ty.Core.Json;

namespace Ty.Core.Generic
{
    public sealed class Dynamic : IJson, IConvertible, IEnumerable<Dynamic>, IEnumerable
    {
        private object _value;
        private Type _type;

        private Dictionary<string, Dynamic> _props;
        private int _keySeq;
        private const string __KEY_PREFIX = "__DYNAMIC_AUTO_KEY_";

        private bool _readonly;
        private bool _isList;
        private bool _isDictionary;

        private string NextKey => __KEY_PREFIX + _keySeq++;

        private int Count => Properties.Count;

        private Dictionary<string, Dynamic> Properties
        {
            get
            {
                if (_props == null)
                {
                    _props = new Dictionary<string, Dynamic>(StringComparer.OrdinalIgnoreCase);
                }
                return _props;
            }
            set
            {
                var props = Properties;
                props.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        props[item.Key] = value[item.Key];
                    }
                }
            }
        }

        public ICollection<string> Keys => Properties.Keys;

        private void Initialize()
        {
            this._value = null;
            this._type = typeof(object);
            this._keySeq = 0;
            this._readonly = false;
            this._isList = false;
            this._isDictionary = false;
            this._props = null;
        }

        private void CopyData(Dynamic dynamicValue)
        {
            this._value = dynamicValue._value;
            this._type = dynamicValue._type;
            this._keySeq = dynamicValue._keySeq;
            this._readonly = dynamicValue._readonly;
            this._isList = dynamicValue._isList;
            this._isDictionary = dynamicValue._isDictionary;
            this._props = dynamicValue._props;
        }

        private string GetKeyAt(int index)
        {
            if(index >= 0 && index < Count)
            {
                var n = 0;

                foreach(var key in Keys)
                {
                    if(n == index)
                    {
                        return key;
                    }
                    n++;
                }
            }

            return null;
        }

        private void SetValue(string key, object value)
        {
            SetValue(key, new Dynamic(value));
        }

        private void SetValue(string key, Dynamic value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = NextKey;
            }

            if(key.StartsWith(__KEY_PREFIX))
            {
                IsList = true;
            }
            else
            {
                IsDictionary = true;
            }

            Properties[key] = value;
        }

        private void SetValueAt(int index, Dynamic value)
        {
            var key = GetKeyAt(index);
            SetValue(key, value);
        }

        private Dynamic GetValueAt(int index)
        {
            var key = GetKeyAt(index);

            if(key != null)
            {
                return Properties[key];
            }
            return new Dynamic();
        }

        public Dynamic Add(object value)
        {
            return Add(new Dynamic(value));
        }

        public Dynamic Add(Dynamic value)
        {
            return Add(null, value);
        }

        public Dynamic Add(string key, Dynamic value)
        {
            this[key] = value;
            return this;
        }

        public bool ContainsKey(string key)
        {
            return !string.IsNullOrEmpty(key) && Properties.ContainsKey(key);
        }

        public Dynamic this[Dynamic key]
        {
            get
            {
                Dynamic value;

                if (IsList && key.IsInt && key >= 0 && key < Count)
                {
                    value = GetValueAt(key);
                }
                else if (ContainsKey(key))
                {
                    value = Properties[key];
                }
                else
                {
                    value = new Dynamic();
                    SetValue(key, value);
                }
                return value;
            }
            set
            {
                if (key != null && key.IsInt && key >= 0 && key < Count)
                {
                    SetValueAt(key, value);
                }
                else
                {
                    SetValue(key, value);
                }
            }
        }

        public Dynamic FirstOrDefault()
        {
            return GetValueAt(0);
        }

        public void MakeReadonly()
        {
            this._readonly = true;
        }

        public Dynamic() : this(null)
        {

        }

        public Dynamic(object value)
        {
            Initialize();

            if (TypeHelper.IsNull(value))
            {
                this._type = TypeHelper.Object;
                this._value = null;
            }
            else if (TypeHelper.IsDynamic(value))
            {
                Dynamic dynamicValue = value as Dynamic;

                if (dynamicValue == null)
                {
                    throw new Exception("Runtime Error occurred in Dynamic._Constructor. Failed to convert data type to Dynamic");
                }

                CopyData(dynamicValue);
            }
            else if (TypeHelper.IsStringKeyDictionary(value))
            {
                IEnumerable enumerable = value as IEnumerable;

                if (enumerable != null)
                {
                    foreach (var item in enumerable)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        var itype = item.GetType();
                        var pkey = itype.GetProperty("Key");
                        var pval = itype.GetProperty("Value");

                        if (pkey == null || pval == null || pkey.PropertyType != TypeHelper.String)
                        {
                            continue;
                        }

                        var key = pkey.GetValue(item);
                        var val = pval.GetValue(item);

                        SetValue(key?.ToString(), val);
                    }
                }
            }
            else if (TypeHelper.IsGenericList(value))
            {
                var itype = value?.GetType().GetGenericArguments()[0];
                IEnumerable enumerable = value as IEnumerable;

                if (itype != null && enumerable != null)
                {
                    var sb = new StringBuilder();
                    var arr = new List<byte>();

                    var isChar = TypeHelper.IsChar(itype);
                    var isByte = TypeHelper.IsByte(itype);

                    foreach (var item in enumerable)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        if (IsChar)
                        {
                            sb.Append(item);
                        }
                        else if (IsByte)
                        {
                            arr.Add((byte)item);
                        }
                        else
                        {
                            Add(item);
                        }
                    }

                    if (isChar)
                    {
                        this._type = TypeHelper.String;
                        this._value = sb.ToString();
                    }
                    else if (isByte)
                    {
                        this._type = TypeHelper.ByteArray;
                        this._value = Convert.ToBase64String(arr.ToArray());
                    }
                }
                else
                {
                    this._type = value?.GetType() ?? TypeHelper.Object;
                    this._value = value;
                }
            }
            else
            {
                this._type = value?.GetType() ?? TypeHelper.Object;
                this._value = value;
            }
        }

        public object Value => _value;

        public Type Type => _type;

        public bool IsEnum => TypeHelper.IsEnum(this.Type);

        public bool IsNumber => TypeHelper.IsNumber(this.Type);

        public bool IsBoolean => TypeHelper.IsBoolean(this.Type);

        public bool IsString => TypeHelper.IsString(this.Type);

        public bool IsList
        {
            get => _isList;
            private set
            {
                //can be setup once only
                if (!IsList && !IsDictionary)
                {
                    _isList = value;
                }
            }
        }
        
        public bool IsDictionary
        {
            get => _isDictionary;
            private set
            {
                // can be setup once only
                if (!IsList && !IsDictionary)
                {
                    _isDictionary = value;
                }
            }
        }

        public bool IsSimpleValue => !_isList && !_isDictionary;

        public bool IsDateTime => TypeHelper.IsDateTime(this.Type);

        public bool IsInt => TypeHelper.IsInt(this.Type);

        public bool IsByte => TypeHelper.IsByte(this.Type);

        public bool IsByteArray => TypeHelper.IsByteArray(this.Type);

        public bool IsChar => TypeHelper.IsChar(this.Type);

        public bool IsUri => TypeHelper.IsUri(this.Type);

        public bool IsSecureString => TypeHelper.IsSecureString(this.Type);

        public bool IsGuid => TypeHelper.IsGuid(this.Type);

        public bool IsType => TypeHelper.IsType(this.Type);

        public bool IsRefType => TypeHelper.IsRefType(this.Type);

        public bool IsDbParameter => TypeHelper.IsDbParameter(this.Type);

        public bool IsDbParameterArray
        {
            get
            {
                var result = IsList;

                if (result)
                {
                    foreach(var p in Properties.Values)
                    {
                        if (!p.IsDbParameter)
                        {
                            result = false;
                            break;
                        }
                    }
                }

                return result;
            }
        }

        public bool IsNull => Value == null && Count == 0;

        public bool IsReadonly => _readonly;

        public static Dynamic Parse(string value, Func<string, Dynamic, Dynamic> reviver = null)
        {
            return Json.Json.Parse(value, reviver);
        }

        public string ToJson(Func<string, object, object> replacer = null)
        {
            return Json.Json.Serialize(this, replacer);
        }

        #region Change Type
        public static T? ToNullable<T>(Dynamic value) where T : struct
        {
            try
            {
                return (T)Convert.ChangeType(value.Value, typeof(T));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Dynamic FromNullable<T>(T? value) where T : struct
        {
            Dynamic t = new Dynamic
            {
                _value = value,
                _type = typeof(T)
            };
            return t;
        }

        //bool
        public static implicit operator bool(Dynamic value)
        {
            bool? t = value;
            return t.HasValue ? t.Value : false;
        }

        public static implicit operator bool? (Dynamic value)
        {
            return ToNullable<bool>(value);
        }

        public static implicit operator Dynamic(bool value)
        {
            return (bool?)value;
        }

        public static implicit operator Dynamic(bool? value)
        {
            return FromNullable(value);
        }

        //byte
        public static implicit operator byte(Dynamic value)
        {
            byte? t = value;
            return t.HasValue ? t.Value : (byte)0;
        }

        public static implicit operator byte? (Dynamic value)
        {
            return ToNullable<byte>(value);
        }

        public static implicit operator Dynamic(byte value)
        {
            return (byte?)value;
        }

        public static implicit operator Dynamic(byte? value)
        {
            return FromNullable(value);
        }

        //char
        public static implicit operator char(Dynamic value)
        {
            char? t = value;
            return t.HasValue ? t.Value : (char)0;
        }

        public static implicit operator char? (Dynamic value)
        {
            return ToNullable<char>(value);
        }

        public static implicit operator Dynamic(char value)
        {
            return (char?)value;
        }

        public static implicit operator Dynamic(char? value)
        {
            return FromNullable(value);
        }

        //DateTime
        public static implicit operator DateTime(Dynamic value)
        {
            DateTime? t = value;
            return t.HasValue ? t.Value : DateTime.MinValue;
        }

        public static implicit operator DateTime? (Dynamic value)
        {
            return ToNullable<DateTime>(value);
        }

        public static implicit operator Dynamic(DateTime value)
        {
            return (DateTime?)value;
        }

        public static implicit operator Dynamic(DateTime? value)
        {
            return FromNullable(value);
        }

        //decimal
        public static implicit operator decimal(Dynamic value)
        {
            decimal? t = value;
            return t.HasValue ? t.Value : (decimal)0;
        }

        public static implicit operator decimal? (Dynamic value)
        {
            return ToNullable<decimal>(value);
        }

        public static implicit operator Dynamic(decimal value)
        {
            return (decimal?)value;
        }

        public static implicit operator Dynamic(decimal? value)
        {
            return FromNullable(value);
        }

        //double
        public static implicit operator double(Dynamic value)
        {
            double? t = value;
            return t.HasValue ? t.Value : (double)0;
        }

        public static implicit operator double? (Dynamic value)
        {
            return ToNullable<double>(value);
        }

        public static implicit operator Dynamic(double value)
        {
            return (double?)value;
        }

        public static implicit operator Dynamic(double? value)
        {
            return FromNullable(value);
        }

        //short
        public static implicit operator short(Dynamic value)
        {
            short? t = value;
            return t.HasValue ? t.Value : (short)0;
        }

        public static implicit operator short? (Dynamic value)
        {
            return ToNullable<short>(value);
        }

        public static implicit operator Dynamic(short value)
        {
            return (short?)value;
        }

        public static implicit operator Dynamic(short? value)
        {
            return FromNullable(value);
        }

        //int
        public static implicit operator int(Dynamic value)
        {
            int? t = value;
            return t.HasValue ? t.Value : (int)0;
        }

        public static implicit operator int? (Dynamic value)
        {
            return ToNullable<int>(value);
        }

        public static implicit operator Dynamic(int value)
        {
            return (int?)value;
        }

        public static implicit operator Dynamic(int? value)
        {
            return FromNullable(value);
        }

        //long
        public static implicit operator long(Dynamic value)
        {
            long? t = value;
            return t.HasValue ? t.Value : (long)0;
        }

        public static implicit operator long? (Dynamic value)
        {
            return ToNullable<long>(value);
        }

        public static implicit operator Dynamic(long value)
        {
            return (long?)value;
        }

        public static implicit operator Dynamic(long? value)
        {
            return FromNullable(value);
        }

        //ushort
        public static implicit operator ushort(Dynamic value)
        {
            ushort? t = value;
            return t.HasValue ? t.Value : (ushort)0;
        }

        public static implicit operator ushort? (Dynamic value)
        {
            return ToNullable<ushort>(value);
        }

        public static implicit operator Dynamic(ushort value)
        {
            return (ushort?)value;
        }

        public static implicit operator Dynamic(ushort? value)
        {
            return FromNullable(value);
        }

        //uint
        public static implicit operator uint(Dynamic value)
        {
            uint? t = value;
            return t.HasValue ? t.Value : (uint)0;
        }

        public static implicit operator uint? (Dynamic value)
        {
            return ToNullable<uint>(value);
        }

        public static implicit operator Dynamic(uint value)
        {
            return (uint?)value;
        }

        public static implicit operator Dynamic(uint? value)
        {
            return FromNullable(value);
        }

        //ulong
        public static implicit operator ulong(Dynamic value)
        {
            ulong? t = value;
            return t.HasValue ? t.Value : (ulong)0;
        }

        public static implicit operator ulong? (Dynamic value)
        {
            return ToNullable<ulong>(value);
        }

        public static implicit operator Dynamic(ulong value)
        {
            return (ulong?)value;
        }

        public static implicit operator Dynamic(ulong? value)
        {
            return FromNullable(value);
        }

        //sbyte
        public static implicit operator sbyte(Dynamic value)
        {
            sbyte? t = value;
            return t.HasValue ? t.Value : (sbyte)0;
        }

        public static implicit operator sbyte? (Dynamic value)
        {
            return ToNullable<sbyte>(value);
        }

        public static implicit operator Dynamic(sbyte value)
        {
            return (sbyte?)value;
        }

        public static implicit operator Dynamic(sbyte? value)
        {
            return FromNullable(value);
        }

        //float
        public static implicit operator float(Dynamic value)
        {
            float? t = value;
            return t.HasValue ? t.Value : (float)0;
        }

        public static implicit operator float? (Dynamic value)
        {
            return ToNullable<float>(value);
        }

        public static implicit operator Dynamic(float value)
        {
            return (float?)value;
        }

        public static implicit operator Dynamic(float? value)
        {
            return FromNullable(value);
        }

        //string
        public static implicit operator string(Dynamic value)
        {
            if (value == null || value.IsNull)
            {
                return "";
            }
            else if (value.IsType)
            {
                return (value.Value as Type)?.FullName;

            }
            else if (value.IsByteArray)
            {
                return Convert.ToBase64String((byte[])value.Value);
            }
            else if (value.IsEnum)
            {
                return Enum.GetName(value.Type, value.Value);
            }
            else if (value.Count > 0)
            {
                return value.Properties.ToString();
            }
            else
            {
                return value.Value.ToString();
            }
        }

        public static implicit operator Dynamic(string value)
        {
            Dynamic t = new Dynamic(value)
            {
                _type = TypeHelper.String
            };

            return t;
        }

        //Uri
        public static implicit operator Uri(Dynamic value)
        {
            try
            {
                Uri uri = null;
                if (value?.IsUri ?? false)
                {
                    uri = value._value as Uri;
                }
                return uri ?? new Uri(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static implicit operator Dynamic(Uri value)
        {
            return new Dynamic(value)
            {
                _type = TypeHelper.Uri
            };
        }

        //Guid
        public static implicit operator Guid(Dynamic value)
        {
            try
            {
                Guid guid = Guid.Empty;
                if (value?.IsGuid ?? false)
                {
                    guid = (Guid)value._value;
                }
                return new Guid((string)value);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        public static implicit operator Dynamic(Guid value)
        {
            return new Dynamic(value)
            {
                _type = TypeHelper.Guid
            };
        }

        //SecureString
        public static implicit operator SecureString(Dynamic value)
        {
            SecureString secureString;
            if (value == null && !value.IsSecureString)
            {
                secureString = new SecureString();

                if (value?.IsString ?? false)
                {
                    foreach (char c in (string)value ?? "")
                    {
                        secureString.AppendChar(c);
                    }
                }

                secureString.MakeReadOnly();
            }
            else
            {
                secureString = (SecureString)value._value;
            }

            return secureString;
        }

        public static implicit operator Dynamic(SecureString value)
        {
            return new Dynamic(value)
            {
                _type = TypeHelper.SecureString
            };
        }

        public static implicit operator DbParameter(Dynamic value)
        {
            DbParameter parameter = null;
            if (value?.IsDbParameter ?? false)
            {
                parameter = value.Value as DbParameter;
            }

            return parameter;
        }

        public static implicit operator Dynamic(DbParameter value)
        {
            return new Dynamic(value)
            {
                _type = TypeHelper.DbParameter
            };
        }

        public static implicit operator List<DbParameter>(Dynamic value)
        {
            List<DbParameter> parameters = new List<DbParameter>();
            if (value?.IsDbParameter ?? false)
            {
                parameters.Add(value.Value as DbParameter);
            }
            else if (value?.IsDbParameterArray ?? false)
            {
                foreach(var p in value.Properties.Values)
                {
                    parameters.Add(p);
                }
            }

            return parameters;
        }

        //byte[]
        public static implicit operator byte[] (Dynamic value)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public static implicit operator Dynamic(byte[] value)
        {
            return new Dynamic()
            {
                _value = Convert.ToBase64String(value),
                _type = TypeHelper.String
            };
        }

        //char[]
        public static implicit operator char[] (Dynamic value)
        {
            try
            {
                return ((string)value).ToCharArray();
            }
            catch (Exception)
            {
                return new char[0];
            }
        }

        public static implicit operator Dynamic(char[] value)
        {
            return new Dynamic()
            {
                _value = string.Join("", value),
                _type = TypeHelper.String
            };
        }

        //char[]
        public static implicit operator Type(Dynamic value)
        {
            try
            {
                return (Type)value.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static implicit operator Dynamic(Type value)
        {
            return new Dynamic()
            {
                _value = value,
                _type = TypeHelper.Type
            };
        }

        #endregion

        #region Interface IEnumerable<Dynamic>
        public IEnumerator<Dynamic> GetEnumerator()
        {
            return Properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Interface IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return this;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return this;
        }

        public char ToChar(IFormatProvider provider)
        {
            return this;
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return this;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return this;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return this;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return this;
        }

        public int ToInt32(IFormatProvider provider)
        {
            return this;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return this;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return this;
        }

        public float ToSingle(IFormatProvider provider)
        {
            return this;
        }

        public string ToString(IFormatProvider provider)
        {
            return this;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return this;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return this;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return this;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return this;
        }
        #endregion
    }
}
