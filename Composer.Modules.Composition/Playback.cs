using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using Composer.Infrastructure;
using System.Xml.Linq;
using Composer.Infrastructure.Constants;
using System.Windows.Browser;
using System;

namespace Composer.Modules.Composition
{
	using System.Collections.Generic;

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

        private static void FilterActionableNotes(Repository.DataService.Measure mE)
        {
            var chords =
                new List<Repository.DataService.Chord>(mE.Chords.OrderBy(p => p.StartTime));

            foreach (var cH in chords)
            {
                if (cH.StartTime >= EditorState.ResumeStarttime)
                {
                    foreach (var nT in cH.Notes)
                    {
                        if (CollaborationManager.IsActive(nT))
                        {
                            Cache.PlaybackNotes.Add(nT);
                        }
                    }
                }
            }
        }

        public static void OnPlayMeasure(Repository.DataService.Measure m)
        {
            Cache.PlaybackNotes = new List<Repository.DataService.Note>();
            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple)
            {
                FilterActionableNotes(m);
            }
            else
            {
                if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand)
                {
                    var mStaff = Utils.GetStaff(m.Staff_Id);
                    var mStaffgroup = Utils.GetStaffgroup(mStaff.Staffgroup_Id);
                    foreach (var staff in mStaffgroup.Staffs)
                    {
                        foreach (var measure in staff.Measures.Where(measure => m.Sequence == measure.Sequence))
                        {
                            FilterActionableNotes(measure);
                        }
                    }
                }
            }
            _ea.GetEvent<Play>().Publish(0);
        }

        public static void OnPlayComposition(_Enum.PlaybackInitiatedFrom initiatedFrom)
        {
            Cache.PlaybackNotes = new List<Repository.DataService.Note>();
            foreach (var mStaffgroup in CompositionManager.Composition.Staffgroups.SelectMany(cStaffgroup => cStaffgroup.Staffs.SelectMany(staff => staff.Measures)))
            {
                FilterActionableNotes(mStaffgroup);
            }
            _ea.GetEvent<Play>().Publish(0);
			
            // when playback initiated from the hub, there is no compoaition object, so CompositionManager.Composition object
            // is null. But when a composition is played, it compiles the playback using methods in CompositionManager that expect the Composition to be not null. So,
            // to reuse code, we memntarily set CompositionManager.Composition to the selected hub composition. Then when the playback is complete, we set it back to null.
            
			if (initiatedFrom == _Enum.PlaybackInitiatedFrom.Hub)
            {
                CompositionManager.Composition = null;
            }
        }
    }
}
