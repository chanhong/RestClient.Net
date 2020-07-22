using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#if !NET45

namespace RestClient.Net.UnitTests
{
    public static class TestValuesHolder
    {
        public static DateTime FakeTime { get; set; } = new DateTime(2000, 1, 1);
        public static List<string> SentBearerTokens = new List<string>();
    }

    public delegate DateTime GetTime();

    [TestClass]
    public class TestFunc
    {
        [TestMethod]
        public async Task Test()
        {
            var currentToken = new Guid().ToString();

            var managedTokenSender = new ManagedTokenSender((c) =>
            {
                if (c <= TestValuesHolder.FakeTime.AddMinutes(-60))
                {
                    //The token expired and is being refreshed
                    currentToken = Guid.NewGuid().ToString();
                }

                return currentToken;
            },
            (t) => 
            {
                TestValuesHolder.SentBearerTokens.Add(t);

                //Increment fake time by 1 minute
                TestValuesHolder.FakeTime += new TimeSpan(0, 1, 0);
            },
            () => TestValuesHolder.FakeTime
            );

            var mockHttpMessageHandler = new MockHttpMessageHandler();
            const string uri = "http://www.test.com";

            mockHttpMessageHandler.
            When(HttpMethod.Get, uri)
            .Respond(
                new Dictionary<string, string>(),
                "application/json",
                File.ReadAllText("JSON/RestCountries.json"));

            CreateHttpClient _createHttpClient = (n) => mockHttpMessageHandler.ToHttpClient();

            var client = new Client(sendHttpRequestFunc: managedTokenSender.SendAsync, createHttpClient: _createHttpClient);

            var tasks = new List<Task>();

            for (var i = 0; i < 61; i++)
            {
                tasks.Add(client.GetAsync<string>(new Uri(uri)));
            }

            await Task.WhenAll(tasks);
        }
    }
}

#endif