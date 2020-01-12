namespace AudioMeterEvent
{
    class HttpClient
    {
        public HttpClient(string username, string password, Logger logger)
        {
            Logger = logger;

            var httpHandler = new System.Net.Http.HttpClientHandler();
            if (username != "" || password != "")
                httpHandler.Credentials = new System.Net.NetworkCredential(username, password);
            Client = new System.Net.Http.HttpClient(httpHandler);
        }

        readonly Logger Logger;
        readonly System.Net.Http.HttpClient Client;

        public void SendHttpRequest(string uri, Logger logger)
        {
            try
            {
                Client.GetAsync(uri).GetAwaiter().GetResult().EnsureSuccessStatusCode();
            }
            catch (System.Exception exception)
            {
                Logger.Log("HTTP request failed: " + exception + " (URI: " + uri + ")");
            }
        }
    }
}