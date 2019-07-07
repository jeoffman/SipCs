using System;
using System.Collections.Generic;
using System.Text;

namespace SipCs.Headers
{
    //BNF:
    //Via               =  ( "Via" / "v" ) HCOLON via-parm *(COMMA via-parm)
    //via-parm          =  sent-protocol LWS sent-by *( SEMI via-params )
    //via-params        =  via-ttl / via-maddr
    //                     / via-received / via-branch
    //                     / via-extension
    //via-ttl           =  "ttl" EQUAL ttl
    //via-maddr         =  "maddr" EQUAL host
    //via-received      =  "received" EQUAL (IPv4address / IPv6address)
    //via-branch        =  "branch" EQUAL token
    //via-extension     =  generic-param

    /// <summary>like System.Net.Http.Headers, except different</summary>
    /// <remarks>I wonder how well all of these string operations are going to perform....</remarks> 
    public class ViaSipHeaderValue //: ICloneable    necessary?
    {
        public string TransportProtocol { get; set; }
        public string ClientHost { get; set; }
        public string Maddr { get; set; }
        public string Ttl { get; set; }
        public string Received { get; set; }
        public string Branch { get; set; }

        public static ViaSipHeaderValue ParseViaHeaderValue(string headerValue)
        {
            string transportProtocol;
            string clientHost;
            string maddr = null;    //optional
            string ttl = null;      //optional
            string received = null; //optional
            string branch = null;   //optional

            headerValue = headerValue.Trim();
            string[] splitHeader = headerValue.Split('/');

            string protocolName = splitHeader[0].Trim();
            string protocolVersion = splitHeader[1].Trim();
            string theRest = splitHeader[2].Trim();
            string transport = theRest.Substring(0, theRest.IndexOf(' '));
            transportProtocol = $"{protocolName}/{protocolVersion}/{transport}";

            string[] splitClientHostAndTags = theRest.Substring(theRest.IndexOf(' ')).Trim().Split(';');

            var clientHostText = splitClientHostAndTags[0].Trim();
            string[] clientHostParts = clientHostText.Split(':');
            string clientHostAddress = clientHostParts[0].Trim();
            string clientHostPort = null;
            if (clientHostParts.Length > 1)
                clientHostPort = clientHostParts[1].Trim();

            if (string.IsNullOrEmpty(clientHostPort))
                clientHost = clientHostAddress;
            else
                clientHost = $"{clientHostAddress}:{clientHostPort}";

            for (int countTags = 1; countTags < splitClientHostAndTags.Length; countTags++)
            {
                var tag = splitClientHostAndTags[countTags].Trim();

                int equalsSignIndex = tag.IndexOf('=');
                if (equalsSignIndex > 0)
                {
                    var tagName = tag.Substring(0, equalsSignIndex).Trim();
                    var tagValue = tag.Substring(equalsSignIndex + 1).Trim();

                    switch (tagName.ToLower())
                    {
                        case "maddr":
                            maddr = tagValue;
                            break;
                        case "ttl":
                            ttl = tagValue;
                            break;
                        case "received":
                            received = tagValue;
                            break;
                        case "branch":
                            branch = tagValue;
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown Via tag {tagName} in header");
                    }
                }
            }

            return new ViaSipHeaderValue
            {
                TransportProtocol = transportProtocol,
                ClientHost = clientHost,
                Maddr = maddr,
                Ttl = ttl,
                Received = received,
                Branch = branch,
            };
        }
    }
}
