using System;
using System.Collections.Generic;
using System.Text;

namespace Ty.Core.Generic
{
    internal static class TypeHelper
    {
        public static Type Boolean { get; } = typeof(bool);

        public static Type NullableBoolean { get; } = typeof(bool?);

        public static Type Char { get; } = typeof(char);

        public static Type NullableChar { get; } = typeof(char?);

        public static Type SByte { get; } = typeof(sbyte);

        public static Type NullableSByte { get; } = typeof(sbyte?);

        public static Type Byte { get; } = typeof(byte);

        public static Type NullableByte { get; } = typeof(byte?);

        public static Type Short { get; } = typeof(short);

        public static Type NullableShort { get; } = typeof(short?);

        public static Type UShort { get; } = typeof(ushort);

        public static Type NullableUShort { get; } = typeof(ushort?);

        public static Type Int { get; } = typeof(int);

        public static Type NullableInt { get; } = typeof(int?);

        public static Type UInt { get; } = typeof(uint);

        public static Type NullableUInt { get; } = typeof(uint?);

        public static Type Long { get; } = typeof(long);

        public static Type NullableLong { get; } = typeof(long?);

        public static Type ULong { get; } = typeof(ulong);

        public static Type NullableULong { get; } = typeof(ulong?);

        public static Type Single { get; } = typeof(float);

        public static Type NullableSingle { get; } = typeof(float?);

        public static Type Double { get; } = typeof(double);

        public static Type NullableDouble { get; } = typeof(double?);

        public static Type Decimal { get; } = typeof(decimal);

        public static Type NullableDecimal { get; } = typeof(decimal?);

        public static Type DateTime { get; } = typeof(DateTime);

        public static Type NullableDateTime { get; } = typeof(DateTime?);

        public static Type String { get; } = typeof(String);

        public static Type Object { get; } = typeof(object);

        public static Type SecureString { get; } = typeof(System.Security.SecureString);

        public static Type DbParameter { get; } = typeof(System.Data.Common.DbParameter);

        public static Type Uri { get; } = typeof(Uri);

        public static Type Guid { get; } = typeof(Guid);

        public static Type ByteArray { get; } = typeof(byte[]);

        public static Type Dynamic { get; } = typeof(Dynamic);

        public static Type Type { get; } = typeof(Type);

        public static bool IsDynamic(Type type)
        {
            return type == Dynamic;
        }

        public static bool IsDynamic(object value)
        {
            return IsDynamic(value?.GetType());
        }

        public static bool IsByteArray(Type type)
        {
            return type == ByteArray;
        }

        public static bool IsEnum(Type type)
        {
            return type?.IsEnum ?? false;
        }

        public static bool IsUri(Type type)
        {
            return type == Uri;
        }

        public static bool IsGuid(Type type)
        {
            return type == Guid;
        }

        public static bool IsString(Type type)
        {
            return type == String || type == Char || type == NullableChar;
        }

        public static bool IsSecureString(Type type)
        {
            return type == SecureString;
        }

        public static bool IsDateTime(Type type)
        {
            return type == DateTime || type == NullableDateTime;
        }

        public static bool IsNumber(Type type)
        {
            return type == Int
                || type == UInt
                || type == NullableInt
                || type == NullableUInt
                || type == Long
                || type == ULong
                || type == NullableLong
                || type == NullableULong
                || type == Short
                || type == UShort
                || type == NullableShort
                || type == NullableUShort
                || type == Byte
                || type == NullableByte
                || type == SByte
                || type == NullableSByte
                || type == Single
                || type == NullableSingle
                || type == Double
                || type == NullableDouble
                || type == Decimal
                || type == NullableDecimal;
        }

        public static bool IsInt(Type type)
        {
            return type == Int
                || type == UInt;
        }

        public static bool IsChar(Type type)
        {
            return type == Char
                || type == NullableChar;
        }

        public static bool IsByte(Type type)
        {
            return type == Byte
                || type == NullableByte
                || type == SByte
                || type == NullableSByte;
        }

        public static bool IsBoolean(Type type)
        {
            return type == Boolean;
        }

        public static bool IsRefType(Type type)
        {
            return type?.IsClass ?? false;
        }

        public static bool IsRefType(object value)
        {
            return IsRefType(value?.GetType());
        }

        public static bool IsStringKeyDictionary(Type type)
        {
            return type != null
                && type.IsGenericType
                && type.GetInterface("System.Collections.IDictionary") != null
                && IsString(type.GetGenericArguments()[0]);
        }

        public static bool IsStringKeyDictionary(object value)
        {
            return IsStringKeyDictionary(value?.GetType());
        }

        public static bool IsGenericList(Type type)
        {
            return type != null
                 && type.IsGenericType
                && type.GetInterface("System.Collections.IList") != null
                && type.GetGenericArguments().Length == 1;
        }

        public static bool IsGenericList(object value)
        {
            return IsGenericList(value?.GetType());
        }

        public static bool IsNull(object value)
        {
            return value == null && Convert.IsDBNull(value);
        }

        public static bool IsType(Type type)
        {
            return type == Type;
        }

        public static bool IsDbParameter (Type type)
        {
            return type?.IsSubclassOf(DbParameter) ?? false;
        }

        public static bool IsDbParameterArray(Type type)
        {
            return IsGenericList(type) && IsDbParameter(type.GetGenericArguments()[0]);
        }
    }
}
