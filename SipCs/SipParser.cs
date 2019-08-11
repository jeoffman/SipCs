using SipCs.Headers;
using SipCs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipCs
{
    public class SipParser
    {
        private const byte ByteCR = (byte)'\r';
        private const byte ByteLF = (byte)'\n';
        private static readonly byte[] ByteCRLR = { (byte)'\r', (byte)'\n' };

        public string RequestLine { get; set; }
        public SipRequest SipRequest { get; set; }

        public BaseHeaderDictionary Headers { get; set; } = new BaseHeaderDictionary();
        public List<ViaSipHeaderValue> ViaHeaders { get; private set; } = new List<ViaSipHeaderValue>();

        public string Body { get; set; }

        private ISipParserHandler _handler;

        public SipParser()
        {
            _handler = null;
        }

        public SipParser(ISipParserHandler handler)
        {
            _handler = handler;
        }

        /// <summary>Request line, optional headers, empty line, optional message body</summary>
        /// <param name="requestBytes">Complete request</param>
        /// <returns>sure</returns>
        public bool ParseRequest(byte[] requestBytes)
        {
            bool keepScanningHeaders = true;
            int scanPosition = 0;

            //Request Line
            int foundPosition = IndexOfSequence(requestBytes, ByteCRLR, scanPosition);
            if (foundPosition > 10) //minimum request line is like 12 bytes or something
            {
                //copy first (request) line for now and move on
                RequestLine = Encoding.UTF8.GetString(requestBytes, 0, foundPosition);
                SipRequest = new SipRequest(RequestLine);
                if (_handler != null)
                    _handler.OnRequestLine(RequestLine);
                scanPosition = foundPosition + ByteCRLR.Length;

                //find and extract all headers
                while (keepScanningHeaders)
                {
                    foundPosition = IndexOfSequence(requestBytes, ByteCRLR, scanPosition);
                    if (foundPosition == scanPosition)
                    {   //we must have found the body = copy and we're done
                        foundPosition += ByteCRLR.Length;
                        Body = Encoding.UTF8.GetString(requestBytes, foundPosition, requestBytes.Length - foundPosition);
                        keepScanningHeaders = false;
                        break;
                    }
                    else if (foundPosition > 0)
                    {
                        scanPosition = ExtractNextHeader(requestBytes, scanPosition);
                    }
                    else
                    {
                        keepScanningHeaders = false;
                        break;
                    }
                }

            }
            return false;
        }

        /// <summary>Read entire header, i.e. name and value(s), from byte array and add it to the Headers property</summary>
        /// <param name="requestBytes"></param>
        /// <param name="scanPosition"></param>
        /// <returns></returns>
        private int ExtractNextHeader(byte[] requestBytes, int scanPosition)
        {
            int foundPosition;
            //found a header, now what?
            int headerStartBytePosition = scanPosition;

            foundPosition = scanPosition + ByteCRLR.Length;   //skip CRLF from above
            int seekHeaderStopPosition = ScanForEndOfHeader(requestBytes, foundPosition);

            if (headerStartBytePosition < seekHeaderStopPosition)
            {   // create header buffer text
                string entireHeaderText = Encoding.ASCII.GetString(requestBytes, headerStartBytePosition, seekHeaderStopPosition - headerStartBytePosition);
                int indexOfColon = entireHeaderText.IndexOf(':');
                if (indexOfColon > 0)
                {
                    string headerName = entireHeaderText.Substring(0, indexOfColon).Trim();
                    indexOfColon++; //skip the colon while we get the value
                    string headerValue = entireHeaderText.Substring(indexOfColon, entireHeaderText.Length - indexOfColon).Trim();

                    ProcessHeader(headerName, headerValue);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid SIP header {entireHeaderText} missing colon separator");
                }
            }
            scanPosition = seekHeaderStopPosition + ByteCRLR.Length;
            return scanPosition;
        }

        private void ProcessHeader(string headerName, string headerValue)
        {
            //Save the "RAW" header value text
            if (!Headers.ContainsKey(headerName))
                Headers[headerName] = new List<string>();
            Headers[headerName].Add(headerValue);

            if (HeaderHelpers.LookupComapactHeader(headerName).Equals("Via"))
            {
                ViaSipHeaderValue viaHeaderValue = ViaSipHeaderValue.ParseViaHeaderValue(headerValue);
                ViaHeaders.Add(viaHeaderValue);
            }
        }

        /// <summary>Seeks through byte array to find the end of your current header</summary>
        /// <param name="requestBytes"></param>
        /// <param name="foundPosition"></param>
        /// <returns></returns>
        private static int ScanForEndOfHeader(byte[] requestBytes, int foundPosition)
        {
            int seekHeaderStopPosition = 0;
            bool scanningHeader = true;
            while (scanningHeader)
            {
                seekHeaderStopPosition = IndexOfSequence(requestBytes, ByteCRLR, foundPosition);
                byte possibleEndOfHeaderByte = requestBytes[seekHeaderStopPosition + ByteCRLR.Length];
                if (!char.IsWhiteSpace((char)possibleEndOfHeaderByte) ||
                    (requestBytes[seekHeaderStopPosition + ByteCRLR.Length] == ByteCR && requestBytes[seekHeaderStopPosition + ByteCRLR.Length + 1] == ByteLF))
                {   //We have found the end of the Header
                    scanningHeader = false;
                    break;
                }
                foundPosition = seekHeaderStopPosition + ByteCRLR.Length;
            }

            return seekHeaderStopPosition;
        }

        /// <summary>Based on = https://stackoverflow.com/a/332667/573377</summary>
        /// <param name="buffer">bytes to search</param>
        /// <param name="pattern">pattern to find</param>
        /// <param name="startIndex">index/position to begin searching</param>
        /// <returns>-1 for not found or the index/position where the pattern begins</returns>
        public static int IndexOfSequence(byte[] buffer, byte[] pattern, int startIndex)
        {
            int positions = -1;
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                System.Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                {
                    positions = i;
                    break;
                }
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }
    }
}
