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
        private static readonly byte[] ByteCRLRCRLR = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

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

        /// <summary>
        /// Request line, optional headers, empty line, optional message body.
        /// Returns true if bytes were a complete SIP message and headers were
        /// parsed.  Returns false if passed in bytes are NOT a complete SIP message.
        /// </summary>
        /// <param name="requestBytes">Complete request</param>
        /// <returns>sure</returns>
        public bool ParseRequest(byte[] requestBytes)
        {
            bool keepScanningHeaders = true;
            int scanPosition = 0;

            bool isSipMessageComplete = false;

            int indexOfHeaderEndSequence = IndexOfSequence(requestBytes, ByteCRLRCRLR, 0);

            if (indexOfHeaderEndSequence > 0)
            {
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
                        {
                            //This is the case where we have received the CRLF/CRLF
                            //we must have found the body = copy and we're done
                            foundPosition += ByteCRLR.Length;

                            int contentLength = GetContentLength();

                            if(requestBytes.Length - foundPosition == contentLength)
                            {
                                //we have the whole body, so let's parse it!
                                Body = Encoding.UTF8.GetString(requestBytes, foundPosition, requestBytes.Length - foundPosition);
                                //The sip message is now complete, so let's let the caller know...
                                isSipMessageComplete = true;
                            }

                            //We either have the complete body parsed, or we don't
                            //either way we're done!
                            keepScanningHeaders = false;
                        }
                        else if (foundPosition > 0)
                        {
                            //we have a single CRLF, so that means we have one header to extract
                            scanPosition = ExtractNextHeader(requestBytes, scanPosition);

                            //but we haven't received CRLF/CRLF yet so let's tell caller that the SIP
                            //message isn't complete
                        }
                        else
                        {
                            //We have not found a CRLF, let's stop scanning headers but also tell caller
                            //that this is not a complete SIP Message
                            keepScanningHeaders = false;
                        }
                    }
                }
            }

            return isSipMessageComplete;
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
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                {
                    positions = i;
                    break;
                }
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }

        private int GetContentLength()
        {
            if (Headers.Headers.TryGetValue("content-length", out var lengthHeaderValues))
            {
                if (lengthHeaderValues.Count > 1)
                    throw new ArgumentException($"Sip message has multiple content-length values!");

                string contentLengthValueString = lengthHeaderValues.Single();

                if (int.TryParse(contentLengthValueString, out var contentLength))
                {
                    return contentLength;
                }
                else
                {
                    throw new ArgumentException($"Content length is not a number!");
                }
            }
            else
            {
                throw new ArgumentException($"Sip message is missing content-length!");
            }
        }
    }
}
