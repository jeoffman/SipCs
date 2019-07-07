namespace SipCs
{
    /// <summary>Loosely modeled after  AspNetCore/src/Servers/Kestrel/Core/src/Internal/Http/IHttpHeadersHandler.cs </summary>
    public interface ISipParserHandler
    {
        //void OnStartLine(SipMethod method, SipVersion version, string targetUri); //maybe some day, if I want to drill in that deep....
        void OnRequestLine(string targetUri);
        void OnHeader(string name, string value);
        void OnHeadersComplete();
    }
}
