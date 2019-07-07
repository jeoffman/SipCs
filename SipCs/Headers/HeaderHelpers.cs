using System.Collections.Generic;

namespace SipCs.Headers
{
    public static class HeaderHelpers
    {
        public static string LookupComapactHeader(string possibleCompactHeader)
        {
            string retval;
            if (HeaderHelpers.CompactHeaders.ContainsKey(possibleCompactHeader))
                retval = HeaderHelpers.CompactHeaders[possibleCompactHeader];
            else
                retval = possibleCompactHeader; //probably not a compact header, I hope you know what you are doing
            return retval;
        }

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
