using SipCs.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SipCs.Tests
{
    public class ViaHeaderValueTests
    {
        [Fact]
        public void ParseViaHeaderValueTest()
        {
            var testee = ViaSipHeaderValue.ParseViaHeaderValue(@"   SIP / 2.0	/ UDP first.example.com: 4000;ttl=16 ;maddr=224.2.0.1 ;branch=z9hG4bKa7c6a8dlze.1   ");
            Assert.Equal("SIP/2.0/UDP", testee.TransportProtocol);
            Assert.Equal("first.example.com:4000", testee.ClientHost);
            Assert.Equal("16", testee.Ttl);
            Assert.Equal("224.2.0.1", testee.Maddr);
            Assert.Equal("z9hG4bKa7c6a8dlze.1", testee.Branch);
        }

        [Fact]
        public void ParseViaHeaderValueNoClientHostPortButWithTagsTest()
        {
            var testee = ViaSipHeaderValue.ParseViaHeaderValue(@"SIP/2.0/UDP first.example.com;maddr=224.2.0.1");
            Assert.Equal("SIP/2.0/UDP", testee.TransportProtocol);
            Assert.Equal("first.example.com", testee.ClientHost);
            Assert.Equal("224.2.0.1", testee.Maddr);
        }

        [Fact]
        public void ParseViaHeaderValueNoClientHostPortAndNoTagsTest()
        {
            var testee = ViaSipHeaderValue.ParseViaHeaderValue(@"SIP/2.0/UDP first.example.com");
            Assert.Equal("SIP/2.0/UDP", testee.TransportProtocol);
            Assert.Equal("first.example.com", testee.ClientHost);
        }

        [Fact]
        public void ParseViaHeaderValueWithlientHostPortAndNoTagsTest()
        {
            var testee = ViaSipHeaderValue.ParseViaHeaderValue(@"SIP/2.0/UDP first.example.com:4000");
            Assert.Equal("SIP/2.0/UDP", testee.TransportProtocol);
            Assert.Equal("first.example.com:4000", testee.ClientHost);
        }
    }
}
