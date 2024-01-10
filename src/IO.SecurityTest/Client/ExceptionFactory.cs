using System;
using RestSharp;

namespace IO.SecurityTest.Client
{
    public delegate Exception ExceptionFactory(string methodName, IRestResponse response);
}
