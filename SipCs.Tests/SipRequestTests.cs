using SipCs.Request;
using Xunit;

namespace SipCs.Tests
{
    public class SipRequestTests
    {
        [Fact]
        public void SimpleRequestTest()
        {
            var testee = new SipRequest("INVITE sip:B@bbb.com SIP/2.0");
            Assert.Equal("INVITE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("B", testee.Host.UserName);
            Assert.Equal("bbb.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void SimpleRequestWithPortInDomainTest()
        {
            var testee = new SipRequest("INVITE sip:B@bbb.com:5060 SIP/2.0");
            Assert.Equal("INVITE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("B", testee.Host.UserName);
            Assert.Equal("bbb.com", testee.Host.Domain);
            Assert.Equal(5060, testee.Host.Port);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void UnknownHeaderWithUnusualValueTortureTest()
        {
            var testee = new SipRequest("INVITE sip:vivekg@chair-dnrc.example.com;unknownparam SIP/2.0");
            Assert.Equal("INVITE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("vivekg", testee.Host.UserName);
            Assert.Equal("chair-dnrc.example.com", testee.Host.Domain);
            Assert.Single(testee.Host.UriParameters);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void InviteRequestFromEscapingTortureTest()
        {   //RFC 4475 - Section 3.1.1.3.  Valid Use of the % Escaping Mechanism
            var testee = new SipRequest("INVITE sip:sips%3Auser%40example.com@example.net SIP/2.0");
            Assert.Equal("INVITE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("sips%3Auser%40example.com", testee.Host.UserName);
            Assert.Equal("example.net", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void FunkyRegisterTortureTest()
        {   //RFC 4475 - 3.1.1.5.  Use of % When It Is Not an Escape
            var testee = new SipRequest("RE%47IST%45R sip:registrar.example.com SIP/2.0");
            Assert.Equal("RE%47IST%45R", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("", testee.Host.UserName);
            Assert.Equal("registrar.example.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseMessageTest()
        {   // from http://docs.yate.ro/wiki/SIP_Methods
            var testee = new SipRequest("MESSAGE sip:234@192.168.168.156 SIP/2.0");
            Assert.Equal("MESSAGE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("234", testee.Host.UserName);
            Assert.Equal("192.168.168.156", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseReferTest()
        {   // from RFC 3515
            var testee = new SipRequest("REFER sip:b@atlanta.example.com SIP/2.0");
            Assert.Equal("REFER", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("b", testee.Host.UserName);
            Assert.Equal("atlanta.example.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseAckTest()
        {   // from SIP Call Flow Examples - IETF
            var testee = new SipRequest("ACK sip:UserB@there.com SIP/2.0");
            Assert.Equal("ACK", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("UserB", testee.Host.UserName);
            Assert.Equal("there.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseByeTest()
        {   // from SIP Call Flow Examples - IETF
            var testee = new SipRequest("BYE sip:UserA@here.com SIP/2.0");
            Assert.Equal("BYE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("UserA", testee.Host.UserName);
            Assert.Equal("here.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }
        
        [Fact]
        public void ParseCancelTest()
        {   // from https://community.cisco.com/t5/ip-telephony-and-phones/sip-cancel-message-sent-too-soon/td-p/1685925
            var testee = new SipRequest("CANCEL sip:12225551234@did.voip.les.net:5060 SIP/2.0");
            Assert.Equal("CANCEL", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("12225551234", testee.Host.UserName);
            Assert.Equal("did.voip.les.net", testee.Host.Domain);
            Assert.Equal(5060, testee.Host.Port);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParsePublishTest()
        {   // from https://community.cisco.com/t5/ip-telephony-and-phones/sip-cancel-message-sent-too-soon/td-p/1685925
            var testee = new SipRequest("PUBLISH sip:Andrew@example.com SIP/2.0");
            Assert.Equal("PUBLISH", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("Andrew", testee.Host.UserName);
            Assert.Equal("example.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseSubscribeTest()
        {   // from https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-sip/d0f0ef0c-0702-4573-b096-cf3cc646a310
            var testee = new SipRequest("SUBSCRIBE sip:user1@server.contoso.com SIP/2.0");
            Assert.Equal("SUBSCRIBE", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("user1", testee.Host.UserName);
            Assert.Equal("server.contoso.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseNotifyTest()
        {   // from https://www.sharetechnote.com/html/IMS_SIP_Procedure_Subscribe_Reg.html
            var testee = new SipRequest("NOTIFY sip:+11234567890@test.3gpp.com SIP/2.0");
            Assert.Equal("NOTIFY", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("+11234567890", testee.Host.UserName);
            Assert.Equal("test.3gpp.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }

        [Fact]
        public void ParseOptionsTest()
        {   // from https://www.avaya.com/blogs/archives/2014/04/understanding-the-sip-options-request.html
            var testee = new SipRequest("OPTIONS sip:carol@chicago.com SIP/2.0");
            Assert.Equal("OPTIONS", testee.Method);
            Assert.Equal("sip", testee.Host.Scheme);
            Assert.Equal("carol", testee.Host.UserName);
            Assert.Equal("chicago.com", testee.Host.Domain);
            Assert.Equal("SIP/2.0", testee.Version);
        }
        
    }
}
