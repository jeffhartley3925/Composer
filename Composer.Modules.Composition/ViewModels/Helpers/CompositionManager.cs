using System;
using System.Linq;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using System.Data.Services.Client;
using Composer.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Browser;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class CompositionManager
    {
        public static List<short> ClefIds = null;
        public static double YScrollOffset = 0;
        public static double XScrollOffset = 0;
        private static Int16 _measureIndex;
        private static int _staffgroupSequence;
        private static int _staffSequence;
        private static int _measureSequence;
        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        public static IEventAggregator Ea;

        static CompositionManager()
        {

        }

        public static void Initialize()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
            }
        }

        private static void SubscribeEvents()
        {
            Ea.GetEvent<CreateNewComposition>().Subscribe(OnCreateNewComposition);
            Ea.GetEvent<UpdateScrollOffset>().Subscribe(OnUpdateScrollOffset);
            Ea.GetEvent<UpdatePinterestImage>().Subscribe(OnUpdatePinterestImage);
            Ea.GetEvent<ShowSocialChannels>().Subscribe(OnShowSocialChannels);
            Ea.GetEvent<HideSocialChannels>().Subscribe(OnHideSocialChannels);
            Ea.GetEvent<SetSocialChannels>().Subscribe(OnSetSocialChannels);
            Ea.GetEvent<SetRequestPrompt>().Subscribe(OnSetRequestPrompt);
            Ea.GetEvent<PublishFacebookAction>().Subscribe(OnPublishFacebookAction, true);
        }

        public static void OnSetRequestPrompt(object obj)
        {
            HtmlDocument htmlDoc = HtmlPage.Document;
            if (htmlDoc != null)
            {
                HtmlPage.Window.Invoke("hideAllRequestPrompt");
                HtmlPage.Window.Invoke("setRequestPrompt", Composition.Collaborations.Count,
                                                           Composition.Verses.Count,
                                                           Composition.Id.ToString(),
                                                           Composition.Provenance.TitleLine,
                                                           Collaborations.Index);
            }
        }

        public static void OnPublishFacebookAction(object obj)
        {
            //HtmlPage.Window.Invoke("publishAction", Composition.Id.ToString(), Collaborations.Index);
        }

        private static Repository.DataService.Composition _composition;
        public static Repository.DataService.Composition Composition
        {
            get { return _composition; }
            set
            {
                _composition = value;
                Initialize();
                if (Composition.Collaborations == null) return;
                if (Composition.Collaborations.Count <= 0) return;
                Collaborations.Initialize(_composition.Id, _composition.Audit.Author_Id);
                // if compositionId and collaboratorIndex was passed in via the query string, manually 
                // set the Collaborations.Index 
                if (!EditorState.IsQueryStringSource()) return;
                var index = int.Parse(EditorState.qsIndex);
                Collaborations.Index = int.Parse(EditorState.qsIndex);

                var c = (from a in _composition.Collaborations where a.Index == index select a);
                if (c.Count() != 1) return;
                var d = (from s in Collaborations.Collaborators where s.Index == index select s);
                if (d.Count() != 1) return;

                Collaborations.CurrentCollaborator = d.Single();
            }
        }

        public static void HideSocialChannels()
        {
            Ea.GetEvent<HideSocialChannels>().Publish(_Enum.SocialChannel.All);
        }

        public static void ShowSocialChannels()
        {
            Ea.GetEvent<ShowSocialChannels>().Publish(_Enum.SocialChannel.All);
        }

        public static void DeleteUnusedContainers()
        {
            var b = Composition.Staffgroups.OrderBy(a => a.Index);
            var e = b as List<Repository.DataService.Staffgroup> ?? b.ToList();
            if (e.Any())
            {
                var staffgroup = b.Last();
                if (StaffgroupManager.IsEmpty(staffgroup))
                {
                    for (var i = 0; i < staffgroup.Staffs.Count(); i++)
                    {
                        var staff = staffgroup.Staffs[i];
                        for (var j = 0; j < staff.Measures.Count(); j++)
                        {
                            var measure = staff.Measures[j];
                            staff.Measures.Remove(measure);
                            Cache.Measures.Remove(measure);
                            _repository.Delete(measure);
                            _repository.Update(staff);

                        }
                        staffgroup.Staffs.Remove(staff);
                        Cache.Staffs.Remove(staff);
                        _repository.Delete(staff);
                        _repository.Update(staffgroup);
                    }
                    Composition.Staffgroups.Remove(staffgroup);
                    Cache.Staffgroups.Remove(staffgroup);
                    _repository.Delete(staffgroup);
                    _repository.Update(Composition);
                    Ea.GetEvent<UpdateComposition>().Publish(Composition);
                }
            }
        }

        private static void SetSocialChannelsVisibility(_Enum.SocialChannel obj, string visibility)
        {
            //granular control of social channel visibility
            switch (obj)
            {
                case _Enum.SocialChannel.All:
                    SetElementStyleAttribute("pinterestButtonContainer", "display", visibility);
                    SetElementStyleAttribute("likeButtonContainer", "display", visibility);
                    //SetElementStyleAttribute("sendButtonContainer", "display", visibility);
                    SetElementStyleAttribute("tweetButtonContainer", "display", visibility);
                    SetElementStyleAttribute("googlePlusoneButtonContainer", "display", visibility);
                    SetElementStyleAttribute("requestContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.FacebookFeed:
                    break;
                case _Enum.SocialChannel.FacebookLike:
                    SetElementStyleAttribute("likeButtonContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.FacebookSend:
                    //SetElementStyleAttribute("sendButtonContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.FacebookAll:
                    SetElementStyleAttribute("likeButtonContainer", "display", visibility);
                    SetElementStyleAttribute("sendButtonContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.Pinterest:
                    SetElementStyleAttribute("pinterestButtonContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.Twitter:
                    SetElementStyleAttribute("tweetButtonContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.GooglePlusone:
                    SetElementStyleAttribute("googlePlusoneButtonContainer", "display", visibility);
                    break;
                case _Enum.SocialChannel.Requests:
                    SetElementStyleAttribute("requestContainer", "display", visibility);
                    break;
            }
        }

        public static void OnShowSocialChannels(_Enum.SocialChannel obj)
        {
            SetSocialChannelsVisibility(obj, "inline-block");
        }

        public static void OnHideSocialChannels(_Enum.SocialChannel obj)
        {
            SetSocialChannelsVisibility(obj, "none");
        }

        private static void SetElementStyleAttribute(string elementId, string styleAttribute, string styleValue)
        {
            var htmlDoc = HtmlPage.Document;
            var htmlEl = htmlDoc.GetElementById(elementId);
            htmlEl.SetStyleAttribute(styleAttribute, styleValue);
        }

        public static void OnUpdatePinterestImage(object obj)
        {
            var htmlDoc = HtmlPage.Document;
            var htmlEl = htmlDoc.GetElementById("compositionImage");
            htmlEl.SetStyleAttribute("display", "block");
            htmlEl.SetStyleAttribute("position", "absolute");
            htmlEl.SetStyleAttribute("left", "-2000px");
            const string path = "/composer/CompositionImages/";
            htmlEl.SetAttribute("src", "");
            htmlEl.SetAttribute("src", string.Format("{0}{1}_{2}.bmp", path, Composition.Id, Current.User.Index));
            Ea.GetEvent<ShowSocialChannels>().Publish(_Enum.SocialChannel.Pinterest);
        }

        public static void OnSetSocialChannels(object obj)
        {
            var htmlDoc = HtmlPage.Document;
            var htmlEl = htmlDoc.GetElementById("like");
            if (htmlEl != null)
            {
                HtmlPage.Window.Invoke("setLikeButtonHref", Composition.Id.ToString(), "0", htmlEl);
            }
            else
            {
                throw new Exception("DOM Error. Could not initialize Facebook Like button");
            }

            htmlEl = htmlDoc.GetElementById("tweetButton");
            if (htmlEl != null)
            {
                HtmlPage.Window.Invoke("setTwitterButtonUrl", Composition.Id.ToString(), htmlEl);
            }
            else
            {
                throw new Exception("DOM Error. Could not initialize Tweet button");
            }
        }

        public static void OnUpdateScrollOffset(Tuple<double, double> payload)
        {
            XScrollOffset = payload.Item1;
            YScrollOffset = payload.Item2;
        }

        public static void OnCreateNewComposition(object obj)
        {
            var payload = (Tuple<string, List<string>, _Enum.StaffConfiguration, List<short>>)obj;
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();

            var verses = new DataServiceCollection<Repository.DataService.Verse>(null, TrackingMode.None);
            var staffgroups = new DataServiceCollection<Staffgroup>(null, TrackingMode.None);
            var collaborations = new DataServiceCollection<Repository.DataService.Collaboration>(null, TrackingMode.None);

            var composition = Create();
            composition.Provenance.TitleLine = payload.Item1;
            composition.StaffConfiguration = (short)payload.Item3;
            ClefIds = payload.Item4;
            for (int sgIndex = 0; sgIndex < Infrastructure.Support.Densities.StaffgroupDensity; sgIndex++)
            {
                var staffgroup = StaffgroupManager.Create(composition.Id, _staffgroupSequence);
                _staffgroupSequence += Defaults.SequenceIncrement;
                var staffs = new DataServiceCollection<Staff>(null, TrackingMode.None);
                _staffSequence = 0;
                for (int sIndex = 0; sIndex < Infrastructure.Support.Densities.StaffDensity; sIndex++)
                {
                    var staff = StaffManager.Create(staffgroup.Id, _staffSequence);

                    //clefIds was populated in NewCompositionPanel. it's a list of selected staff clef Ids top to bottom.
                    //if Dimensions.StaffDensity > then in multi instrument staffconfiguration - each staff clef is the 
                    //same as the first staff clef. otherwise the staff clefs are whatever was specified in the newcompositionpanel
                    staff.Clef_Id = (Infrastructure.Support.Densities.StaffDensity > 2) ? ClefIds[0] : ClefIds[sIndex % 2];

                    _staffSequence += Defaults.SequenceIncrement;
                    var measures = new DataServiceCollection<Repository.DataService.Measure>(null, TrackingMode.None);
                    _measureSequence = 0;
                    for (int mIndex = 0; mIndex < Infrastructure.Support.Densities.MeasureDensity; mIndex++)
                    {
                        var measure = MeasureManager.Create(staff.Id, _measureSequence);

                        _measureSequence += Defaults.SequenceIncrement;
                        measure.Index = _measureIndex;
                        _measureIndex++;
                        _repository.Context.AddLink(staff, "Measures", measure);
                        //if this is the last m on the last staff then m.bar is the EndBar.
                        if (mIndex == Infrastructure.Support.Densities.MeasureDensity - 1)
                        {
                            if (sgIndex == Infrastructure.Support.Densities.StaffgroupDensity - 1)
                            {
                                if (sIndex == Infrastructure.Support.Densities.StaffDensity - 1)
                                {
                                    measure.Bar_Id = 1;
                                }
                            }
                        }
                        measures.Add(measure);
                    }
                    staff.Measures = measures;
                    _repository.Context.AddLink(staffgroup, "Staffs", staff);
                    staffs.Add(staff);
                }
                staffgroup.Staffs = staffs;
                _repository.Context.AddLink(composition, "Staffgroups", staffgroup);
                staffgroups.Add(staffgroup);
            }

            composition.Staffgroups = staffgroups;
            Repository.DataService.Collaboration collaboration = CollaborationManager.Create(composition, 0);
            _repository.Context.AddLink(composition, "Collaborations", collaboration);
            collaborations.Add(collaboration);
            composition.Collaborations = collaborations;
            composition.Verses = verses;
            Ea.GetEvent<NewComposition>().Publish(composition);
        }

        public static Repository.DataService.Composition Create()
        {
            Cache.Clear();
            var composition = _repository.Create<Repository.DataService.Composition>();
            composition.Id = Guid.NewGuid();
            composition.Audit = new Audit { Author_Id = Current.User.Id };
            composition.Provenance = GetProvenance();
            composition.StaffConfiguration = (short)Preferences.DefaultStaffConfiguration;
            composition.Audit.CreateDate = DateTime.Now;
            composition.Audit.ModifyDate = DateTime.Now;
            composition.Audit.Author_Id = Current.User.Id;
            composition.Instrument_Id = Infrastructure.Dimensions.Instruments.Instrument.Id;
            composition.TimeSignature_Id = Infrastructure.Dimensions.TimeSignatures.TimeSignature.Id;
            composition.Key_Id = Infrastructure.Dimensions.Keys.Key.Id;
            return composition;
        }

        private static Provenance GetProvenance()
        {
            var provenance = new Provenance
                                 {
                                     TitleLine = "Discard",
                                     FontFamily = Preferences.ProvenanceFontFamily,
                                     SmallFontSize = Preferences.ProvenanceSmallFontSize,
                                     LargeFontSize = Preferences.ProvenanceTitleFontSize
                                 };
            return provenance;
        }

        public static Repository.DataService.Composition FlattenComposition(Repository.DataService.Composition composition)
        {
            Cache.Initialize();
            Infrastructure.Support.Densities.StaffgroupDensity = composition.Staffgroups.Count;
            Infrastructure.Support.Densities.StaffDensity = composition.Staffgroups[0].Staffs.Count;
            Infrastructure.Support.Densities.MeasureDensity = composition.Staffgroups[0].Staffs[0].Measures.Count;

            ClefIds = new List<short>();
            try
            {
                Cache.Staffgroups = new ObservableCollection<Staffgroup>();
                Cache.Staffs = new ObservableCollection<Staff>();
                Cache.Measures = new ObservableCollection<Repository.DataService.Measure>();
                Cache.Chords = new ObservableCollection<Chord>();
                Cache.Notes = new ObservableCollection<Note>();
                Cache.Spans = new ObservableCollection<LocalSpan>();

                composition = SortStaffgroups(composition);
                if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.None)
                {
                    EditorState.StaffConfiguration = (_Enum.StaffConfiguration)composition.StaffConfiguration;
                }
                Infrastructure.Dimensions.Keys.Key = (from m in Infrastructure.Dimensions.Keys.KeyList where m.Id == composition.Key_Id select m).First();

                for (var i = 0; i < composition.Staffgroups.Count; i++)
                {
                    var staffgroup = composition.Staffgroups[i];
                    staffgroup = SortStaffs(staffgroup);
                    Cache.AddStaffgroup(staffgroup);
                    for (var j = 0; j < staffgroup.Staffs.Count; j++)
                    {

                        var staff = composition.Staffgroups[i].Staffs[j];
                        if (i == 0)
                        {
                            //need to store the ClefIds of each staff so if we add a staffgroup later, 
                            //we know what clef to assign to each staff in the staffgroup
                            if (staff.Clef_Id != null) ClefIds.Add((short)staff.Clef_Id);
                        }
                        staff = SortMeasures(staff);
                        Cache.AddStaff(staff);
                        foreach (var measure in staff.Measures)
                        {
                            Cache.AddMeasure(measure);
                            foreach (var chord in measure.Chords)
                            {
                                Cache.Chords.Add(chord);
                                foreach (var note in chord.Notes)
                                {
                                    Cache.Notes.Add(note);
                                }
                            }
                        }
                    }
                }
                //the following line has to be here so that trestle height can be calculated 
                //as the composition loads. Also, so that verseHeight can be added to staffHeight
                //to get the true staffHeight for calculations involving staffHeight such as the y pop coordinates of the context menu(s)
                EditorState.VerseCount = composition.Verses.Count;
                var repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                repository.Attach(composition);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return composition;
        }

        public static Repository.DataService.Composition SortStaffgroups(Repository.DataService.Composition composition)
        {
            for (var j = 0; j < composition.Staffgroups.Count - 1; j++)
            {
                for (var i = 0; i < composition.Staffgroups.Count - 1; i++)
                {
                    var staffgroupA = composition.Staffgroups[i];
                    var staffgroupB = composition.Staffgroups[i + 1];
                    if (staffgroupA.Sequence > staffgroupB.Sequence)
                    {
                        composition.Staffgroups[i] = staffgroupB;
                        composition.Staffgroups[i + 1] = staffgroupA;
                    }
                }
            }
            return composition;
        }

        public static Staffgroup SortStaffs(Staffgroup staffgroup)
        {
            for (var j = 0; j < staffgroup.Staffs.Count - 1; j++)
            {
                for (var i = 0; i < staffgroup.Staffs.Count - 1; i++)
                {
                    var staffA = staffgroup.Staffs[i];
                    var staffB = staffgroup.Staffs[i + 1];
                    if (staffA.Sequence > staffB.Sequence)
                    {
                        staffgroup.Staffs[i] = staffB;
                        staffgroup.Staffs[i + 1] = staffA;
                    }
                }
            }
            return staffgroup;
        }

        public static Staff SortMeasures(Staff staff)
        {
            for (var j = 0; j < staff.Measures.Count - 1; j++)
            {
                for (var i = 0; i < staff.Measures.Count - 1; i++)
                {
                    var measureA = staff.Measures[i];
                    var measureB = staff.Measures[i + 1];
                    if (measureA.Sequence > measureB.Sequence)
                    {
                        staff.Measures[i] = measureB;
                        staff.Measures[i + 1] = measureA;
                    }
                }
            }
            return staff;
        }
    }
}
