using System;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public class DispositionChangeItem
    {
        public Guid SourceId { get; set; }
        public Guid ItemId { get; set; }
        public _Enum.Disposition Disposition { get; set; }

        public DispositionChangeItem(Guid itemId, Guid sourceId)
        {
            ItemId = itemId;
            SourceId = sourceId;
            Disposition = _Enum.Disposition.None;
        }

        public DispositionChangeItem(Guid itemId, Guid sourceId, _Enum.Disposition disposition)
        {
            ItemId = itemId;
            SourceId = sourceId;
            Disposition = disposition;
        }
    }
}
