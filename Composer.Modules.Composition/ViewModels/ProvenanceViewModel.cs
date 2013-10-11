using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Media.Imaging;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Data.Services.Client;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class ProvenanceViewModel : BaseViewModel, IProvenanceViewModel
    {
        private string _previousFontSize = string.Empty;
        private string _previousFontFamily = string.Empty;
        private string _previousTitle = string.Empty;

        private DateTime _createDate = DateTime.Now;
        public DateTime CreateDate
        {
            get { return _createDate; }
            set
            {
                _createDate = value;
                OnPropertyChanged(() => CreateDate);
            }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged(() => Width);
            }
        }

        private string _authorName = Current.User.Name;
        public string AuthorName
        {
            get { return _authorName; }
            set
            {
                _authorName = value;
                OnPropertyChanged(() => AuthorName);
            }
        }

        private string _titleLine = string.Empty;
        public string TitleLine
        {
            get { return _titleLine; }
            set
            {
                _titleLine = value;
                if (_previousTitle != string.Empty && _previousTitle != _titleLine)
                {
                   EditorState.Dirty = true;
					SaveButtonEnabled = true;
                }
                else
                {
                    EditorState.Dirty = false;
					SaveButtonEnabled = false;
                }
                if (_previousTitle == string.Empty && _titleLine != string.Empty)
                {
                    _previousTitle = _titleLine;
                }
                EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
                OnPropertyChanged(() => TitleLine);
            }
        }

        private ObservableCollection<Infrastructure.Support.FontFamily> _fontFamilies = new ObservableCollection<Infrastructure.Support.FontFamily>();
        public ObservableCollection<Infrastructure.Support.FontFamily> FontFamilies
        {
            get { return _fontFamilies; }
            set
            {
                _fontFamilies = value;
                OnPropertyChanged(() => FontFamilies);
            }
        }

        private ObservableCollection<Infrastructure.Support.FontSize> _fontSizes = new ObservableCollection<Infrastructure.Support.FontSize>();
        public ObservableCollection<Infrastructure.Support.FontSize> FontSizes
        {
            get { return _fontSizes; }
            set
            {
                _fontSizes = value;
                OnPropertyChanged(() => FontSizes);
            }
        }

        private string _selectedTitleFontSize = string.Empty;
        public string SelectedTitleFontSize
        {
            get { return _selectedTitleFontSize; }
            set
            {
                _selectedTitleFontSize = value;
                if (_previousFontSize != string.Empty && _previousFontSize != _selectedTitleFontSize)
                {
                    EditorState.Dirty = true;
					SaveButtonEnabled = true;
                }
                else
                {
                    EditorState.Dirty = false;
					SaveButtonEnabled = false;
                }
                if (_previousFontSize == string.Empty && _selectedTitleFontSize != string.Empty)
                {
                    _previousFontSize = _selectedTitleFontSize;
                }
                EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
                OnPropertyChanged(() => SelectedTitleFontSize);
            }
        }

        private string _selectedSmallFontSize = Preferences.ProvenanceSmallFontSize;
        public string SelectedSmallFontSize
        {
            get { return _selectedSmallFontSize; }
            set
            {
                _selectedSmallFontSize = value;
                OnPropertyChanged(() => SelectedSmallFontSize);
            }
        }

        private string _selectedFontFamily = string.Empty;
        public string SelectedFontFamily
        {
            get { return _selectedFontFamily; }
            set
            {
                _selectedFontFamily = value;
                if (_previousFontFamily != string.Empty && _previousFontFamily != _selectedFontFamily)
                {
                    EditorState.Dirty = true;
					SaveButtonEnabled = true;
                }
                else
                {
                    EditorState.Dirty = false;
					SaveButtonEnabled = false;
                }
                if (_previousFontFamily == string.Empty && _selectedFontFamily != string.Empty)
                {
                    _previousFontFamily = _selectedFontFamily;
                }
                EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(string.Empty);
                OnPropertyChanged(() => SelectedFontFamily);
            }
        }

        private BitmapImage _imageUrl;
        public BitmapImage ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                OnPropertyChanged(() => ImageUrl);
            }
        }

        private int _imageSize = Preferences.FacebookPictureSize;
        public int ImageSize
        {
            get { return _imageSize; }
            set
            {
                _imageSize = value;
                OnPropertyChanged(() => ImageSize);
            }
        }

        private string _cursor = "Hand";
        public string Cursor
        {
            get { return _cursor; }
            set
            {
                _cursor = value;
                OnPropertyChanged(() => Cursor);
            }
        }
        private bool _saveButtonEnabled;
        public bool SaveButtonEnabled
        {
            get { return _saveButtonEnabled; }
            set
            {
                _saveButtonEnabled = value;
                OnPropertyChanged(() => SaveButtonEnabled);
            }
        }
        private string _titleBorderColor = "#FFFFFF";
        public string TitleBorderColor
        {
            get { return _titleBorderColor; }
            set
            {
                _titleBorderColor = value;
                OnPropertyChanged(() => TitleBorderColor);
            }
        }

        private double _editControlsOpacity;
        public double EditControlsOpacity
        {
            get { return _editControlsOpacity; }
            set
            {
                _editControlsOpacity = value;
                OnPropertyChanged(() => EditControlsOpacity);
            }
        }

        private Visibility _contributorsHyperlinkVisibility = Visibility.Collapsed;
        public Visibility ContributorsHyperlinkVisibility
        {
            get { return _contributorsHyperlinkVisibility; }
            set
            {
                _contributorsHyperlinkVisibility = value;
                OnPropertyChanged(() => ContributorsHyperlinkVisibility);
            }
        }

        private Visibility _editButtonsVisibility = Visibility.Collapsed;
        public Visibility EditButtonsVisibility
        {
            get { return _editButtonsVisibility; }
            set
            {
                _editButtonsVisibility = value;
                OnPropertyChanged(() => EditButtonsVisibility);
            }
        }

        private Visibility _collaborationsVisibility = Visibility.Collapsed;
        public Visibility CollaborationsVisibility
        {
            get { return _collaborationsVisibility; }
            set
            {
                _collaborationsVisibility = value;
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
        }

        private void OnTitleFontSizeSelected(Infrastructure.Support.FontSize fontSize)
        {
            SelectedTitleFontSize = fontSize.Size;
            
        }

        private void OnFontFamilySelected(Infrastructure.Support.FontFamily fontFamily)
        {
            SelectedFontFamily = fontFamily.Name;
            CompositionManager.Composition.Provenance.FontFamily = SelectedFontFamily;
        }

        private DataServiceCollection<Repository.DataService.Collaboration> _collaborations;
        public DataServiceCollection<Repository.DataService.Collaboration> Collaborations
        {
            get { return _collaborations; }
            set
            {
                _collaborations = value;
                foreach (var c in _collaborations.Where(c => c.PictureUrl.IndexOf("https", StringComparison.Ordinal) == 0))
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
            EA.GetEvent<SetProvenanceWidth>().Subscribe(OnSetProvenanceWidth);
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

        public void OnSetProvenanceWidth(double width)
        {
            Width = width;
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

        private DelegatedCommand<object> _editClickedCommand;
        public DelegatedCommand<object> EditClickedCommand
        {
            get { return _editClickedCommand; }
            set
            {
                _editClickedCommand = value;
                OnPropertyChanged(() => EditClickedCommand);
            }
        }

        private DelegatedCommand<object> _contributorsClickedCommand;
        public DelegatedCommand<object> ContributorsClickedCommand
        {
            get { return _contributorsClickedCommand; }
            set
            {
                _contributorsClickedCommand = value;
                OnPropertyChanged(() => ContributorsClickedCommand);
            }
        }

        private DelegatedCommand<object> _closeClickedCommand;
        public DelegatedCommand<object> CloseClickedCommand
        {
            get { return _closeClickedCommand; }
            set
            {
                _closeClickedCommand = value;
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

            for (var i = 12; i <= 36; i++, i++)
            {
                FontSizes.Add(new Infrastructure.Support.FontSize(i.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}
