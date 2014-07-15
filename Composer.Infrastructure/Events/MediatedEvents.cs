using Microsoft.Practices.Composite.Presentation.Events;
using System.Windows;
using System;
using System.Collections.Generic;

namespace Composer.Infrastructure.Events
{

    public sealed class SetThreshholdStarttime : CompositePresentationEvent<Tuple<Guid, double>> { }
    public sealed class ShiftChords : CompositePresentationEvent<Tuple<Guid, int, double, int, Guid>> { }
    public sealed class SetSequenceWidth : CompositePresentationEvent<Tuple<Guid, int, int>> { }
    public sealed class AdjustChords : CompositePresentationEvent<Tuple<Guid, bool, Guid>> { }
    public sealed class UpdateMeasurePackState : CompositePresentationEvent<Tuple<Guid, _Enum.EntityFilter>> {}
    public sealed class FlagMeasure : CompositePresentationEvent<Guid> { }
    public sealed class ResetNoteActivationState : CompositePresentationEvent<object> { }
    public sealed class DeactivateNotes : CompositePresentationEvent<object> { }
    public sealed class UpdateAllNotes : CompositePresentationEvent<object> { }
    public sealed class UpdateActiveChords : CompositePresentationEvent<Guid> { }
    public sealed class NotifyActiveChords : CompositePresentationEvent<Tuple<Guid, object, object, object>> { }
    public sealed class UpdateCollaborationNotifications : CompositePresentationEvent<object> { }
    public sealed class UpdateCollaborationStatisticss : CompositePresentationEvent<object> { }
    public sealed class HubCompositionMouseEnter : CompositePresentationEvent<string> { }
    public sealed class HubCompositionMouseLeave : CompositePresentationEvent<string> { }
    public sealed class HubCompositionMouseClick : CompositePresentationEvent<string> { }
    public sealed class HubCollaboratorMouseEnter : CompositePresentationEvent<string> { }
    public sealed class HubCollaboratorMouseLeave : CompositePresentationEvent<string> { }
    public sealed class HubCollaboratorMouseClick : CompositePresentationEvent<string> { }
    public sealed class BroadcastArcs : CompositePresentationEvent<object> { }
    public sealed class UpdateCompositionImage : CompositePresentationEvent<string> { }
    public sealed class HideMeasureEditHelpers : CompositePresentationEvent<object> { }
    public sealed class SetMeasureEndBar : CompositePresentationEvent<object> { }
    public sealed class DisplayMessage : CompositePresentationEvent<string> { }
    public sealed class AdjustBracketHeight : CompositePresentationEvent<object> { }
    public sealed class SetSocialChannels : CompositePresentationEvent<object> { }
    public sealed class SetPlaybackControlVisibility : CompositePresentationEvent<Guid> { }
    public sealed class SetRequestPrompt : CompositePresentationEvent<object> { }
    public sealed class PublishFacebookAction : CompositePresentationEvent<object> { }
    public sealed class ShowSocialChannels : CompositePresentationEvent<_Enum.SocialChannel> { }
    public sealed class HideSocialChannels : CompositePresentationEvent<_Enum.SocialChannel> { }
    public sealed class UpdatePinterestImage : CompositePresentationEvent<object> { }
    public sealed class DeselectAllBars : CompositePresentationEvent<object> { }
    public sealed class UpdateArc : CompositePresentationEvent<object> { }
    public sealed class UpdateSaveButtonHyperlink : CompositePresentationEvent<object> { }
    public sealed class ToggleHyperlinkVisibility : CompositePresentationEvent<Tuple<Visibility, _Enum.HyperlinkButton>> { }
    public sealed class CreateAndUploadImage : CompositePresentationEvent<object> { }
    public sealed class CreateAndUploadFile : CompositePresentationEvent<object> { }
    public sealed class SetCompositionWidthHeight : CompositePresentationEvent<object> { }
    public sealed class CheckFacebookDataLoaded : CompositePresentationEvent<object> { }
    public sealed class Backspace : CompositePresentationEvent<object> { }
    public sealed class UpdateCollaborationPanelSaveButtonEnableState : CompositePresentationEvent<bool> { }
    public sealed class Paste : CompositePresentationEvent<object> { }
    public sealed class KeyDown : CompositePresentationEvent<string> { }
    public sealed class KeyUp : CompositePresentationEvent<string> { }
    public sealed class SetMeasureBackground : CompositePresentationEvent<Guid> { } // if Guid == Guid.Empty is empty, then set the background of all measures, otherwise set background of specified measure
    public sealed class DeleteEntireChord : CompositePresentationEvent<Tuple<Guid, Guid>> { }
    public sealed class FacebookDataLoaded : CompositePresentationEvent<Tuple<string, string, string, string, string, string>> { }
    public sealed class UpdateStaffDimensionWidth : CompositePresentationEvent<short> { }
    public sealed class UpdateComposition : CompositePresentationEvent<Repository.DataService.Composition> { }
    public sealed class UpdateCompositionProvenance : CompositePresentationEvent<Tuple<string, string, string>> { }
    public sealed class UpdateScrollOffset : CompositePresentationEvent<Tuple<double, double>> { }
    public sealed class Save : CompositePresentationEvent<object> { }
    public sealed class SetAccidental : CompositePresentationEvent<Tuple<_Enum.Accidental, Repository.DataService.Note>> { }
    public sealed class CommitTransposition : CompositePresentationEvent<Tuple<Guid, object>> { }
    public sealed class BumpMeasureWidth : CompositePresentationEvent<Tuple<Guid, double, int>> { }
    public sealed class UpdateMeasureBar : CompositePresentationEvent<short> { }
    public sealed class UpdateMeasureBarX : CompositePresentationEvent<Tuple<Guid, double>> { }
    public sealed class UpdateMeasureBarColor : CompositePresentationEvent<Tuple<Guid, string>> { }
    public sealed class AnimateViewBorder : CompositePresentationEvent<string> { }
    public sealed class UpdateLyricsPanel : CompositePresentationEvent<object> { }
    public sealed class AppendRest : CompositePresentationEvent<double> { }
    public sealed class EditPopupItemClicked : CompositePresentationEvent<Tuple<string, string, _Enum.PasteCommandSource>> { }
    public sealed class UpdateEditPopupMenuItemsEnableState : CompositePresentationEvent<object> { }
    public sealed class UpdateEditPopupMenuTargetMeasure : CompositePresentationEvent<object> { }
    public sealed class BroadcastNewMeasureRequest : CompositePresentationEvent<object> { }
    public sealed class SetEditPopupMenu : CompositePresentationEvent<Tuple<Point, int, int, double, double, string, Guid>> { }
    public sealed class PopEditPopupMenu : CompositePresentationEvent<Guid> { }
    public sealed class ShowEditPopupMenu : CompositePresentationEvent<object> { }
    public sealed class HideEditPopup : CompositePresentationEvent<object> { }
    public sealed class ShowNoteEditor : CompositePresentationEvent<object> { }
    public sealed class HideNoteEditor : CompositePresentationEvent<object> { }
    public sealed class ShowDispositionButtons : CompositePresentationEvent<Guid> { }
    public sealed class HideDispositionButtons : CompositePresentationEvent<object> { }
    public sealed class ShowNewCompositionPanel : CompositePresentationEvent<object> { }
    public sealed class HideNewCompositionPanel : CompositePresentationEvent<object> { }
    public sealed class ShowLyricsPanel : CompositePresentationEvent<object> { }
    public sealed class HideLyricsPanel : CompositePresentationEvent<object> { }
    public sealed class ShowSavePanel : CompositePresentationEvent<object> { }
    public sealed class HideSavePanel : CompositePresentationEvent<object> { }
    public sealed class SelectChord : CompositePresentationEvent<Guid> { }
    public sealed class DeSelectChord : CompositePresentationEvent<Guid> { }
    public sealed class SelectNote : CompositePresentationEvent<Guid> { }
    public sealed class DeSelectNote : CompositePresentationEvent<Guid> { }
    public sealed class SelectMeasure : CompositePresentationEvent<Guid> { }
    public sealed class DeSelectMeasure : CompositePresentationEvent<Guid> { }
    public sealed class SelectStaff : CompositePresentationEvent<Guid> { }
    public sealed class SelectStaffgroup : CompositePresentationEvent<Guid> { }
    public sealed class SelectComposition : CompositePresentationEvent<object> { }
    public sealed class DeSelectComposition : CompositePresentationEvent<object> { }
    public sealed class SetProvenancePanel : CompositePresentationEvent<object> { }
    public sealed class SetPrint : CompositePresentationEvent<object> { }
    public sealed class ClosePrintPreview : CompositePresentationEvent<object> { }
    public sealed class FinishedPlayback : CompositePresentationEvent<object> { }
    public sealed class AcceptClick : CompositePresentationEvent<Guid> { }
    public sealed class RejectClick : CompositePresentationEvent<Guid> { }
    public sealed class ForwardComposition : CompositePresentationEvent<string> { }
    public sealed class UpdateChord : CompositePresentationEvent<Repository.DataService.Chord> { }
    public sealed class NotifyChord : CompositePresentationEvent<Guid> { }
    public sealed class SetChordLocationAndStarttime : CompositePresentationEvent<Tuple<Guid, Guid, double, bool, bool>> { }
    public sealed class MeasureLoaded : CompositePresentationEvent<Guid> { }
    public sealed class ResizeViewport : CompositePresentationEvent<Point> { }
    public sealed class BlurComposition : CompositePresentationEvent<int> { }
    public sealed class UpdateNoteDuration : CompositePresentationEvent<Tuple<Guid, decimal>> { }
    public sealed class PlayMeasure : CompositePresentationEvent<Repository.DataService.Measure> { }
    public sealed class PlayComposition : CompositePresentationEvent<_Enum.PlaybackInitiatedFrom> { }
    public sealed class PlayCompositionFromHub : CompositePresentationEvent<Guid> { }
    public sealed class ShowMeasureFooter : CompositePresentationEvent<_Enum.MeasureFooter> { }
    public sealed class HideMeasureFooter : CompositePresentationEvent<Guid> { }
    public sealed class Play : CompositePresentationEvent<object> { }
    public sealed class PlaceCompositionPanel : CompositePresentationEvent<Point> { }
    public sealed class PausePlay : CompositePresentationEvent<object> { }
    public sealed class StopPlay : CompositePresentationEvent<object> { }
    public sealed class UpdateProvenancePanel : CompositePresentationEvent<object> { }
    public sealed class SetProvenanceWidth : CompositePresentationEvent<double> { }
    public sealed class ArcSelected : CompositePresentationEvent<object> { }
    public sealed class DeleteArc : CompositePresentationEvent<Guid> { }
    public sealed class FlipArc : CompositePresentationEvent<Guid> { }
    public sealed class RenderArc : CompositePresentationEvent<Guid> { }
    public sealed class ArrangeArcs : CompositePresentationEvent<Repository.DataService.Measure> { }
    public sealed class ResetMeasureFooter : CompositePresentationEvent<object> { }
    public sealed class DeleteNote : CompositePresentationEvent<Repository.DataService.Note> { }
    public sealed class DeleteTrailingRests : CompositePresentationEvent<object> { }
    public sealed class DeleteChord : CompositePresentationEvent<Repository.DataService.Chord> { }
    public sealed class SpanMeasure : CompositePresentationEvent<Guid> { }
    public sealed class UpdateSpanManager : CompositePresentationEvent<object> { }
    public sealed class UpdateNote : CompositePresentationEvent<Repository.DataService.Note> { }
    public sealed class AddArc : CompositePresentationEvent<Repository.DataService.Arc> { }
    public sealed class ShowBusyIndicator : CompositePresentationEvent<object> { }
    public sealed class HideBusyIndicator : CompositePresentationEvent<object> { }
    public sealed class ScaleViewportChanged : CompositePresentationEvent<double> { }
    public sealed class ToggleUiScaleEnable : CompositePresentationEvent<bool> { }
    public sealed class TogglePaletteEnable : CompositePresentationEvent<bool> { }
    public sealed class SuspendEditing : CompositePresentationEvent<object> { }
    public sealed class ResumeEditing : CompositePresentationEvent<object> { }
    public sealed class CloneVerse : CompositePresentationEvent<object> { }
    public sealed class UpdateSubverses : CompositePresentationEvent<object> { }
    public sealed class ToggleVerseInclusion : CompositePresentationEvent<Tuple<string, int>> { }
    public sealed class DeleteVerse : CompositePresentationEvent<int> { }
    public sealed class AddVerse : CompositePresentationEvent<object> { }
    public sealed class ApplyVerse : CompositePresentationEvent<Tuple<object, int, int, Guid, int, int>> { }
    public sealed class ArrangeVerse : CompositePresentationEvent<Repository.DataService.Measure> { }
    public sealed class ReorderVerses : CompositePresentationEvent<Tuple<_Enum.Direction, int>> { }
    public sealed class ClearVerses : CompositePresentationEvent<object> { }
    public sealed class UpdateVerseIndexes : CompositePresentationEvent<int> { }
    public sealed class DeSelectAll : CompositePresentationEvent<object> { }
    public sealed class SendMeasureClickToStaff : CompositePresentationEvent<object> { }
    public sealed class StaffClicked : CompositePresentationEvent<object> { }
    public sealed class AreaSelect : CompositePresentationEvent<object> { }
    public sealed class SpanUpdate : CompositePresentationEvent<object> { }
    public sealed class UpdateCollaborators : CompositePresentationEvent<object> { }
    public sealed class ChordClicked : CompositePresentationEvent<object> { }
    public sealed class SynchronizeChord : CompositePresentationEvent<Repository.DataService.Chord> { }
    public sealed class ReverseNoteStem : CompositePresentationEvent<Repository.DataService.Note> { }
    public sealed class ReverseSelectedNotes : CompositePresentationEvent<object> { }
    public sealed class RemoveNotegroupFlag : CompositePresentationEvent<object> { }
    public sealed class FlagNotegroup : CompositePresentationEvent<object> { }
    public sealed class CompositionLoaded : CompositePresentationEvent<object> { }
    public sealed class ToggleSidebarVisibility : CompositePresentationEvent<Visibility> { }
    public sealed class HideCollaborationPanel : CompositePresentationEvent<object> { }
    public sealed class ShowCollaborationPanel : CompositePresentationEvent<object> { }
    public sealed class HideTransposePanel : CompositePresentationEvent<object> { }
    public sealed class ShowTransposePanel : CompositePresentationEvent<object> { }
    public sealed class ShowHub : CompositePresentationEvent<object> { }
    public sealed class HideHub : CompositePresentationEvent<object> { }
    public sealed class CreateNewComposition : CompositePresentationEvent<Tuple<string, List<string>, _Enum.StaffConfiguration, List<short>>> { }
    public sealed class NewComposition : CompositePresentationEvent<Repository.DataService.Composition> { }
    public sealed class LoadComposition : CompositePresentationEvent<object> { }
    public sealed class Login : CompositePresentationEvent<object> { }
    public sealed class ResizeSequence : CompositePresentationEvent<object> { }
    public sealed class RespaceMeasureGroup : CompositePresentationEvent<Tuple<Guid, int?>> { }
    public sealed class CloneCompositionEvent : CompositePresentationEvent<object> { }
    public sealed class HideProvenancePanel : CompositePresentationEvent<object> { }
    public sealed class SetCollaboratorIndex : CompositePresentationEvent<int> { }
    public sealed class ShowProvenancePanel : CompositePresentationEvent<object> { }
    public sealed class DurationPaletteClicked : CompositePresentationEvent<string> { }
    public sealed class EditorStateChanged : CompositePresentationEvent<object> { }
    public sealed class ToolPaletteClicked : CompositePresentationEvent<string> { }
    public sealed class RejectChange : CompositePresentationEvent<Guid> { }
    public sealed class AcceptChange : CompositePresentationEvent<Guid> { }
    public sealed class UpdateCollaboratorName : CompositePresentationEvent<string> { }
    public sealed class DisplayExceptionMessage : CompositePresentationEvent<string> { }
    public sealed class HideNewCompositionTitleValidator : CompositePresentationEvent<object> { }
    public sealed class ShowNewCompositionTitleValidator : CompositePresentationEvent<string> { }
    public sealed class SetNewCompositionTitleForeground : CompositePresentationEvent<string> { }
    public sealed class NewCompositionPanelStaffConfigurationChanged : CompositePresentationEvent<_Enum.StaffConfiguration> { }
}