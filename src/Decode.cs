using System;


namespace ywcai.normal.socket
{
    class Decode
    {
        public byte[] getToken(byte[] inStream)
        {
            byte[] token = new byte[ProtocalConfig.PROTOCOL_HEAD_SIZE_TOKEN];
            Array.Copy(inStream, ProtocalConfig.PROTOCOL_HEAD_POS_TOKEN, token, 0, ProtocalConfig.PROTOCOL_HEAD_SIZE_TOKEN);  
            return token;
        }
        public byte getHeadTag(byte[] inStream, Int32 pos)
        {
            return inStream[pos];
        }
        public Int32 getDataLen(byte[] inStream)
        {
            Int32 len;
            byte[] dtemp = new byte[4];
            dtemp[0] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 3];
            dtemp[1] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 2];
            dtemp[2] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 1];
            dtemp[3] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 0];
            len = BitConverter.ToInt32(dtemp, 0);
            return len;
        }
        public Int32 getPackLen(byte[] inStream,Int32 pos)
        {
            Int32 len;
            byte[] dtemp = new byte[4];
            dtemp[0] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 3 + pos];
            dtemp[1] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 2 + pos];
            dtemp[2] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 1 + pos];
            dtemp[3] = inStream[ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN + 0 + pos];
            Int32 dlenth = BitConverter.ToInt32(dtemp, 0);
            len = dlenth + ProtocalConfig.PROTOCOL_HEAD_POS_DATA;
            return len;
        }
        public byte[] getData(byte[] inStream)
        {
            Int32 dataLen = getDataLen(inStream);
            byte[] data = new byte[dataLen];
            Array.Copy(inStream, ProtocalConfig.PROTOCOL_HEAD_POS_DATA, data, 0, dataLen);
            return data;
        }
    }
}
