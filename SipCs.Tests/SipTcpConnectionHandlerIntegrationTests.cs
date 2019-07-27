using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SipCs.Tests.SampleSipMessages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SipCs.Tests
{
    public class SipTcpConnectionHandlerIntegrationTests
    {
        [Fact]
        public async Task Connection_handler_should_parse_sip_bytes_correctly()
        {
            //start up a kestrel server
            //don't await this task because it won't complete until
            //the server is shutdown
            var webHostTask = WebHost
                            .CreateDefaultBuilder()
                            //.ConfigureKestrel((KestrelServerOptions options) =>
                            //{
                            //    options
                            //    .Listen(new IPEndPoint(IPAddress.Any, 8009), listenOptions =>
                            //    {
                            //        listenOptions.UseConnectionHandler<SipTcpConnectionHandler>();
                            //    });
                            //})
                            .ConfigureLogging(logBuilder =>
                            {
                                logBuilder.AddConsole();
                            })
                            .UseKestrel(options =>
                            {
                                options.ListenLocalhost(8007, listenOptions =>
                                {
                                    listenOptions.UseConnectionHandler<SipTcpConnectionHandler>();
                                });
                            })
                            .UseStartup<Startup>()
                            .Build()
                            .RunAsync();

            //open a TCP connection to it
            TcpClient tcpClient = new TcpClient();

            byte[] messageBytes = Encoding.ASCII.GetBytes(Rfc4475TestMessages.AShortTortuousINVITE);

            await tcpClient.ConnectAsync(IPAddress.Loopback, 8007);

            var stream = tcpClient.GetStream();

            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            //Make sure we parsed sip stuff somehow?

            byte[] responseBuffer = new byte[1024];

            var response = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

        }


        class Startup
        {
            // This method gets called by the runtime. Use this method to add services to the container.
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddLogging(options =>
                {
                    options.AddConsole();
                });

                services.AddSingleton(new SipParser());
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
            }
        }
    }
}
