/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using OSCsharp.Data;

namespace OSCsharp.Net
{
    public class OscMessageReceivedEventArgs : EventArgs
    {
        public OscMessage Message { get; private set; }

        public OscMessageReceivedEventArgs(OscMessage message)
        {
            Message = message;
        }
    }
}