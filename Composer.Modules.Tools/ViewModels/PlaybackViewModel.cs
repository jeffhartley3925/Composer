using System;
using Composer.Modules.Palettes.Services;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using System.Linq;
using Composer.Infrastructure.Constants;
using System.Windows.Browser;
using System.Xml.Linq;

namespace Composer.Modules.Palettes.ViewModels
{
    public class PlaybackViewModel : BasePaletteViewModel, IPlaybackViewModel
    {
        private readonly PlaybackControlsService _service;

        public PlaybackViewModel()
        {
            _service = (PlaybackControlsService)Unity.Container.Resolve(typeof(IPlaybackControlsService), "PlaybackControlsService");
            Items = _service.PaletteItems;
            Caption = Items[0].PaletteCaption;
            Id = Items[0].PaletteId;
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            Ea.GetEvent<Play>().Unsubscribe(OnPlay);
            Ea.GetEvent<Play>().Subscribe(OnPlay, true);
        }

        public void OnPlay(object obj)
        {
            EditorState.PaletteId = Id;
            //var xmlRoot =
            //    new XElement("root",
            //        from b in Cache.PlaybackNotes where b.Pitch != Defaults.RestSymbol  select
            //            new XElement("row",
            //                new XAttribute("instrument","piano"),
            //                new XAttribute("pitch",b.Pitch),
            //                new XAttribute("duration",b.Duration),
            //                new XAttribute("starttime",b.StartTime),
            //                new XAttribute("status",b.Status)
            //        )
            //    );

            //var htmlDoc = HtmlPage.Document;
            //var htmlEl = htmlDoc.GetElementById("playbackXml");
            //if (htmlEl != null)
            //{
            //    htmlEl.SetAttribute("value", @"<?xml version='1.0' encoding='ISO-8859-1'?>" + xmlRoot);
            //    HtmlPage.Window.Invoke("playSelection", "piano", xmlRoot);
            //}
            //else
            //{
            //    throw new Exception("DOM Error: Could not play composition. Missing element.");
            //}

        }
    }
}
