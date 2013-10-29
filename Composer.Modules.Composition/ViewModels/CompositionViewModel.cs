using System;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Dimensions;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.EventArgs;
using Composer.Modules.Composition.Service;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Reactive.Linq;
using Composer.Modules.Composition.Views;
using System.Windows.Input;
using System.Windows.Browser;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Composer.Messaging;
using System.Net;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class CompositionViewModel : BaseViewModel, ICompositionViewModel
    {
        private Uri _uri;
        private WebClient _client;

        public event EventHandler LoadComplete;
        public event EventHandler ErrorLoading;
        DataServiceRepository<Repository.DataService.Composition> _repository;
        public Grid CompositionGrid = null;

        private const int FooterHeight = 27;
        private const int VerticalScrollOffset = 34;
        private const int HorizontalScrollOffset = 412;

        public IEnumerable<Repository.DataService.Staffgroup> GetStaffgroups()
        {
            for (var i = 0; i < _composition.Staffgroups.Count(); ++i)
            {
                yield return _composition.Staffgroups[i];
            }
        }

        private Repository.DataService.Composition _composition;

        public Repository.DataService.Composition Composition
        {
            get { return _composition; }
            set
            {
                _composition = value;
                Staffgroups = new ObservableCollection<Repository.DataService.Staffgroup>();
                var observable = GetStaffgroups().ToObservable();
                observable.Subscribe(sg => Staffgroups.Add(sg));
                OnPropertyChanged(() => Composition);
            }
        }

        private int _timeSignatureId;
        public int TimeSignature_Id
        {
            get { return _timeSignatureId; }
            set
            {
                _timeSignatureId = value;

                var timeSignature = (from a in TimeSignatures.TimeSignatureList
                                     where a.Id == _timeSignatureId
                                     select a.Name).First();

                if (string.IsNullOrEmpty(timeSignature))
                {
                    timeSignature =
                        (from a in TimeSignatures.TimeSignatureList
                         where a.Id == Preferences.DefaultTimeSignatureId
                         select a.Name).First();
                }

                DurationManager.Bpm = Int32.Parse(timeSignature.Split(',')[0]);
                DurationManager.BeatUnit = Int32.Parse(timeSignature.Split(',')[1]);
                DurationManager.Initialize();
            }
        }

        private PrintPageItem SetPrintProvenance(PrintPageItem item)
        {
            item.Title = Composition.Provenance.TitleLine;
            item.SmallFontFamily = Composition.Provenance.FontFamily;
            item.TitleFontFamily = Composition.Provenance.FontFamily;
            item.TitleFontSize = Composition.Provenance.LargeFontSize;
            item.SmallFontSize = Composition.Provenance.SmallFontSize;
            item.Date = Composition.Audit.ModifyDate.ToLongDateString();

            var authors = new List<Author>();
            foreach (var c in Composition.Collaborations)
            {
                if (c.PictureUrl.IndexOf("https", StringComparison.Ordinal) == 0)
                {
                    c.PictureUrl = c.PictureUrl.Replace("https", "http");
                }
                var author = new Author { Name = c.Name, PictureUrl = c.PictureUrl, PercentContribution = 0 };
                authors.Add(author);
            }
            item.Authors = authors;
            return item;
        }

        private void CompilePrintPages()
        {
            int staffGroupIndex;

            var page = new PrintPage { PrintPageItems = new ObservableCollection<PrintPageItem>() };

            var p = new PrintPageItem();
            p = SetPrintProvenance(p);
            p.Staffgroup = null;
            page.PrintPageItems.Add(p);

            for (staffGroupIndex = 0; staffGroupIndex <= Staffgroups.Count() - 1; staffGroupIndex++)
            {
                if (staffGroupIndex % Preferences.PrintItemsPerPage == 0 && staffGroupIndex != 0)
                {
                    PrintPages.Add(page);
                    page = new PrintPage { PrintPageItems = new ObservableCollection<PrintPageItem>() };
                }

                var item = new PrintPageItem { Staffgroup = Staffgroups[staffGroupIndex] };
                page.PrintPageItems.Add(item);
            }
            if (staffGroupIndex % Preferences.PrintItemsPerPage > 0)
            {
                PrintPages.Add(page);
            }
        }

        private ObservableCollection<PrintPage> _printPages = new ObservableCollection<PrintPage>();
        public ObservableCollection<PrintPage> PrintPages
        {
            get { return _printPages; }
            set
            {
                _printPages = value;
                OnPropertyChanged(() => PrintPages);
            }
        }

        private ObservableCollection<Repository.DataService.Staffgroup> _staffgroups;
        public ObservableCollection<Repository.DataService.Staffgroup> Staffgroups
        {
            get { return _staffgroups; }
            set
            {
                _staffgroups = value;
                OnPropertyChanged(() => Staffgroups);
            }
        }

        private Repository.DataService.Staffgroup _selectedStaffgroup;
        public Repository.DataService.Staffgroup SelectedStaffgroup
        {
            get { return _selectedStaffgroup; }
            set
            {
                _selectedStaffgroup = value;
                OnPropertyChanged(() => SelectedStaffgroup);
            }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(() => Id);
            }
        }

        private double _scrollWidth;
        public double ScrollWidth
        {
            get { return _scrollWidth; }
            set
            {
                _scrollWidth = value;
                OnPropertyChanged(() => ScrollWidth);
            }
        }

        private ScrollBarVisibility _scrollVisibility;
        public ScrollBarVisibility ScrollVisibility
        {
            get { return _scrollVisibility; }
            set
            {
                _scrollVisibility = value;
                OnPropertyChanged(() => ScrollVisibility);
            }
        }

        public void OnHideProvenancePanel(object obj)
        {
            EditorState.Provenancing = false;
            ProvenanceVisibility = Visibility.Collapsed;
            //EA.GetEvent<AdjustArcTops>().Publish(_Enum.Direction.Up);
        }

        public void OnShowProvenancePanel(object obj)
        {
            EditorState.Provenancing = true;
            ProvenanceVisibility = Visibility.Visible;
            //EA.GetEvent<AdjustArcTops>().Publish(_Enum.Direction.Down);
        }

        private double _scrollHeight;
        public double ScrollHeight
        {
            get { return _scrollHeight; }
            set
            {
                _scrollHeight = value - ((UploadDetailsVisibility == Visibility.Visible) ? FooterHeight : 0);
                OnPropertyChanged(() => ScrollHeight);
            }
        }

        private string _background = Preferences.CompositionBackground;
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private string _scrollBackground = Preferences.CompositionScrollBackground;
        public string ScrollBackground
        {
            get { return _scrollBackground; }
            set
            {
                _scrollBackground = value;
                OnPropertyChanged(() => ScrollBackground);
            }
        }

        private int _blurRadius;
        public int BlurRadius
        {
            get { return _blurRadius; }
            set
            {
                _blurRadius = value;
                OnPropertyChanged(() => BlurRadius);
            }
        }

        private double _provenanceX;
        public double Provenance_X
        {
            get { return _provenanceX; }
            set
            {
                _provenanceX = value;
                OnPropertyChanged(() => Provenance_X);
            }
        }

        private double _provenanceY;
        public double Provenance_Y
        {
            get { return _provenanceY; }
            set
            {
                _provenanceY = value;
                OnPropertyChanged(() => Provenance_Y);
            }
        }

        private Visibility _provenanceVisibility;
        public Visibility ProvenanceVisibility
        {
            get { return _provenanceVisibility; }
            set
            {
                _provenanceVisibility = value;
                OnPropertyChanged(() => ProvenanceVisibility);
            }
        }

        private Visibility _uploadDetailsVisibility = Visibility.Collapsed;
        public Visibility UploadDetailsVisibility
        {
            get {  return _uploadDetailsVisibility; }
            set
            {
                _uploadDetailsVisibility = value;
                OnPropertyChanged(() => UploadDetailsVisibility);
            }
        }

        public CompositionViewModel(ICompositionService service)
        {
            // TODO: I thought this was done in ShellViewModel. Investigate
            Provenance_X = Palette.TruePaletteWidth;
            Provenance_Y = Defaults.MeasureHeight;
            ProvenanceVisibility = Visibility.Collapsed;
            UploadDetailsVisibility = Visibility.Collapsed;

            Hide();
            MeasureManager.Initialize();
            var service1 = service;
            if (service1.Composition == null || service1.Composition.Staffgroups.Count == 0)
            {
                service.CompositionLoadingComplete += CompositionLoadingComplete;
                service.CompositionLoadingError += CompositionLoadingError;
                service.GetCompositionAsync();
            }
            else
            {
                LoadComposition(service.Composition);
            }

            SubscribeEvents();
            DefineCommands();
            ScaleX = 1;
            ScaleY = 1;
            ScrollWidth = EditorState.ViewportWidth - HorizontalScrollOffset;
            ScrollHeight = EditorState.ViewportHeight - VerticalScrollOffset;

            ScrollVisibility = ScrollBarVisibility.Auto; 
        }

        private void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
        }
        private static void UpdateMeasureBarPackState()
        {
            EditorState.IsCalculatingStatistics = true;
            foreach (var m in Cache.Measures)
            {
               //Statistics.Add(m);
            }
            EditorState.IsCalculatingStatistics = false;
        }

        private void LoadComposition(Repository.DataService.Composition composition)
        {
            TimeSignature_Id = composition.TimeSignature_Id;
            composition = CompositionManager.FlattenComposition(composition);
            CompositionManager.Composition = composition;
            EditorState.IsReturningContributor = CollaborationManager.IsReturningContributor(composition);
            EditorState.IsAuthor = composition.Audit.Author_Id == Current.User.Id;

            if (EditorState.EditContext == _Enum.EditContext.Contributing && !EditorState.IsReturningContributor)
            {
                //EditorState.IsReturningContributor gets set in Collaborations.Initialize().
                SetRepository();
                Collaborations.Index = composition.Collaborations.Count();
                var collaborator = CollaborationManager.Create(composition, Collaborations.Index);
                _repository.Context.AddLink(composition, "Collaborations", collaborator);
                composition.Collaborations.Add(collaborator);
            }
            else
            {
                var a = (from b in composition.Collaborations where b.Collaborator_Id == Current.User.Id select b.Index);
                var e = a as List<int> ?? a.ToList();
                if (e.Any())
                {
                    Collaborations.Index = e.First();
                }
            }
            CompositionManager.Composition = composition;
            EditorState.IsCollaboration = composition.Collaborations.Count > 1;
            CollaborationManager.Initialize(); // TODO: do we need to initialize CollabrationManager when there are no collaborations?
            UpdateMeasureBarPackState();
            Composition = composition;
            Verses = composition.Verses;
            CompilePrintPages();
            SetProvenanceWidth();
        }

        private void SetProvenanceWidth()
        {
            var s = Utils.GetStaff(Composition.Staffgroups[0].Staffs[0].Id);
            var w = (from a in s.Measures select double.Parse(a.Width)).Sum() + Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin - 70;
            EA.GetEvent<SetProvenanceWidth>().Publish(w);
        }
        private void CompositionLoadingError(object sender, CompositionErrorEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (ErrorLoading != null) ErrorLoading(this, null);
            });
        }

        private void CompositionLoadingComplete(object sender, CompositionLoadingEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var composition in e.Results)
                {
                    LoadComposition(composition);
                    break;
                }
                if (LoadComplete != null) LoadComplete(this, null);
                EA.GetEvent<CompositionLoaded>().Publish(new object());
            });
        }

        public DataServiceCollection<Repository.DataService.Verse> Verses { get; set; }

        public void OnReverseSelectedNotes(object obj)
        {
            var processedStartimes = new List<double>();
            var processedDurations = new List<decimal>();

            foreach (var note in Infrastructure.Support.Selection.Notes)
            {
                var chord = (from a in Cache.Chords where a.Id == note.Chord_Id select a).SingleOrDefault();
                var notegroup = NotegroupManager.ParseChord(chord, note);

                if (notegroup == null) continue;
                if (notegroup.IsRest) continue;

                if (processedStartimes.Contains(notegroup.StartTime) && processedDurations.Contains(notegroup.Duration))
                    continue;

                processedStartimes.Add(notegroup.StartTime);
                processedDurations.Add(notegroup.Duration);
                foreach (var n in notegroup.Notes)
                {
                    EA.GetEvent<ReverseNoteStem>().Publish(n);
                }
                notegroup.Reverse();
                EA.GetEvent<FlagNotegroup>().Publish(notegroup);
            }
            foreach (var measure in Infrastructure.Support.Selection.ImpactedMeasures)
            {
                EA.GetEvent<UpdateSpanManager>().Publish(measure.Id);
                EA.GetEvent<SpanMeasure>().Publish(measure);
            }
        }

        public void OnDeselectAll(object obj)
        {
            EA.GetEvent<DeSelectComposition>().Publish(string.Empty);
            EA.GetEvent<HideNoteEditor>().Publish(string.Empty);
            EA.GetEvent<ResetMeasureFooter>().Publish(string.Empty);
            Infrastructure.Support.Selection.RemoveAll();
            Infrastructure.Support.Selection.Clear();
        }

        public void OnScaleViewportChanged(double newScale)
        {
            ScaleX = newScale;
            ScaleY = newScale;
            ScrollVisibility = (newScale < .72) ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
        }

        public void OnArrangeArcs(Repository.DataService.Measure measure)
        {
            foreach (Repository.DataService.Arc arc in Composition.Arcs)
            {
                var chId1 = arc.Chord_Id1;
                var chId2 = arc.Chord_Id2;
                var ch1 = (from a in Cache.Chords where a.Id == chId1 select a).Single();
                var ch2 = (from a in Cache.Chords where a.Id == chId2 select a).Single();
                if (ch1.Measure_Id == measure.Id || ch2.Measure_Id == measure.Id)
                {
                    EA.GetEvent<RenderArc>().Publish(arc.Id);
                }
            }
        }

        public void OnAreaSelect(object obj)
        {
            var p = (Point)obj;
            switch (EditorState.ClickState)
            {
                case _Enum.ClickState.None:
                    Rectangle_X1 = p.X;
                    Rectangle_Y1 = p.Y;
                    EditorState.ClickState = _Enum.ClickState.First;
                    EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                    break;
                case _Enum.ClickState.First:
                    Rectangle_X2 = p.X;
                    Rectangle_Y2 = p.Y;
                    SelectorMargin = GetSelectorMargin();
                    SelectorHeight = GetSelectorHeight();
                    SelectorWidth = GetSelectorWidth();
                    SelectorVisible = Visibility.Visible;
                    if (EditorState.ClickMode == "Click")
                    {
                        EditorState.ClickState = _Enum.ClickState.Second;
                        SelectAllSelectedNotes();
                        SelectorVisible = Visibility.Collapsed;
                        Rectangle_X1 = 0;
                        Rectangle_Y1 = 0;
                        Rectangle_X2 = 0;
                        Rectangle_Y2 = 0;
                        EditorState.ClickState = _Enum.ClickState.None;
                    }
                    break;
                default:
                    EditorState.ClickState = _Enum.ClickState.None;
                    SelectorVisible = Visibility.Collapsed;
                    EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                    break;
            }
        }

        private void SelectAllSelectedNotes()
        {
            foreach (var staffgroup in CompositionManager.Composition.Staffgroups)
            {
                foreach (var staff in staffgroup.Staffs)
                {
                    foreach (var measure in staff.Measures)
                    {
                        foreach (var chord in measure.Chords)
                        {
                            if (CollaborationManager.IsActive(chord))
                            {
                                double x = chord.Location_X + 8;
                                foreach (var note in chord.Notes.Where(CollaborationManager.IsActive))
                                {
                                    double y = note.Location_Y + 20;
                                    var p = Infrastructure.Support.Utilities.CoordinateSystem.TranslateToCompositionCoords(x, y, measure.Sequence, measure.Index, measure.Index * 4, 4, measure.Width, measure.Staff_Id);
                                    if (p.X > Rectangle_X1 && p.X < Rectangle_X2)
                                    {
                                        if (p.Y > Rectangle_Y1 && p.Y < Rectangle_Y2)
                                        {
                                            EA.GetEvent<SelectNote>().Publish(note.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetSelectorMargin()
        {
            return string.Format("{0},{1},{2},{3}", Math.Min(Rectangle_X1, Rectangle_X2), Math.Min(Rectangle_Y1, Rectangle_Y2), 0, 0);
        }

        private double GetSelectorHeight()
        {
            return Math.Max(0, Math.Abs(Rectangle_Y1 - Rectangle_Y2) - 7);
        }

        private double GetSelectorWidth()
        {
            return Math.Max(0, Math.Abs(Rectangle_X1 - Rectangle_X2) - 7);
        }

        private double _selectorHeight;
        public double SelectorHeight
        {
            get { return _selectorHeight; }
            set
            {
                _selectorHeight = value;
                OnPropertyChanged(() => SelectorHeight);
            }
        }

        private double _selectorWidth;
        public double SelectorWidth
        {
            get { return _selectorWidth; }
            set
            {
                _selectorWidth = value;
                OnPropertyChanged(() => SelectorWidth);
            }
        }

        private double _width = double.NaN;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged(() => Width);
            }
        }

        private double _height = double.NaN;
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged(() => Height);
            }
        }

        private string _selectorMargin;
        public string SelectorMargin
        {
            get { return _selectorMargin; }
            set
            {
                _selectorMargin = value;
                OnPropertyChanged(() => SelectorMargin);
            }
        }

        private double _rectangleX1;
        public double Rectangle_X1
        {
            get { return _rectangleX1; }
            set
            {
                _rectangleX1 = value;
                OnPropertyChanged(() => Rectangle_X1);
            }
        }

        private double _rectangleY1;
        public double Rectangle_Y1
        {
            get { return _rectangleY1; }
            set
            {
                _rectangleY1 = value;
                OnPropertyChanged(() => Rectangle_Y1);
            }
        }

        private double _rectangleX2;
        public double Rectangle_X2
        {
            get { return _rectangleX2; }
            set
            {
                _rectangleX2 = value;
                OnPropertyChanged(() => Rectangle_X2);
            }
        }

        private double _rectangleY2;
        public double Rectangle_Y2
        {
            get { return _rectangleY2; }
            set
            {
                _rectangleY2 = value;
                OnPropertyChanged(() => Rectangle_Y2);
            }
        }

        private string _scrollOffsets;
        public string ScrollOffsets
        {
            get { return _scrollOffsets; }
            set
            {
                _scrollOffsets = value;
                OnPropertyChanged(() => ScrollOffsets);
            }
        }

        private string _rawSize;
        public string RawSize
        {
            get { return _rawSize; }
            set
            {
                _rawSize = value;
                OnPropertyChanged(() => RawSize);
            }
        }

        private string _compressedSize;
        public string CompressedSize
        {
            get { return _compressedSize; }
            set
            {
                _compressedSize = value;
                OnPropertyChanged(() => CompressedSize);
            }
        }

        private string _uploadResponse;
        public string UploadResponse
        {
            get { return _uploadResponse; }
            set
            {
                _uploadResponse = value;
                OnPropertyChanged(() => UploadResponse);
            }
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<UpdateComposition>().Subscribe(OnUpdateComposition);
            EA.GetEvent<UpdateCompositionProvenance>().Subscribe(OnUpdateCompositionProvenance);
            EA.GetEvent<Save>().Subscribe(OnSaveChanges);
            EA.GetEvent<KeyDown>().Subscribe(OnKeyDown);
            EA.GetEvent<SetPrint>().Subscribe(OnSetPrint);
            EA.GetEvent<KeyUp>().Subscribe(OnKeyUp);
            EA.GetEvent<ReverseSelectedNotes>().Subscribe(OnReverseSelectedNotes);
            EA.GetEvent<BlurComposition>().Subscribe(OnBlurComposition);
            EA.GetEvent<Login>().Subscribe(Show, true);
            EA.GetEvent<ScaleViewportChanged>().Subscribe(OnScaleViewportChanged, true);
            EA.GetEvent<ResizeViewport>().Subscribe(OnResizeViewPort);
            EA.GetEvent<AreaSelect>().Subscribe(OnAreaSelect);
            EA.GetEvent<DeSelectAll>().Subscribe(OnDeselectAll);
            EA.GetEvent<HideProvenancePanel>().Subscribe(OnHideProvenancePanel, true);
            EA.GetEvent<ShowProvenancePanel>().Subscribe(OnShowProvenancePanel, true);
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition, true);
            EA.GetEvent<UpdateScrollOffset>().Subscribe(OnUpdateScrollOffset);
            EA.GetEvent<DeleteArc>().Subscribe(OnDeleteArc);
            EA.GetEvent<ArrangeArcs>().Subscribe(OnArrangeArcs);
            EA.GetEvent<CreateAndUploadImage>().Subscribe(OnCreateAndUploadImage, true);
            EA.GetEvent<CreateAndUploadFile>().Subscribe(OnCreateAndUploadFile, true);
            EA.GetEvent<SetCompositionWidthHeight>().Subscribe(OnSetCompositionWidthHeight, true);
        }

        public void OnUpdateCompositionProvenance(Tuple<string, string, string> payload)
        {
            Composition.Provenance.TitleLine = payload.Item1;
            Composition.Provenance.FontFamily = payload.Item2;
            Composition.Provenance.LargeFontSize = payload.Item3;
            SetRepository();
            _repository.Update(Composition);
        }

        public void OnSetPrint(object obj)
        {
            EditorState.IsPrinting = true;
        }

        public void OnSetCompositionWidthHeight(object obj)
        {
            SetImageHeight();
            SetImageWidth();
        }

        public void SetImageHeight()
        {
            if (CompositionGrid == null) return;
            Height = (int)(Math.Ceiling(CompositionGrid.ActualHeight));
            if (Height < 10)
            {
                Height = Infrastructure.Support.Densities.StaffgroupDensity * Infrastructure.Support.Densities.StaffDensity * (Defaults.MeasureHeight + (EditorState.VerseCount * (Defaults.VerseHeight + 2)));
            }
        }

        public void SetImageWidth()
        {
            if (CompositionGrid == null) return;
            Width = (int)(Math.Ceiling(CompositionGrid.ActualWidth));
        }

        public void OnCreateAndUploadFile(object obj)
        {
            SendFile();
        }

        public void OnCreateAndUploadImage(object obj)
        {
            EA.GetEvent<SetCompositionWidthHeight>().Publish(string.Empty);

            var document = HtmlPage.Document;
            var txtArea = document.GetElementById("MainContent_txtPNGBytes");

            if (txtArea != null)
            {
                try
                {
                    const double scale = 1;

                    var bmp = new WriteableBitmap(CompositionGrid, null);
                    var buffer = bmp.ToByteArray();

                    bmp = new WriteableBitmap(int.Parse(Width.ToString(CultureInfo.InvariantCulture)), int.Parse(Height.ToString(CultureInfo.InvariantCulture)));
                    bmp.FromByteArray(buffer);

                    var transform = new ScaleTransform {ScaleX = scale, ScaleY = scale};

                    bmp.Render(CompositionGrid, transform);
                    bmp.Invalidate();

                    var stream = bmp.GetStream();
                    var binaryData = new Byte[stream.Length];
                    var bytesRead = stream.Read(binaryData, 0, (int)stream.Length);
                    var base64 = Convert.ToBase64String(binaryData, 0, binaryData.Length);

                    RawSize = base64.Length.ToString(CultureInfo.InvariantCulture);
                    var message = Compression.Compress(base64);
                    base64 = message.Text;
                    CompressedSize = base64.Length.ToString(CultureInfo.InvariantCulture);
                    txtArea.SetProperty("value", base64);

                    UploadDetailsVisibility = Visibility.Visible;

                    SendImage();
                }
                catch (Exception ex)
                {
                    Exceptions.HandleException(ex, "Error in: OnCreateAndUploadImage");
                }
            }
        }

        private void SendFile()
        {
            _uri = new Uri(@"/composer/Home/CreateFile", UriKind.Relative);
            _client = new WebClient();

            // you MUST modify the header fields for this to work otherwise it will respond
            // with regular HTTP headers.
            _client.Headers["content-type"] = "application/json";

            // this will be fired after the upload is complete.
            _client.UploadStringCompleted += (sndr, evnt) =>
            {
                if (evnt.Error != null)
                {
                    UploadResponse = string.IsNullOrWhiteSpace(evnt.Error.Message)
                        ? @"An exception occurred."
                        : evnt.Error.Message;
                }
                else if (evnt.Cancelled)
                {
                    UploadResponse = "Operation was canceled.";
                }
            };

            var myObject = new Message { CompositionId = Composition.Id.ToString(), CollaborationId = Current.User.Index, Text = "", CompositionTitle = Composition.Provenance.TitleLine };
            string json = Serialization.ToJson(myObject);
            _client.UploadStringAsync(_uri, "POST", json);
        }

        private void SendImage()
        {
            _uri = new Uri(@"/composer/Home/PushMessage", UriKind.Relative);
            _client = new WebClient();

            // you MUST modify the header fields for this to work otherwise it will respond
            // with regular HTTP headers.
            _client.Headers["content-type"] = "application/json";

            // this will be fired after the upload is complete.
            _client.UploadStringCompleted += (sndr, evnt) =>
            {
                if (evnt.Error != null)
                {
                    UploadResponse = string.IsNullOrWhiteSpace(evnt.Error.Message)
                        ? @"An exception occurred."
                        : evnt.Error.Message;
                }
                else if (evnt.Cancelled)
                {
                    UploadResponse = "Operation was canceled.";
                }
                EA.GetEvent<CreateAndUploadFile>().Publish(string.Empty); // chain the uploads so only one is happening at a time for now.
            };

            var document = HtmlPage.Document;
            var txtArea1 = document.GetElementById("MainContent_txtPNGBytes");
            if (txtArea1 == null) return;
            var base64 = txtArea1.GetProperty("value").ToString();
            var myObject = new Message { CompositionId = Composition.Id.ToString(), CollaborationId = Current.User.Index, Text = base64 };
            var json = Serialization.ToJson(myObject);
            _client.UploadStringAsync(_uri, "POST", json);
        }

        public void OnKeyUp(string key)
        {
            switch (key.ToLower())
            {
                case "c":
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Infrastructure.Support.Clipboard.Notes = Infrastructure.Support.Selection.Notes;
                    }
                    break;
                case "x":
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Infrastructure.Support.Selection.Delete();
                    }
                    break;
                case "v":
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {

                    }
                    break;
            }
        }

        public void OnKeyDown(string key)
        {
            if (Infrastructure.Support.Selection.Exists())
            {
                switch (key.ToLower())
                {
                    case "delete":
                        Infrastructure.Support.Selection.Delete();
                        break;
                    case "f":
                        Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.Flat);
                        break;
                    case "s":
                        Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.Sharp);
                        break;
                    case "n":
                        Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.Natural);
                        break;
                    case "b":
                        Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.None);
                        break;
                }
            }
            else
            {
                if (EditorState.ActiveMeasureId != Guid.Empty)
                {
                    switch (key.ToLower())
                    {
                        case "back":
                            EA.GetEvent<Backspace>().Publish(string.Empty);
                            break;
                    }
                }
            }
        }

        public void OnDeleteArc(Guid arcId)
        {
            foreach (var arc in Composition.Arcs.Where(arc => arc.Id == arcId))
            {
                SetRepository();
                Composition.Arcs.Remove(arc);
                _repository.Delete(arc);
                break;
            }
        }

        public void OnUpdateComposition(Repository.DataService.Composition composition)
        {
            Composition = composition;
        }

        public void OnUpdateScrollOffset(Tuple<double, double> payload)
        {
            ScrollOffsets = string.Format("{0}, {1}", payload.Item1, payload.Item2);
        }

        public void OnSaveChanges(object obj)
        {
            SetRepository();
            EA.GetEvent<SuspendEditing>().Publish(string.Empty);
            EA.GetEvent<ShowBusyIndicator>().Publish(string.Empty);
            EA.GetEvent<PublishFacebookAction>().Publish(string.Empty);
            _repository.SaveChanges(this);
        }

        public void OnSaveChangesComplete()
        {
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
            EA.GetEvent<UpdatePinterestImage>().Publish(string.Empty);
            EA.GetEvent<ShowSocialChannels>().Publish(_Enum.SocialChannel.All);
            EditorState.IsSaving = false;
        }

        public void OnCommitTransposition(Tuple<Guid, object> payload)
        {
            var state = (TranspositionState)payload.Item2;
            Composition.Key_Id = state.Key.Id;
        }

        public void OnResizeViewPort(Point coordinate)
        {
            ScrollWidth = coordinate.X - HorizontalScrollOffset;
            ScrollHeight = coordinate.Y - VerticalScrollOffset;
        }

        public void OnBlurComposition(int radius)
        {
            if (!EditorState.IsSaving)
            {
                BlurRadius = radius;
            }
        }

        public void DefineCommands()
        {
            CancelCommand = new DelegatedCommand<object>(OnCancelCommand);
        }

        public void OnCancelCommand(object obj)
        {
            EA.GetEvent<ClosePrintPreview>().Publish(string.Empty);
        }

        private DelegatedCommand<object> _cancelCommand;

        public DelegatedCommand<object> CancelCommand
        {
            get { return _cancelCommand; }
            set
            {
                _cancelCommand = value;
                OnPropertyChanged(() => CancelCommand);
            }

        }
    }
}