using System;
using Composer.Infrastructure.Events;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using Composer.Infrastructure.Constants;

namespace Composer.Infrastructure.Support
{
    public static class FacebookData
    {
        private static IEventAggregator _ea;

        private static List<Friend> _friends = new List<Friend>();
        public static List<Friend> Friends
        {
            get { return _friends; }
            set
            {
                if (value != _friends)
                {
                    _friends = value;
                }
            }
        }

        private static List<string> _friendIds;
        public static List<string> FriendIds
        {
            get { return _friendIds; }
            set
            {
                if (value != _friendIds)
                {
                    _friendIds = value;
                }
            }
        }

        private static List<string> _friendNames;
        public static List<string> FriendNames
        {
            get { return _friendNames; }
            set
            {
                if (value != _friendNames)
                {
                    _friendNames = value;
                }
            }
        }

        private static List<string> _friendPictures;
        public static List<string> FriendPictures
        {
            get { return _friendPictures; }
            set
            {
                if (value != _friendPictures)
                {
                    _friendPictures = value;
                }
            }
        }

        static FacebookData()
        {

        }

        public static void Initialize()
        {
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            DefineCommands();
            SubscribeEvents();
        }

        private static void DefineCommands()
        {
        }

        private static void SubscribeEvents()
        {
            _ea.GetEvent<FacebookDataLoaded>().Subscribe(OnFacebookDataLoaded);
        }

        public static void OnFacebookDataLoaded(Tuple<string, string, string, string, string, string> payload)
        {
            EditorState.IsFacebookDataLoaded = true;

            FriendIds = new List<string>();
            foreach (string id in payload.Item4.Split(','))
            {
                FriendIds.Add(id);
            }

            FriendNames = new List<string>();
            foreach (string name in payload.Item5.Split(','))
            {
                FriendNames.Add(name);
            }

            FriendPictures = new List<string>();
            foreach (string url in payload.Item6.Split(','))
            {
                FriendPictures.Add(url);
            }
            Friend friend;
            for (var i = 0; i < FriendIds.Count; i++)
            {
                friend = new Friend(FriendIds[i], FriendNames[i], FriendPictures[i]);
                Friends.Add(friend);
            }
            friend = new Friend(Current.User.Id, Current.User.Name, Current.User.PictureUrl);
            Friends.Add(friend);
        }
    }

    public class Friend
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }

        public Friend(string userId, string userName, string userUrl)
        {
            UserId = userId;
            Username = userName;

            if (userUrl.ToLower().IndexOf(".gif") > 0)
                userUrl = Defaults.DefaultImageUrl;

            ImageUrl = userUrl;
        }

        public Friend(string userId, string userName)
        {
            UserId = userId;
            Username = userName;
            ImageUrl = Defaults.DefaultImageUrl;
        }
    }
}
