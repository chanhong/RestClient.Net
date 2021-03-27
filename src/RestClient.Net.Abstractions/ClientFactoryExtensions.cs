using System;

namespace RestClient.Net.Abstractions
{
    public static class ClientFactoryExtensions
    {
        public static IClient CreateClient(this CreateClient createClient, string name)
        {
            if (createClient == null) throw new ArgumentNullException(nameof(createClient));
            var client = createClient(name);
            return client;
        }
    }
}
