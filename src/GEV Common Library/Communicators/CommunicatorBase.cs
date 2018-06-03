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
    public abstract class CommunicatorBase<T>
    {
        public event EventHandler Connected;
        public event EventHandler<T> MessageReceived;

        public ConcurrentQueue<T> InMessages { get; } = new ConcurrentQueue<T>();
        public ConcurrentQueue<T> OutMessages { get; } = new ConcurrentQueue<T>();

        public string IP { get; set; }
        public int Port { get; set; }
        public bool DispatchMessageInEvent { get; set; }

        public bool IsRunning { get; protected set; }

        protected Thread m_Thread;

        protected void PerformConnected()
        {
            this.Connected?.Invoke(this, new EventArgs());
        }

        protected void PerformMessageReceived(T message)
        {
            this.MessageReceived?.Invoke(this, message);
        }
    }
}
 