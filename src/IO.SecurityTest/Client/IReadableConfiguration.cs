using System.Collections.Generic;

namespace IO.SecurityTest.Client
{
    public interface IReadableConfiguration
    {
        string AccessToken { get; }
        IDictionary<string, string> ApiKey { get; }
        IDictionary<string, string> ApiKeyPrefix { get; }
        string BasePath { get; }
        string DateTimeFormat { get; }
        IDictionary<string, string> DefaultHeader { get; }
        string TempFolderPath { get; }
        int Timeout { get; }
        string UserAgent { get; }
        string Username { get; }
        string Password { get; }
        string GetApiKeyWithPrefix(string apiKeyIdentifier);
    }
}
