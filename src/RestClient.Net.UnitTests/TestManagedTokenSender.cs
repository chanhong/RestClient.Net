using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#if !NET45

namespace RestClient.Net.UnitTests
{
    [TestClass]
    public class TestManagedTokenSender
    {
        #region Fields
        MockHttpMessageHandler mockHttpMessageHandler;
        CreateHttpClient _createHttpClient;
        string uri = "http://www.test.com";
        DateTime FakeNow { get; set; } = new DateTime(2000, 1, 1);
        List<string> SentBearerTokens = new List<string>();

        #endregion

        [TestMethod]
        public async Task TestConcurrency()
        {
            string currentToken = null;

            //Create a managed token sender that refreshes the token when 60 minutes has lapsed
            var managedTokenSender = new ManagedTokenSender((c) =>
            {
                if (c <= FakeNow.AddMinutes(-60))
                {
                    //The token expired and is being refreshed
                    currentToken = Guid.NewGuid().ToString();
                }

                return currentToken;
            },
            (t) =>
            {
                //Track the tokens that are physically sent
                SentBearerTokens.Add(t);

                //Simulate some time being taken
                Thread.Sleep(10);

                //Increment fake time by 1 minute on each call
                FakeNow += new TimeSpan(0, 1, 0);
            },
            //This is a time abstraction
            () => FakeNow
            );

            var client = new Client(sendHttpRequestFunc: managedTokenSender.SendAsync, createHttpClient: _createHttpClient);

            //Initialize the token and put the first one in the SentBearerTokens list
            await client.GetAsync<string>(new Uri(uri));

            //Get the first token
            var firstToken = managedTokenSender.BearerToken;

            var tasks = new List<Task>();

            //Createa tasks
            for (var i = 0; i < 60; i++)
            {
                tasks.Add(client.GetAsync<string>(new Uri(uri)));
            }

            //Execute all 60 in parallel
            await Task.WhenAll(tasks);

            //Get all sent tokens that are equal to the first sent token
            var firstTokenList = SentBearerTokens.Where(t => t == $"Bearer {firstToken}").ToList();

            //60 should have been sent within the hour
            Assert.AreEqual(60, firstTokenList.Count);

            //1 should be sent afterwards representing the token refresh
            Assert.AreEqual(1, SentBearerTokens.Count - firstTokenList.Count);
        }

        [TestInitialize]
        public void Setup()
        {
            //Create the mock http message handler
            mockHttpMessageHandler = new MockHttpMessageHandler();
            mockHttpMessageHandler.
            When(HttpMethod.Get, uri)
            .Respond(
                new Dictionary<string, string>(),
                "application/json",
                File.ReadAllText("JSON/RestCountries.json"));

            _createHttpClient = (n) => mockHttpMessageHandler.ToHttpClient();
        }
    }
}

#endif