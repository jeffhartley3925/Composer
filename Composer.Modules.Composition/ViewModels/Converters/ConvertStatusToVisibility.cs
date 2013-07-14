using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Composer.Modules.Composition.ViewModels;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Converters
{
    public class ConvertStatusToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Visible;
            var note = (Repository.DataService.Note)value;
            if (note != null)
            {
                if (! CollaborationManager.IsActionable(note, null))
                {
                    return Visibility.Collapsed;
                }
                var status = Collaborations.GetStatus(note);
                switch (status)
                {
                    case (int)_Enum.Status.AuthorAccepted:
                        break;
                    case (int)_Enum.Status.ContributorAccepted:
                        break;
                    case (int)_Enum.Status.AuthorOriginal:
                        break;
                    case (int)_Enum.Status.AuthorAdded:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                break;
                            case _Enum.EditContext.Contributing:
                                if (note.Foreground == Preferences.AddedColor)
                                {
                                    note.Foreground = Preferences.NoteForeground;
                                }
                                else
                                {
                                    visibility = Visibility.Collapsed;
                                }
                                break;
                        }
                        break;
                    case (int)_Enum.Status.PendingAuthorAction:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                visibility = Visibility.Collapsed;
                                break;
                            case _Enum.EditContext.Contributing:
                                visibility = Visibility.Visible;
                                break;
                        }
                        break;
                    case (int)_Enum.Status.ContributorAdded:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                break;
                            case _Enum.EditContext.Contributing:

                                break;
                        }
                        break;
                    case (int)_Enum.Status.ContributorDeleted:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                break;
                            case _Enum.EditContext.Contributing:
                                visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case (int)_Enum.Status.ContributorRejectedAdd:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:

                                break;
                            case _Enum.EditContext.Contributing:
                                visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case (int)_Enum.Status.AuthorRejectedAdd:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                visibility = Visibility.Collapsed;
                                break;
                            case _Enum.EditContext.Contributing:

                                break;
                        }
                        break;
                    case (int)_Enum.Status.AuthorDeleted:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                visibility = Visibility.Collapsed;
                                break;
                            case _Enum.EditContext.Contributing:
                                break;
                        }
                        break;
                    case (int)_Enum.Status.AuthorRejectedDelete:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                break;
                            case _Enum.EditContext.Contributing:
                                visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case (int)_Enum.Status.ContributorRejectedDelete:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                visibility = Visibility.Collapsed;
                                break;
                            case _Enum.EditContext.Contributing:
                                break;
                        }
                        break;
                    case (int)_Enum.Status.WaitingOnContributor:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                break;
                            case _Enum.EditContext.Contributing:
                                visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case (int)_Enum.Status.WaitingOnAuthor:
                        switch (EditorState.EditContext)
                        {
                            case _Enum.EditContext.Authoring:
                                visibility = Visibility.Collapsed;
                                break;
                            case _Enum.EditContext.Contributing:
                                break;
                        }
                        break;
                    case (int)_Enum.Status.Purged:
                        visibility = Visibility.Collapsed;
                        break;
                    case (int)_Enum.Status.NotApplicable:
                        visibility = Visibility.Collapsed;
                        break;
                }
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
