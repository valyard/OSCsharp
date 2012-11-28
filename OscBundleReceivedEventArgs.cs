using System;
using OSCsharp.Utils;

namespace OSCsharp {
    /// <summary>
    /// Arguments for bundle received events.
    /// </summary>
    public class OscBundleReceivedEventArgs : EventArgs {
        /// <summary>
        /// Gets the <see cref="OscBundle"/> received.
        /// </summary>
        public OscBundle Bundle {
            get { return mBundle; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscBundleReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="bundle">The <see cref="OscBundle"/> received.</param>
        public OscBundleReceivedEventArgs(OscBundle bundle) {
            Assert.ParamIsNotNull(bundle);

            mBundle = bundle;
        }

        private OscBundle mBundle;
    }
}