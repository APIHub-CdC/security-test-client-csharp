using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

using IO.SecurityTest.Interceptor;

namespace IO.SecurityTest.Client
{
    public partial class ApiClient
    {
        private IO.SecurityTest.Client.Configuration Configuration;
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };
        private bool InterceptRequest(IRestRequest request){
            bool isOk = false;
            string body = null;
            foreach(Parameter param in request.Parameters){
                if(param.Name == "application/json"){
                    body = (string)param.Value;
                }
            }
            Console.WriteLine("payload: "+body);
            string xSignature = Signer.GetPayloadSignature(body);
            Console.WriteLine("x-signature: "+xSignature);
            if(xSignature != null){
                isOk = true;
                request.AddHeader("x-signature", xSignature);
            }

            return isOk;
        }
        private void InterceptResponse(IRestRequest request, IRestResponse response){
            bool isVerify = Signer.GetResponseVerification(response);
            if(isVerify){
                Console.WriteLine("x-signature has been verified successfully");
            }
            else{
                Console.WriteLine("Could not verify the x-signature header");
            }
        }
        public ApiClient(Configuration config)
        {
            Configuration = config;
            RestClient = new RestClient(config.BasePath);
        }
        public RestClient RestClient { get; set; }
        private RestRequest PrepareRequest(
            String path, RestSharp.Method method, List<KeyValuePair<String, String>> queryParams, Object postBody,
            Dictionary<String, String> headerParams, Dictionary<String, String> formParams,
            Dictionary<String, FileParameter> fileParams, Dictionary<String, String> pathParams,
            String contentType)
        {
            var request = new RestRequest(path, method);
            foreach(var param in pathParams)
                request.AddParameter(param.Key, param.Value, ParameterType.UrlSegment);
            foreach(var param in headerParams)
                request.AddHeader(param.Key, param.Value);
            foreach(var param in queryParams)
                request.AddQueryParameter(param.Key, param.Value);
            foreach(var param in formParams)
                request.AddParameter(param.Key, param.Value);
            foreach(var param in fileParams)
            {
                request.AddFile(param.Value.Name, param.Value.Writer, param.Value.FileName, param.Value.ContentType);
            }
            if (postBody != null)
            {
                request.AddParameter(contentType, postBody, ParameterType.RequestBody);
            }
            return request;
        }
        public Object CallApi(
            String path, RestSharp.Method method, List<KeyValuePair<String, String>> queryParams, Object postBody,
            Dictionary<String, String> headerParams, Dictionary<String, String> formParams,
            Dictionary<String, FileParameter> fileParams, Dictionary<String, String> pathParams,
            String contentType)
        {
            var request = PrepareRequest(
                path, method, queryParams, postBody, headerParams, formParams, fileParams,
                pathParams, contentType);
            
            RestClient.Timeout = Configuration.Timeout;
            RestClient.UserAgent = Configuration.UserAgent;
            InterceptRequest(request);
            var response = RestClient.Execute(request);
            InterceptResponse(request, response);
            return (Object) response;
        }
        public string EscapeString(string str)
        {
            return UrlEncode(str);
        }
        public FileParameter ParameterToFile(string name, Stream stream)
        {
            if (stream is FileStream)
                return FileParameter.Create(name, ReadAsBytes(stream), Path.GetFileName(((FileStream)stream).Name));
            else
                return FileParameter.Create(name, ReadAsBytes(stream), "no_file_name_provided");
        }
        public string ParameterToString(object obj)
        {
            if (obj is DateTime)
                return ((DateTime)obj).ToString (Configuration.DateTimeFormat);
            else if (obj is DateTimeOffset)
                return ((DateTimeOffset)obj).ToString (Configuration.DateTimeFormat);
            else if (obj is IList)
            {
                var flattenedString = new StringBuilder();
                foreach (var param in (IList)obj)
                {
                    if (flattenedString.Length > 0)
                        flattenedString.Append(",");
                    flattenedString.Append(param);
                }
                return flattenedString.ToString();
            }
            else
                return Convert.ToString (obj);
        }
        public object Deserialize(IRestResponse response, Type type)
        {
            IList<Parameter> headers = response.Headers;
            if (type == typeof(byte[]))
            {
                return response.RawBytes;
            }
            if (type == typeof(Stream))
            {
                if (headers != null)
                {
                    var filePath = String.IsNullOrEmpty(Configuration.TempFolderPath)
                        ? Path.GetTempPath()
                        : Configuration.TempFolderPath;
                    var regex = new Regex(@"Content-Disposition=.*filename=['""]?([^'""\s]+)['""]?$");
                    foreach (var header in headers)
                    {
                        var match = regex.Match(header.ToString());
                        if (match.Success)
                        {
                            string fileName = filePath + SanitizeFilename(match.Groups[1].Value.Replace("\"", "").Replace("'", ""));
                            File.WriteAllBytes(fileName, response.RawBytes);
                            return new FileStream(fileName, FileMode.Open);
                        }
                    }
                }
                var stream = new MemoryStream(response.RawBytes);
                return stream;
            }
            if (type.Name.StartsWith("System.Nullable`1[[System.DateTime"))
            {
                return DateTime.Parse(response.Content,  null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
            if (type == typeof(String) || type.Name.StartsWith("System.Nullable"))
            {
                return ConvertType(response.Content, type);
            }
            try
            {
                return JsonConvert.DeserializeObject(response.Content, type, serializerSettings);
            }
            catch (Exception e)
            {
                throw new ApiException(500, e.Message);
            }
        }
        public String Serialize(object obj)
        {
            try
            {
                return obj != null ? JsonConvert.SerializeObject(obj) : null;
            }
            catch (Exception e)
            {
                throw new ApiException(500, e.Message);
            }
        }
        public bool IsJsonMime(String mime)
        {
            var jsonRegex = new Regex("(?i)^(application/json|[^;/ \t]+/[^;/ \t]+[+]json)[ \t]*(;.*)?$");
            return mime != null && (jsonRegex.IsMatch(mime) || mime.Equals("application/json-patch+json"));
        }
        public String SelectHeaderContentType(String[] contentTypes)
        {
            if (contentTypes.Length == 0)
                return "application/json";
            foreach (var contentType in contentTypes)
            {
                if (IsJsonMime(contentType.ToLower()))
                    return contentType;
            }
            return contentTypes[0];
        }
        public String SelectHeaderAccept(String[] accepts)
        {
            if (accepts.Length == 0)
                return null;
            if (accepts.Contains("application/json", StringComparer.OrdinalIgnoreCase))
                return "application/json";
            return String.Join(",", accepts);
        }
        public static string Base64Encode(string text)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        }
        public static dynamic ConvertType(dynamic fromObject, Type toObject)
        {
            return Convert.ChangeType(fromObject, toObject);
        }
        public static byte[] ReadAsBytes(Stream inputStream)
        {
            byte[] buf = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int count;
                while ((count = inputStream.Read(buf, 0, buf.Length)) > 0)
                {
                    ms.Write(buf, 0, count);
                }
                return ms.ToArray();
            }
        }
        public static string UrlEncode(string input)
        {
            const int maxLength = 32766;
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (input.Length <= maxLength)
            {
                return Uri.EscapeDataString(input);
            }
            StringBuilder sb = new StringBuilder(input.Length * 2);
            int index = 0;
            while (index < input.Length)
            {
                int length = Math.Min(input.Length - index, maxLength);
                string subString = input.Substring(index, length);
                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }
            return sb.ToString();
        }
        public static string SanitizeFilename(string filename)
        {
            Match match = Regex.Match(filename, @".*[/\\](.*)$");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return filename;
            }
        }
        public IEnumerable<KeyValuePair<string, string>> ParameterToKeyValuePairs(string collectionFormat, string name, object value)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            if (IsCollection(value) && collectionFormat == "multi")
            {
                var valueCollection = value as IEnumerable;
                parameters.AddRange(from object item in valueCollection select new KeyValuePair<string, string>(name, ParameterToString(item)));
            }
            else
            {
                parameters.Add(new KeyValuePair<string, string>(name, ParameterToString(value)));
            }
            return parameters;
        }
        private static bool IsCollection(object value)
        {
            return value is IList || value is ICollection;
        }
    }
}
