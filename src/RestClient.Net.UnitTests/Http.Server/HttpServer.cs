using System;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1054 // URI-like parameters should not be strings
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA1063 // Implement IDisposable Correctly

//Thanks! https://gist.github.com/yetanotherchris/fb50071bced8bf0849ecd2cbbc3e9dce

namespace Http.Server
{
    public class HttpServer : IDisposable
    {
        internal readonly HttpListener listener = new();
        internal readonly Func<HttpListenerContext, Task> serve;
        internal readonly Task task;

        public HttpServer(string url, Func<HttpListenerContext, Task> serve)
        {
            this.serve = serve ?? throw new ArgumentNullException(nameof(serve));
            listener.Prefixes.Add(url);
            listener.Start();
            var context = listener.GetContext();
            task = serve(context);
        }

        public void Dispose()
        {
            listener.Stop();
            ((IDisposable)listener).Dispose();
            task.Dispose();
        }
    }
}


