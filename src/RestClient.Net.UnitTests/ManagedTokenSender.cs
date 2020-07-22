using System;
using System.Linq;
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
        Action<string> _tokenSentAction;
        #endregion

        #region Public Properties
        public string BearerToken => _bearerToken;
        #endregion

        #region Constructor
        public ManagedTokenSender(Func<DateTime, string> refreshTokenFunc, Action<string> tokenSentAction, GetTime getTime)
        {
            _getTime = getTime;
            _refreshToken = refreshTokenFunc;
            _tokenSentAction = tokenSentAction;
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

                _bearerToken = newToken;

                var httpRequestMessage = httpRequestMessageFunc.Invoke();

                httpRequestMessage.Headers.Add("Authorization", "Bearer " + _bearerToken);

                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

                var authorizationHeaderValue = httpRequestMessage.Headers.ToList().FirstOrDefault(h => h.Key == "Authorization").Value;

                _tokenSentAction(authorizationHeaderValue.First());

                return response;
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
}

#endif