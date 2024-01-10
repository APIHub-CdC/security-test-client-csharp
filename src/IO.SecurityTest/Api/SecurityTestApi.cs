using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using IO.SecurityTest.Client;

namespace IO.SecurityTest.Api
{
    public interface ISecurityTestApi : IApiAccessor
    {
        #region Synchronous Operations
        string GetSecurityTest (string xApiKey, string request);
        ApiResponse<string> GetSecurityTestWithHttpInfo (string xApiKey, string request);
        #endregion Synchronous Operations
    }
    public partial class SecurityTestApi : ISecurityTestApi
    {
        private IO.SecurityTest.Client.ExceptionFactory _exceptionFactory = (name, response) => null;
        public SecurityTestApi(String basePath)
        {
            this.Configuration = new IO.SecurityTest.Client.Configuration(basePath);
            ExceptionFactory = IO.SecurityTest.Client.Configuration.DefaultExceptionFactory;
        }
        public String GetBasePath()
        {
            return this.Configuration.ApiClient.RestClient.BaseUrl.ToString();
        }
        public IO.SecurityTest.Client.Configuration Configuration {get; set;}
        public IO.SecurityTest.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }
        [Obsolete("DefaultHeader is deprecated, please use Configuration.DefaultHeader instead.")]
        public IDictionary<String, String> DefaultHeader()
        {
            return new ReadOnlyDictionary<string, string>(this.Configuration.DefaultHeader);
        }
        [Obsolete("AddDefaultHeader is deprecated, please use Configuration.AddDefaultHeader instead.")]
        public void AddDefaultHeader(string key, string value)
        {
            this.Configuration.AddDefaultHeader(key, value);
        }
        public string GetSecurityTest (string xApiKey, string request)
        {
             ApiResponse<string> localVarResponse = GetSecurityTestWithHttpInfo(xApiKey, request);
             return localVarResponse.Data;
        }
        public ApiResponse< string > GetSecurityTestWithHttpInfo (string xApiKey, string request)
        {
            if (xApiKey == null)
                throw new ApiException(400, "Missing required parameter 'xApiKey' when calling ReporteDeCrditoPerApi->GetRC");
             if (request == null)
                throw new ApiException(400, "Missing required parameter 'request' when calling ReporteDeCrditoPerApi->GetRC");
         

            var localVarPath = "";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;
            String[] localVarHttpContentTypes = new String[] {
                "application/json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);
            if (xApiKey != null) localVarHeaderParams.Add("x-api-key", this.Configuration.ApiClient.ParameterToString(xApiKey));
            
            if (request != null && request.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(request);
            }
            else
            {
                localVarPostBody = request;
            }
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);
            int localVarStatusCode = (int) localVarResponse.StatusCode;
            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetSecurityTest", localVarResponse);
                if (exception != null) throw exception;
            }
            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }
    }
}
