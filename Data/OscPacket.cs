/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Text;
using OSCsharp.Utils;

namespace OSCsharp.Data
{
    public abstract class OscPacket
    {
        public static bool LittleEndianByteOrder
        {
            get { return littleEndianByteOrder; }
            set { littleEndianByteOrder = value; }
        }

        public abstract bool IsBundle { get; }

        public string Address
        {
            get { return address; }
            set
            {
                if (value == null) value = "";
                address = value;
            }
        }

        public IList<object> Data
        {
            get { return data.AsReadOnly(); }
        }

        protected static bool littleEndianByteOrder;
        protected string address;
        protected List<object> data;

        static OscPacket()
        {
            littleEndianByteOrder = false;
        }

        public OscPacket(string address)
        {
            Address = address;
            data = new List<object>();
        }

        // To prevent "ExecutionEngineException: Attempting to JIT compile method" error on iOS we use a non-generic method version.
        public abstract int Append(object value);

        public T At<T>(int index)
        {
            if (index > data.Count || index < 0) throw new IndexOutOfRangeException();
            if ((data[index] is T) == false) throw new InvalidCastException();

            return (T)data[index];
        }

        public abstract byte[] ToByteArray();

        public static OscPacket FromByteArray(byte[] data)
        {
            if (data == null) throw new ArgumentException();

            int start = 0;
            return FromByteArray(data, ref start, data.Length);
        }

        public static OscPacket FromByteArray(byte[] data, ref int start, int end)
        {
            if (data[start] == '#')
            {
                return OscBundle.FromByteArray(data, ref start, end);
            } else
            {
                return OscMessage.FromByteArray(data, ref start);
            }
        }

        public static T ValueFromByteArray<T>(byte[] data, ref int start)
        {
            Type type = typeof(T);
            object value;

            switch (type.Name)
            {
                case "String":
                {
                    int count = 0;
                    for (int index = start; index < data.Length && data[index] != 0; index++)
                    {
                        count++;
                    }

                    value = Encoding.ASCII.GetString(data, start, count);
                    start += count + 1;
                    start = ((start + 3)/4)*4;
                    break;
                }

                case "Byte[]":
                {
                    int length = ValueFromByteArray<int>(data, ref start);
                    byte[] buffer = data.CopySubArray(start, length);

                    value = buffer;
                    start += buffer.Length + 1;
                    start = ((start + 3)/4)*4;
                    break;
                }

                case "OscTimeTag":
                {
                    byte[] buffer = data.CopySubArray(start, 8);
                    value = new OscTimeTag(buffer);
                    start += buffer.Length;
                    break;
                }

                case "Char":
                {
                    value = Convert.ToChar(ValueFromByteArray<int>(data, ref start));
                    break;
                }

                default:
                {
                    int bufferLength;
                    switch (type.Name)
                    {
                        case "Int32":
                        case "Single":
                            bufferLength = 4;
                            break;

                        case "Int64":
                        case "Double":
                            bufferLength = 8;
                            break;

                        default:
                            throw new Exception("Unsupported data type.");
                    }

                    byte[] buffer = data.CopySubArray(start, bufferLength);
                    start += buffer.Length;

                    if (BitConverter.IsLittleEndian != littleEndianByteOrder)
                    {
                        buffer = Utility.SwapEndian(buffer);
                    }

                    switch (type.Name)
                    {
                        case "Int32":
                            value = BitConverter.ToInt32(buffer, 0);
                            break;

                        case "Int64":
                            value = BitConverter.ToInt64(buffer, 0);
                            break;

                        case "Single":
                            value = BitConverter.ToSingle(buffer, 0);
                            break;

                        case "Double":
                            value = BitConverter.ToDouble(buffer, 0);
                            break;

                        default:
                            throw new Exception("Unsupported data type.");
                    }
                    break;
                }
            }

            return (T)value;
        }

        public static byte[] ValueToByteArray<T>(T value)
        {
            byte[] data = null;
            object valueObject = value;

            if (valueObject != null)
            {
                Type type = value.GetType();

                switch (type.Name)
                {
                    case "Int32":
                    {
                        data = BitConverter.GetBytes((int)valueObject);
                        if (BitConverter.IsLittleEndian != littleEndianByteOrder)
                        {
                            data = Utility.SwapEndian(data);
                        }
                        break;
                    }

                    case "Int64":
                    {
                        data = BitConverter.GetBytes((long)valueObject);
                        if (BitConverter.IsLittleEndian != littleEndianByteOrder)
                        {
                            data = Utility.SwapEndian(data);
                        }
                        break;
                    }

                    case "Single":
                    {
                        float floatValue = (float)valueObject;

                        // No payload for Infinitum data tag.
                        if (float.IsPositiveInfinity(floatValue) == false)
                        {
                            data = BitConverter.GetBytes(floatValue);
                            if (BitConverter.IsLittleEndian != littleEndianByteOrder)
                            {
                                data = Utility.SwapEndian(data);
                            }
                        }
                        break;
                    }

                    case "Double":
                    {
                        data = BitConverter.GetBytes((double)valueObject);
                        if (BitConverter.IsLittleEndian != littleEndianByteOrder)
                        {
                            data = Utility.SwapEndian(data);
                        }
                        break;
                    }

                    case "String":
                    {
                        data = Encoding.ASCII.GetBytes((string)valueObject);
                        break;
                    }

                    case "Byte[]":
                    {
                        byte[] valueData = ((byte[])valueObject);
                        List<byte> bytes = new List<byte>();
                        bytes.AddRange(ValueToByteArray(valueData.Length));
                        bytes.AddRange(valueData);
                        data = bytes.ToArray();
                        break;
                    }

                    case "OscTimeTag":
                    {
                        data = ((OscTimeTag)valueObject).ToByteArray();
                        break;
                    }

                    case "Char":
                    {
                        data = ValueToByteArray<int>(Convert.ToInt32((char)valueObject));
                        break;
                    }

                    case "Boolean":
                    {
                        // No payload for Boolean data tag.
                        break;
                    }

                    default:
                        throw new Exception("Unsupported data type.");
                }
            }

            return data;
        }

        public static void PadNull(List<byte> data)
        {
            byte zero = 0;
            int pad = 4 - (data.Count%4);
            for (int i = 0; i < pad; i++)
            {
                data.Add(zero);
            }
        }
    }
}