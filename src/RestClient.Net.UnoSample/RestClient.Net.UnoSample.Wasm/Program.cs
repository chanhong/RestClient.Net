
#nullable enable

#pragma warning disable IDE0052 // Remove unread private members


namespace RestClient.Net.UnoSample.Wasm
{
    public class Program
    {
        private static App? _app;

        private static int Main()
        {
            Windows.UI.Xaml.Application.Start(_ => _app = new App());

            return 0;
        }
    }
}
