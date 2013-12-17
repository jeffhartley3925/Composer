using System;
using System.Globalization;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using Composer.Repository;

using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
    public static class MeasureManager
    {
        private static DataServiceRepository<Repository.DataService.Composition> _repository;

        public static int CurrentDensity { get; set; }

        static MeasureManager()
        {
            CurrentDensity = Defaults.DefaultMeasureDensity;
        }

        public static bool IsEmpty(Repository.DataService.Measure m)
        {
            return m.Chords.Count == 0;
        }

        public static Repository.DataService.Measure Create(Guid pId, int seq)
        {
            var o = _repository.Create<Repository.DataService.Measure>();
            o.Id = Guid.NewGuid();
            o.Staff_Id = pId;
            o.Sequence = seq;
            o.Key_Id = Infrastructure.Dimensions.Keys.Key.Id;
            o.Bar_Id = Infrastructure.Dimensions.Bars.Bar.Id;
            o.Instrument_Id = Infrastructure.Dimensions.Instruments.Instrument.Id;
            o.Width = Preferences.MeasureWidth.ToString(CultureInfo.InvariantCulture);
            o.TimeSignature_Id = Infrastructure.Dimensions.TimeSignatures.TimeSignature.Id;
            o.Spacing = EditorState.ChordSpacing;
            o.LedgerColor = Preferences.NoteForeground;
            o.Audit = Common.GetAudit();
            o.Status = CollaborationManager.GetBaseStatus();
            Cache.AddMeasure(o);
            return o;
        }

        public static void Initialize()
        {
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
        }

        public static bool IsPacked(Repository.DataService.Measure m)
        {
            bool result = (Statistics.MeasureStatistics.Where(
                b => b.MeasureId == m.Id && b.CollaboratorIndex == 0).Select(b => b.IsPacked)).First();
            return result;
        }

        public static bool IsFull(Repository.DataService.Measure m)
        {
            bool result = (Statistics.MeasureStatistics.Where(
                b => b.MeasureId == m.Id && b.CollaboratorIndex == 0).Select(b => b.IsFull)).First();
            return result;
        }
    }
}
