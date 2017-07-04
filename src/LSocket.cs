using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ywcai.normal.socket
{
    public class LSocket
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public event Action<Boolean> ServerStart;
        public event Action<WorkSocket> AcceptClient;
        public event Action<Boolean> ClientStart;
        public event Action<WorkSocket> sessionClosed;
        public event Action<WorkSocket, Byte[], Byte[]> ReceiveMessage;
        public event Action<WorkSocket,int,String> ErrLog;
        private Socket service = null;
        private Decode decode = new Decode();
        private byte[] token = { 0x3a, 0x3b, 0x3c, 0x3d, 0x4a, 0x4b, 0x4c, 0x4d, 0x5a, 0x5b, 0x5c, 0x5d, 0x6a, 0x6b, 0x6c, 0x6d };
        private byte[] healthPackage = { 0x3d };
        public Int32 healthCheckTime = 5000;

        public byte[] getToken()
        {
            return token;
        }
        public void setHealthCheckTime(Int32 pTime)
        {
            healthCheckTime = pTime;
        }
        public void CreateServer(int PORT)
        {
            try
            {
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, PORT);
                service = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                service.Bind(iep);
                service.Listen(2);
                service.BeginAccept(new AsyncCallback(Accept), service);
                ServerStart(true);
                allDone.WaitOne();
            }
            catch (Exception e)
            {
                ErrLog(null,ProtocalConfig.ERR_CODE_START_SERVER, e.ToString());
                ServerStart(false);
            }
        }
        private void Accept(IAsyncResult ar)
        {
            Socket socket = ((Socket)ar.AsyncState).EndAccept(ar);
            service.BeginAccept(new AsyncCallback(Accept), service);
            AsynRecive(socket);
        }
        private void AsynRecive(Socket socket)
        {
            WorkSocket workSocket=new WorkSocket();
            workSocket.session=socket;
            workSocket.remoteIp = socket.RemoteEndPoint.ToString().Split(':')[0];
            workSocket.remotePort = socket.RemoteEndPoint.ToString().Split(':')[1];
            AcceptClient(workSocket);
            ThreadPool.QueueUserWorkItem(new WaitCallback(healthCheck),(Object)workSocket);
            StartRecive(workSocket);
        }
        private void healthCheck(Object _workSocket)
        {
            WorkSocket workSocket = (WorkSocket)_workSocket;
            while (workSocket.isConn)
            {
                if (!sent(workSocket, healthPackage))
                {
                    //sessionClosed(remoteIP);
                    ErrLog(workSocket, ProtocalConfig.ERR_CODE_HEALTH_CHECKED, "心跳检测网络异常");
                }
                else
                {
                    ErrLog(workSocket, ProtocalConfig.ERR_CODE_HEALTH_CHECKED, "心跳检测网络正常");
                }
                Thread.Sleep(healthCheckTime);
            }
        }
        public Socket CreateSession(String ip, Int32 port)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(ip, port);
                client.ReceiveBufferSize = ProtocalConfig.INT_SOCKET_BUFFER_SIZE;
                client.SendBufferSize = ProtocalConfig.INT_SOCKET_BUFFER_SIZE;
                ClientStart(true);
            }
            catch (Exception e)
            {
                ErrLog(null,ProtocalConfig.ERR_CODE_START_CLIENT,e.ToString());
                ClientStart(false);
            }
            return client;
        }
        public void CloseSession(WorkSocket worksocket)
        {
            try
            {
                worksocket.session.Close();
            }
            catch (Exception e)
            {
                ErrLog(worksocket, ProtocalConfig.ERR_CODE_CLOSE, e.ToString());
            }
            worksocket.isConn = false;
            sessionClosed(worksocket);
        }

        public Boolean sent(WorkSocket worksocket, byte[] data)
        {

            Encode encode = new Encode();
            byte[] buf = encode.enData(token,data);
            try
            {

                worksocket.session.Send(buf);
            }
            catch (Exception e)
            {
                ErrLog(worksocket,ProtocalConfig.ERR_CODE_SEND_DATA, e.ToString());
                //CloseSession(worksocket);
                return false;
            }
            return true;
        }


        private Boolean assembleData(WorkSocket worksocket, BufferState bufState)
        {
            if (!bufState.hasRemaing)
            {
                try
                {
                    bufState.remaining = worksocket.session.Receive(bufState.buf);
                    bufState.hasRemaing = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    ErrLog(worksocket,ProtocalConfig.ERR_CODE_RECEIVE_DATA, e.ToString());
                    CloseSession(worksocket);
                    return false;
                }
            }
            if (bufState.hasHead)
            {
                while (bufState.remaining < ProtocalConfig.PROTOCOL_HEAD_POS_DATA)
                {
                    Array.Copy(bufState.buf, bufState.bufPos, bufState.buf, 0, bufState.remaining);
                    Int32 size = 0;
                    try
                    {
                        size = worksocket.session.Receive(bufState.buf, bufState.remaining, ProtocalConfig.INT_SOCKET_BUFFER_SIZE - bufState.remaining, SocketFlags.None);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        ErrLog(worksocket,ProtocalConfig.ERR_CODE_RECEIVE_DATA, e.ToString());
                        CloseSession(worksocket);
                        return false;
                    }
                    bufState.remaining = bufState.remaining + size;
                    bufState.bufPos = 0;
                    bufState.tempPos = 0;
                }
                byte headFlag = decode.getHeadTag(bufState.buf, bufState.bufPos);
                if (headFlag == ProtocalConfig.PROTOCOL_HEAD_FLAG)
                {
                    bufState.target = decode.getPackLen(bufState.buf, bufState.bufPos);
                    if (bufState.target < ProtocalConfig.PROTOCOL_HEAD_POS_DATA || bufState.target >= Int32.MaxValue)
                    {
                        bufState.drop();
                    }
                    else
                    {
                        bufState.isRightPackage = true;
                        bufState.pending = bufState.target;
                        bufState.temp = new byte[bufState.target];
                    }
                }
                else
                {
                    bufState.drop();
                    ErrLog(worksocket,ProtocalConfig.ERR_CODE_FLAG, "接收到非法标识符");
                    CloseSession(worksocket);
                    return false;
                }
            }

            if (bufState.pending == bufState.remaining && bufState.isRightPackage == true)
            {
                Array.Copy(bufState.buf, bufState.bufPos, bufState.temp, bufState.tempPos, bufState.remaining);
                byte[] token = decode.getToken(bufState.temp);
                byte[] result = decode.getData(bufState.temp);
                ReceiveMessage(worksocket,token, result);
                bufState.init();
            }

            if (bufState.pending < bufState.remaining && bufState.isRightPackage == true)
            {
                Array.Copy(bufState.buf, bufState.bufPos, bufState.temp, bufState.tempPos, bufState.pending);
                byte[] token = decode.getToken(bufState.temp);
                byte[] result = decode.getData(bufState.temp);
                ReceiveMessage(worksocket, token, result);
                bufState.skip();
            }
            if (bufState.pending > bufState.remaining && bufState.isRightPackage == true)
            {
                Array.Copy(bufState.buf, bufState.bufPos, bufState.temp, bufState.tempPos, bufState.remaining);
                bufState.connect();
            }
            return true;
        }
        public void StartRecive(WorkSocket worksocket)
        {
            BufferState bufferState = new BufferState();
            bufferState.init();
            while (true)
            {
                if (!assembleData(worksocket, bufferState))
                {
                    break;
                }
            }
        }
    }
}
