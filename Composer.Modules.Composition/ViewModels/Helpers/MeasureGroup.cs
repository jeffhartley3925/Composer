using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Composer.Infrastructure.Events;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class MeasureGroup
    {
        private static readonly IEventAggregator Ea;

        static MeasureGroup()
        {
            Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {

        }

        public static void Resize(MeasureViewModel.MeasureWidthChangePayload payload)
        {
            IEnumerable<Measure> measureGroup = Utils.GetMeasuresBySequence(payload.MeasureId);
            if (payload.Sequence == null) return;
            Measure masterMeasure = Utils.GetMeasureWithMaxChordCountBySequence((int)payload.Sequence);
            payload.MeasureId = masterMeasure.Id;
            Ea.GetEvent<ResizeMeasure>().Publish(payload);
        }
    }
}
