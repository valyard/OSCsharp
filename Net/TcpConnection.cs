using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using OSCsharp.Utils;

namespace OSCsharp.Net {
    /// <summary>
    /// Represents a single TCP/IP connection.
    /// </summary>
    /// <remarks><see cref="TcpConnection"/> objects can be used in conjunction with <see cref="TcpServer"/> or as simple
    /// wrappers to a <see cref="Socket"/>. Data can be received automatically by the TcpConnection object by invoking the
    /// ReceiveDataAsynchronously method. This mechanism transmits the data asynchronously using Socket.BeginReceive/EndReceive.
    /// Additionally, messages are framed using a 4-byte integer containing the length of the message. The DataReceived event
    /// is raised as complete messages are received. Users can perform their own message handling by not invoking the
    /// ReceiveDataAsynchronously method and accessing the TcpConnection.Client/Reader/Writer properties directly.
    /// When performing manual data handling, in conjunction with a <see cref="TcpServer"/> object, set the <see cref="TcpServer.ReceiveDataInline"/>
    /// property to false.
    /// </remarks>
    public class TcpConnection : IDisposable {
        /// <summary>
        /// The possible states a connection can be in while receiving data.
        /// </summary>
        private enum DataReceiveState {
            /// <summary>
            /// Processing the framing data to describe the forthcoming message.
            /// </summary>
            MessageFraming = 0,

            /// <summary>
            /// Processing the message data.
            /// </summary>
            Message
        }

        /// <summary>
        /// Raised when a connection is disconnected.
        /// </summary>
        public event EventHandler<TcpConnectionEventArgs> Disconnected;

        /// <summary>
        /// Raised when data is received.
        /// </summary>
        public event EventHandler<TcpDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Gets the associated socket.
        /// </summary>
        /// <remarks>This property is primarily useful for manual data handling. If you employ inline
        /// data handling through the ReceiveDataAsynchronously method, you should attach a handler
        /// to the DataReceived event to process incoming messages.</remarks>
        public Socket Client {
            get { return mClient; }
        }

        /// <summary>
        /// Gets the associated reader.
        /// </summary>
        public BinaryReader Reader {
            get { return mReader; }
        }

        /// <summary>
        /// Gets the associated writer.
        /// </summary>        
        public BinaryWriter Writer {
            get { return mWriter; }
        }

        /// <summary>
        /// Gets or sets the expected endianness of integral value types.
        /// </summary>
        public bool LittleEndianByteOrder {
            get { return mLittleEndianByteOrder; }
            set { mLittleEndianByteOrder = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnection"/> class.
        /// </summary>
        /// <param name="client">The associated socket.</param>
        /// <param name="littleEndianByteOrder">The expected endianness of integral value types.</param>
        public TcpConnection(Socket client, bool littleEndianByteOrder = true) {
            Assert.ParamIsNotNull(client);

            mClient = client;
            mNetworkStream = new NetworkStream(client);
            mReader = new BinaryReader(mNetworkStream);
            mWriter = new BinaryWriter(mNetworkStream);
            mLittleEndianByteOrder = littleEndianByteOrder;

            mReceiveState = DataReceiveState.MessageFraming;
            mPartiallyReceivedData = new byte[ReceivedDataBufferSize];
            mCompletelyReceivedData = new List<byte>();
            mMessageLengthData = new byte[4];
            mBytesToProcessRemaining = 0;
        }

        /// <summary>
        /// Release the resources associated with this object.
        /// </summary>
        public void Dispose() {
            if (mIsClosed == false) {
                Close();
            }
        }

        /// <summary>
        /// Closes the current connection and associated helpers.
        /// </summary>
        public void Close() {
            lock (this) {
                mReader.Close();
                mWriter.Close();
                mNetworkStream.Close();
                if (mClient.Connected) {
                    mClient.Shutdown(SocketShutdown.Both);
                    mClient.Close();
                }

                mDataReceivedCallback = null;
                mIsClosed = true;
                OnDisconnected(new TcpConnectionEventArgs(this));
            }
        }

        /// <summary>
        /// Initiates the asynchronous system to receive framed messages.
        /// </summary>
        /// <remarks>When using this system, all messages must begin with a 4-byte integer containing the length of the message.</remarks>
        public void ReceiveDataAsynchronously() {
            mDataReceivedCallback = new AsyncCallback(DataReceivedCallback);
            InitDataReceivedCallback();
        }

        #region Private Methods

        /// <summary>
        /// Clear the associated buffer and invoke Socket.BeginReceive().
        /// </summary>
        private void InitDataReceivedCallback() {
            Array.Clear(mPartiallyReceivedData, 0, mPartiallyReceivedData.Length);
            mClient.BeginReceive(mPartiallyReceivedData, 0, mPartiallyReceivedData.Length, SocketFlags.None, mDataReceivedCallback, null);
        }

        /// <summary>
        /// The callback paired with Socket.BeginReceive().
        /// </summary>
        /// <param name="asyncResult"></param>
        private void DataReceivedCallback(IAsyncResult asyncResult) {
            if (mIsClosed) {
                return;
            }

            try {
                int bytesReceived = mClient.EndReceive(asyncResult);
                if (bytesReceived > 0) {
                    mBytesToProcessRemaining = bytesReceived;
                    mCompletelyReceivedData.AddRange(new SubArray<byte>(mPartiallyReceivedData, 0, bytesReceived));

                    while (mBytesToProcessRemaining > 0) {
                        switch (mReceiveState) {
                            case TcpConnection.DataReceiveState.MessageFraming: {
                                // Read message length
                                if (mCompletelyReceivedData.Count >= 4) {
                                    mCompletelyReceivedData.CopyTo(0, mMessageLengthData, 0, 4);
                                    if (BitConverter.IsLittleEndian != mLittleEndianByteOrder) {
                                        mMessageLengthData = Utility.SwapEndian(mMessageLengthData);
                                    }

                                    mMessageLength = BitConverter.ToInt32(mMessageLengthData, 0);
                                    Assert.IsTrue(mMessageLength > 0);

                                    mReceiveState = TcpConnection.DataReceiveState.Message;
                                    mCompletelyReceivedData.RemoveRange(0, 4);
                                    mBytesToProcessRemaining -= 4;
                                }

                                break;
                            }

                            case DataReceiveState.Message: {
                                if (mCompletelyReceivedData.Count >= mMessageLength) {
                                    byte[] message = new byte[mMessageLength];
                                    mCompletelyReceivedData.CopyTo(0, message, 0, mMessageLength);
                                    OnDataReceived(new TcpDataReceivedEventArgs(this, message));

                                    mReceiveState = TcpConnection.DataReceiveState.MessageFraming;
                                    mCompletelyReceivedData.RemoveRange(0, mMessageLength);
                                }

                                mBytesToProcessRemaining = 0;
                                break;
                            }
                        }
                    }

                    InitDataReceivedCallback();
                } else {
                    // bytesReceived == 0 when the socket gets shutdown; therefore, remove the connection.
                    mCompletelyReceivedData.Clear();
                    Close();
                    return;
                }
            } catch {
                Close();
            }
        }

        /// <summary>
        /// Raise the DataReceived event.
        /// </summary>
        /// <param name="e">An <see cref="TcpDataReceivedEventArgs"/> object that contains the event data.</param>
        private void OnDataReceived(TcpDataReceivedEventArgs e) {
            if (DataReceived != null) {
                DataReceived(this, e);
            }
        }

        /// <summary>
        /// Raise the Disconnected event.
        /// </summary>
        /// <param name="e">An <see cref="TcpConnectionEventArgs"/> object that contains the event data.</param>
        private void OnDisconnected(TcpConnectionEventArgs e) {
            if (Disconnected != null) {
                Disconnected(this, e);
            }
        }

        #endregion

        private static readonly int ReceivedDataBufferSize = 65535;

        private Socket mClient;
        private NetworkStream mNetworkStream;
        private BinaryReader mReader;
        private BinaryWriter mWriter;
        private volatile bool mIsClosed;
        private bool mLittleEndianByteOrder;

        private DataReceiveState mReceiveState;
        private AsyncCallback mDataReceivedCallback;
        private byte[] mPartiallyReceivedData;
        private List<byte> mCompletelyReceivedData;
        private int mMessageLength;
        private byte[] mMessageLengthData;
        private int mBytesToProcessRemaining;
    }
}