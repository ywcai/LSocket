namespace ywcai.normal.socket
{
  
    class ProtocalConfig
    {
        //协议
        public const int PROTOCOL_HEAD_FLAG = 0x01;
        public const int PROTOCOL_HEAD_POS_FLAG = 0;
        public const int PROTOCOL_HEAD_POS_TOKEN = 1;
        public const int PROTOCOL_HEAD_SIZE_TOKEN = 16;
        public const int PROTOCOL_HEAD_POS_DATALEN = 17;
        public const int PROTOCOL_HEAD_POS_DATA = 21;
        public const int INT_SOCKET_BUFFER_SIZE = 2048 * 16;


        public const int ERR_CODE_START_SERVER = 0;
        public const int ERR_CODE_START_CLIENT = 1;
        public const int ERR_CODE_SEND_DATA = 2;
        public const int ERR_CODE_RECEIVE_DATA = 3;
        public const int ERR_CODE_FLAG = 4;
        public const int ERR_CODE_CLOSE = 5;
        public const int ERR_CODE_HEALTH_CHECKED = 6;

        //0:服务端启动失败
        //1:客户端监理连接失败
        //2:发送数据失败
        //3:接收数据失败
        //4:接收数据头非法
        //5:关闭数据异常
    }
}
