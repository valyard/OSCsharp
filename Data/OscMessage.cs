/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;

namespace OSCsharp.Data
{
    public class OscMessage : OscPacket
    {
        public const string ADRESS_PREFIX = "/";
        public const char TAG_DEFAULT = ',';
        public const char TAG_INTEGER = 'i';
        public const char TAG_FLOAT = 'f';
        public const char TAG_LONG = 'h';
        public const char TAG_DOUBLE = 'd';
        public const char TAG_STRING = 's';
        public const char TAG_SYMBOL = 'S';
        public const char TAG_BLOB = 'b';
        public const char TAG_TIME = 't';
        public const char TAG_CHARACTER = 'c';
        public const char TAG_COLOR = 'r';
        public const char TAG_TRUE = 'T';
        public const char TAG_FALSE = 'F';
        public const char TAG_NIL = 'N';
        public const char TAG_INFINITY = 'I';

        public override bool IsBundle
        {
            get { return false; }
        }

        public string TypeTag
        {
            get { return typeTag; }
        }

        private string typeTag;

        public OscMessage(string address, object value)
            : this(address)
        {
            Append(value);
        }

        public OscMessage(string address)
            : base(address)
        {
            if (!address.StartsWith(ADRESS_PREFIX)) throw new ArgumentException("Address must start with " + ADRESS_PREFIX + ".");

            typeTag = TAG_DEFAULT.ToString();
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(ValueToByteArray(address));
            PadNull(bytes);

            bytes.AddRange(ValueToByteArray(typeTag));
            PadNull(bytes);

            var count = data.Count;
            for (var i = 0; i < count; i++)
            {
                object value = data[i];
                byte[] valueBytes = ValueToByteArray(value);
                if (valueBytes != null)
                {
                    bytes.AddRange(valueBytes);
                    if (value is string || value is byte[])
                    {
                        PadNull(bytes);
                    }
                }
            }

            return bytes.ToArray();
        }

        public static OscMessage FromByteArray(byte[] data, ref int start)
        {
            string address = ValueFromByteArray<string>(data, ref start);
            OscMessage message = new OscMessage(address);

            char[] tags = ValueFromByteArray<string>(data, ref start).ToCharArray();
            var count = tags.Length;
            for (var i = 0; i < count; i++)
            {
                char tag = tags[i];
                object value;
                switch (tag)
                {
                    case TAG_DEFAULT:
                        continue;

                    case TAG_INTEGER:
                        value = ValueFromByteArray<int>(data, ref start);
                        break;

                    case TAG_LONG:
                        value = ValueFromByteArray<long>(data, ref start);
                        break;

                    case TAG_FLOAT:
                        value = ValueFromByteArray<float>(data, ref start);
                        break;

                    case TAG_DOUBLE:
                        value = ValueFromByteArray<double>(data, ref start);
                        break;

                    case TAG_STRING:
                    case TAG_SYMBOL:
                        value = ValueFromByteArray<string>(data, ref start);
                        break;

                    case TAG_BLOB:
                        value = ValueFromByteArray<byte[]>(data, ref start);
                        break;

                    case TAG_TIME:
                        value = ValueFromByteArray<OscTimeTag>(data, ref start);
                        break;

                    case TAG_CHARACTER:
                        value = ValueFromByteArray<char>(data, ref start);
                        break;

                    case TAG_TRUE:
                        value = true;
                        break;

                    case TAG_FALSE:
                        value = false;
                        break;

                    case TAG_NIL:
                        value = null;
                        break;

                    case TAG_INFINITY:
                        value = float.PositiveInfinity;
                        break;

                    default:
                        continue;
                }

                message.Append(value);
            }

            return message;
        }

        // To prevent "ExecutionEngineException: Attempting to JIT compile method" error on iOS we use a non-generic method version.
        public override int Append(object value)
        {
            char typeTag;

            if (value == null)
            {
                typeTag = TAG_NIL;
            } else
            {
                Type type = value.GetType();
                switch (type.Name)
                {
                    case "Int32":
                        typeTag = TAG_INTEGER;
                        break;

                    case "Int64":
                        typeTag = TAG_LONG;
                        break;

                    case "Single":
                        typeTag = (float.IsPositiveInfinity((float)(object)value) ? TAG_INFINITY : TAG_FLOAT);
                        break;

                    case "Double":
                        typeTag = TAG_DOUBLE;
                        break;

                    case "String":
                        typeTag = TAG_STRING;
                        break;

                    case "Byte[]":
                        typeTag = TAG_BLOB;
                        break;

                    case "OscTimeTag":
                        typeTag = TAG_TIME;
                        break;

                    case "Char":
                        typeTag = TAG_CHARACTER;
                        break;

                    case "Color":
                        typeTag = TAG_COLOR;
                        break;

                    case "Boolean":
                        typeTag = ((bool)(object)value ? TAG_TRUE : TAG_FALSE);
                        break;

                    default:
                        throw new Exception("Unsupported data type.");
                }
            }

            this.typeTag += typeTag;
            data.Add(value);

            return data.Count - 1;
        }

        public int AppendNil()
        {
            return Append(null);
        }

        public virtual void UpdateDataAt(int index, object value)
        {
            if (data.Count == 0 || data.Count <= index)
            {
                throw new ArgumentOutOfRangeException();
            }

            data[index] = value;
        }

        public void ClearData()
        {
            typeTag = TAG_DEFAULT.ToString();
            data.Clear();
        }
    }
}