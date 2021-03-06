﻿using System;
using System.Windows.Data;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Converters
{
    public class ConvertStatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string color = Preferences.NoteForeground;
            var note = (Repository.DataService.Note)value;
            if (!CollaborationManager.IsActive(note, Collaborations.CurrentCollaborator)) //if n is not actionable, it is not visible and color does not matter.
            {
                return color;
            }
            int? status = Collaborations.GetStatus(note);
            switch (status)
            {
                case (int)_Enum.Status.AuthorAccepted:
                    color = Preferences.NoteForeground;
                    break;
                case (int)_Enum.Status.ContributorAccepted:
                    color = Preferences.NoteForeground;
                    break;
                case (int)_Enum.Status.AuthorOriginal:
                    color = Preferences.NoteForeground;
                    break;
                case (int)_Enum.Status.AuthorRejectedDelete:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            color = Preferences.NoteForeground;
                            break;
                        case _Enum.EditContext.Contributing:
                            break;
                    }
                    break;
                case (int)_Enum.Status.ContributorRejectedDelete:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            break;
                        case _Enum.EditContext.Contributing:
                            color = Preferences.NoteForeground;
                            break;
                    }
                    break;
                case (int)_Enum.Status.WaitingOnContributor:
                    color = Preferences.NoteForeground;
                    break;
                case (int)_Enum.Status.WaitingOnAuthor:
                    color = Preferences.NoteForeground;
                    break;
                case (int)_Enum.Status.AuthorRejectedAdd:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            break;
                        case _Enum.EditContext.Contributing:
                            color = Preferences.NoteForeground;
                            break;
                    }
                    break;
                case (int)_Enum.Status.ContributorRejectedAdd:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            color = Preferences.NoteForeground;
                            break;
                        case _Enum.EditContext.Contributing:
                            break;
                    }
                    break;
                case (int)_Enum.Status.AuthorAdded:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            color = Preferences.NoteForeground;
                            break;
                        case _Enum.EditContext.Contributing:
                            color = Preferences.AddedColor;
                            break;
                    }
                    break;
                case (int)_Enum.Status.ContributorAdded:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            color = Preferences.AddedColor;
                            break;
                        case _Enum.EditContext.Contributing:
                            color = Preferences.NoteForeground;
                            break;
                    }
                    break;
                case (int)_Enum.Status.ContributorDeleted:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            color = (Collaborations.CurrentCollaborator == null) ? Preferences.NoteForeground : Preferences.DeletedColor;
                            break;
                        case _Enum.EditContext.Contributing:
                            color = Preferences.PurgedColor;
                            break;
                    }
                    break;
                case (int)_Enum.Status.AuthorDeleted:
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring:
                            color = Preferences.PurgedColor;
                            break;
                        case _Enum.EditContext.Contributing:
                            color = (Collaborations.CurrentCollaborator == null) ? Preferences.NoteForeground : Preferences.DeletedColor;
                            break;
                    }
                    break;
                case (int)_Enum.Status.Purged:
                    color = Preferences.PurgedColor;
                    break;
                case (int)_Enum.Status.Null:
                    color = Preferences.PurgedColor;
                    break;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
