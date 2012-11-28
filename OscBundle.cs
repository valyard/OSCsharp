using System.Collections.Generic;
using System.Net;
using OSCsharp.Utils;

namespace OSCsharp {
    /// <summary>
    /// Represents a bundle of <see cref="OscMessage"/> and other <see cref="OscBundle"/> objects.
    /// </summary>
    public sealed class OscBundle : OscPacket {
        /// <summary>
        /// Specifies if the packet is an OSC bundle.
        /// </summary>
        public override bool IsBundle {
            get { return true; }
        }

        /// <summary>
        /// Gets the creation time of the bundle.
        /// </summary>
        public OscTimeTag TimeStamp {
            get { return mTimeStamp; }
        }

        /// <summary>
        /// Gets the array of nested bundles.
        /// </summary>
        public IList<OscBundle> Bundles {
            get {
                List<OscBundle> bundles = new List<OscBundle>();
                foreach (object value in mData) {
                    if (value is OscBundle) {
                        bundles.Add((OscBundle) value);
                    }
                }

                return bundles.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the array of contained messages.
        /// </summary>
        public IList<OscMessage> Messages {
            get {
                List<OscMessage> messages = new List<OscMessage>();
                foreach (object value in mData) {
                    if (value is OscMessage) {
                        messages.Add((OscMessage) value);
                    }
                }

                return messages.AsReadOnly();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscBundle"/> class.
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="client">The destination of sent packets when using TransportType.Tcp.</param>
        public OscBundle(IPEndPoint sourceEndPoint, OscClient client = null)
            : this(sourceEndPoint, new OscTimeTag(), client) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="OscBundle"/> class.
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="timeStamp">The creation time of the bundle.</param>
        /// <param name="client">The destination of sent packets when using TransportType.Tcp.</param>
        public OscBundle(IPEndPoint sourceEndPoint, OscTimeTag timeStamp, OscClient client = null)
            : base(sourceEndPoint, BundlePrefix, client) {
            mTimeStamp = timeStamp;
        }

        /// <summary>
        /// Serialize the packet.
        /// </summary>
        /// <returns>The newly serialized packet.</returns>
        public override byte[] ToByteArray() {
            List<byte> data = new List<byte>();

            data.AddRange(OscPacket.ValueToByteArray(mAddress));
            OscPacket.PadNull(data);

            data.AddRange(OscPacket.ValueToByteArray(mTimeStamp));

            foreach (object value in mData) {
                if ((value is OscPacket)) {
                    byte[] packetBytes = ((OscPacket) value).ToByteArray();
                    Assert.IsTrue(packetBytes.Length%4 == 0);

                    data.AddRange(OscPacket.ValueToByteArray(packetBytes.Length));
                    data.AddRange(packetBytes);
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
        /// <param name="end">The ending index into the serialized data stream.</param>
        /// <returns>The newly deserialized packet.</returns>
        public new static OscBundle FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start, int end) {
            string address = OscPacket.ValueFromByteArray<string>(data, ref start);
            Assert.IsTrue(address == BundlePrefix);

            OscTimeTag timeStamp = OscPacket.ValueFromByteArray<OscTimeTag>(data, ref start);
            OscBundle bundle = new OscBundle(sourceEndPoint, timeStamp);

            while (start < end) {
                int length = OscPacket.ValueFromByteArray<int>(data, ref start);
                int packetEnd = start + length;
                bundle.Append(OscPacket.FromByteArray(sourceEndPoint, data, ref start, packetEnd));
            }

            return bundle;
        }

        /// <summary>
        /// Appends a value to the packet.
        /// </summary>
        /// <typeparam name="T">The type of object being appended.</typeparam>
        /// <param name="value">The value to append.</param>
        /// <returns>The index of the newly added value within the Data property.</returns>
        /// <remarks>The value must be of type OscPacket.</remarks>
        public override int Append<T>(T value) {
            Assert.IsTrue(value is OscPacket);

            OscBundle nestedBundle = value as OscBundle;
            if (nestedBundle != null) {
                Assert.IsTrue(nestedBundle.mTimeStamp >= mTimeStamp);
            }

            mData.Add(value);

            return mData.Count - 1;
        }

        private const string BundlePrefix = "#bundle";

        private OscTimeTag mTimeStamp;
    }
}