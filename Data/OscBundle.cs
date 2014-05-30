/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;

namespace OSCsharp.Data
{
    public sealed class OscBundle : OscPacket
    {
        public const string BUNDLE_PREFIX = "#bundle";

        public override bool IsBundle
        {
            get { return true; }
        }

        public OscTimeTag TimeStamp
        {
            get { return timeStamp; }
        }

        private OscTimeTag timeStamp;

        public IList<OscBundle> Bundles
        {
            get
            {
                List<OscBundle> bundles = new List<OscBundle>();
                foreach (object value in data)
                {
                    if (value is OscBundle)
                    {
                        bundles.Add((OscBundle)value);
                    }
                }

                return bundles.AsReadOnly();
            }
        }

        public IList<OscMessage> Messages
        {
            get
            {
                List<OscMessage> messages = new List<OscMessage>();
                foreach (object value in data)
                {
                    if (value is OscMessage)
                    {
                        messages.Add((OscMessage)value);
                    }
                }

                return messages.AsReadOnly();
            }
        }

        public OscBundle() : this(new OscTimeTag())
        {}

        public OscBundle(OscTimeTag timeStamp) : base(BUNDLE_PREFIX)
        {
            this.timeStamp = timeStamp;
        }

        public override byte[] ToByteArray()
        {
            List<byte> data = new List<byte>();

            data.AddRange(ValueToByteArray(address));
            PadNull(data);

            data.AddRange(ValueToByteArray(timeStamp));

            foreach (object value in base.data)
            {
                if ((value is OscPacket))
                {
                    byte[] packetBytes = ((OscPacket)value).ToByteArray();
                    if (packetBytes.Length%4 != 0) throw new Exception();

                    data.AddRange(ValueToByteArray(packetBytes.Length));
                    data.AddRange(packetBytes);
                }
            }

            return data.ToArray();
        }

        public static new OscBundle FromByteArray(byte[] data, ref int start, int end)
        {
            string address = ValueFromByteArray<string>(data, ref start);
            if (address != BUNDLE_PREFIX) throw new ArgumentException();

            OscTimeTag timeStamp = ValueFromByteArray<OscTimeTag>(data, ref start);
            OscBundle bundle = new OscBundle(timeStamp);

            while (start < end)
            {
                int length = ValueFromByteArray<int>(data, ref start);
                int packetEnd = start + length;
                bundle.Append(OscPacket.FromByteArray(data, ref start, packetEnd));
            }

            return bundle;
        }

        // To prevent "ExecutionEngineException: Attempting to JIT compile method" error on iOS we use a non-generic method version.
        public override int Append(object value)
        {
            if (!(value is OscPacket)) throw new ArgumentException();

            OscBundle nestedBundle = value as OscBundle;
            if (nestedBundle != null)
            {
                if (nestedBundle.timeStamp < timeStamp) throw new Exception("Nested bundle's timestamp must be >= than parent bundle's timestamp.");
            }

            data.Add(value);

            return data.Count - 1;
        }
    }
}