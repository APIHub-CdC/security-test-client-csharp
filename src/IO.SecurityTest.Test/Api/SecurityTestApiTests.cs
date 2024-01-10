using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using RestSharp;
using NUnit.Framework;
using IO.SecurityTest.Client;
using IO.SecurityTest.Api;

namespace IO.SecurityTest.Test
{
    [TestFixture]
    public class SecurityTestApiTests
    {
        private string xApiKey;

        private SecurityTestApi apiTest;
        [SetUp]
        public void Init()
        {
            string basePath = "url_api";
            this.xApiKey = "your_api_key";
            this.apiTest = new SecurityTestApi(basePath);
        }

        
[Test]
public void test()
{
    string valor = "hola mundo";

    var response = this.apiTest.GetSecurityTest(this.xApiKey, valor);
    Console.WriteLine("response: " + response);
}

    }
}
