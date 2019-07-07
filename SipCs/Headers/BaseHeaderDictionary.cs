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
            return Headers.ContainsKey(HeaderHelpers.LookupComapactHeader(headerName));
        }

        public List<string> this[string headerName]
        {
            get
            {
                return Headers[HeaderHelpers.LookupComapactHeader(headerName)];
            }

            set
            {
                Headers[HeaderHelpers.LookupComapactHeader(headerName)] = value;
            }
        }
    }
}
