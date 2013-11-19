using System;
using OSCsharp.Data;

namespace OSCsharp
{
    public class OscBundleReceivedEventArgs : EventArgs
    {
        public OscBundle Bundle { get; private set; }

        public OscBundleReceivedEventArgs(OscBundle bundle)
        {
            Bundle = bundle;
        }
    }
}