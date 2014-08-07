using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Dimensions;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Support;
using Composer.Modules.Composition.EventArgs;
using Composer.Modules.Composition.Service;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Reactive.Linq;
using Composer.Modules.Composition.Views;
using System.Windows.Input;
using System.Net;
using Composer.Infrastructure.Constants;
using Clipboard = Composer.Infrastructure.Support.Clipboard;
using Measure = Composer.Repository.DataService.Measure;
using Selection = Composer.Infrastructure.Support.Selection;

namespace Composer.Modules.Composition.ViewModels
{
	using Composer.Modules.Composition.Models;

    public sealed partial class CompositionViewModel : BaseViewModel, ICompositionViewModel, IEventCatcher
    {
        private Uri _uri;
        private WebClient _client;

        public event EventHandler LoadComplete;
        public event EventHandler ErrorLoading;
        DataServiceRepository<Repository.DataService.Composition> _repository;
        public Grid CompositionGrid = null;


        public IEnumerable<Staffgroup> GetStaffgroups()
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
                Staffgroups = new ObservableCollection<Staffgroup>();
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

		private List<Sequence> _sequences = new List<Sequence>();
		public List<Sequence> Sequences
		{
			get { return _sequences; }
			set
			{
				_sequences = value;
				OnPropertyChanged(() => Sequences);
			}
		}

		private List<Measuregroup> _measureGroups = new List<Measuregroup>();
		public List<Measuregroup> Measuregroups
		{
			get { return _measureGroups; }
			set
			{
				_measureGroups = value;
				OnPropertyChanged(() => Measuregroups);
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

        private ObservableCollection<Staffgroup> _staffgroups;
        public ObservableCollection<Staffgroup> Staffgroups
        {
            get { return _staffgroups; }
            set
            {
                _staffgroups = value;
                OnPropertyChanged(() => Staffgroups);
            }
        }

        private Staffgroup _selectedStaffgroup;
        public Staffgroup SelectedStaffgroup
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

        public void OnUpdateMeasurePackState(Tuple<Guid, _Enum.EntityFilter> payload)
        {
            EditorState.IsCalculatingStatistics = true;
            List<Measure> measures = null;
            var filter = payload.Item2;
            var entityId = payload.Item1;
            switch (filter)
            {
                case _Enum.EntityFilter.Composition :
                    measures = Cache.Measures.ToList();
                    break;
                case _Enum.EntityFilter.Staffgroup:
                    measures = Utils.GetMeasures(entityId).ToList();
                    break;
                case _Enum.EntityFilter.Measure:
                    measures = new List<Measure>();
                    measures.Add(Utils.GetMeasure(entityId));
                    break;
            }
            if (measures != null)
            {
                foreach (var m in measures)
                {
                    Statistics.Add(m);
                }
            }
            EditorState.IsCalculatingStatistics = false;
        }

        private void LoadComposition(Repository.DataService.Composition c)
        {
            TimeSignature_Id = c.TimeSignature_Id;
            c = CompositionManager.FlattenComposition(c);
            CompositionManager.Composition = c;
            EditorState.IsReturningContributor = CollaborationManager.IsReturningContributor(c);
            EditorState.IsAuthor = c.Audit.Author_Id == Current.User.Id;

            if (EditorState.EditContext == _Enum.EditContext.Contributing && !EditorState.IsReturningContributor)
            {
                //EditorState.IsReturningContributor gets set in Collaborations.Initialize().
                SetRepository();
                Collaborations.Index = c.Collaborations.Count();
                var collaborator = CollaborationManager.Create(c, Collaborations.Index);
                _repository.Context.AddLink(c, "Collaborations", collaborator);
                c.Collaborations.Add(collaborator);
            }
            else
            {
                var a = (from b in c.Collaborations where b.Collaborator_Id == Current.User.Id select b.Index);
                var e = a as List<int> ?? a.ToList();
                if (e.Any())
                {
                    Collaborations.Index = e.First();
                }
            }
            CompositionManager.Composition = c;
            EditorState.IsCollaboration = c.Collaborations.Count > 1;
            CollaborationManager.Initialize(); // TODO: do we need to initialize CollabrationManager when there are no collaborations?
            EA.GetEvent<UpdateMeasurePackState>().Publish(new Tuple<Guid, _Enum.EntityFilter>(Guid.Empty, _Enum.EntityFilter.Composition));
            Composition = c;
            Verses = c.Verses;
            CompilePrintPages();
            EA.GetEvent<SetCompositionWidth>().Publish(Composition.Staffgroups[0].Staffs[0].Id);
            SequenceManager.Spinup();
            MeasuregroupManager.Spinup();
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
            var starttimes = new List<double>();
            var durations = new List<decimal>();

            foreach (var n in Selection.Notes)
            {
                var ch = Utils.GetChord(n.Chord_Id);
                if (ch == null) continue;
                var ng = NotegroupManager.ParseChord(ch, n);
                if (ng == null) continue;
                if (ng.IsRest) continue;
                if (starttimes.Contains(ng.StartTime) && durations.Contains(ng.Duration)) continue; // this stem has been reversed already
                starttimes.Add(ng.StartTime);
                durations.Add(ng.Duration);
                foreach (var ngN in ng.Notes)
                {
                    EA.GetEvent<ReverseNoteStem>().Publish(ngN);
                }
                ng.Reverse();
                EA.GetEvent<FlagNotegroup>().Publish(ng);
            }
            foreach (var measure in Selection.ImpactedMeasures)
            {
                EA.GetEvent<UpdateSpanManager>().Publish(measure.Id);
                EA.GetEvent<SpanMeasure>().Publish(measure.Id);
            }
        }

        public void OnDeselectAll(object obj)
        {
            EA.GetEvent<DeSelectComposition>().Publish(string.Empty);
            EA.GetEvent<HideNoteEditor>().Publish(string.Empty);
            EA.GetEvent<ResetMeasureFooter>().Publish(string.Empty);
            Selection.RemoveAll();
            Selection.Clear();
        }

        public void OnArrangeArcs(Measure mE)
        {
            foreach (Arc arc in Composition.Arcs)
            {
                var id1 = arc.Chord_Id1;
                var id2 = arc.Chord_Id2;
                var ch1 = Utils.GetChord(id1);
                var ch2 = Utils.GetChord(id2);
                if (ch1.Measure_Id == mE.Id || ch2.Measure_Id == mE.Id)
                {
                    EA.GetEvent<RenderArc>().Publish(arc.Id);
                }
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
                                    var p = Utilities.CoordinateSystem.TranslateToCompositionCoords(x, y, measure.Sequence, measure.Index, measure.Index * 4, 4, measure.Width, measure.Staff_Id);
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

        public void SubscribeEvents()
        {
            EA.GetEvent<SetCompositionWidth>().Subscribe(OnSetCompositionWidth);
            EA.GetEvent<UpdateMeasurePackState>().Subscribe(OnUpdateMeasurePackState);
            EA.GetEvent<UpdateComposition>().Subscribe(OnUpdateComposition);
            EA.GetEvent<UpdateCompositionProvenance>().Subscribe(OnUpdateCompositionProvenance);
            EA.GetEvent<Save>().Subscribe(OnSaveChanges);
            EA.GetEvent<KeyDown>().Subscribe(OnKeyDown);
            EA.GetEvent<SetPrint>().Subscribe(OnSetPrint);
            EA.GetEvent<KeyUp>().Subscribe(OnKeyUp);
            EA.GetEvent<ReverseSelectedNotes>().Subscribe(OnReverseSelectedNotes);
            EA.GetEvent<Login>().Subscribe(Show, true);
            EA.GetEvent<DeSelectAll>().Subscribe(OnDeselectAll);
            EA.GetEvent<HideProvenancePanel>().Subscribe(OnHideProvenancePanel, true);
            EA.GetEvent<ShowProvenancePanel>().Subscribe(OnShowProvenancePanel, true);
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition, true);
            EA.GetEvent<DeleteArc>().Subscribe(OnDeleteArc);
            EA.GetEvent<ArrangeArcs>().Subscribe(OnArrangeArcs);
            EA.GetEvent<SetCompositionWidthHeight>().Subscribe(OnSetCompositionWidthHeight, true);
            EA.GetEvent<UpdateSequences>().Subscribe(OnUpdateSequences);
            EA.GetEvent<UpdateMeasuregroups>().Subscribe(OnUpdateMeasuregroups);
            SubscribeFilesEvents();
            SubscribeUIEvents();
        }

        public void OnSetCompositionWidth(Guid sFiD)
        {
            Staff s;
            try
            {
                s = Utils.GetStaff(sFiD);
                var width = (from a in s.Measures select double.Parse(a.Width)).Sum() + Defaults.StaffDimensionWidth +
                            Defaults.CompositionLeftMargin - 30;
                EditorState.GlobalStaffWidth = width;
                Width = width;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "OnSetCompositionWidth");
            }
        }

        public void OnUpdateMeasuregroups(object obj)
        {
            Measuregroups = (List<Measuregroup>)obj;
        }

        public void OnUpdateSequences(object obj)
        {
            Sequences = (List<Sequence>)obj;
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
                Height = Densities.StaffgroupDensity * Densities.StaffDensity * (Defaults.MeasureHeight + (EditorState.VerseCount * (Defaults.VerseHeight + 2)));
            }
        }

        public void SetImageWidth()
        {
            if (CompositionGrid == null) return;
            Width = (int)(Math.Ceiling(CompositionGrid.ActualWidth));
        }

        public void OnKeyUp(string key)
        {
            switch (key.ToLower())
            {
                case "c":
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Clipboard.Notes = Selection.Notes;
                    }
                    break;
                case "x":
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Selection.Delete();
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
            if (Selection.Exists())
            {
                switch (key.ToLower())
                {
                    case "delete":
                        Selection.Delete();
                        break;
                    case "f":
                        Selection.SetAccidental(_Enum.Accidental.Flat);
                        break;
                    case "s":
                        Selection.SetAccidental(_Enum.Accidental.Sharp);
                        break;
                    case "n":
                        Selection.SetAccidental(_Enum.Accidental.Natural);
                        break;
                    case "b":
                        Selection.SetAccidental(_Enum.Accidental.None);
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