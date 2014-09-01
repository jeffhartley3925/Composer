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

    public sealed partial class CompositionViewModel : ICompositionViewModel
    {
        private Uri uri;
        private WebClient client;

        public event EventHandler LoadComplete;
        public event EventHandler ErrorLoading;
        DataServiceRepository<Repository.DataService.Composition> repository;
        public Grid CompositionGrid = null;


        public IEnumerable<Staffgroup> GetStaffgroups()
        {
            for (var sGiX = 0; sGiX < this.composition.Staffgroups.Count(); ++sGiX)
            {
                yield return this.composition.Staffgroups[sGiX];
            }
        }

        private Repository.DataService.Composition composition;
        public Repository.DataService.Composition Composition
        {
            get { return this.composition; }
            set
            {
                this.composition = value;
                Staffgroups = new ObservableCollection<Staffgroup>();
                var observable = GetStaffgroups().ToObservable();
                observable.Subscribe(sG => Staffgroups.Add(sG));
                OnPropertyChanged(() => Composition);
            }
        }

        private int timeSignatureId;
        public int TimeSignature_Id
        {
            get { return this.timeSignatureId; }
            set
            {
                this.timeSignatureId = value;

                var tS = (from a in TimeSignatures.TimeSignatureList
                                     where a.Id == this.timeSignatureId
                                     select a.Name).First();

                if (string.IsNullOrEmpty(tS))
                {
                    tS =
                        (from a in TimeSignatures.TimeSignatureList
                         where a.Id == Preferences.DefaultTimeSignatureId
                         select a.Name).First();
                }

                DurationManager.Bpm = Int32.Parse(tS.Split(',')[0]);
                DurationManager.BeatUnit = Int32.Parse(tS.Split(',')[1]);
                DurationManager.Initialize();
            }
        }

        private PrintPageItem SetPrintProvenance(PrintPageItem pI)
        {
            pI.Title = Composition.Provenance.TitleLine;
            pI.SmallFontFamily = Composition.Provenance.FontFamily;
            pI.TitleFontFamily = Composition.Provenance.FontFamily;
            pI.TitleFontSize = Composition.Provenance.LargeFontSize;
            pI.SmallFontSize = Composition.Provenance.SmallFontSize;
            pI.Date = Composition.Audit.ModifyDate.ToLongDateString();

            var authors = new List<Author>();
            foreach (var cN in Composition.Collaborations)
            {
                if (cN.PictureUrl.IndexOf("https", StringComparison.Ordinal) == 0)
                {
                    cN.PictureUrl = cN.PictureUrl.Replace("https", "http");
                }
                var author = new Author { Name = cN.Name, PictureUrl = cN.PictureUrl, PercentContribution = 0 };
                authors.Add(author);
            }
            pI.Authors = authors;
            return pI;
        }

        private void CompilePrintPages()
        {
            int sGiX;

            var pG = new PrintPage { PrintPageItems = new ObservableCollection<PrintPageItem>() };

            var pI = new PrintPageItem();
            pI = SetPrintProvenance(pI);
            pI.Staffgroup = null;
            pG.PrintPageItems.Add(pI);

            for (sGiX = 0; sGiX <= Staffgroups.Count() - 1; sGiX++)
            {
                if (sGiX % Preferences.PrintItemsPerPage == 0 && sGiX != 0)
                {
                    PrintPages.Add(pG);
                    pG = new PrintPage { PrintPageItems = new ObservableCollection<PrintPageItem>() };
                }

                var item = new PrintPageItem { Staffgroup = Staffgroups[sGiX] };
                pG.PrintPageItems.Add(item);
            }
            if (sGiX % Preferences.PrintItemsPerPage > 0)
            {
                PrintPages.Add(pG);
            }
        }

		private List<Sequencegroup> sequences = new List<Sequencegroup>();
		public List<Sequencegroup> Sequences
		{
			get { return this.sequences; }
			set
			{
				this.sequences = value;
				OnPropertyChanged(() => Sequences);
			}
		}

		private List<Measuregroup> measureGroups = new List<Measuregroup>();
		public List<Measuregroup> Measuregroups
		{
			get { return this.measureGroups; }
			set
			{
				this.measureGroups = value;
				OnPropertyChanged(() => Measuregroups);
			}
		}

        private ObservableCollection<PrintPage> printPages = new ObservableCollection<PrintPage>();
        public ObservableCollection<PrintPage> PrintPages
        {
            get { return this.printPages; }
            set
            {
                this.printPages = value;
                OnPropertyChanged(() => PrintPages);
            }
        }

        private ObservableCollection<Staffgroup> staffgroups;
        public ObservableCollection<Staffgroup> Staffgroups
        {
            get { return this.staffgroups; }
            set
            {
                this.staffgroups = value;
                OnPropertyChanged(() => Staffgroups);
            }
        }

        private Staffgroup selectedStaffgroup;
        public Staffgroup SelectedStaffgroup
        {
            get { return this.selectedStaffgroup; }
            set
            {
                this.selectedStaffgroup = value;
                OnPropertyChanged(() => SelectedStaffgroup);
            }
        }

        private string id;
        public string Id
        {
            get { return this.id; }
            set
            {
                this.id = value;
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

        private double provenanceX;
        public double Provenance_X
        {
            get { return this.provenanceX; }
            set
            {
                this.provenanceX = value;
                OnPropertyChanged(() => Provenance_X);
            }
        }

        private double provenanceY;
        public double Provenance_Y
        {
            get { return this.provenanceY; }
            set
            {
                this.provenanceY = value;
                OnPropertyChanged(() => Provenance_Y);
            }
        }

        private Visibility provenanceVisibility;
        public Visibility ProvenanceVisibility
        {
            get { return this.provenanceVisibility; }
            set
            {
                this.provenanceVisibility = value;
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
            if (this.repository == null)
            {
                this.repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
        }

        private void LoadComposition(Repository.DataService.Composition cO)
        {
            TimeSignature_Id = cO.TimeSignature_Id;
            cO = CompositionManager.FlattenComposition(cO);
            CompositionManager.Composition = cO;
            EditorState.IsReturningContributor = CollaborationManager.IsReturningContributor(cO);
            EditorState.IsAuthor = cO.Audit.Author_Id == Current.User.Id;

            if (EditorState.EditContext == _Enum.EditContext.Contributing && !EditorState.IsReturningContributor)
            {
                //EditorState.IsReturningContributor gets set in Collaborations.Initialize().
                SetRepository();
                Collaborations.Index = cO.Collaborations.Count();
                var collaborator = CollaborationManager.Create(cO, Collaborations.Index);
                this.repository.Context.AddLink(cO, "Collaborations", collaborator);
                cO.Collaborations.Add(collaborator);
            }
            else
            {
                var a = (from b in cO.Collaborations where b.Collaborator_Id == Current.User.Id select b.Index);
                var e = a as List<int> ?? a.ToList();
                if (e.Any())
                {
                    Collaborations.Index = e.First();
                }
            }
            CompositionManager.Composition = cO;
            EditorState.IsCollaboration = cO.Collaborations.Count > 1;
            CollaborationManager.Initialize(); // TODO: do we need to initialize CollabrationManager when there are no collaborations?
            Composition = cO;
            Verses = cO.Verses;
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
                foreach (var cO in e.Results)
                {
                    LoadComposition(cO);
                    break;
                }
                if (LoadComplete != null) LoadComplete(this, null);
                EA.GetEvent<CompositionLoaded>().Publish(new object());
            });
        }

        public DataServiceCollection<Verse> Verses { get; set; }

        public void OnReverseSelectedNotes(object obj)
        {
            var sTs = new List<double>();
            var dUs = new List<decimal>();
            foreach (var nT in Selection.Notes)
            {
                var cH = Utils.GetChord(nT.Chord_Id);
                if (cH == null) continue;
                var nG = NotegroupManager.ParseChord(cH, nT);
                if (nG == null) continue;
                if (nG.IsRest) continue;
				if (sTs.Contains(nG.StartTime) && dUs.Contains(nG.Duration)) continue; // this stem has been reversed already
                sTs.Add(nG.StartTime);
				dUs.Add(nG.Duration);
                foreach (var n in nG.Notes)
                {
                    EA.GetEvent<ReverseNoteStem>().Publish(n);
                }
                nG.Reverse();
                EA.GetEvent<FlagNotegroup>().Publish(nG);
            }
            foreach (var mE in Selection.ImpactedMeasures)
            {
                EA.GetEvent<UpdateSpanManager>().Publish(mE.Id);
                EA.GetEvent<SpanMeasure>().Publish(mE.Id);
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
            foreach (var arc in Composition.Arcs)
            {
                var iD1 = arc.Chord_Id1;
                var iD2 = arc.Chord_Id2;
                var cH1 = Utils.GetChord(iD1);
                var cH2 = Utils.GetChord(iD2);
                if (cH1.Measure_Id == mE.Id || cH2.Measure_Id == mE.Id)
                {
                    EA.GetEvent<RenderArc>().Publish(arc.Id);
                }
            }
        }

        private void SelectAllSelectedNotes()
        {
            foreach (var sG in CompositionManager.Composition.Staffgroups)
            {
                foreach (var sF in sG.Staffs)
                {
                    foreach (var measure in sF.Measures)
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

        private double width = double.NaN;
        public double Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                OnPropertyChanged(() => Width);
            }
        }

        private double height = double.NaN;
        public double Height
        {
            get { return this.height; }
            set
            {
                this.height = value;
                OnPropertyChanged(() => Height);
            }
        }

        public void SubscribeEvents()
        {
			EA.GetEvent<RespaceComposition>().Subscribe(OnRespaceComposition);
            EA.GetEvent<SetCompositionWidth>().Subscribe(OnSetCompositionWidth);
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

		public void OnRespaceComposition(object obj)
		{
			try
			{
				foreach (var sQ in this.Sequences)
				{
					EA.GetEvent<RespaceSequence>().Publish(new Tuple<Guid, int?>(Guid.Empty, sQ.Sequence));
				}
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex, "OnRespaceSequence");
			}
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

        public void OnUpdateMeasuregroups(object obj)
        {
            Measuregroups = (List<Measuregroup>)obj;
        }

        public void OnUpdateSequences(object obj)
        {
            Sequences = (List<Sequencegroup>)obj;
        }

        public void OnUpdateCompositionProvenance(Tuple<string, string, string> payload)
        {
            Composition.Provenance.TitleLine = payload.Item1;
            Composition.Provenance.FontFamily = payload.Item2;
            Composition.Provenance.LargeFontSize = payload.Item3;
            SetRepository();
            this.repository.Update(Composition);
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
                this.repository.Delete(arc);
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
            this.repository.SaveChanges(this);
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

        private DelegatedCommand<object> cancelCommand;

        public DelegatedCommand<object> CancelCommand
        {
            get { return this.cancelCommand; }
            set
            {
                this.cancelCommand = value;
                OnPropertyChanged(() => CancelCommand);
            }

        }
    }
}