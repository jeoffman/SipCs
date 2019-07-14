using SipCs.Uri;
using System;

namespace SipCs.Request
{
    public class SipRequest
    {
        public string Method { get; set; }  //TODO: make this an enum, except the torture tests make it sound like the PARSER should allow weird methods (i.e. the application layer reject them)
        public SipUri Host { get; set; }
        public string Version { get; set; } //TODO: drill into this a bit

        public SipRequest(string requestLine)
        {
            Parse(requestLine);
        }

        public void Parse(string requestLine)
        {
            var requestLineSplit = requestLine.Split(' ');
            if (requestLineSplit.Length != 3)
                throw new InvalidOperationException($"{nameof(SipRequest)} {nameof(Parse)} got invalid Request Line \"{requestLine}\"");
            Method = requestLineSplit[0];
            Host = new SipUri(requestLineSplit[1]);
            Version = requestLineSplit[2];
        }
    }
}
