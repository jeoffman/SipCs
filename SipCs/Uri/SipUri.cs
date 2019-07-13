using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SipCs.Uri
{
    public class SipUri
    {
        public string Contact { get; set; }
        public string Scheme { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }

        public List<UriParameter> UriParameters { get; set; } = new List<UriParameter>();

        public SipUri(string uriText)
        {
            Parse(uriText);
        }

        public void Parse(string uriText)
        {
            //couldn't get the other ones to work so I fumbled into this one
            // try    https://regex101.com/   for playing around
            string regexSipUriText = @"(?:"")?([^<""]*)(?:"")?[ ]*(?:<)?(sip(?:s)?|tel):([^@]+)@([^> ;]+)(?:>)?(?:[ ;])?(?:[;])?(.*)";

            Regex sipTry = new Regex(regexSipUriText);
            var matches = sipTry.Match(uriText);

            Contact = matches.Groups[1].Value.Trim();
            Scheme = matches.Groups[2].Value;

            var usernamePassword = matches.Groups[3].Value;
            var indexOfColon = usernamePassword.IndexOf(':');
            if (indexOfColon > 0)
            {
                UserName = usernamePassword.Substring(0, indexOfColon);
                Password = usernamePassword.Substring(indexOfColon + 1);
            }
            else
            {
                UserName = usernamePassword;
            }

            var domainAndPort = matches.Groups[4].Value;
            string regexIpv6Text = @"(\[.*\])(?::)?([0-9]*)?";
            Regex ipv6Try = new Regex(regexIpv6Text);
            var matchIpv6 = ipv6Try.Match(domainAndPort);
            if(matchIpv6.Success)
            {
                Domain = matchIpv6.Groups[1].Value;
                if (!string.IsNullOrEmpty(matchIpv6.Groups[2].Value))
                    Port = int.Parse(matchIpv6.Groups[2].Value);
            }
            else
            {
                indexOfColon = domainAndPort.IndexOf(':');
                if (indexOfColon > 0)
                {
                    Domain = domainAndPort.Substring(0, indexOfColon);
                    Port = int.Parse(domainAndPort.Substring(indexOfColon + 1));
                }
                else
                {
                    Domain = domainAndPort;
                }
            }

            var tags = matches.Groups[5].Value;
            var splitParameters = tags.Split(';');
            foreach (var parameter in splitParameters)
            {
                var parameterTrim = parameter.Trim();
                if (parameterTrim.Length > 0)   //this is a HACK because my regex fails on some trailing parameters with spaces after the first semi-colon
                {
                    var nameValueSplit = parameterTrim.Split('=');
                    UriParameter newParameter = new UriParameter();
                    newParameter.Name = nameValueSplit[0].Trim();
                    if (nameValueSplit.Length > 1)
                        newParameter.Value = nameValueSplit[1].Trim();
                    UriParameters.Add(newParameter);
                }
            }

            //Potential Java implementation:
            ///https://github.com/r3gis3r/CSipSimple/blob/master/src/com/csipsimple/api/SipUri.java
            //private final static String SIP_SCHEME_RULE = "sip(?:s)?|tel";
            //private final static String DIGIT_NBR_RULE = "^[0-9\\-#\\+\\*\\(\\)]+$";
            //private final static Pattern SIP_CONTACT_ADDRESS_PATTERN = Pattern.compile("^([^@:]+)@([^@]+)$");
            //private final static Pattern SIP_CONTACT_PATTERN = Pattern.compile("^(?:\")?([^<\"]*)(?:\")?[ ]*(?:<)?("+SIP_SCHEME_RULE+"):([^@]+)@([^>]+)(?:>)?$");
            //private final static Pattern SIP_HOST_PATTERN = Pattern.compile("^(?:\")?([^<\"]*)(?:\")?[ ]*(?:<)?("+SIP_SCHEME_RULE+"):([^@>]+)(?:>)?$");
            //java "Contact"   ^(?:\")?([^<\"]*)(?:\")?[ ]*(?:<)?(sip(?:s)?|tel):([^@]+)@([^>]+)(?:>)?$

            //Potential Python implementation:
            //https://stackoverflow.com/questions/25516948/python-regular-expression-for-sip-uri-variables
            //python = (?P<scheme>\w+):(?:(?P<user>[\w\.]+):?(?P<password>[\w\.]+)?@)?\[?(?P<host>(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|(?:(?:[0-9a-fA-F]{1,4}):){7}[0-9a-fA-F]{1,4}|(?:(?:[0-9A-Za-z]+\.)+[0-9A-Za-z]+))\]?:?(?P<port>\d{1,6})?(?:\;(?P<params>[^\?]*))?(?:\?(?P<headers>.*))?
        }
    }
}
