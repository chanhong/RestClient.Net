using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RestClient.Net.Abstractions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace RestClient.Net
{
    public class DefaultSendHttpRequestMessage : ISendHttpRequestMessage, IDisposable
    {
        //public static DefaultSendHttpRequestMessage Instance { get; } = new DefaultSendHttpRequestMessage();

        internal readonly Lazy<HttpClient> lazyHttpClient;
        /// <summary>
        /// Delegate used for getting or creating HttpClient instances when the SendAsync call is made
        /// </summary>
        internal readonly CreateHttpClient createHttpClient;

        public DefaultSendHttpRequestMessage(string name, CreateHttpClient? createHttpClient = null)

        {
            this.createHttpClient = createHttpClient ?? new CreateHttpClient((n) => new HttpClient());
            lazyHttpClient = new Lazy<HttpClient>(() => this.createHttpClient(name));

        }

        public void Dispose() => lazyHttpClient.Value?.Dispose();
        public async Task<HttpResponseMessage> SendHttpRequestMessage<TRequestBody>(
            IGetHttpRequestMessage httpRequestMessageFunc,
            IRequest<TRequestBody> request,
            ILogger logger,
            ISerializationAdapter serializationAdapter)
        {
            if (httpRequestMessageFunc == null) throw new ArgumentNullException(nameof(httpRequestMessageFunc));
            if (request == default) throw new ArgumentNullException(nameof(request));

            logger ??= NullLogger.Instance;

            try
            {
                var httpRequestMessage = httpRequestMessageFunc.GetHttpRequestMessage(request, logger, serializationAdapter);

                logger.LogTrace(Messages.InfoAttemptingToSend, request);

                var httpResponseMessage = await lazyHttpClient.Value.SendAsync(httpRequestMessage, request.CancellationToken).ConfigureAwait(false);

                logger.LogInformation(Messages.InfoSendReturnedNoException);

                return httpResponseMessage;
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, Messages.ErrorMessageOperationCancelled, request);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, Messages.ErrorOnSend, request);

                throw;
            }
        }
    }
}