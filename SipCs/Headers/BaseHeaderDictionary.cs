using System;
using System.Collections.Generic;

namespace SipCs.Headers
{
    /// <summary>Mostly a wrapper around a case-insensitive dictionary that will also translate "Compact Header Names"</summary>
    public class BaseHeaderDictionary
    {
        public Dictionary<string, List<string>> Headers { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public int Count
        {
            get { return Headers.Count; }
        }

        internal bool ContainsKey(string headerName)
        {
            return Headers.ContainsKey(LookupComapactHeader(headerName));
        }

        public List<string> this[string headerName]
        {
            get
            {
                return Headers[LookupComapactHeader(headerName)];
            }

            set
            {
                Headers[LookupComapactHeader(headerName)] = value;
            }
        }

        public string LookupComapactHeader(string possibleCompactHeader)
        {
            string retval;
            if (HeaderHelpers.CompactHeaders.ContainsKey(possibleCompactHeader))
                retval = HeaderHelpers.CompactHeaders[possibleCompactHeader];
            else
                retval = possibleCompactHeader; //probably not a compact header, I hope you know what you are doing
            return retval;
        }
    }
}
