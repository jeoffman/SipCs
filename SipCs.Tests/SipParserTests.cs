using Moq;
using SipCs.Tests.SampleSipMessages;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Xunit;

namespace SipCs.Tests
{
    public class SipParserTests
    {
        [Fact]
        public void TortuousInviteRawSipHeaderTextTest()
        {
            string expectedBody = @"v=0
o=mhandley 29739 7272939 IN IP4 192.0.2.3
s=-
c=IN IP4 192.0.2.4
t=0 0
m=audio 49217 RTP/AVP 0 12
m=video 3227 RTP/AVP 31
a=rtpmap:31 LPC
";
            var mock = new Mock<ISipParserHandler>();
            SipParser parser = new SipParser(mock.Object);

            byte[] messageBytes = Encoding.ASCII.GetBytes(Rfc4475TestMessages.AShortTortuousINVITE);

            parser.ParseRequest(messageBytes);

            //NOTE: these asserts look ugly because we are testing the RAW text from the message

            Assert.Equal("INVITE sip:vivekg@chair-dnrc.example.com;unknownparam SIP/2.0", parser.RequestLine);  //TODO: drill in for just the parts: verb, sip address (without that "unknownparam" bit), etc.
            Assert.Equal(13, parser.Headers.Count);
            Assert.Equal("sip:vivekg@chair-dnrc.example.com ;   tag    = 1918181833n", parser.Headers["to"].First());   //TODO: drill in to strip that "tag" bit
            Assert.Equal(@"""J Rosenberg \\\""""       <sip:jdrosen@example.com>
  ;
  tag = 98asjd8", parser.Headers["from"].First());   //TODO: drill in to separate the torture cruft

            Assert.Equal("0068", parser.Headers["max-forwards"].First());
            Assert.Equal("wsinv.ndaksdj@192.0.2.1", parser.Headers["call-id"].First());
            Assert.Equal("150", parser.Headers["Content-Length"].First());
            Assert.Equal(@"0009
  INVITE", parser.Headers["cseq"].First()); //TODO - its ugly, probably not right
            Assert.Equal(@"SIP  /   2.0
 /UDP
    192.0.2.2;branch=390skdjuw", parser.Headers["via"].First()); //TODO - its ugly, probably not right
            Assert.Equal("", parser.Headers["s"].First());

            Assert.Equal(@"newfangled value
 continued newfangled value", parser.Headers["NewFangledHeader"].First());
            Assert.Equal(";;,,;;,;", parser.Headers["UnknownHeaderWithUnusualValue"].First());
            Assert.Equal("application/sdp", parser.Headers["Content-Type"].First());
            Assert.Equal("<sip:services.example.com;lr;unknownwith=value;unknown-no-value>", parser.Headers["Route"].First());
            Assert.Equal(@"SIP  / 2.0  / TCP     spindle.example.com   ;
  branch  =   z9hG4bK9ikj8  ,
 SIP  /    2.0   / UDP  192.168.255.111   ; branch=
 z9hG4bK30239", parser.Headers["v"].Last());    //NOTE: there were 2 "Via" in the torture header, one was a "v", ahem...
            Assert.Equal(@"""Quoted string \""\"""" <sip:jdrosen@example.com> ; newparam =
      newvalue ;
  secondparam ; q = 0.33", parser.Headers["m"].First());

            Assert.Equal(expectedBody, parser.Body);  //TODO: drill in
        }

        [Fact]
        public void MissingHeaderThrowsSomethingTest()
        {
            var mock = new Mock<ISipParserHandler>();
            SipParser parser = new SipParser(mock.Object);

            byte[] messageBytes = Encoding.ASCII.GetBytes(ExampleSipMessages.SimpleInvite);

            parser.ParseRequest(messageBytes);
            Assert.Throws<KeyNotFoundException>(() => parser.Headers["Header Which Is Not There"]);   //TODO: maybe some other kind of exception? or a NULL return value?

            //ViaHeaderValue val = new ViaHeaderValue("", "");
        }

        [Fact]
        public void UnderstandCompactHeadersTest()
        {
            var mock = new Mock<ISipParserHandler>();
            SipParser parser = new SipParser(mock.Object);

            byte[] messageBytes = Encoding.ASCII.GetBytes(ExampleSipMessages.SimpleInvite);

            parser.ParseRequest(messageBytes);

            Assert.Equal(@"""Test 15"" <sip:15@10.10.1.99>tag=as58f4201b", parser.Headers["f"].First());  //NOTE: "f" is "from"

            Assert.Equal(@"<sip:13@10.10.1.13>", parser.Headers["t"].First());  //NOTE: "t" is "from"
            Assert.Equal(@"SIP/2.0/UDP 10.10.1.99:5060;branch=z9hG4bK343bf628;rport", parser.Headers["v"].First());  //NOTE: "v" is "Via"

            Assert.Equal(@"<sip:15@10.10.1.99>", parser.Headers["m"].First());  //NOTE: "m" is "Contact"
            Assert.Equal(@"326371826c80e17e6cf6c29861eb2933@10.10.1.99", parser.Headers["i"].First());  //NOTE: "i" is "call-id"
            Assert.Equal(@"INVITE, ACK, CANCEL, OPTIONS, BYE, REFER, SUBSCRIBE, NOTIFY", parser.Headers["u"].First());  //NOTE: "u" is "Allow-Events"
            Assert.Equal(@"replaces", parser.Headers["k"].First());  //NOTE: "k" is "supported"
            Assert.Equal(@"application/sdp", parser.Headers["c"].First());  //NOTE: "c" is "Content-Type"
            Assert.Equal(@"258", parser.Headers["l"].First());  //NOTE: "l" is "Content-Length"
            //Assert.Equal(@"xxxxxxxx", parser.Headers["o"].First());  //NOTE: "o" is "Event"
            //Assert.Equal(@"xxxxxxxx", parser.Headers["e"].First());  //NOTE: "e" is "Content-Encoding"
//CSeq: 102 INVITE
//User-Agent: Asterisk PBX
//Max-Forwards: 70
//Date: Wed, 06 Dec 2009 14:12:45 GMT
        }
    }
}
