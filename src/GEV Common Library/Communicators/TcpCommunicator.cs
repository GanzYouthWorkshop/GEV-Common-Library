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
    /// <summary>
    /// TCP communicator class for message-based communication.
    /// </summary>
    /// <typeparam name="T">Type of the message objects the communicator willk handle.</typeparam>
    public class TcpCommunicator<T> : CommunicatorBase<T>
    {
        protected TcpListener m_Listener;
        protected TcpClient m_Client;

        protected bool m_IsListener;

        /// <summary>
        /// Creates a new TCP communicator.
        /// </summary>
        public TcpCommunicator()
        {

        }

        /// <summary>
        /// Creates a new TCP communicator.
        /// </summary>
        public TcpCommunicator(string IP, int port)
        {
            this.IP = IP;
            this.Port = port;
        }

        /// <summary>
        /// Starts the communicator as a listener that a client can connect to.
        /// </summary>
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

        /// <summary>
        /// Starts the comunicator as a client that can connect to a listener.
        /// </summary>
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

        public bool Reonnect()
        {
            try
            {
                this.m_Client = new TcpClient(this.IP, this.Port);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected virtual void Runner()
        {
            if(this.m_IsListener)
            {
                this.m_Listener = new TcpListener(new IPEndPoint(IPAddress.Parse(this.IP), this.Port));
                this.m_Listener.Start();
                this.m_Client = this.m_Listener.AcceptTcpClient();
            }
            else
            {
                while (!this.Reonnect());
            }

            NetworkStream ns = this.m_Client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();

            this.PerformConnected();

            using (MemoryStream ms = new MemoryStream())
            {
                while (this.IsRunning)
                {
                    if(this.m_Client == null || this.m_Client.Client.Connected)
                    {
                        if(!this.Reonnect())
                        {
                            this.PerformDisconnected();
                            Thread.Sleep(5);
                            continue;
                        }
                        else
                        {
                            this.PerformConnected();
                        }
                    }

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
