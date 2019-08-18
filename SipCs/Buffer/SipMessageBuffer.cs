using System;
using System.Buffers;
using System.Linq;
using System.Text;

namespace SipCs.Buffer
{
    public class SipMessageBuffer
    {
        public const int StartBufferSize = 1024;
        public const int BufferGrowSize = 1024;

        byte[] _buffer = new byte[StartBufferSize];
        long _currentPosition = 0;
        int? _contentLength = null;
        int? _bodyStartPosition = null;


        public bool AddBytes(ReadOnlySequence<byte> bytes)
        {
            bool retval = false;

            long bufferRemaining = _buffer.Length - _currentPosition;
            if (bytes.Length > bufferRemaining)
            {
                int numberOfChunksToGrowBy = (int)((bytes.Length - bufferRemaining) / BufferGrowSize);
                if (((bytes.Length - bufferRemaining) % BufferGrowSize > 0))
                    numberOfChunksToGrowBy++;
                Array.Resize(ref _buffer, _buffer.Length + numberOfChunksToGrowBy * BufferGrowSize);
            }
            Array.Copy(bytes.ToArray(), 0, _buffer, _currentPosition, bytes.Length);
            _currentPosition += bytes.Length;

            return retval;
        }

        public byte[] GetBytes()
        {
            byte[] retval = new byte[_currentPosition];
            Array.Copy(_buffer, 0, retval, 0, _currentPosition);
            return retval;
        }

        public byte[] GetCompletedMessage()
        {
            byte[] retval = null;

            if(_contentLength == null)
            {
                byte[] contentLengthBytes = Encoding.UTF8.GetBytes("\nContent-Length");

                //scan for Content-Length and math plus BODY, or missing Content-Length and at least BODY
                int position = SimpleBoyerMooreSearch(_buffer, contentLengthBytes);
                if (position != -1)
                {   // we found content length so try to get the length of the BODY from this message
                    position += contentLengthBytes.Length;
                }
                else if (position == -1)
                {   //try searching for "Compact Header" l:
                    bool compactHeaderFound = false;
                    position = 0;
                    while (position < _currentPosition)  //don't go past the head
                    {
                        if (_buffer[position] == (byte)'l' || _buffer[position] == (byte)'L')   //case insensitive compact header l:
                        {// skip white space and look for : (good) or CRLF (bad)
                            position++; //skip the l
                            while (position < _currentPosition)  //don't go past the head
                            {
                                if (_buffer[position] == (byte)' ' || _buffer[position] == (byte)'\t')
                                {
                                    //skip white space and keep scanning
                                }
                                else if (_buffer[position] == (byte)':')
                                {
                                    compactHeaderFound = true;
                                    break;
                                }
                                else
                                {
                                    compactHeaderFound = false;
                                    break;
                                }
                                position++;
                            }
                            if (compactHeaderFound)
                                break;
                        }
                        position++;
                    }
                    if (!compactHeaderFound)
                        position = -1;  //I smell a HACK here!
                }

                if (position != -1)
                {   // we found content length so try to get the length of the BODY from this message
                    //seek the :
                    while (position < _currentPosition)  //don't go past the head
                    {
                        if (_buffer[position] == (byte)':')
                        {
                            position++;
                            break;
                        }
                        position++;
                    }

                    //find first non-white space
                    while (position < _currentPosition)  //don't go past the head
                    {
                        if (_buffer[position] != (byte)' ' && _buffer[position] != (byte)'\t')  //skip all white space
                            break;
                        position++;
                    }

                    int positionContentLengthBegin = position;
                    while (position < _currentPosition)  //don't go past the head
                    {
                        if (_buffer[position] == (byte)'\r' || _buffer[position] == (byte)'\n') //scan for \r
                            break;
                        position++;
                    }
                    int positionContentLengthEnd = position;

                    if (positionContentLengthEnd < _currentPosition)
                    {   //parse Content-Length integer text
                        byte[] contentLengthBuffer = new byte[positionContentLengthEnd - positionContentLengthBegin];
                        Array.Copy(_buffer, positionContentLengthBegin, contentLengthBuffer, 0, contentLengthBuffer.Length);
                        string contentLengthText = System.Text.Encoding.UTF8.GetString(contentLengthBuffer);
                        _contentLength = int.Parse(contentLengthText);
                    }
                }
            }

            if (_bodyStartPosition == null)
            {
                byte[] crlFCrLf = Encoding.UTF8.GetBytes("\r\n\r\n");

                //scan for Content-Length and math plus BODY, or missing Content-Length and at least BODY
                int position = SimpleBoyerMooreSearch(_buffer, crlFCrLf);
                if (position != -1)
                {
                    _bodyStartPosition = position + crlFCrLf.Length;
                }
            }

            int messageLength = 0;
            //check for body with no content-length or content-length = 0
            if (_bodyStartPosition != null && (_contentLength == null || _contentLength.Value == 0))
            {
                messageLength = _bodyStartPosition.Value;
            }
            //check for content-length set, body started, and we got all the bytes
            else if (_contentLength.HasValue && _bodyStartPosition.HasValue && _currentPosition >= (_bodyStartPosition.Value + _contentLength.Value))
            {
                messageLength = _bodyStartPosition.Value + _contentLength.Value;
            }

            if (messageLength != 0)
            {
                retval = new byte[messageLength];
                Array.Copy(_buffer, 0, retval, 0, messageLength);

                Array.Copy(_buffer, messageLength, _buffer, 0, _currentPosition - messageLength);

                _currentPosition -= messageLength;
                _bodyStartPosition = null;
                _contentLength = null;
            }

            return retval;
        }

        //https://stackoverflow.com/a/9890164/573377
        static int SimpleBoyerMooreSearch(byte[] haystack, byte[] needle)
        {
            int[] lookup = new int[256];
            for (int i = 0; i < lookup.Length; i++) { lookup[i] = needle.Length; }

            for (int i = 0; i < needle.Length; i++)
            {
                lookup[needle[i]] = needle.Length - i - 1;
            }

            int index = needle.Length - 1;
            var lastByte = needle.Last();
            while (index < haystack.Length)
            {
                var checkByte = haystack[index];
                if (CaseInsensitveUtf8Comapare(haystack[index], lastByte))
                {
                    bool found = true;
                    for (int j = needle.Length - 2; j >= 0; j--)
                    {
                        if (!CaseInsensitveUtf8Comapare(haystack[index - needle.Length + j + 1], needle[j]))
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return index - needle.Length + 1;
                    else
                        index++;
                }
                else
                {
                    index += lookup[checkByte];
                }
            }
            return -1;
        }

        static bool CaseInsensitveUtf8Comapare(byte left, byte right)
        {
            return Char.ToLower((char)left) == Char.ToLower((char)right);
        }
    }
}
