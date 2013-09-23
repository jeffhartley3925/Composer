using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Composer.Infrastructure;
using System.Xml.Linq;
using Composer.Infrastructure.Constants;
using System.Windows.Browser;
using System;

namespace Composer.Modules.Composition
{
    public class Playback
    {
        private static IEventAggregator _ea;

        public static void Initialize()
        {
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
            _ea.GetEvent<PausePlay>().Subscribe(OnPausePlay, true);
            _ea.GetEvent<PlayMeasure>().Subscribe(OnPlayMeasure, true);
            _ea.GetEvent<PlayComposition>().Subscribe(OnPlayComposition, true);
            _ea.GetEvent<Play>().Subscribe(OnPlay, true);
        }

        public static void OnPlay(object obj)
        {
            var xmlRoot =
                new XElement("root",
                    from b in Cache.PlaybackNotes
                    where b.Pitch != Defaults.RestSymbol
                    select
                        new XElement("row",
                        new XAttribute("instrument", "piano"),
                        new XAttribute("pitch", b.Pitch),
                        new XAttribute("duration", b.Duration),
                        new XAttribute("starttime", b.StartTime),
                        new XAttribute("status", b.Status)
                        )
                );

            var htmlDoc = HtmlPage.Document;
            var htmlEl = htmlDoc.GetElementById("playbackXml");
            if (htmlEl != null)
            {
                htmlEl.SetAttribute("value", @"<?xml version='1.0' encoding='ISO-8859-1'?>" + xmlRoot);
                HtmlPage.Window.Invoke("playSelection", "piano", xmlRoot);
            }
            else
            {
                throw new Exception("DOM Error: Could not play composition. Missing element.");
            }

        }

        public static void OnPausePlay(object obj)
        {
        }

        private static void FilterActionableNotes(Repository.DataService.Measure m)
        {
            ObservableCollection<Repository.DataService.Chord> chords =
                new ObservableCollection<Repository.DataService.Chord>(m.Chords.OrderBy(p => p.StartTime));

            foreach (Repository.DataService.Chord ch in chords)
            {
                if (ch.StartTime >= EditorState.ResumeStarttime)
                {
                    foreach (Repository.DataService.Note n in ch.Notes)
                    {
                        if (CollaborationManager.IsActive(n))
                        {
                            Cache.PlaybackNotes.Add(n);
                        }
                    }
                }
            }
        }

        public static void OnPlayMeasure(Repository.DataService.Measure m)
        {
            Cache.PlaybackNotes = new System.Collections.ObjectModel.ObservableCollection<Repository.DataService.Note>();
            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple)
            {
                FilterActionableNotes(m);
            }
            else
            {
                if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand)
                {
                    var s = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
                    var sg = (from a in Cache.Staffgroups where a.Id == s.Staffgroup_Id select a).First();
                    foreach (Repository.DataService.Staff _s in sg.Staffs)
                    {
                        foreach (Repository.DataService.Measure _m in _s.Measures)
                        {
                            if (m.Sequence == _m.Sequence)
                            {
                                FilterActionableNotes(_m);
                            }
                        }
                    }
                }
            }
            _ea.GetEvent<Play>().Publish(0);
        }

        public static void OnPlayComposition(_Enum.PlaybackInitiatedFrom location)
        {
            Cache.PlaybackNotes = new System.Collections.ObjectModel.ObservableCollection<Repository.DataService.Note>();
            foreach (Repository.DataService.Staffgroup sg in CompositionManager.Composition.Staffgroups)
            {
                foreach (Repository.DataService.Staff s in sg.Staffs)
                {
                    foreach (Repository.DataService.Measure m in s.Measures)
                    {
                        FilterActionableNotes(m);
                    }
                }
            }
            _ea.GetEvent<Play>().Publish(0);
            //when playback initiated from the hub, no composition has been selected and opened, so CompositionManager.Composition
            //contains nothing. But when a composition is played, it compiles the playback using CompositionManager.Composition. So,
            //to avoid having yet another PlayComposition method, we borrow CompositionManager.Composition and set it 
            //to whatever composition the user has selected in the Hub. Then when the playback is complete, we set it back to null.
            if (location == _Enum.PlaybackInitiatedFrom.Hub)
            {
                CompositionManager.Composition = null;
            }
        }
    }
}
