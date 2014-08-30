using System;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed partial class MeasureViewModel
    {
        private DelegatedCommand<object> clickFooterAcceptAllCommand;
        private DelegatedCommand<object> clickFooterCompareCommand;
        private DelegatedCommand<object> clickFooterDeleteCommand;
        private DelegatedCommand<object> clickFooterPickCommand;

        private DelegatedCommand<object> clickFooterRejectAllCommand;
        private DelegatedCommand<object> clickFooterSelectAllCommand;
        private Visibility collaborationFooterVisible = Visibility.Collapsed;
        private Visibility editingFooterVisible = Visibility.Collapsed;
        private string footerSelectAllText = "Select";
        private Visibility footerSelectAllVisibility = Visibility.Collapsed;

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
            get { return this.clickFooterAcceptAllCommand; }
            set
            {
                this.clickFooterAcceptAllCommand = value;
                OnPropertyChanged(() => ClickFooterAcceptAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterRejectAllCommand
        {
            get { return this.clickFooterRejectAllCommand; }
            set
            {
                this.clickFooterRejectAllCommand = value;
                OnPropertyChanged(() => ClickFooterRejectAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterCompareCommand
        {
            get { return this.clickFooterCompareCommand; }
            set
            {
                this.clickFooterCompareCommand = value;
                OnPropertyChanged(() => ClickFooterCompareCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterPickCommand
        {
            get { return this.clickFooterPickCommand; }
            set
            {
                this.clickFooterPickCommand = value;
                OnPropertyChanged(() => ClickFooterPickCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterSelectAllCommand
        {
            get { return this.clickFooterSelectAllCommand; }
            set
            {
                this.clickFooterSelectAllCommand = value;
                OnPropertyChanged(() => ClickFooterSelectAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterDeleteCommand
        {
            get { return this.clickFooterDeleteCommand; }
            set
            {
                this.clickFooterDeleteCommand = value;
                OnPropertyChanged(() => ClickFooterDeleteCommand);
            }
        }

        public string FooterSelectAllText
        {
            get { return this.footerSelectAllText; }
            set
            {
                this.footerSelectAllText = value;
                OnPropertyChanged(() => FooterSelectAllText);
            }
        }

        public Visibility FooterSelectAllVisibility
        {
            get { return this.footerSelectAllVisibility; }
            set
            {
                this.footerSelectAllVisibility = value;
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
                        if (Measure.Chords.Count - this.ActiveChs.Count > 0)
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
            get { return this.collaborationFooterVisible; }
            set
            {
                this.collaborationFooterVisible = value;
                OnPropertyChanged(() => CollaborationFooterVisible);
            }
        }

        public Visibility EditingFooterVisible
        {
            get { return this.editingFooterVisible; }
            set
            {
                this.editingFooterVisible = value;
                OnPropertyChanged(() => EditingFooterVisible);
            }
        }
    }
}
