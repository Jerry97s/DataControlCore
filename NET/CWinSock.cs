using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace iNervCore.NET
{
    public class AsyncObject
    {
        public byte[] buffer;
        public Socket socket;
        public EndPoint pRemoteEp = null;
        public readonly int size;
        public StringBuilder sb = null;

        public AsyncObject(int bufferSize)
        {
            size = bufferSize;
            buffer = new byte[size];
            sb = new StringBuilder();
            ClearBuffer();
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer, 0, size);
        }
    }

    public class CWinSock
    {
        Socket pSocket = null;
        Socket pClient = null;
        public EndPoint pRemoteEp = null;
        Thread threadAccept = null;

        int nIndex = 0;

        int nSendState = 0;
        int nConnectState = 0;

        string sRemoteIP = "";
        int nRemotePort = 0;

        public delegate void DeleConnectedComplete();
        private DeleConnectedComplete deleConn = null;

        public delegate void DeleSendComplete();
        private DeleSendComplete deleSend = null;

        public delegate void DeleRecvComplete(string sRecvData);
        private DeleRecvComplete deleRecv = null;

        public int INDEX
        { 
            get { return nIndex; }
            set { nIndex = value; }
        }

        public int SEND_STATE
        {
            get { return nSendState; }
            set { nSendState = value; }
        }

        public int CONNECT_STATE
        {
            get { return nConnectState; }
            set { nConnectState = value; }
        }

        public string REMOTE_HOST_IP
        {
            get { return sRemoteIP; }
            set { sRemoteIP = value; }
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return !(pClient.Poll(1, SelectMode.SelectRead)
                                    && pClient.Available == 0);
                }
                catch (Exception)
                {
                    Console.WriteLine("Not Connect!!!!");
                    return false;
                }
            }
        }

        public CWinSock(DeleConnectedComplete dfnConnComplete = null, DeleSendComplete dfnSendComplete = null, DeleRecvComplete dfnRecvComplete = null)
        {
            deleConn = dfnConnComplete;
            deleSend = dfnSendComplete;
            deleRecv = dfnRecvComplete;
        }

        /// <summary>
        /// TCP SERVER
        /// </summary>
        /// <param name="nPort"></param>
        public void Listen(int nPort)
        {
            try
            {
                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ep = new IPEndPoint(IPAddress.Any, nPort);
                pSocket.Bind(ep);
                pSocket.Listen(0);
                pSocket.BeginAccept(acceptCallback, null);
                CONNECT_STATE = 2;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// TCP CLIENT
        /// </summary>
        /// <param name="sIP"></param>
        /// <param name="nPort"></param>
        public void Connect(string sIP, int nPort)
        {
            try
            {
                sRemoteIP = sIP;
                nRemotePort = nPort;

                pClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                pClient.NoDelay = true;

                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(sIP), nPort);
                SocketAsyncEventArgs evArgs = new SocketAsyncEventArgs();
                evArgs.Completed += onConnected;
                evArgs.RemoteEndPoint = ep;

                CONNECT_STATE = 6;
                pClient.ConnectAsync(evArgs);
            }
            catch (Exception)
            {
            }
        }
        
        /// <summary>
        /// UDP SERVER
        /// </summary>
        /// <param name="nPort"></param>
        public void Bind(int nPort)
        {
            try
            {
                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint ep = new IPEndPoint(IPAddress.Any, nPort);
                pSocket.Bind(ep);

                AsyncObject obj = new AsyncObject(1024);
                obj.socket = pSocket;
                obj.pRemoteEp = new IPEndPoint(IPAddress.None, nPort);
                pSocket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdp, obj);
                CONNECT_STATE = 2;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// UDP CLIENT
        /// </summary>
        public void Udp()
        {
            try
            {   
            }
            catch (Exception)
            {
            }
        }

        public void Close()
        {
            try
            {
                if (threadAccept != null)
                {
                    threadAccept.Abort();
                }
                if (pSocket != null)
                {
                    pSocket.Shutdown(SocketShutdown.Both);
                    pSocket.Close();
                }
                if(pClient != null)
                {
                    pClient.Shutdown(SocketShutdown.Both);
                    pClient.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// TCP SEND
        /// </summary>
        /// <param name="sData"></param>
        public void Send(string sData)
        {
            try
            {
                nSendState = 0;
                byte[] btData = Encoding.ASCII.GetBytes(sData);
                pClient.BeginSend(btData, 0, btData.Length, 0, new AsyncCallback(sendCallback), pClient);

                //DATA.gQueue.QUEUE_SetMode(INDEX, 2);
            }
            catch (SocketException ex)
            {
                CONNECT_STATE = 0;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                }
            }
        }

        /// <summary>
        /// UDP SEND
        /// </summary>
        /// <param name="sData"></param>
        public void Send_UDP(string sIP, int nPort, string sData)
        {
            try
            {
                sRemoteIP = sIP;
                nRemotePort = nPort;

                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                AsyncObject obj = new AsyncObject(1024);
                obj.pRemoteEp = new IPEndPoint(IPAddress.Parse(sIP), nPort);
                obj.socket = pSocket;

                byte[] btData = Encoding.ASCII.GetBytes(sData);
                //pSocket.SendTo(btData, obj.pRemoteEp);
                pSocket.BeginSendTo(btData, 0, btData.Length, 0, obj.pRemoteEp, sendToCallback, obj);
                pSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);
                pSocket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdpClient, obj);
            }
            catch (Exception)
            {
                //UTIL.PR("Send_UDP Exp: " + e.Message);
            }
        }

        public void Send_UDP(string sData)
        {
            if(pRemoteEp != null)
            {
                byte[] btData = Encoding.ASCII.GetBytes(sData);
                //pSocket.SendTo(btData, pRemoteEp);
                pSocket.BeginSendTo(btData, 0, btData.Length, 0, pRemoteEp, sendToCallback, pSocket);
                //DATA.gQueue.QUEUE_SetMode(INDEX, 2);
                //pRemoteEp = null;
            }
        }

        // Data Receive
        private void Recv()
        {
        }

        private void acceptCallback(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("acceptCallback");
                pClient = pSocket.EndAccept(ar);
                pSocket.BeginAccept(acceptCallback, null);

                AsyncObject obj = new AsyncObject(1024);
                obj.socket = pClient;
                pClient.BeginReceive(obj.buffer, 0, obj.size, 0, dataReceive, obj);
            }
            catch (Exception)
            {
            }
        }
        private void onConnected(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    CONNECT_STATE = 7;
                    AsyncObject obj = new AsyncObject(1024);
                    obj.socket = pClient;
                    pClient.BeginReceive(obj.buffer, 0, obj.size, 0, dataReceive, obj);
                }
                else
                {
                    CONNECT_STATE = 0;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (deleConn != null)
                {
                    deleConn();
                }
            }
            
        }
        private void dataReceive(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("dataReceive");
                AsyncObject obj = (AsyncObject)ar.AsyncState;
                int read = obj.socket.EndReceive(ar);
                if(read > 0)
                {
                    CONNECT_STATE = 7;
                    obj.sb.Append(System.Text.Encoding.UTF8.GetString(obj.buffer));
                    //string sData = System.Text.Encoding.UTF8.GetString(obj.buffer);
                    obj.ClearBuffer();
                    if (read < obj.size && read > 0)
                    {
                        if (deleRecv != null)
                        {
                            deleRecv(obj.sb.ToString());
                            //deleRecv(INDEX, sData.ToString());
                            obj.sb.Clear();
                        }
                    }
                    obj.socket.BeginReceive(obj.buffer, 0, obj.size, 0, dataReceive, obj);
                }
                else
                {
                    obj.socket.Close();
                    CONNECT_STATE = 0;
                }
            }
            catch (Exception)
            {
            }
        }

        private void dataReceiveUdp(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("dataReceiveUdp");
                AsyncObject obj = (AsyncObject)ar.AsyncState;
                int read = obj.socket.EndReceiveFrom(ar, ref obj.pRemoteEp);
                if (read > 0)
                {
                    CONNECT_STATE = 7;
                    obj.sb.Append(System.Text.Encoding.UTF8.GetString(obj.buffer));
                    obj.ClearBuffer();
                    if (read < obj.size && read > 0)
                    {
                        if (deleRecv != null)
                        {
                            pRemoteEp = obj.pRemoteEp;
                            deleRecv(obj.sb.ToString());
                            obj.sb.Clear();
                        }
                    }
                    obj.socket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdp, obj);
                }
                else
                {
                    obj.socket.Close();
                    CONNECT_STATE = 0;
                }
            }
            catch (Exception)
            {
            }
        }

        private void dataReceiveUdpClient(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("dataReceiveUdp");
                AsyncObject obj = (AsyncObject)ar.AsyncState;
                int read = obj.socket.EndReceiveFrom(ar, ref obj.pRemoteEp);
                if (read > 0)
                {
                    CONNECT_STATE = 7;
                    obj.sb.Append(System.Text.Encoding.UTF8.GetString(obj.buffer));
                    obj.ClearBuffer();
                    if (read < obj.size && read > 0)
                    {
                        if (deleRecv != null)
                        {
                            deleRecv(obj.sb.ToString());
                            obj.sb.Clear();
                        }
                        obj.socket.Close();
                        CONNECT_STATE = 0;
                        return;
                    }
                    obj.socket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdp, obj);
                }
                else
                {
                    obj.socket.Close();
                    CONNECT_STATE = 0;
                }
            }
            catch (Exception)
            {
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                nSendState = 1;
                Socket client = (Socket)ar.AsyncState;
                int nSentSize = client.EndSend(ar);
                nSendState = 2;
                if (deleSend != null)
                    deleSend();
            }
            catch (Exception)
            {
            }
        }

        private void sendToCallback(IAsyncResult ar)
        {
            try
            {
                nSendState = 1;
                Socket client = (Socket)ar.AsyncState;
                int nSentSize = client.EndSendTo(ar);
                nSendState = 2;
                if (deleSend != null)
                    deleSend();
            }
            catch (Exception)
            {
            }
        }
    }
}