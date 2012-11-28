using System;

namespace OSCsharp.Utils {
    /// <summary>
    /// Provides a set of methods to verify conditions.
    /// </summary>
    public static class Assert {
        /// <summary>
        /// Verify that a parameter is not null.
        /// </summary>
        /// <param name="paramName">The name of the paramater to verify.</param>
        /// <param name="param">The object to test for null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="param"/> is null.</exception>
        public static void ParamIsNotNull(string paramName, object param) {
            if (param == null) {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Verify that a parameter is not null.
        /// </summary>
        /// <param name="param">The object to test for null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="param"/> is null.</exception>
        public static void ParamIsNotNull(object param) {
            if ((param == null) || ((param is string) && (string.IsNullOrEmpty((string) param)))) {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// Verify that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to verify.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> is false.</exception>
        public static void IsTrue(bool condition) {
            IsTrue(String.Empty, condition);
        }

        /// <summary>
        /// Verify that a condition is true.
        /// </summary>
        /// <param name="paramName">The name of the paramater to verify.</param>
        /// <param name="condition">The condition to verify.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> is false.</exception>
        public static void IsTrue(string paramName, bool condition) {
            if (condition == false) {
                throw new ArgumentException("Condition false", paramName);
            }
        }

        /// <summary>
        /// Verify that a condition is false.
        /// </summary>
        /// <param name="condition">The condition to verify.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> is true.</exception>
        public static void IsFalse(bool condition) {
            IsFalse(String.Empty, condition);
        }

        /// <summary>
        /// Verify that a condition is false.
        /// </summary>
        /// <param name="paramName">The name of the paramater to verify.</param>
        /// <param name="condition">The condition to verify.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> is true.</exception>
        public static void IsFalse(string paramName, bool condition) {
            if (condition == true) {
                throw new ArgumentException("Condition true", paramName);
            }
        }
    }
}