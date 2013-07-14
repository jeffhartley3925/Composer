
namespace Composer.Repository
{
    public static class DataServiceUri
    {
        private static string _domain = "www.wecontrib.com";
        private static string _directory = "composer";
        private static string _host = string.Empty;
        private static string _path = string.Empty;
        private static string _protocol = string.Empty;
        private static string _service = @"DataService.svc";

        public static string Uri = string.Empty;

        static DataServiceUri()
        {
            Initialize();
        }

        private static void Initialize()
        {
            _host = (Host.Value == _domain) ? _domain : Host.Value;
            _protocol = (Host.Value == _domain) ? "https" : "http";

            Uri = string.Format("{0}://{1}/{2}/{3}/", _protocol, _host, _directory, _service);
        }

        public static string GetUri()
        {
            if (string.IsNullOrEmpty(Uri))
            {
                Initialize();
            }
            return Uri;
        }
    }
}
