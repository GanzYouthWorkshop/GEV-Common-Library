using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GEV.Common
{
    /// <summary>
    /// Abstract class for creating network-based communicators.
    /// </summary>
    /// <typeparam name="T">Type of the message objects the communicator willk handle.</typeparam>
    public abstract class CommunicatorBase<T>
    {
        /// <summary>
        /// Fires when a communication is succesfully made.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Fires when a communication is closed or is lost.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Fires when a message has arrived and the <see cref="DispatchMessageInEvent"/> property is set to true.
        /// </summary>
        public event EventHandler<T> MessageReceived;

        /// <summary>
        /// Contains incoming messages. Will only fill if <see cref="DispatchMessageInEvent"/> property is set to false.
        /// </summary>
        public ConcurrentQueue<T> InMessages { get; } = new ConcurrentQueue<T>();

        /// <summary>
        /// Contains messages to be sent to the remote station.
        /// </summary>
        public ConcurrentQueue<T> OutMessages { get; } = new ConcurrentQueue<T>();

        /// <summary>
        /// IP address of the remote station.
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Port of the remote station.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// If set to true, every message will raise the <see cref="MessageReceived"/> event instead of getting stored in the <see cref="InMessages"/> collection.
        /// </summary>
        public bool DispatchMessageInEvent { get; set; }

        /// <summary>
        /// Shows if the communicator is in a working state.
        /// </summary>
        public bool IsRunning { get; protected set; }

        protected Thread m_Thread;

        protected void PerformConnected()
        {
            this.Connected?.Invoke(this, new EventArgs());
        }

        protected void PerformDisconnected()
        {
            this.Disconnected?.Invoke(this, new EventArgs());
        }


        protected void PerformMessageReceived(T message)
        {
            this.MessageReceived?.Invoke(this, message);
        }
    }
}
 