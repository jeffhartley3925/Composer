namespace Composer.Repository
{
    public static class Host
    {
        private static string _value = string.Empty;
        public static string Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }
        private static string _application = string.Empty;
        public static string Application
        {
            get { return _application; }
            set
            {
                _application = value;
            }
        }
        private static string _compositionImageDirectory = string.Empty;
        public static string CompositionImageDirectory
        {
            get { return _compositionImageDirectory; }
            set
            {
                _compositionImageDirectory = value;
            }
        }
        private static string _compositionFileDirectory = string.Empty;
        public static string CompositionFileDirectory
        {
            get { return _compositionFileDirectory; }
            set
            {
                _compositionFileDirectory = value;
            }
        }
    }
}
