using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Ty.Core.Generic;

namespace Ty.Core.Json
{
    internal class JsonSerializer
    {
        private const int MAX_DEPTH = 10;
        private int _depth = 0;
        private object _value;
        private Func<string, object, object> _replacer;

        private StringBuilder _writer = null;
        private static readonly Dictionary<string, string> _escapee = new Dictionary<string, string>()
        {
            ["\""] = "\\\"",
            ["\\"] = "\\\\",
            ["/"] = "/",
            ["\b"] = "\\b",
            ["\f"] = "\\f",
            ["\n"] = "\\n",
            ["\r"] = "\\r",
            ["\t"] = "\\t"
        };
        private static Regex _escapable = new Regex("[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]", RegexOptions.Multiline);
        private static string _dateFormat = "yyyy-MM-ddTHH:mm:ss.fffffff";

        public JsonSerializer(object value) : this(value, null)
        {
        }

        public JsonSerializer(object value, Func<string, object, object> replacer = null)
        {
            _value = value;
            _replacer = replacer ?? ((s, o) => o);
        }

        private void Null()
        {
            _writer.Append("null");
        }

        private void True()
        {
            _writer.Append("true");
        }

        private void False()
        {
            _writer.Append("false");
        }

        private void Comma()
        {
            _writer.Append(",");
        }
        private void RemoveLastComma()
        {
            var len = _writer.Length;
            if (len > 0 && _writer[len - 1] == ',')
            {
                _writer.Remove(len - 1, 1);
            }
        }

        private void Colon()
        {
            _writer.Append(":");
        }

        private void Quote()
        {
            _writer.Append("\"");
        }

        private void ArrayStart()
        {
            _writer.Append("[");
        }

        private void ArrayEnd()
        {
            RemoveLastComma();
            _writer.Append("]");
        }

        private void ObjectStart()
        {
            _writer.Append("{");
        }

        private void ObjectEnd()
        {
            RemoveLastComma();
            _writer.Append("}");
        }

        private void Boolean(bool? value)
        {
            (value.HasValue ? value.Value ? (Action)True : False : Null)();
        }

        private void DateTime(DateTime? value)
        {
            (value.HasValue ? () =>
            {
                Quote();
                _writer.Append(value.Value.ToString(_dateFormat));
                Quote();
            }
            : (Action)Null)();
        }

        private void Number(string number)
        {
            _writer.Append(number);
        }

        private void String(string value)
        {
            if (_escapable.IsMatch(value))
            {
                value = _escapable.Replace(value, (a) =>
                {
                    string k = a.Value, c;

                    if (_escapee.ContainsKey(k))
                    {
                        c = _escapee[k];
                    }
                    else
                    {
                        string hex = "0000" + Convert.ToByte(k[0]).ToString("x");

                        c = "\\u" + hex.Substring(hex.Length - 4);
                    }

                    return c;
                });
            }

            Quote();
            _writer.Append(value);
            Quote();
        }

        private void Object(object any)
        {
            try
            {
                Dynamic val = new Dynamic(any);
                Type type = val.Type;

                if (val.IsNull)
                {
                    Null();
                }
                else if (val.IsString || val.IsType)
                {
                    String(val);
                }
                else if (val.IsUri)
                {
                    Uri uri = val;
                    String(uri?.AbsolutePath);
                }
                else if (val.IsNumber)
                {
                    Number(val);
                }
                else if (val.IsBoolean)
                {
                    Boolean(val);
                }
                else if (val.IsDateTime)
                {
                    DateTime(val);
                }
                else if (val.IsEnum)
                {
                    Number(((int)val).ToString());
                }
                else
                {
                    _depth++;
                    if (_depth > MAX_DEPTH)
                    {
                        return;
                    }
                    if (val.IsList)
                    {
                        ArrayStart();

                        foreach (var item in val)
                        {
                            Object(item);
                            Comma();
                        }

                        ArrayEnd();
                    }
                    else if (val.IsDictionary)
                    {
                        ObjectStart();

                        foreach (var key in val.Keys)
                        {
                            var v = _replacer(key, val[key]);
                            String(key);
                            Colon();
                            Object(v);
                            Comma();
                        }

                        ObjectEnd();
                    }
                    else if (val.IsRefType)
                    {
                        var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (props != null && props.Length > 0)
                        {
                            ObjectStart();

                            foreach (var p in props)
                            {
                                var ps = p.GetMethod.GetParameters();
                                if (ps != null && ps.Length > 0)
                                {
                                    continue;
                                }
                                var v = _replacer(p.Name, p.GetValue(val.Value));
                                String(p.Name);
                                Colon();
                                Object(v);
                                Comma();
                            }
                            ObjectEnd();
                        }
                    }
                    else
                    {
                        Null();
                    }
                    _depth--;
                }
            }
            catch(Exception e)
            {
                Null();
            }
        }

        public string Serialize()
        {
            _depth = 0;
            _writer = new StringBuilder();
            Object(_value);
            return _writer.ToString();
        }
    }
}
