using System;
using System.Collections.Generic;
using System.Net;
using OSCsharp.Utils;

namespace OSCsharp {
    /// <summary>
    /// Represents an Osc Message packet.
    /// </summary>
    public class OscMessage : OscPacket {
        /// <summary>
        /// Specifies if the packet is an Osc bundle.
        /// </summary>
        public override bool IsBundle {
            get { return false; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscMessage"/> class.
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="address">The Osc address pattern.</param>
        /// <param name="value">A value to append to the message.</param>
        public OscMessage(IPEndPoint sourceEndPoint, string address, object value)
            : this(sourceEndPoint, address) {
            Append(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscMessage"/> class.
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="address">The Osc address pattern.</param>
        /// <param name="client">The destination of sent packets when using TransportType.Tcp.</param>
        public OscMessage(IPEndPoint sourceEndPoint, string address, OscClient client = null)
            : base(sourceEndPoint, address, client) {
            Assert.IsTrue(address.StartsWith(AddressPrefix));

            mTypeTag = DefaultTag.ToString();
        }

        /// <summary>
        /// Serialize the packet.
        /// </summary>
        /// <returns>The newly serialized packet.</returns>
        public override byte[] ToByteArray() {
            List<byte> data = new List<byte>();

            data.AddRange(OscPacket.ValueToByteArray(mAddress));
            OscPacket.PadNull(data);

            data.AddRange(OscPacket.ValueToByteArray(mTypeTag));
            OscPacket.PadNull(data);

            foreach (object value in mData) {
                byte[] bytes = OscPacket.ValueToByteArray(value);
                if (bytes != null) {
                    data.AddRange(bytes);
                    if (value is string || value is byte[]) {
                        OscPacket.PadNull(data);
                    }
                }
            }

            return data.ToArray();
        }

        /// <summary>
        /// Deserialize the packet.
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="data">The serialized packet.</param>
        /// <param name="start">The starting index into the serialized data stream.</param>
        /// <returns>The newly deserialized packet.</returns>
        public static OscMessage FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start) {
            string address = OscPacket.ValueFromByteArray<string>(data, ref start);
            OscMessage message = new OscMessage(sourceEndPoint, address);

            char[] tags = OscPacket.ValueFromByteArray<string>(data, ref start).ToCharArray();
            foreach (char tag in tags) {
                object value;
                switch (tag) {
                    case DefaultTag:
                        continue;

                    case IntegerTag:
                        value = OscPacket.ValueFromByteArray<int>(data, ref start);
                        break;

                    case LongTag:
                        value = OscPacket.ValueFromByteArray<long>(data, ref start);
                        break;

                    case FloatTag:
                        value = OscPacket.ValueFromByteArray<float>(data, ref start);
                        break;

                    case DoubleTag:
                        value = OscPacket.ValueFromByteArray<double>(data, ref start);
                        break;

                    case StringTag:
                    case SymbolTag:
                        value = OscPacket.ValueFromByteArray<string>(data, ref start);
                        break;

                    case BlobTag:
                        value = OscPacket.ValueFromByteArray<byte[]>(data, ref start);
                        break;

                    case TimeTag:
                        value = OscPacket.ValueFromByteArray<OscTimeTag>(data, ref start);
                        break;

                    case CharacterTag:
                        value = OscPacket.ValueFromByteArray<char>(data, ref start);
                        break;

                        //case ColorTag:
                        //    value = OscPacket.ValueFromByteArray<Color>(data, ref start);
                        //    break;

                    case TrueTag:
                        value = true;
                        break;

                    case FalseTag:
                        value = false;
                        break;

                    case NilTag:
                        value = null;
                        break;

                    case InfinitumTag:
                        value = float.PositiveInfinity;
                        break;

                    default:
                        continue;
                }

                message.Append(value);
            }

            return message;
        }

        /// <summary>
        /// Appends a value to the message.
        /// </summary>
        /// <typeparam name="T">The type of object being appended.</typeparam>
        /// <param name="value">The value to append.</param>
        /// <returns>The index of the newly appended Data value.</returns>
        public override int Append<T>(T value) {
            char typeTag;

            if (value == null) {
                typeTag = NilTag;
            } else {
                Type type = value.GetType();
                switch (type.Name) {
                    case "Int32":
                        typeTag = IntegerTag;
                        break;

                    case "Int64":
                        typeTag = LongTag;
                        break;

                    case "Single":
                        typeTag = (float.IsPositiveInfinity((float) (object) value) ? InfinitumTag : FloatTag);
                        break;

                    case "Double":
                        typeTag = DoubleTag;
                        break;

                    case "String":
                        typeTag = StringTag;
                        break;

                    case "Byte[]":
                        typeTag = BlobTag;
                        break;

                    case "OscTimeTag":
                        typeTag = TimeTag;
                        break;

                    case "Char":
                        typeTag = CharacterTag;
                        break;

                    case "Color":
                        typeTag = ColorTag;
                        break;

                    case "Boolean":
                        typeTag = ((bool) (object) value ? TrueTag : FalseTag);
                        break;

                    default:
                        throw new Exception("Unsupported data type.");
                }
            }

            mTypeTag += typeTag;
            mData.Add(value);

            return mData.Count - 1;
        }

        /// <summary>
        /// Appends a Nil value to the message.
        /// </summary>
        /// <returns>The index of the newly appended Data value.</returns>
        public int AppendNil() {
            return Append<object>(null);
        }

        /// <summary>
        /// Update a value within the message at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to update.</param>
        /// <param name="value">The value to update the element with.</param>
        public virtual void UpdateDataAt(int index, object value) {
            if (mData.Count == 0 || mData.Count <= index) {
                throw new ArgumentOutOfRangeException();
            }

            mData[index] = value;
        }

        /// <summary>
        /// Remove all data from the message.
        /// </summary>
        public void ClearData() {
            mTypeTag = DefaultTag.ToString();
            mData.Clear();
        }

        /// <summary>
        /// The prefix required by Osc address patterns.
        /// </summary>
        protected const string AddressPrefix = "/";

        /// <summary>
        /// The beginning character in an Osc message type tag.
        /// </summary>
        protected const char DefaultTag = ',';

        /// <summary>
        /// The type tag for a 32-bit integer.
        /// </summary>
        protected const char IntegerTag = 'i';

        /// <summary>
        /// The type tag for an floating point value.
        /// </summary>
        protected const char FloatTag = 'f';

        /// <summary>
        /// The type tag for a 64-bit integer.
        /// </summary>
        protected const char LongTag = 'h';

        /// <summary>
        /// The type tag for an double-precision floating point value.
        /// </summary>
        protected const char DoubleTag = 'd';

        /// <summary>
        /// The type tag for a string.
        /// </summary>
        protected const char StringTag = 's';

        /// <summary>
        /// The type tag for a symbol.
        /// </summary>
        protected const char SymbolTag = 'S';

        /// <summary>
        /// The type tag for a blob (binary large object -- byte array).
        /// </summary>
        protected const char BlobTag = 'b';

        /// <summary>
        /// The type tag for an Osc Time Tag.
        /// </summary>
        protected const char TimeTag = 't';

        /// <summary>
        /// The type tag for an ASCII character (sent as a 32-bit int).
        /// </summary>
        protected const char CharacterTag = 'c';

        /// <summary>
        /// The type tag for a 32-bit RGBA color;
        /// </summary>
        protected const char ColorTag = 'r';

        /// <summary>
        /// The type tag for True. No bytes are allocated in the argument data.
        /// </summary>
        protected const char TrueTag = 'T';

        /// <summary>
        /// The type tag for False. No bytes are allocated in the argument data.
        /// </summary>
        protected const char FalseTag = 'F';

        /// <summary>
        /// The type tag for Nil. No bytes are allocated in the argument data.
        /// </summary>
        protected const char NilTag = 'N';

        /// <summary>
        /// The type tag for inifinitum. No bytes are allocated in the argument data.
        /// </summary>
        protected const char InfinitumTag = 'I';

        private string mTypeTag;
    }
}