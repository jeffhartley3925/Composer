public static class Current
{
    public static class User
    {
        private static string _id;
        public static string Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        private static int _index;
        public static int Index
        {
            get { return _index; }
            set
            {
                _index = value;
            }
        }

        private static string _name;
        public static string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        private static string _pictureUrl;
        public static string PictureUrl
        {
            get { return _pictureUrl; }
            set
            {
                _pictureUrl = value;
            }
        }

        private static int _collaboratorIndex;
        public static int CollaboratorIndex
        {
            get { return _collaboratorIndex; }
            set
            {
                _collaboratorIndex = value;
            }
        }
    }
}