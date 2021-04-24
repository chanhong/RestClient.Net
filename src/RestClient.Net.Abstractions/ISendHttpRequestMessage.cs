using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestClient.Net.Abstractions
{
    public interface ISendHttpRequestMessage : IDisposable
    {
        Task<HttpResponseMessage> SendHttpRequestMessage<TRequestBody>(
            IGetHttpRequestMessage httpRequestMessageFunc,
            IRequest<TRequestBody> request,
            ILogger logger,
            ISerializationAdapter serializationAdapter);
    }
}