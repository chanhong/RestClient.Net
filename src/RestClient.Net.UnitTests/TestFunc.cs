
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#if !NET45

namespace RestClient.Net.UnitTests
{
    public delegate DateTime GetTime();

    public class ManagedTokenSender
    {
        #region Fields
        private string _bearerToken;
        private DateTime _tokenCreationTime;
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private Func<DateTime, string> _refreshToken;
        private GetTime _getTime;
        #endregion

        #region Constructor
        public ManagedTokenSender(Func<DateTime, string> refreshTokenFunc, GetTime getTime)
        {
            _getTime = getTime;
            _refreshToken = refreshTokenFunc;
        }
        #endregion

        #region Public Methods
        public async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, Func<HttpRequestMessage> httpRequestMessageFunc, Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                var newToken = _refreshToken(_tokenCreationTime);

                if (string.Compare(newToken, _bearerToken) != 0) _tokenCreationTime = _getTime();

                var httpRequestMessage = httpRequestMessageFunc.Invoke();

                httpRequestMessage.Headers.Add("Authorization", "Bearer " + _bearerToken);

                return await httpClient.SendAsync(httpRequestMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        #endregion
    }

    [TestClass]
    public class TestFunc
    {
        [TestMethod]
        public async Task Test()
        {
            var currentToken = new Guid().ToString();

            var fakeTime = new DateTime(2000, 0, 0);

            var managedTokenSender = new ManagedTokenSender((c) =>
            {
                if (c < DateTime.Now.AddMinutes(-60))
                {
                    currentToken = Guid.NewGuid().ToString();
                }

                return currentToken;
            }, () => fakeTime);


            var client = new Client(sendHttpRequestFunc: managedTokenSender.SendAsync);



        }
    }
}

#endif