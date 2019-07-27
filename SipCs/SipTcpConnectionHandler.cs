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
        private readonly SipParser sipParser;

        public SipTcpConnectionHandler(ILogger<SipTcpConnectionHandler> logger, SipParser sipParser)
        {
            this.logger = logger;
            this.sipParser = sipParser;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            logger.LogDebug($"Received connection: {connection.ConnectionId}");

            while(true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;

                //sipParser.ParseRequest(buffer);


            }
        }
    }
}
