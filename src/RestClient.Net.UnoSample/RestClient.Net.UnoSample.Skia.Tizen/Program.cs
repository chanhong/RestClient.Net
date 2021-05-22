using Uno.UI.Runtime.Skia;

namespace RestClient.Net.UnoSample.Skia.Tizen
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = new TizenHost(() => new App(), args);
            host.Run();
        }
    }
}
