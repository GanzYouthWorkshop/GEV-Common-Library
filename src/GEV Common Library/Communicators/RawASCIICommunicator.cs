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
    public class RawASCIICommunicator : TcpCommunicator<string>
    {
        /// <summary>
        /// If true, output messages will be padded to <see cref="ConstantLength"/>
        /// </summary>
        public bool UseConstantLength { get; set; } = true;

        /// <summary>
        /// If  <see cref="UseConstantLength"/> is set to true the output messages will be padded to this length with zero-bytes
        /// </summary>
        public int ConstantLength { get; set; } = 20;

        /// <summary>
        /// Creates a new raw ASCII string communicator.
        /// </summary>
        public RawASCIICommunicator() : base()
        {
        }

        /// <summary>
        /// Creates a new raw ASCII string communicator.
        /// </summary>
        public RawASCIICommunicator(string IP, int port) : base(IP, port)
        {
        }


        protected override void Runner()
        {
            if (this.m_IsListener)
            {
                this.m_Listener = new TcpListener(new IPEndPoint(IPAddress.Parse(this.IP), this.Port));
                this.m_Listener.Start();
                this.m_Client = this.m_Listener.AcceptTcpClient();
            }
            else
            {
                while (!this.Reonnect()) ;
            }

            NetworkStream ns = this.m_Client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();

            this.PerformConnected();

            while (this.IsRunning)
            {
                if (this.m_Client == null || this.m_Client.Client.Connected)
                {
                    if (!this.Reonnect())
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
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ns.CopyTo(ms);
                        byte[] buffer =  ms.ToArray();
                        string msg = Encoding.ASCII.GetString(buffer).Split((char)0).First();

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

                string send = default(string);
                if (this.OutMessages.TryDequeue(out send))
                {
                    using (StreamWriter sw = new StreamWriter(ns, new ASCIIEncoding()))
                    {
                        byte[] buffer = System.Text.Encoding.ASCII.GetBytes(send.ToString());
                        if(this.UseConstantLength)
                        {
                            byte[] extendedBuffer = new byte[this.ConstantLength];
                            buffer.CopyTo(extendedBuffer, 0);
                            buffer = extendedBuffer;
                        }

                        ns.Write(buffer, 0, buffer.Length);
                    }
                }

                Thread.Sleep(5);
            }
        }

    }
}
