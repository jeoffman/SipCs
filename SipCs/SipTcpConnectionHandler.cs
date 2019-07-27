using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SipCs
{
    public class SipTcpConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<SipTcpConnectionHandler> logger;

        public SipTcpConnectionHandler(ILogger<SipTcpConnectionHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            logger.LogDebug($"Received connection: {connection.ConnectionId}");

            while(true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;
            }
        }
    }
}
