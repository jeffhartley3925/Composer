using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Media.Imaging;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Data.Services.Client;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class ProvenanceViewModel : BaseViewModel, IProvenanceViewModel, IEventCatcher
    {
        private string previousFontSize = string.Empty;
        private string previousFontFamily = string.Empty;
        private string previousTitle = string.Empty;

        private DateTime createDate = DateTime.Now;
        public DateTime CreateDate
        {
            get { return this.createDate; }
            set
            {
                this.createDate = value;
                DisplayDate = this.createDate.ToShortDateString();
            }
        }

        private string displayDate;
        public string DisplayDate
        {
            get { return this.displayDate; }
            set
            {
                this.displayDate = value;
                OnPropertyChanged(() => DisplayDate);
            }
        }

        private double width;
        public double Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                OnPropertyChanged(() => Width);
            }
        }

        private string authorName = Current.User.Name;
        public string AuthorName
        {
            get { return this.authorName; }
            set
            {
                this.authorName = value;
                OnPropertyChanged(() => AuthorName);
            }
        }

        private string titleLine = string.Empty;
        public string TitleLine
        {
            get { return this.titleLine; }
            set
            {
                this.titleLine = value;
                if (this.previousTitle != string.Empty && this.previousTitle != this.titleLine)
                {
                   EditorState.Dirty = true;
					SaveButtonEnabled = true;
                }
                else
                {
                    EditorState.Dirty = false;
					SaveButtonEnabled = false;
                }
                if (this.previousTitle == string.Empty && this.titleLine != string.Empty)
                {
                    this.previousTitle = this.titleLine;
                }
                EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
                OnPropertyChanged(() => TitleLine);
            }
        }

        private ObservableCollection<Infrastructure.Support.FontFamily> fontFamilies = new ObservableCollection<Infrastructure.Support.FontFamily>();
        public ObservableCollection<Infrastructure.Support.FontFamily> FontFamilies
        {
            get { return this.fontFamilies; }
            set
            {
                this.fontFamilies = value;
                OnPropertyChanged(() => FontFamilies);
            }
        }

        private ObservableCollection<Infrastructure.Support.FontSize> fontSizes = new ObservableCollection<Infrastructure.Support.FontSize>();
        public ObservableCollection<Infrastructure.Support.FontSize> FontSizes
        {
            get { return this.fontSizes; }
            set
            {
                this.fontSizes = value;
                OnPropertyChanged(() => FontSizes);
            }
        }

        private string selectedTitleFontSize = string.Empty;
        public string SelectedTitleFontSize
        {
            get { return this.selectedTitleFontSize; }
            set
            {
                this.selectedTitleFontSize = value;
                if (this.previousFontSize != string.Empty && this.previousFontSize != this.selectedTitleFontSize)
                {
                    EditorState.Dirty = true;
					SaveButtonEnabled = true;
                }
                else
                {
                    EditorState.Dirty = false;
					SaveButtonEnabled = false;
                }
                if (this.previousFontSize == string.Empty && this.selectedTitleFontSize != string.Empty)
                {
                    this.previousFontSize = this.selectedTitleFontSize;
                }
                EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
                OnPropertyChanged(() => SelectedTitleFontSize);
            }
        }

        private string selectedSmallFontSize = Preferences.ProvenanceSmallFontSize;
        public string SelectedSmallFontSize
        {
            get { return this.selectedSmallFontSize; }
            set
            {
                this.selectedSmallFontSize = value;
                OnPropertyChanged(() => SelectedSmallFontSize);
            }
        }

        private string selectedFontFamily = string.Empty;
        public string SelectedFontFamily
        {
            get { return this.selectedFontFamily; }
            set
            {
                this.selectedFontFamily = value;
                if (this.previousFontFamily != string.Empty && this.previousFontFamily != this.selectedFontFamily)
                {
                    EditorState.Dirty = true;
					SaveButtonEnabled = true;
                }
                else
                {
                    EditorState.Dirty = false;
					SaveButtonEnabled = false;
                }
                if (this.previousFontFamily == string.Empty && this.selectedFontFamily != string.Empty)
                {
                    this.previousFontFamily = this.selectedFontFamily;
                }
                EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
                OnPropertyChanged(() => SelectedFontFamily);
            }
        }

        private BitmapImage imageUrl;
        public BitmapImage ImageUrl
        {
            get { return this.imageUrl; }
            set
            {
                this.imageUrl = value;
                OnPropertyChanged(() => ImageUrl);
            }
        }

        private int imageSize = Preferences.FacebookPictureSize;
        public int ImageSize
        {
            get { return this.imageSize; }
            set
            {
                this.imageSize = value;
                OnPropertyChanged(() => ImageSize);
            }
        }

        private string cursor = "Hand";
        public string Cursor
        {
            get { return this.cursor; }
            set
            {
                this.cursor = value;
                OnPropertyChanged(() => Cursor);
            }
        }
        private bool saveButtonEnabled;
        public bool SaveButtonEnabled
        {
            get { return this.saveButtonEnabled; }
            set
            {
                this.saveButtonEnabled = value;
                OnPropertyChanged(() => SaveButtonEnabled);
            }
        }
        private string titleBorderColor = "#FFFFFF";
        public string TitleBorderColor
        {
            get { return this.titleBorderColor; }
            set
            {
                this.titleBorderColor = value;
                OnPropertyChanged(() => TitleBorderColor);
            }
        }

        private double editControlsOpacity;
        public double EditControlsOpacity
        {
            get { return this.editControlsOpacity; }
            set
            {
                this.editControlsOpacity = value;
                OnPropertyChanged(() => EditControlsOpacity);
            }
        }

        private Visibility contributorsHyperlinkVisibility = Visibility.Collapsed;
        public Visibility ContributorsHyperlinkVisibility
        {
            get { return this.contributorsHyperlinkVisibility; }
            set
            {
                this.contributorsHyperlinkVisibility = value;
                OnPropertyChanged(() => ContributorsHyperlinkVisibility);
            }
        }

        private Visibility editButtonsVisibility = Visibility.Collapsed;
        public Visibility EditButtonsVisibility
        {
            get { return this.editButtonsVisibility; }
            set
            {
                this.editButtonsVisibility = value;
                OnPropertyChanged(() => EditButtonsVisibility);
            }
        }

        private Visibility collaborationsVisibility = Visibility.Collapsed;
        public Visibility CollaborationsVisibility
        {
            get { return this.collaborationsVisibility; }
            set
            {
                this.collaborationsVisibility = value;
                OnPropertyChanged(() => CollaborationsVisibility);
            }
        }

        public ProvenanceViewModel()
        {
            DefineCommands();
            SubscribeEvents();
            PopulateFontData();
            OnCloseClicked("");
        }

        public void DefineCommands()
        {
            CloseClickedCommand = new DelegatedCommand<object>(OnCloseClicked);
            ContributorsClickedCommand = new DelegatedCommand<object>(OnContributorsClicked);
            EditClickedCommand = new DelegatedCommand<object>(OnEditClicked);
            FontFamilySelectedCommand = new DelegateCommand<Infrastructure.Support.FontFamily>(OnFontFamilySelected);
            TitleFontSizeSelectedCommand = new DelegateCommand<Infrastructure.Support.FontSize>(OnTitleFontSizeSelected);
            SmallFontSizeSelectedCommand = new DelegateCommand<Infrastructure.Support.FontSize>(OnSmallFontSizeSelected);
        }

        private void OnTitleFontSizeSelected(Infrastructure.Support.FontSize fontSize)
        {
            SelectedTitleFontSize = fontSize.Size;
            
        }
        private void OnSmallFontSizeSelected(Infrastructure.Support.FontSize fontSize)
        {
            SelectedSmallFontSize = fontSize.Size;

        }
        private void OnFontFamilySelected(Infrastructure.Support.FontFamily fontFamily)
        {
            SelectedFontFamily = fontFamily.Name;
            CompositionManager.Composition.Provenance.FontFamily = SelectedFontFamily;
        }

        private DataServiceCollection<Repository.DataService.Collaboration> collaborations;
        public DataServiceCollection<Repository.DataService.Collaboration> Collaborations
        {
            get { return this.collaborations; }
            set
            {
                this.collaborations = value;
                foreach (var c in this.collaborations.Where(c => c.PictureUrl.IndexOf("https", StringComparison.Ordinal) == 0))
                {
                    c.PictureUrl = c.PictureUrl.Replace("https", "http");
                }
                OnPropertyChanged(() => Collaborations);
            }
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<UpdateProvenancePanel>().Subscribe(OnUpdateProvenancePanel);
            EA.GetEvent<ShowSavePanel>().Subscribe(OnShowSavePanel);
            EA.GetEvent<SetCompositionWidth>().Subscribe(OnSetCompositionWidth);
            EA.GetEvent<ShowProvenancePanel>().Subscribe(OnShowProvenancePanel);
            EA.GetEvent<HideProvenancePanel>().Subscribe(OnHideProvenancePanel);
        }

        public void OnShowSavePanel(object obj)
        {
            Collaborations.Clear();
            CollaborationsVisibility = Visibility.Collapsed;
            EditButtonsVisibility = Visibility.Collapsed;
        }

        public void OnHideProvenancePanel(object obj)
        {
            TitleBorderColor = "#FFFFFF";
            EditControlsOpacity = 0;
            UpdateCompositionProvenance();
			SaveButtonEnabled = false;
            EditButtonsVisibility = Visibility.Collapsed;
            EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
            EA.GetEvent<ToggleHyperlinkVisibility>().Publish(new Tuple<Visibility, _Enum.HyperlinkButton>(Visibility.Visible, _Enum.HyperlinkButton.All));
        }

        public void OnShowProvenancePanel(object obj)
        {
            TitleBorderColor = "#FFFFFF";
            EditControlsOpacity = 0;
			SaveButtonEnabled = false;
            EditButtonsVisibility = Visibility.Visible;
            EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
            EA.GetEvent<ToggleHyperlinkVisibility>().Publish(new Tuple<Visibility, _Enum.HyperlinkButton>(Visibility.Collapsed, _Enum.HyperlinkButton.All));
        }

        public void OnSetCompositionWidth(Guid sFiD)
        {
	        try
            {
                var sF = Utils.GetStaff(sFiD);
                var wI = (from a in sF.Measures select double.Parse(a.Width)).Sum() + Defaults.StaffDimensionWidth +
                            Defaults.CompositionLeftMargin - 30;
                EditorState.GlobalStaffWidth = wI;
                Width = wI;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "OnSetCompositionWidth");
            }
        }

        public void OnUpdateProvenancePanel(object obj)
        {
            try
            {
                var c = (Repository.DataService.Composition)obj;
                TitleLine = c.Provenance.TitleLine;
                AuthorName = Current.User.Name;
                Collaborations = c.Collaborations;
                ContributorsHyperlinkVisibility = (Collaborations.Count > 1) ? Visibility.Visible : Visibility.Collapsed;
                if (!string.IsNullOrEmpty(Current.User.PictureUrl))
                {
                    ImageUrl = new BitmapImage(new Uri(Current.User.PictureUrl.Replace("https", "http"), UriKind.Absolute));
                }
                ImageSize = Preferences.FacebookPictureSize;
                CreateDate = c.Audit.CreateDate;
                SelectedFontFamily = c.Provenance.FontFamily;
                SelectedTitleFontSize = c.Provenance.LargeFontSize;
                SelectedSmallFontSize = c.Provenance.SmallFontSize;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "OnUpdateProvenancePanel");
            }
        }

        public void OnEditClicked(object obj)
        {
            if (Math.Abs(EditControlsOpacity - 1) < double.Epsilon)
            {
                TitleBorderColor = "#ffffff";
                EditControlsOpacity = 0;
                UpdateCompositionProvenance();
            }
            else
            {
                TitleBorderColor = "#3d599a";
                EditControlsOpacity = 1;
            }
        }

        public void OnContributorsClicked(object obj)
        {
            CollaborationsVisibility = (CollaborationsVisibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnCloseClicked(object obj)
        {
            EA.GetEvent<HideProvenancePanel>().Publish(false);
        }

        private DelegatedCommand<object> editClickedCommand;
        public DelegatedCommand<object> EditClickedCommand
        {
            get { return this.editClickedCommand; }
            set
            {
                this.editClickedCommand = value;
                OnPropertyChanged(() => EditClickedCommand);
            }
        }

        private DelegatedCommand<object> contributorsClickedCommand;
        public DelegatedCommand<object> ContributorsClickedCommand
        {
            get { return this.contributorsClickedCommand; }
            set
            {
                this.contributorsClickedCommand = value;
                OnPropertyChanged(() => ContributorsClickedCommand);
            }
        }

        private DelegatedCommand<object> closeClickedCommand;
        public DelegatedCommand<object> CloseClickedCommand
        {
            get { return this.closeClickedCommand; }
            set
            {
                this.closeClickedCommand = value;
                OnPropertyChanged(() => CloseClickedCommand);
            }
        }

        private void UpdateCompositionProvenance()
        {
            if (!EditorState.Dirty) return;

            var payload = new Tuple<string, string, string>(TitleLine, SelectedFontFamily, SelectedTitleFontSize);
            EA.GetEvent<UpdateCompositionProvenance>().Publish(payload);
        }

        public DelegateCommand<Infrastructure.Support.FontFamily> FontFamilySelectedCommand { get; private set; }
        public DelegateCommand<Infrastructure.Support.FontSize> TitleFontSizeSelectedCommand { get; private set; }
        public DelegateCommand<Infrastructure.Support.FontSize> SmallFontSizeSelectedCommand { get; private set; }

        private void PopulateFontData()
        {
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Arial"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Arial Black"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Georgia"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Comic Sans MS"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Courier New"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Lucida Grande"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Lucida Sans Unicode"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Times New Roman"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Trebuchet MS"));
            FontFamilies.Add(new Infrastructure.Support.FontFamily("Verdana"));

            for (var i = 8; i <= 36; i++, i++)
            {
                FontSizes.Add(new Infrastructure.Support.FontSize(i.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public bool IsTargetVM(Guid iD)
        {
            throw new NotImplementedException();
        }
    }
}
