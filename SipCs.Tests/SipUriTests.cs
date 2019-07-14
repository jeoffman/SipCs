using SipCs.Uri;
using System;
using System.Linq;
using Xunit;

namespace SipCs.Tests
{
    public class SipUriTests
    {
        #region Basic "Don't Throw an Exception" tests
        [Theory]
        [InlineData("sip:watson@bell-telephone.com")]
        [InlineData("<sip:watson@bell-telephone.com>")]
        [InlineData("Test a contact <sip:watson@bell-telephone.com>")]
        [InlineData("sip:watson:password@bell-telephone.com")]
        [InlineData("<sip:watson:password@bell-telephone.com>")]
        [InlineData("Test a contact <sip:watson:password@bell-telephone.com>")]
        [InlineData("\"Test\" <sip:watson@bell-telephone.com>")]
        [InlineData("Test       <sip:watson@bell-telephone.com>")]
        [InlineData("sip:alice@atlanta.com")]
        [InlineData("sip:alice:secretword@atlanta.com;transport=tcp")]
        [InlineData("sips:alice@atlanta.com?subject=project%20x&priority=urgent")]
        [InlineData("sip:+1-212-555-1212:1234@gateway.com;user=phone")]
        [InlineData("sips:1212@gateway.com")]
        [InlineData("sip:alice@192.0.2.4")]
        [InlineData("sip:alice;day=tuesday@atlanta.com")]
        [InlineData("sip:%61lice@atlanta.com;transport=TCP")]
        [InlineData("sip:alice@AtLanTa.CoM;Transport=tcp")]
        [InlineData("sip:+358-555-1234567;postd=pp22@foo.com;user=phone")]
        [InlineData("\"Bob\" <sips:bob@biloxi.com> ;tag=a48s")]
        [InlineData("sip:+12125551212@phone2net.com;tag=887s")]
        [InlineData("Anonymous <sip:c8oqz84zk7z@privacy.org>;tag=hyh8")]
        [InlineData("<sip:carol@chicago.com>")]
        [InlineData("Alice <sip:alice@atlanta.com>;tag=1928301774")]
        [InlineData("<sip:alice@pc33.atlanta.com>")]
        [InlineData("\"Caller\" <sip:caller@[2001:db8::1]")]
        [InlineData("\"Joe\" <sip:joe@example.org>;;;;")]
        [InlineData("\"T. desk phone\" <sip:ted@[::ffff:192.0.2.2]>")]
        [InlineData("\"T. desk phone\" <sip:ted@[::ffff:192.0.2.2]:5060>")]
        [InlineData("sip:user;par=u%40example.net @example.com")]
        //[InlineData("sip:atlanta.com;method=REGISTER?to=alice%40atlanta.com")]    //not sure what to do with this one
        //[InlineData("sip:user@host?Subject=foo&Call-Info=<http://www.foo.com>")]  //not sure what to do with this one
        //[InlineData("sip:watson@bell-telephone.com\r\n;\r\nparam=value")]             //CRLF in parameters is messing with my regex
        //[InlineData("<sip:watson@bell-telephone.com>\r\n;\r\nparam\r\n=\r\nvalue")]   //CRLF in parameters is messing with my regex
        public void JustTryNotToBarfTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);
            Assert.True(true);  //no exception = no barf?
        }
        #endregion Basic "Don't Throw an Exception" tests

        #region Basic parse tests: Contact, password, port, and various spacings
        [Theory]
        [InlineData("sip:watson@bell-telephone.com")]
        [InlineData("<sip:watson@bell-telephone.com>")]
        public void SipWatsonTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@bell-telephone.com>")]
        [InlineData("\"Test a contact\"  sip:watson@bell-telephone.com")]
        [InlineData("Test a contact     sip:watson@bell-telephone.com")]
        public void SipWatsonContactTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@bell-telephone.com")]
        [InlineData("  sip:watson:password@bell-telephone.com")]
        public void SipWatsonEmptyContactPasswordTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
        }

        [Theory]
        [InlineData("sip:watson@bell-telephone.com:5060")]
        [InlineData("<sip:watson@bell-telephone.com:5060>")]
        public void SipWatsonPortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@bell-telephone.com:5060>")]
        [InlineData("\"Test a contact\"  sip:watson@bell-telephone.com:5060")]
        [InlineData("Test a contact     sip:watson@bell-telephone.com:5060")]
        public void SipWatsonContactPortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@bell-telephone.com:5060")]
        [InlineData("  sip:watson:password@bell-telephone.com:5060")]
        public void SipWatsonEmptyContactPasswordPortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }

        [Theory]
        [InlineData("\"Test a contact\"  sip:watson:password@bell-telephone.com")]
        [InlineData("Test a contact sip:watson:password@bell-telephone.com")]
        [InlineData("Test a contact           sip:watson:password@bell-telephone.com")]
        [InlineData("\"Test a contact\"  <sip:watson:password@bell-telephone.com>")]
        [InlineData("Test a contact <sip:watson:password@bell-telephone.com>")]
        [InlineData("Test a contact           <sip:watson:password@bell-telephone.com>")]
        public void SipWatsonContactPasswordTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
        }

        [Theory]
        [InlineData("\"Test a contact\"  sip:watson:password@bell-telephone.com:5060")]
        [InlineData("Test a contact sip:watson:password@bell-telephone.com:5060")]
        [InlineData("Test a contact           sip:watson:password@bell-telephone.com:5060")]
        [InlineData("\"Test a contact\"  <sip:watson:password@bell-telephone.com:5060>")]
        [InlineData("Test a contact <sip:watson:password@bell-telephone.com:5060>")]
        [InlineData("Test a contact           <sip:watson:password@bell-telephone.com:5060>")]
        public void SipWatsonContactPasswordPortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }
        #endregion Basic parse tests: Contact, password, port, and various spacings

        #region IPv6 tests with contact, password, port, and various spacings
        [Theory]
        [InlineData("sip:watson@[::ffff:192.0.2.2]")]
        [InlineData("<sip:watson@[::ffff:192.0.2.2]>")]
        public void SipWatsonIpv6Test(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@[::ffff:192.0.2.2]>")]
        [InlineData("\"Test a contact\"  sip:watson@[::ffff:192.0.2.2]")]
        [InlineData("Test a contact     sip:watson@[::ffff:192.0.2.2]")]
        public void SipWatsonIpv6ContactTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@[::ffff:192.0.2.2]")]
        [InlineData("  sip:watson:password@[::ffff:192.0.2.2]")]
        public void SipWatsonIpv6EmptyContactPasswordTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
        }

        [Theory]
        [InlineData("sip:watson@[::ffff:192.0.2.2]:5060")]
        [InlineData("<sip:watson@[::ffff:192.0.2.2]:5060>")]
        public void SipWatsonIpv6PortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@[::ffff:192.0.2.2]:5060>")]
        [InlineData("\"Test a contact\"  sip:watson@[::ffff:192.0.2.2]:5060")]
        [InlineData("Test a contact     sip:watson@[::ffff:192.0.2.2]:5060")]
        public void SipWatsonIpv6ContactPortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@[::ffff:192.0.2.2]:5060")]
        [InlineData("  sip:watson:password@[::ffff:192.0.2.2]:5060")]
        public void SipWatsonIpv6EmptyContactPasswordPortTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal(5060, testee.Port);
        }
        #endregion IPv6 tests with contact, password, port, and various spacings

        #region Testing various parameters
        /// <summary>from RFC4475 - Section 3.1.1.9.  Semicolon separated parameters in URI user part</summary>
        [Fact]
        public void HostWithUserParameterParsesTest()
        {
            var testee = new SipUri("sip:user;par=u%40example.net@example.com");

            Assert.Empty(testee.Contact);
            Assert.Equal("user;par=u%40example.net", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("example.com", testee.Domain);
        }

        [Theory]
        [InlineData("sip:watson@bell-telephone.com;param=value")]
        [InlineData("<sip:watson@bell-telephone.com>;param=value")]
        [InlineData("sip:watson@bell-telephone.com ; param=value")]
        [InlineData("<sip:watson@bell-telephone.com> ; param = value")]
        [InlineData("sip:watson@bell-telephone.com   ;    param=value")]
        [InlineData("<sip:watson@bell-telephone.com> ;   param    =    value")]
        //[InlineData("sip:watson@bell-telephone.com\r\n;\r\nparam=value")]
        //[InlineData("<sip:watson@bell-telephone.com>\r\n;\r\nparam\r\n=\r\nvalue")]
        public void SipWatsonParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Single(testee.UriParameters);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@bell-telephone.com>;param=value")]
        [InlineData("\"Test a contact\"  sip:watson@bell-telephone.com;param=value")]
        [InlineData("Test a contact     sip:watson@bell-telephone.com;param=value")]
        public void SipWatsonContactParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@bell-telephone.com;param=value")]
        [InlineData("  sip:watson:password@bell-telephone.com;param=value")]
        public void SipWatsonEmptyContactPasswordParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("sip:watson@bell-telephone.com:5060;param=value")]
        [InlineData("<sip:watson@bell-telephone.com:5060>;param=value")]
        public void SipWatsonPortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@bell-telephone.com:5060>;param=value")]
        [InlineData("\"Test a contact\"  sip:watson@bell-telephone.com:5060;param=value")]
        [InlineData("Test a contact     sip:watson@bell-telephone.com:5060;param=value")]
        public void SipWatsonContactPortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@bell-telephone.com:5060;param=value")]
        [InlineData("  sip:watson:password@bell-telephone.com:5060;param=value")]
        public void SipWatsonEmptyContactPasswordPortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("\"Test a contact\"  sip:watson:password@bell-telephone.com;param=value")]
        [InlineData("Test a contact sip:watson:password@bell-telephone.com;param=value")]
        [InlineData("Test a contact           sip:watson:password@bell-telephone.com;param=value")]
        [InlineData("\"Test a contact\"  <sip:watson:password@bell-telephone.com>;param=value")]
        [InlineData("Test a contact <sip:watson:password@bell-telephone.com>;param=value")]
        [InlineData("Test a contact           <sip:watson:password@bell-telephone.com>;param=value")]
        public void SipWatsonContactPasswordParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("\"Test a contact\"  sip:watson:password@bell-telephone.com:5060;param=value")]
        [InlineData("Test a contact sip:watson:password@bell-telephone.com:5060;param=value")]
        [InlineData("Test a contact           sip:watson:password@bell-telephone.com:5060;param=value")]
        [InlineData("\"Test a contact\"  <sip:watson:password@bell-telephone.com:5060>;param=value")]
        [InlineData("Test a contact <sip:watson:password@bell-telephone.com:5060>;param=value")]
        [InlineData("Test a contact           <sip:watson:password@bell-telephone.com:5060>;param=value")]
        public void SipWatsonContactPasswordPortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("bell-telephone.com", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("sip:watson@[::ffff:192.0.2.2];param=value")]
        [InlineData("<sip:watson@[::ffff:192.0.2.2]>;param=value")]
        public void SipWatsonIpv6ParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@[::ffff:192.0.2.2]>;param=value")]
        [InlineData("\"Test a contact\"  sip:watson@[::ffff:192.0.2.2];param=value")]
        [InlineData("Test a contact     sip:watson@[::ffff:192.0.2.2];param=value")]
        public void SipWatsonIpv6ContactParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@[::ffff:192.0.2.2];param=value")]
        [InlineData("  sip:watson:password@[::ffff:192.0.2.2];param=value")]
        public void SipWatsonIpv6EmptyContactPasswordParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("sip:watson@[::ffff:192.0.2.2]:5060;param=value")]
        [InlineData("<sip:watson@[::ffff:192.0.2.2]:5060>;param=value")]
        public void SipWatsonIpv6PortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("Test a contact <sip:watson@[::ffff:192.0.2.2]:5060>;param=value")]
        [InlineData("\"Test a contact\"  sip:watson@[::ffff:192.0.2.2]:5060;param=value")]
        [InlineData("Test a contact     sip:watson@[::ffff:192.0.2.2]:5060;param=value")]
        public void SipWatsonIpv6ContactPortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Equal("Test a contact", testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Theory]
        [InlineData("\"\" sip:watson:password@[::ffff:192.0.2.2]:5060;param=value")]
        [InlineData("  sip:watson:password@[::ffff:192.0.2.2]:5060;param=value")]
        public void SipWatsonIpv6EmptyContactPasswordPortParameterTest(string sipUirText)
        {
            var testee = new SipUri(sipUirText);

            Assert.Empty(testee.Contact);
            Assert.Equal("watson", testee.UserName);
            Assert.Equal("password", testee.Password);
            Assert.Equal("[::ffff:192.0.2.2]", testee.Domain);
            Assert.Equal(5060, testee.Port);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }

        [Fact]
        public void HostWithUserParameterAndTrailingParameterTest()
        {
            var testee = new SipUri("sip:user;par=u%40example.net@example.com;param=value");

            Assert.Empty(testee.Contact);
            Assert.Equal("user;par=u%40example.net", testee.UserName);
            Assert.Null(testee.Password);
            Assert.Equal("example.com", testee.Domain);
            Assert.Equal("param", testee.UriParameters.Single().Name);
            Assert.Equal("value", testee.UriParameters.Single().Value);
        }
        #endregion Testing various parameters

        [Theory]
        [InlineData("BARF")]
        [InlineData("sip:user@domain.com:NaN")]
        public void GarbageSipUrisShouldThrowTest(string sipUirText)
        {
            Assert.Throws<InvalidOperationException>(() => {var testee = new SipUri(sipUirText); } );
        }

        [Theory(Skip ="These SIP URIs are invalid but my parser is too lenient")]
        [InlineData("sip:us@er@domain.com")]
        [InlineData("test:sip:watson:password@[::ffff:192.0.2.2]:5060;param=value")]  //regex is too permissive methinks
        public void GarbageSipUrisShouldThrowAndNeedSomeHelpTest(string sipUirText)
        {
            //TODO: make these URIs throw an exception instead of just passing
            Assert.Throws<InvalidOperationException>(() => { var testee = new SipUri(sipUirText); });
        }
    }
}
