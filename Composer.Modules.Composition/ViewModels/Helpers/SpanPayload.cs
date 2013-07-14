using System.Collections.ObjectModel;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    public class SpanPayload
    {
        public Repository.DataService.Measure Measure { get; set; }

        public ObservableCollection<LocalSpan> LocalSpans { get; set; }

        public SpanPayload(Repository.DataService.Measure measure, ObservableCollection<LocalSpan> localSpans)
        {
            Measure = measure;
            LocalSpans = localSpans;
        }
    }
}
