namespace SipCs
{
    /// <summary>Loosely modeled after  AspNetCore/src/Servers/Kestrel/Core/src/Internal/Http/IHttpHeadersHandler.cs </summary>
    public interface ISipParserHandler
    {
        void OnRequestLine(string targetUri);
        void OnHeader(string name, string value);
        void OnHeadersComplete();
    }
}
