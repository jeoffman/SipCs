namespace SipCs.Tests.SampleSipMessages
{
    public class ExampleSipResponses
    {
        public const string SimpleRinging = @"SIP/2.0 180 Ringing
CSeq: 1 INVITE
Call-ID: MIAMGC0120091027172219041244@209.244.63.32
Contact: <sip:+15678@4.68.250.148:5060;transport=udp;nat=yes>
From: ""Carro Ramon""  <sip:5678@4.68.250.148>;tag=VPSF506071629460
Max-Forwards: 70
Record-Route: <sip:174.37.45.134;lr=on;ftag=VPSF506071629460>, <sip:67.228.177.9;lr=on;ftag=VPSF506071629460>, <sip:216.82.224.202;lr;ftag=VPSF506071629460>, <sip:4.79.212.229;lr;ftag=VPSF506071629460>
To: <sip:+11234@4.79.212.229:5060>;tag=dAmXcBGL
Via: SIP/2.0/UDP 174.37.45.134;branch=z9hG4bK9767.ad406992.0, SIP/2.0/UDP 67.228.177.9;rport=5060;branch=z9hG4bK9767.760c9624.0, SIP/2.0/UDP 216.82.224.202;rport=5060;received=216.82.224.202;branch=z9hG4bK9767.823f8e12.0, SIP/2.0/UDP 216.82.224.202;branch=z9hG4bK9767.723f8e12.0, SIP/2.0/UDP 4.79.212.229;branch=z9hG4bK9767.e30c5303.0, SIP/2.0/UDP 4.68.250.148:5060;branch=z9hG4bK506071629460-1256581032616
Content-Length: 0

";

//from:   https://stackoverflow.com/questions/1632499/problem-with-sip-bye-message

        public const string SimpleAck = @"ACK sip:+15678@xx.xx.xxx.xx:5060;transport=udp SIP/2.0
Record-Route: <sip:174.37.45.134;lr=on;ftag=VPSF506071629460>
Record-Route: <sip:67.228.177.9;lr=on;ftag=VPSF506071629460>
Record-Route: <sip:216.82.224.202;lr;ftag=VPSF506071629460>
Via: SIP/2.0/UDP 174.37.45.134;branch=z9hG4bK9767.ad406992.2
Via: SIP/2.0/UDP 67.228.177.9;rport=5060;branch=z9hG4bK9767.760c9624.2
Via: SIP/2.0/UDP 216.82.224.202;rport=5060;received=216.82.224.202;branch=z9hG4bK9767.723f8e12.2
Via: SIP/2.0/UDP 4.79.212.229;branch=z9hG4bK9767.e30c5303.2
Via: SIP/2.0/UDP 4.68.250.148:5060;branch=z9hG4bK506071629460-1256581032653
From: ""CARRO RAMON    ""  <sip:+15678@4.68.250.148;isup-oli=0>;tag=VPSF506071629460
To: <sip:+11234@4.79.212.229:5060>;tag=BYFeP7T1
Call-ID: MIAMGC0120091027172219041244@209.244.63.32
CSeq: 1 ACK
Contact: <sip:4.68.250.148:5060;transport=udp>
Max-Forwards: 66
Content-Length: 0

";

        public const string SimpleBye = @" SIP/2.0 180 Ringing
Via: SIP/2.0/TCP  client.atlanta.example.com:5060
;branch=z9hG4bK74bf9
;received=192.0.2.101
From: Alice  <sip:alice@atlanta.example.com>
;tag=9fxced76sl
To: Bob  <sip:bob@biloxi.example.com>
;tag=8321234356
Call-ID: 3848276298220188511@atlanta.example.com
CSeq: 1 INVITE
Contact: <sip:bob@client.biloxi.example.com;transport=tcp>
Content-Length: 0

";

        public const string SimpleByeNoContentLength = @"BYE sip:5678@xx.xx.xxx.xx SIP/2.0
Via: SIP/2.0/UDP 192.168.1.121:5060;branch=z9hG4bK-598f1319
From: <sip:3998@xx.xx.xxx.xx>;tag=53cca4372c533924i0
To: ""(null)"" <sip:5678@xx.xx.xxx.xx>;tag=7b2add35
Call-ID: AW6zfKQ8RWl71MipIe4X1WWKfw7xGGR9 @chat.seohosting.com
CSeq: 101 BYE
Max-Forwards: 70
User-Agent: Linksys/SPA941-5.1.8

";
    }
}
