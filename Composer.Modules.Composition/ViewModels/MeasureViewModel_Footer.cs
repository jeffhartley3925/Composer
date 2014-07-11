using System;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed partial class MeasureViewModel
    {
        private DelegatedCommand<object> _clickFooterAcceptAllCommand;
        private DelegatedCommand<object> _clickFooterCompareCommand;
        private DelegatedCommand<object> _clickFooterDeleteCommand;
        private DelegatedCommand<object> _clickFooterPickCommand;

        private DelegatedCommand<object> _clickFooterRejectAllCommand;
        private DelegatedCommand<object> _clickFooterSelectAllCommand;
        private Visibility _collaborationFooterVisible = Visibility.Collapsed;
        private Visibility _editingFooterVisible = Visibility.Collapsed;
        private string _footerSelectAllText = "Select";
        private Visibility _footerSelectAllVisibility = Visibility.Collapsed;

        private void SubscribeFooterEvents()
        {
            EA.GetEvent<ShowMeasureFooter>().Subscribe(OnShowFooter);
            EA.GetEvent<HideMeasureFooter>().Subscribe(OnHideFooter);
            EA.GetEvent<ResetMeasureFooter>().Subscribe(OnResetMeasureFooter);
        }
        private void DefineFooterCommands()
        {
            ClickFooterAcceptAllCommand = new DelegatedCommand<object>(OnClickFooterAcceptAll);
            ClickFooterRejectAllCommand = new DelegatedCommand<object>(OnClickFooterRejectAll);
            ClickFooterCompareCommand = new DelegatedCommand<object>(OnClickFooterCompare);
            ClickFooterSelectAllCommand = new DelegatedCommand<object>(OnClickFooterSelectAll);
            ClickFooterDeleteCommand = new DelegatedCommand<object>(OnClickFooterDelete);
        }

        public DelegatedCommand<object> ClickFooterAcceptAllCommand
        {
            get { return _clickFooterAcceptAllCommand; }
            set
            {
                _clickFooterAcceptAllCommand = value;
                OnPropertyChanged(() => ClickFooterAcceptAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterRejectAllCommand
        {
            get { return _clickFooterRejectAllCommand; }
            set
            {
                _clickFooterRejectAllCommand = value;
                OnPropertyChanged(() => ClickFooterRejectAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterCompareCommand
        {
            get { return _clickFooterCompareCommand; }
            set
            {
                _clickFooterCompareCommand = value;
                OnPropertyChanged(() => ClickFooterCompareCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterPickCommand
        {
            get { return _clickFooterPickCommand; }
            set
            {
                _clickFooterPickCommand = value;
                OnPropertyChanged(() => ClickFooterPickCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterSelectAllCommand
        {
            get { return _clickFooterSelectAllCommand; }
            set
            {
                _clickFooterSelectAllCommand = value;
                OnPropertyChanged(() => ClickFooterSelectAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterDeleteCommand
        {
            get { return _clickFooterDeleteCommand; }
            set
            {
                _clickFooterDeleteCommand = value;
                OnPropertyChanged(() => ClickFooterDeleteCommand);
            }
        }

        public string FooterSelectAllText
        {
            get { return _footerSelectAllText; }
            set
            {
                _footerSelectAllText = value;
                OnPropertyChanged(() => FooterSelectAllText);
            }
        }

        public Visibility FooterSelectAllVisibility
        {
            get { return _footerSelectAllVisibility; }
            set
            {
                _footerSelectAllVisibility = value;
                OnPropertyChanged(() => FooterSelectAllVisibility);
            }
        }
        public void OnResetMeasureFooter(object obj)
        {
            FooterSelectAllText = "Select";
            FooterSelectAllVisibility = Visibility.Collapsed;
        }

        public void OnClickFooterSelectAll(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            if (FooterSelectAllVisibility == Visibility.Visible)
            {
                EA.GetEvent<DeSelectMeasure>().Publish(Measure.Id);
            }
            else
            {
                EA.GetEvent<SelectMeasure>().Publish(Measure.Id);
            }
        }

        public void OnClickFooterDelete(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            DeleteAll();
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
            HideFooters();
        }

        public void OnHideFooter(Guid id)
        {
            if (id == Measure.Id)
            {
                EditingFooterVisible = Visibility.Collapsed;
                PlaybackControlVisibility = Visibility.Collapsed;
            }
        }

        public void OnShowFooter(_Enum.MeasureFooter footer)
        {
            HideFooters();
            if (Measure.Chords.Count > 0)
            {
                switch (footer)
                {
                    case _Enum.MeasureFooter.Collaboration:
                        if (Measure.Chords.Count - ActiveChords.Count > 0)
                        {
                            CollaborationFooterVisible = Visibility.Visible;
                        }
                        break;
                    case _Enum.MeasureFooter.Editing:
                        var mStaff = Utils.GetStaff(Measure.Staff_Id);
                        if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
                            (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && mStaff.Index % 2 == 0))
                        {
                            EditingFooterVisible = Visibility.Visible;
                        }
                        break;
                }
            }
        }

        private void HideFooters()
        {
            EditingFooterVisible = Visibility.Collapsed;
            CollaborationFooterVisible = Visibility.Collapsed;
        }

        public void OnClickFooterAcceptAll(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            AcceptOrRejectAll(_Enum.Disposition.Accept);
        }

        public void OnClickFooterRejectAll(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            AcceptOrRejectAll(_Enum.Disposition.Reject);
        }

        public void OnClickFooterCompare(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
        }

        public Visibility CollaborationFooterVisible
        {
            get { return _collaborationFooterVisible; }
            set
            {
                _collaborationFooterVisible = value;
                OnPropertyChanged(() => CollaborationFooterVisible);
            }
        }

        public Visibility EditingFooterVisible
        {
            get { return _editingFooterVisible; }
            set
            {
                _editingFooterVisible = value;
                OnPropertyChanged(() => EditingFooterVisible);
            }
        }
    }
}
