using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ywcai.normal.socket
{
   
    public class WorkSocket
    {
        public Socket session=null;
        public Boolean isConn=true;
        public String remoteIp = "0.0.0.0";
        public String remotePort = "0";
        public String remoteDeviceId = "000000";
    }
}
