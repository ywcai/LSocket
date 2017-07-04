using System;


namespace ywcai.normal.socket
{
    class Encode
    {
        
        public byte[] enData(byte[] token,byte[] pData)
        {
            Int32 intDataLen = pData.Length;
            byte[] bDataLen = new byte[4];
            bDataLen[3] = (byte)(intDataLen & 0xFF);
            bDataLen[2] = (byte)((intDataLen & 0xFF00) >> 8);
            bDataLen[1] = (byte)((intDataLen & 0xFF0000) >> 16);
            bDataLen[0] = (byte)((intDataLen >> 24) & 0xFF);
            byte[] temp = new byte[ProtocalConfig.PROTOCOL_HEAD_POS_DATA + intDataLen];
            temp[ProtocalConfig.PROTOCOL_HEAD_POS_FLAG] = ProtocalConfig.PROTOCOL_HEAD_FLAG;
            token.CopyTo(temp, ProtocalConfig.PROTOCOL_HEAD_POS_TOKEN);
            bDataLen.CopyTo(temp, ProtocalConfig.PROTOCOL_HEAD_POS_DATALEN);
            pData.CopyTo(temp, ProtocalConfig.PROTOCOL_HEAD_POS_DATA);
            return temp;
        }
    }
}
