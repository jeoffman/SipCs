using SipCs.Buffer;
using SipCs.Tests.SampleSipMessages;
using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace SipCs.Tests
{
    public class SipMessageBufferTests
    {
        [Fact]
        public void BufferWillResizeTest()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye);

            byte[] messageFirstHalf = new byte[messageBytes.Length / 2];
            Array.Copy(messageBytes, 0, messageFirstHalf, 0, messageFirstHalf.Length);
            byte[] messageSecondHalf = new byte[messageBytes.Length - messageFirstHalf .Length];
            Array.Copy(messageBytes, messageFirstHalf.Length, messageSecondHalf, 0, messageSecondHalf.Length);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageFirstHalf));
            buffer.AddBytes(new ReadOnlySequence<byte>(messageSecondHalf));

            byte[] bufferBytes = buffer.GetBytes();

            Assert.Equal(messageBytes, bufferBytes);
        }

        [Fact]
        public void MessageWithoutBodyGetsParsedTest()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));

            var ret = buffer.GetCompletedMessage();
            Assert.NotNull(ret);
            Assert.Equal(messageBytes, ret);
        }

        [Fact]
        public void ShortMessageWaitsForTheRestOfTheBytes1Test()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye);

            byte[] messageFirstHalf = new byte[messageBytes.Length - 1];
            Array.Copy(messageBytes, 0, messageFirstHalf, 0, messageFirstHalf.Length);
            byte[] messageSecondHalf = new byte[messageBytes.Length - messageFirstHalf.Length];
            Array.Copy(messageBytes, messageFirstHalf.Length, messageSecondHalf, 0, messageSecondHalf.Length);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageFirstHalf));
            var ret = buffer.GetCompletedMessage();
            Assert.Null(ret);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageSecondHalf));

            ret = buffer.GetCompletedMessage();
            Assert.NotNull(ret);
            Assert.Equal(messageBytes, ret);
        }

        [Fact]
        public void ShortMessageWaitsForTheRestOfTheBytes2Test()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye);

            byte[] messageFirstHalf = new byte[messageBytes.Length - 2];
            Array.Copy(messageBytes, 0, messageFirstHalf, 0, messageFirstHalf.Length);
            byte[] messageSecondHalf = new byte[messageBytes.Length - messageFirstHalf.Length];
            Array.Copy(messageBytes, messageFirstHalf.Length, messageSecondHalf, 0, messageSecondHalf.Length);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageFirstHalf));
            var ret1 = buffer.GetCompletedMessage();
            Assert.Null(ret1);

            buffer.AddBytes(new ReadOnlySequence<byte>(messageSecondHalf));
            var ret2 = buffer.GetCompletedMessage();
            Assert.NotNull(ret2);
            Assert.Equal(messageBytes, ret2);
        }

        [Fact]
        public void MessageWithBodyGetsParsedTest()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipRequests.SimpleInvite);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));

            var ret = buffer.GetCompletedMessage();
            Assert.NotNull(ret);
            Assert.Equal(messageBytes, ret);
        }

        [Fact]
        public void MessageWithBodySplitAtBodyGetsParsedTest()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipRequests.SimpleInvite);
            
            byte[] messageFirstHalf = new byte[506];    //506 is EXACTLY the start of the BODY  (i.e. after \r\n\r\n)
            Array.Copy(messageBytes, 0, messageFirstHalf, 0, messageFirstHalf.Length);
            byte[] messageSecondHalf = new byte[messageBytes.Length - messageFirstHalf.Length];
            Array.Copy(messageBytes, messageFirstHalf.Length, messageSecondHalf, 0, messageSecondHalf.Length);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageFirstHalf));
            var ret1 = buffer.GetCompletedMessage();
            Assert.Null(ret1);

            buffer.AddBytes(new ReadOnlySequence<byte>(messageSecondHalf));
            var ret2 = buffer.GetCompletedMessage();
            Assert.NotNull(ret2);
            Assert.Equal(messageBytes, ret2);
        }

        [Fact]
        public void MessageWithBodySplitAtHeaderGetsParsedTest()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipRequests.SimpleInvite);

            byte[] messageFirstHalf = new byte[502];    //502 is EXACTLY the end of the HEADERS (i.e. before \r\n\r\n)
            Array.Copy(messageBytes, 0, messageFirstHalf, 0, messageFirstHalf.Length);
            byte[] messageSecondHalf = new byte[messageBytes.Length - messageFirstHalf.Length];
            Array.Copy(messageBytes, messageFirstHalf.Length, messageSecondHalf, 0, messageSecondHalf.Length);

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageFirstHalf));
            var ret1 = buffer.GetCompletedMessage();
            Assert.Null(ret1);

            buffer.AddBytes(new ReadOnlySequence<byte>(messageSecondHalf));
            var ret2 = buffer.GetCompletedMessage();
            Assert.NotNull(ret2);
            Assert.Equal(messageBytes, ret2);
        }

        [Fact]
        public void BufferWithTwoMessagesGetsParsedTest()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye + ExampleSipRequests.SimpleInvite);  //behold! two messages

            SipMessageBuffer buffer = new SipMessageBuffer();
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret1 = buffer.GetCompletedMessage();
            byte[] exectedMessage1 = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye);
            Assert.Equal(exectedMessage1, ret1);

            var ret2 = buffer.GetCompletedMessage();
            Assert.NotNull(ret2);
            byte[] exectedMessage2 = Encoding.UTF8.GetBytes(ExampleSipRequests.SimpleInvite);
            Assert.Equal(exectedMessage2, ret2);
        }

        [Fact]
        public void BufferCleansUpAfterEachGetCompletedMessageTest()
        {
            SipMessageBuffer buffer = new SipMessageBuffer();

            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleBye);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret1 = buffer.GetCompletedMessage();
            Assert.Equal(messageBytes, ret1);

            messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleByeNoContentLength);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret2 = buffer.GetCompletedMessage();
            Assert.NotNull(ret2);
            Assert.Equal(messageBytes, ret2);

            messageBytes = Encoding.UTF8.GetBytes(ExampleSipRequests.SimpleInvite);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret3 = buffer.GetCompletedMessage();
            Assert.NotNull(ret3);
            Assert.Equal(messageBytes, ret3);

            messageBytes = Encoding.UTF8.GetBytes(ExampleSipResponses.SimpleRinging);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret4 = buffer.GetCompletedMessage();
            Assert.NotNull(ret4);
            Assert.Equal(messageBytes, ret4);
        }

        [Theory]
        [InlineData(Rfc4475TestMessages.AShortTortuousINVITE)]
        //[InlineData(Rfc4475TestMessages.CrapAtTheEnd)]    this one appears to be 2 complete messages in one string - not exactly as invalid as my tests would have it
        [InlineData(Rfc4475TestMessages.EscapedCharacters)]
        [InlineData(Rfc4475TestMessages.EscapedNulls)]
        [InlineData(Rfc4475TestMessages.LongValues)]
        [InlineData(Rfc4475TestMessages.NoWhiteSpaces)]
        [InlineData(Rfc4475TestMessages.ParametersInUriUserPart)]
        [InlineData(Rfc4475TestMessages.PercentIsNotEscape)]
        [InlineData(Rfc4475TestMessages.ValidCharacters)]
        [InlineData(ExampleSipResponses.SimpleRinging)]
        [InlineData(ExampleSipResponses.SimpleAck)]
        [InlineData(ExampleSipResponses.SimpleBye)]
        [InlineData(ExampleSipResponses.SimpleByeNoContentLength)]
        [InlineData(ExampleSipRequests.SimpleInvite)]
        public void BufferHandlesSpacesBeforeContentLengthColonTest(string message)
        {
            SipMessageBuffer buffer = new SipMessageBuffer();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret = buffer.GetCompletedMessage();
            Assert.NotNull(ret);
            Assert.Equal(messageBytes, ret);
        }

        
        [Fact]
        public void HandleCaseInsensitiveContentLength()
        {
            SipMessageBuffer buffer = new SipMessageBuffer();

            byte[] messageBytes = Encoding.UTF8.GetBytes(ExampleSipRequests.InviteWithWeirdCaseForContentLength);
            buffer.AddBytes(new ReadOnlySequence<byte>(messageBytes));
            var ret = buffer.GetCompletedMessage();
            Assert.Equal(messageBytes, ret);
        }

    }
}
