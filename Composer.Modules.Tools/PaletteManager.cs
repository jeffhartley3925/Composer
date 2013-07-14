using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure;
using Composer.Modules.Palettes.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Palettes
{
    public class PaletteManager
    {
        private IEventAggregator _ea;
        private List<PaletteButtonViewModel> _accidentalButtonViewModels;
        private PaletteButtonViewModel _dottedButtonViewModel;

        public PaletteManager()
        {
            DefineCommands();
            SubscribeEvents();
        }

        private void DefineCommands()
        {
        }

        private void SubscribeEvents()
        {
            if (_ea == null)
                _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _ea.GetEvent<ResumeEditing>().Subscribe(OnResumeEdit);
            _ea.GetEvent<SuspendEditing>().Subscribe(OnSuspendEdit);
        }

        public void OnResumeEdit(object obj)
        {
            UpdateState(true);
            TogglePaletteButtonEnableState(true);
        }

        public void OnSuspendEdit(object obj)
        {
            EditorState.IsComposing = false;
            TogglePaletteButtonEnableState(false);
        }

        private void TogglePaletteButtonEnableState(bool bState)
        {
            foreach (PaletteButtonViewModel paletteButtonViewModel in PaletteCache.PaletteButtonViewModels)
            {
                paletteButtonViewModel.Enabled = bState;
                paletteButtonViewModel.Background = (bState) ? Preferences.PaletteButtonBackground : Preferences.PaletteButtonBackgroundDisabled;
                paletteButtonViewModel.Foreground = (bState) ? Preferences.PaletteButtonForeground : Preferences.PaletteButtonForegroundDisabled;
                int blurRadius = (bState) ? 0 : EditorState.BlurRadius;
                _ea.GetEvent<BlurComposition>().Publish(blurRadius);
            }
        }

        public void UpdateState(object obj)
        { 
            bool bCompositionLoaded;
            if (bool.TryParse(obj.ToString(), out bCompositionLoaded) && EditorState.IsComposing == false)
            {
                EditorState.IsComposing = true;
                EditorState.PaletteId = Infrastructure.Constants.Palette.DurationPaletteId;
            }
            PaletteButtonViewModel buttonViewModel;

            foreach (PaletteButtonViewModel paletteButtonViewModel in PaletteCache.PaletteButtonViewModels)
            {
                paletteButtonViewModel.IsChecked = false;
                if (EditorState.IsComposing &&
                    (paletteButtonViewModel.Target.StartsWith("Tool") ||
                     paletteButtonViewModel.Target.StartsWith("Note") ||
                     paletteButtonViewModel.Target.StartsWith("Rest") ||
                     paletteButtonViewModel.Target == "Playback,Play"))
                {
                    paletteButtonViewModel.Enabled = true;
                }
            }

            switch (EditorState.PaletteId)
            {
                case Infrastructure.Constants.Palette.DurationPaletteId:
                    if (EditorState.IsNoteSelected() || EditorState.IsRest())
                    {
                        buttonViewModel = (from b in PaletteCache.PaletteButtonViewModels where b.Target == EditorState.DurationType + "," + EditorState.DurationCaption select b).SingleOrDefault();

                        if (buttonViewModel != null)
                            buttonViewModel.IsChecked = true;

                        SetAccidentalButtonsEnableState(EditorState.IsNoteSelected());

                        SetDottedButtonEnableState(true);

                        if (EditorState.AccidentalSelected())
                        {
                            buttonViewModel = (from b in PaletteCache.PaletteButtonViewModels where b.Caption == EditorState.Accidental select b).SingleOrDefault();

                            if (buttonViewModel != null)
                                buttonViewModel.IsChecked = true;
                        }
                    }
                    if (EditorState.DotSelected())
                    {
                        buttonViewModel = (from b in PaletteCache.PaletteButtonViewModels where b.Caption == "Dotted" select b).SingleOrDefault();

                        if (buttonViewModel != null)
                            buttonViewModel.IsChecked = true;
                    }
                    break;
                case Infrastructure.Constants.Palette.ToolPaletteId:

                    SetAccidentalButtonsEnableState(false);
                    SetDottedButtonEnableState(false);

                    buttonViewModel = (from b in PaletteCache.PaletteButtonViewModels where b.Caption == EditorState.Tool select b).SingleOrDefault();
                    if (buttonViewModel != null)
                        buttonViewModel.IsChecked = true;

                    break;
                case Infrastructure.Constants.Palette.PlaybackPaletteId:

                    buttonViewModel = (from b in PaletteCache.PaletteButtonViewModels where b.Caption == EditorState.PlaybackMode select b).SingleOrDefault();
                    if (buttonViewModel != null)
                        buttonViewModel.IsChecked = true;
                    break;
            }
        }

        private void SetDottedButtonEnableState(bool state)
        {
            if (_dottedButtonViewModel == null)
            {
                _dottedButtonViewModel = (from b in PaletteCache.PaletteButtonViewModels where b.Target.StartsWith("Dot,") select b).SingleOrDefault();
            }
            if (_dottedButtonViewModel != null)
            {
                _dottedButtonViewModel.Enabled = state;
            }
        }

        private void SetAccidentalButtonsEnableState(bool state)
        {
            if (_accidentalButtonViewModels == null)
            {
                _accidentalButtonViewModels = (from b in PaletteCache.PaletteButtonViewModels where b.Target.StartsWith("Accidental,") select b).ToList();
            }
            if (_accidentalButtonViewModels != null)
            {
                foreach (PaletteButtonViewModel paletteButtonViewModel in _accidentalButtonViewModels)
                {
                    paletteButtonViewModel.Enabled = state;
                }
            }
        }
    }
}