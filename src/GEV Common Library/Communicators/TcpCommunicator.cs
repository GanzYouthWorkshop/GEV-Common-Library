using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GEV.Common
{
    public class TcpCommunicator<T> : CommunicatorBase<T>
    {
        private TcpListener m_Listener;
        private TcpClient m_Client;

        private bool m_IsListener;

        public TcpCommunicator()
        {

        }

        public TcpCommunicator(string IP, int port)
        {
            this.IP = IP;
            this.Port = port;
        }

        public void Open()
        {
            this.m_IsListener = true;

            this.m_Thread = new Thread(this.Runner)
            {
                IsBackground = true
            };
            this.IsRunning = true;
            this.m_Thread.Start();
        }

        public void Connect()
        {
            this.m_IsListener = false;

            this.m_Thread = new Thread(this.Runner)
            {
                IsBackground = true
            };
            this.IsRunning = true;
            this.m_Thread.Start();
        }

        public void Close()
        {
            this.IsRunning = false;
        }

        private void Runner()
        {
            if(this.m_IsListener)
            {
                this.m_Listener = new TcpListener(new IPEndPoint(IPAddress.Parse(this.IP), this.Port));
                this.m_Listener.Start();
                this.m_Client = this.m_Listener.AcceptTcpClient();
            }
            else
            {
                this.m_Client = new TcpClient(this.IP, this.Port);
            }

            NetworkStream ns = this.m_Client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();

            this.PerformConnected();

            using (MemoryStream ms = new MemoryStream())
            {
                while (this.IsRunning)
                {
                    if (ns.DataAvailable)
                    {
                        object obj = formatter.Deserialize(ns);
                        if (obj is T)
                        {
                            T msg = (T)obj;

                            if (this.DispatchMessageInEvent)
                            {
                                this.PerformMessageReceived(msg);
                            }
                            else
                            {
                                this.InMessages.Enqueue(msg);
                            }
                        }

                    }

                    T send = default(T);
                    if (this.OutMessages.TryDequeue(out send))
                    {
                        formatter.Serialize(ms, send);
                        byte[] tmp = ms.ToArray();
                        ns.Write(tmp, 0, tmp.Length);
                    }

                    Thread.Sleep(5);
                }
            }
        }
    }
}
