namespace AudioMeterEvent
{
    class HttpClient
    {
        public HttpClient(string payloadContentType, string username, string password, Logger logger)
        {
            Logger = logger;
            PayloadContentType = payloadContentType;

            var httpHandler = new System.Net.Http.HttpClientHandler();
            if (username != "" || password != "")
                httpHandler.Credentials = new System.Net.NetworkCredential(username, password);
            Client = new System.Net.Http.HttpClient(httpHandler);
        }

        readonly Logger Logger;
        readonly string PayloadContentType;
        readonly System.Net.Http.HttpClient Client;

        public void SendHttpRequest(string uri, string payload, Logger logger)
        {
            try
            {
                (payload == null ?
                    Client.GetAsync(uri) :
                    Client.PostAsync(uri, new System.Net.Http.StringContent(payload, null, PayloadContentType)))
                    .GetAwaiter().GetResult().EnsureSuccessStatusCode();
            }
            catch (System.Exception exception)
            {
                Logger.Log("HTTP request failed: " + exception + " (URI: " + uri + ")");
            }
        }
    }
}