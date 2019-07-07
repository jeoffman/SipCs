using System.Collections.Generic;

namespace SipCs.Headers
{
    public static class HeaderHelpers
    {
        public static readonly Dictionary<string, string> CompactHeaders = new Dictionary<string, string>
        {
            { "a", "Accept-Contact" },
            { "u", "Allow-Events" },
            { "i", "Call-ID" },
            { "m", "Contact" },
            { "e", "Content-Encoding" },
            { "l", "Content-Length" },
            { "c", "Content-Type" },
            { "o", "Event" },
            { "f", "From" },
            { "b", "Referred-By" },
            { "r", "Refer-to" },
            { "s", "Subject" },
            { "k", "Supported" },
            { "t", "To" },
            { "v", "Via" },
        };
    }
}
