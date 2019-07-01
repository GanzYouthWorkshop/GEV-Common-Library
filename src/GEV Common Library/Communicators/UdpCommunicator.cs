using System;
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
    /// UDP communicator class for message-based communication.
    /// </summary>
    /// <typeparam name="T">Type of the message objects the communicator willk handle.</typeparam>
    public class UdpCommunicator<T> : CommunicatorBase<T>
    {
        private UdpClient m_Client;

        public UdpCommunicator()
        {

        }

        public UdpCommunicator(string IP, int port)
        {
            this.IP = IP;
            this.Port = port;
        }

        public void Open()
        {
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
            this.m_Client = new UdpClient();
            IPEndPoint address = new IPEndPoint(IPAddress.Parse(this.IP), this.Port);
            this.PerformConnected();
            BinaryFormatter formatter = new BinaryFormatter();


            using (MemoryStream ms = new MemoryStream())
            {
                while (this.IsRunning)
                {

                    if (this.m_Client.Available > 0)
                    {
                        byte[] buffer = this.m_Client.Receive(ref address);
                        ms.Write(buffer, 0, buffer.Length);
                        object obj = formatter.Deserialize(ms);

                        if (obj is T)
                        {
                            T msg = (T)obj;

                            if (!this.DispatchMessageInEvent)
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
                        byte[] datagram = ms.ToArray();
                        this.m_Client.Send(datagram, datagram.Length, address);
                    }
                }
            }
        }
    }
}