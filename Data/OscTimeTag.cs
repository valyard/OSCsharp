/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Utils;

namespace OSCsharp.Data {

    public class OscTimeTag {

        public static readonly DateTime Epoch = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        public static readonly OscTimeTag MinValue = new OscTimeTag(Epoch + TimeSpan.FromMilliseconds(1.0));

        public uint SecondsSinceEpoch {
            get { return (uint) (timeStamp - Epoch).TotalSeconds; }
        }

        public uint FractionalSecond {
            get { return (uint) ((timeStamp - Epoch).Milliseconds); }
        }

        public DateTime DateTime {
            get { return timeStamp; }
        }

        private DateTime timeStamp;

        public OscTimeTag()
            : this(DateTime.Now) {}

        public OscTimeTag(DateTime timeStamp) {
            Set(timeStamp);
        }

        public OscTimeTag(byte[] data) {
            byte[] secondsSinceEpochData = data.CopySubArray(0, 4);
            byte[] fractionalSecondData = data.CopySubArray(4, 4);

            if (BitConverter.IsLittleEndian != OscPacket.LittleEndianByteOrder) {
                secondsSinceEpochData = Utility.SwapEndian(secondsSinceEpochData);
                fractionalSecondData = Utility.SwapEndian(fractionalSecondData);
            }

            uint secondsSinceEpoch = BitConverter.ToUInt32(secondsSinceEpochData, 0);
            uint fractionalSecond = BitConverter.ToUInt32(fractionalSecondData, 0);

            timeStamp = Epoch.AddSeconds(secondsSinceEpoch).AddMilliseconds(fractionalSecond);
        }

        public static bool Equals(OscTimeTag lhs, OscTimeTag rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator ==(OscTimeTag lhs, OscTimeTag rhs) {
            if (ReferenceEquals(lhs, rhs)) {
                return true;
            }

            if (((object) lhs == null) || ((object) rhs == null)) {
                return false;
            }

            return lhs.DateTime == rhs.DateTime;
        }

        public static bool operator !=(OscTimeTag lhs, OscTimeTag rhs) {
            return !(lhs == rhs);
        }

        public static bool operator <(OscTimeTag lhs, OscTimeTag rhs) {
            return lhs.DateTime < rhs.DateTime;
        }

        public static bool operator <=(OscTimeTag lhs, OscTimeTag rhs) {
            return lhs.DateTime <= rhs.DateTime;
        }

        public static bool operator >(OscTimeTag lhs, OscTimeTag rhs) {
            return lhs.DateTime > rhs.DateTime;
        }

        public static bool operator >=(OscTimeTag lhs, OscTimeTag rhs) {
            return lhs.DateTime >= rhs.DateTime;
        }

        public static bool IsValidTime(DateTime timeStamp) {
            return (timeStamp >= Epoch + TimeSpan.FromMilliseconds(1.0));
        }

        public void Set(DateTime timeStamp) {
            timeStamp = new DateTime(timeStamp.Ticks - (timeStamp.Ticks%TimeSpan.TicksPerMillisecond), timeStamp.Kind);

            if(!IsValidTime(timeStamp)) throw new ArgumentException("Invalid timestamp.");
            this.timeStamp = timeStamp;
        }

        public byte[] ToByteArray() {
            List<byte> timeStamp = new List<byte>();

            byte[] secondsSinceEpoch = BitConverter.GetBytes(SecondsSinceEpoch);
            byte[] fractionalSecond = BitConverter.GetBytes(FractionalSecond);

            if (BitConverter.IsLittleEndian != OscPacket.LittleEndianByteOrder) {
                secondsSinceEpoch = Utility.SwapEndian(secondsSinceEpoch);
                fractionalSecond = Utility.SwapEndian(fractionalSecond);
            }

            timeStamp.AddRange(secondsSinceEpoch);
            timeStamp.AddRange(fractionalSecond);

            return timeStamp.ToArray();
        }

        public override bool Equals(object value) {
            if (value == null) {
                return false;
            }

            OscTimeTag rhs = value as OscTimeTag;
            if (rhs == null) {
                return false;
            }

            return timeStamp.Equals(rhs.timeStamp);
        }

        public bool Equals(OscTimeTag value) {
            if ((object) value == null) {
                return false;
            }

            return timeStamp.Equals(value.timeStamp);
        }

        public override int GetHashCode() {
            return timeStamp.GetHashCode();
        }

        public override string ToString() {
            return timeStamp.ToString();
        }
        
    }
}