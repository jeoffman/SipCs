using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SipCs
{
    public class SipTcpConnectionHandler : ConnectionHandler
    {
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            throw new NotImplementedException();
        }
    }
}
