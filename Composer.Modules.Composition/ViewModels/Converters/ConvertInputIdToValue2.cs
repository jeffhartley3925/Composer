using System;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels;
using System.Collections.Generic;
using Composer.Infrastructure.Constants;
using Composer.Repository;

namespace Composer.Modules.Composition.Converters
{
    public class ConvertInputIdToValue2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var target = ((string)parameter).Trim();
			try
			{
				switch (target)
				{
					case "HubLyricsLinkCaptionFromVerseCount":
						System.Data.Services.Client.DataServiceCollection<Repository.DataService.Verse> verses = (System.Data.Services.Client.DataServiceCollection<Repository.DataService.Verse>)value;
						return (verses.Count == 0) ? string.Empty : string.Format("{0} - {1}", Preferences.Hub.LyricsCaption, verses.Count);
					case "ImageFromCompositionId":
						return Infrastructure.Support.Utilities.GetCompositionImageUriFromCompositionId(value.ToString());
					case "ImageFromAuthorId":
						string userImageUrl = Defaults.DefaultImageUrl;
						var id = value.ToString();
						var c = (from b in Composer.Infrastructure.Support.FacebookData.Friends where b.UserId == id select b.ImageUrl);
                        var e = c as List<string> ?? c.ToList();
                        if (e.Any())
                        {
                            userImageUrl = e.Single();
                            if (Host.Value == "localhost")
                            {
                                userImageUrl = userImageUrl.Replace("https", "http");
                            }
                        }
                        return userImageUrl; //TODO: should this be the generic DefaultPictureUrl?
                    case "NameFromAuthorId":
                        string name = "Unknown";
                        var authorId = value.ToString();
                        var d = (from g in Composer.Infrastructure.Support.FacebookData.Friends where g.UserId == authorId select g.Username);
                        var f = d as List<string> ?? d.ToList();
                        if (f.Any())
                        {
                            name = f.Single();
                        }
                        return name;
                    case "DispositionVisibility":
                        var visibility = Visibility.Collapsed;
                        var note = (Repository.DataService.Note)value;
                        var status = Collaborations.GetStatus(note);
                        if (Collaborations.CurrentCollaborator != null &&
                           (CollaborationManager.IsPendingAdd(status) || CollaborationManager.IsPendingDelete(status)))
                        {
                            visibility = Visibility.Visible;
                        }
                        return visibility;
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}