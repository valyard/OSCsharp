/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using OSCsharp.Data;

namespace OSCsharp.Net
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