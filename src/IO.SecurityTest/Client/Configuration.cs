using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace IO.SecurityTest.Client
{
    public class Configuration : IReadableConfiguration
    {
        #region Constants
        public const string Version = "1.0.0";
        public const string ISO8601_DATETIME_FORMAT = "o";
        #endregion Constants
        #region Static Members
        public static readonly ExceptionFactory DefaultExceptionFactory = (methodName, response) =>
        {
            var status = (int)response.StatusCode;
            if (status >= 400)
            {
                return new ApiException(status,
                    string.Format("Error calling {0}: {1}", methodName, response.Content),
                    response.Content);
            }
            if (status == 0)
            {
                return new ApiException(status,
                    string.Format("Error calling {0}: {1}", methodName, response.ErrorMessage), response.ErrorMessage);
            }
            return null;
        };
        #endregion Static Members
        #region Private Members
        private IDictionary<string, string> _apiKey = null;
        private IDictionary<string, string> _apiKeyPrefix = null;
        private string _dateTimeFormat = ISO8601_DATETIME_FORMAT;
        private string _tempFolderPath = Path.GetTempPath();
        #endregion Private Members
        #region Constructors
        public Configuration(string basePath)
        {
            UserAgent = "Apihub-Codegen/1.0.0/csharp";
            BasePath = basePath;
            DefaultHeader = new ConcurrentDictionary<string, string>();
            ApiKey = new ConcurrentDictionary<string, string>();
            ApiKeyPrefix = new ConcurrentDictionary<string, string>();
            Timeout = 100000;
        }
        #endregion Constructors
        #region Properties
        private ApiClient _apiClient = null;
        public virtual ApiClient ApiClient
        {
            get
            {
                if (_apiClient == null) _apiClient = CreateApiClient();
                return _apiClient;
            }
        }
        private String _basePath = null;
        public virtual string BasePath {
            get { return _basePath; }
            set {
                _basePath = value;
                if(_apiClient != null) {
                    _apiClient.RestClient.BaseUrl = new Uri(_basePath);
                }
            }
        }
        public virtual IDictionary<string, string> DefaultHeader { get; set; }
        public virtual int Timeout
        {
            
            get { return ApiClient.RestClient.Timeout; }
            set { ApiClient.RestClient.Timeout = value; }
        }
        public virtual string UserAgent { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public string GetApiKeyWithPrefix(string apiKeyIdentifier)
        {
            var apiKeyValue = "";
            ApiKey.TryGetValue (apiKeyIdentifier, out apiKeyValue);
            var apiKeyPrefix = "";
            if (ApiKeyPrefix.TryGetValue (apiKeyIdentifier, out apiKeyPrefix))
                return apiKeyPrefix + " " + apiKeyValue;
            else
                return apiKeyValue;
        }
        public virtual string AccessToken { get; set; }
        public virtual string TempFolderPath
        {
            get { return _tempFolderPath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _tempFolderPath = Path.GetTempPath();
                    return;
                }
                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }
                if (value[value.Length - 1] == Path.DirectorySeparatorChar)
                {
                    _tempFolderPath = value;
                }
                else
                {
                    _tempFolderPath = value + Path.DirectorySeparatorChar;
                }
            }
        }
        public virtual string DateTimeFormat
        {
            get { return _dateTimeFormat; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _dateTimeFormat = ISO8601_DATETIME_FORMAT;
                    return;
                }
                _dateTimeFormat = value;
            }
        }
        public virtual IDictionary<string, string> ApiKeyPrefix
        {
            get { return _apiKeyPrefix; }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("ApiKeyPrefix collection may not be null.");
                }
                _apiKeyPrefix = value;
            }
        }
        public virtual IDictionary<string, string> ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("ApiKey collection may not be null.");
                }
                _apiKey = value;
            }
        }
        #endregion Properties
        #region Methods
        public void AddDefaultHeader(string key, string value)
        {
            DefaultHeader[key] = value;
        }
        public ApiClient CreateApiClient()
        {
            return new ApiClient(this);
        }
        public static String ToDebugReport()
        {
            String report = "C# SDK (IO.SecurityTest) Debug Report:\n";
            report += "    OS: " + System.Environment.OSVersion + "\n";
            report += "    .NET Framework Version: " + System.Environment.Version  + "\n";
            report += "    Version of the API: 1.0.0\n";
            report += "    SDK Package Version: 1.0.0\n";
            return report;
        }
        public void AddApiKey(string key, string value)
        {
            ApiKey[key] = value;
        }
        public void AddApiKeyPrefix(string key, string value)
        {
            ApiKeyPrefix[key] = value;
        }
        #endregion Methods
    }
}
