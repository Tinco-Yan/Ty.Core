using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Ty.Core.Generic;

namespace Ty.Core.Json
{
    internal class JsonParser
    {
        private static Regex regexDate = new Regex("^(\\d{4})-(\\d{2})-(\\d{2})T(\\d{2}):(\\d{2}):(\\d{2}(?:\\.\\d*)?)(?:(Z|\\+\\d{1,2}:\\d{1,2}))?$", RegexOptions.Multiline);
        private static Dictionary<char, char> _escapee = new Dictionary<char, char>()
        {
            ['\"'] = '\"',
            ['\\'] = '\\',
            ['/'] = '/',
            ['b'] = '\b',
            ['f'] = '\f',
            ['n'] = '\n',
            ['r'] = '\r',
            ['t'] = '\t'
        };

        private string _text;
        private Func<string, Dynamic, Dynamic> _reviver;

        private int _at, _len;
        private char _ch;

        public JsonParser(string text) : this(text, null)
        {
        }

        public JsonParser(string text, Func<string, Dynamic, Dynamic> reviver)
        {
            _text = text;
            _len = text?.Length ?? 0;
            _reviver = reviver ?? ((s, o) => o);
        }

        private void Error(string message, Exception innerExeption = null)
        {
            throw new JsonSyntaxErrorException(message, _at, _text, innerExeption);
        }

        private bool Next()
        {
            if (_at < _len)
            {
                _ch = _text[_at++];
                return true;
            }
            return false;
        }

        private bool Next(char c)
        {
            if (c != _ch)
            {
                Error(string.Format("Expected '{0}' instead of '{1}'", c, _ch));
            }
            return Next();
        }

        private void White()
        {
            // Skip whitespace.
            while (_ch <= ' ' && Next())
            {
            }
        }

        private Dynamic Number()
        {
            double value = 0;
            string str = "";

            if (_ch == '-')
            {
                str = "-";
                Next('-');
            }

            while (_ch >= '0' && _ch <= '9')
            {
                str += _ch;
                Next();
            }

            if (_ch == '.')
            {
                str += ".";

                while (Next() && _ch >= '0' && _ch <= '9')
                {
                    str += _ch;
                }
            }

            if (_ch == 'e' || _ch == 'E')
            {
                str += _ch;
                Next();
                if (_ch == '-' || _ch == '+')
                {
                    str += _ch;
                }
                while (Next() && _at < _len && _ch >= '0' && _ch <= '9')
                {
                    str += _ch;
                }
            }

            try
            {
                value = Convert.ToDouble(str);
            }
            catch (Exception e)
            {
                Error("Bad number", e);
            }
            return value;
        }

        private string Str()
        {
            int hex;
            int i;
            string value = "";
            int uffff;

            // When parsing for string values, we must look for " and \ characters.
            if (_ch == '\"')
            {
                while (Next())
                {
                    if (_ch == '\"')
                    {
                        Next();
                        return value;
                    }
                    if (_ch == '\\')
                    {
                        Next();
                        if (_ch == 'u')
                        {
                            uffff = 0;
                            for (i = 0; i < 4; i++)
                            {
                                Next();
                                try
                                {
                                    hex = Int32.Parse(_ch.ToString(), System.Globalization.NumberStyles.HexNumber);
                                    uffff = uffff * 16 + hex;
                                }
                                catch (Exception)
                                {
                                    //invalid hex string
                                    break;
                                }
                            }
                            value += (char)uffff;
                        }
                        else if (_escapee.ContainsKey(_ch))
                        {
                            value += _escapee[_ch];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        value += _ch;
                    }
                }
            }
            Error("Bad string");
            //never reach here
            return value;
        }

        private Dynamic Word()
        {
            switch (_ch)
            {
                case 't':
                    if (Next('t')
                        && Next('r')
                        && Next('u')
                        && Next('e')
                    )
                    {
                        return true;
                    }
                    break;
                case 'f':
                    if (Next('f')
                        && Next('a')
                        && Next('l')
                        && Next('s')
                        && Next('e')
                    )
                    {
                        return false;
                    }
                    break;
                case 'n':
                    if (Next('n')
                        && Next('u')
                        && Next('l')
                        && Next('l')
                    )
                    {
                        return new Dynamic();
                    }
                    break;
                case 'u':
                    if (Next('u')
                        && Next('n')
                        && Next('d')
                        && Next('e')
                        && Next('f')
                        && Next('i')
                        && Next('n')
                        && Next('e')
                        && Next('d')
                        )
                    {
                        return new Dynamic();
                    }
                    break;
                default:
                    break;
            }

            Error(string.Format("Unexpected '{0}'", _ch));
            return new Dynamic();
        }

        private Dynamic Array()
        {
            Dynamic arr = new Dynamic();

            if (_ch == '[')
            {
                Next('[');
                White();

                if (_ch == ']')
                {
                    Next(']');
                    return arr;
                }

                while (true)
                {
                    arr.Add(Value());
                    White();

                    if (_ch == ']')
                    {
                        Next(']');
                        return arr;
                    }

                    if (!Next(','))
                    {
                        break;
                    }
                    White();
                }
            }
            return arr;
        }

        private Dynamic Object()
        {
            string key;
            Dynamic obj = new Dynamic();

            if (_ch == '{')
            {
                Next('{');
                White();
                if (_ch == '}')
                {
                    Next('}');
                    return obj; // empty object
                }

                while (true)
                {
                    key = Str();
                    if (obj.ContainsKey(key))
                    {
                        Error(string.Format("Duplicate key '{0}'", key));
                    }

                    White();
                    Next(':');
                    obj[key] = _reviver(key, Value());
                    White();

                    if (_ch == '}')
                    {
                        Next('}');
                        return obj;
                    }

                    if (!Next(','))
                    {
                        break;
                    }
                    White();
                }
            }

            return obj;
        }

        private Dynamic Value()
        {
            White();

            // Convert all kinds of values to DynamicValues
            Dynamic val = new Dynamic();
            switch (_ch)
            {
                case '{':
                    val = Object();
                    break;
                case '[':
                    val = Array();
                    break;
                case '\"':
                    string str = Str();
                    val = str;

                    if (regexDate.IsMatch(str))
                    {
                        val = DateTime.Parse(str);
                    }


                    break;
                case '-':
                    val = Number();
                    break;
                default:
                    val = (_ch >= '0' && _ch <= '9') ? Number() : Word();
                    break;
            }

            return val;
        }

        public Dynamic Parse()
        {
            _at = 0;
            _ch = ' ';
            Dynamic value = Value();
            White();
            if (Next())
            {
                Error("Syntax error");
            }
            return value;
        }
    }
}
