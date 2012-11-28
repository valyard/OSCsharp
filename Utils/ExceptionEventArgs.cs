using System;

namespace OSCsharp.Utils {
    /// <summary>
    /// Data for exception events.
    /// </summary>
    public class ExceptionEventArgs : EventArgs {
        /// <summary>
        /// Gets the associated <see cref="Exception"/>.
        /// </summary>
        public Exception Exception {
            get { return mException; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="ex">The associated exception.</param>
        public ExceptionEventArgs(Exception ex) {
            mException = ex;
        }

        private Exception mException;
    }
}