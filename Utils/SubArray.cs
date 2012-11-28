using System;
using System.Collections;
using System.Collections.Generic;

namespace OSCsharp.Utils {
    /// <summary>
    /// Represented a subset of an array,
    /// </summary>
    /// <typeparam name="T">The underlying array type.</typeparam>
    public class SubArray<T> : IEnumerable<T> {
        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length {
            get { return mLength; }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0. -or- <paramref name="index"/> is equal to or greater than <see cref="Length"/>.</exception>
        public T this[int index] {
            get {
                if (index < 0 || index >= mLength) {
                    throw new ArgumentOutOfRangeException();
                }

                return mSource[mStart + index];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="start">The index, into the source array, to begin the sub array.</param>
        /// <param name="length">The length of the sub array.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="start"/> is less than 0. -or- <paramref name="start"/> is equal to or greater than <paramref name="source"/>.Length.
        /// -or- <paramref name="length"/> is less and 0. -or- <paramref name="length"/> is greater than <paramref name="source"/>.Length - <paramref name="start"/>.</exception>
        public SubArray(T[] source, int start, int length) {
            if (start < 0 || start >= source.Length) {
                throw new ArgumentOutOfRangeException("start");
            }

            if (length < 0 || length > source.Length - start) {
                throw new ArgumentOutOfRangeException("length");
            }

            mSource = source;
            mStart = start;
            mLength = length;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the array.
        /// </summary>
        /// <returns>An enumerator for the array.</returns>
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < mLength; i++) {
                yield return mSource[mStart + i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the array.
        /// </summary>
        /// <returns>A enumerator for the array.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Copy the contents of the sub array to a new array.
        /// </summary>
        /// <returns>The newly allocated array.</returns>
        public T[] ToArray() {
            T[] result = new T[mLength];
            Array.Copy(mSource, mStart, result, 0, mLength);
            return result;
        }

        private T[] mSource;
        private int mStart;
        private int mLength;
    }
}