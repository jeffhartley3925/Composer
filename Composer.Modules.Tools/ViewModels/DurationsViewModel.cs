using System.Linq;
using Composer.Infrastructure;
using Composer.Modules.Palettes.Services;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Support;

namespace Composer.Modules.Palettes.ViewModels
{
    public class DurationsViewModel : BasePaletteViewModel, IDurationsViewModel
    {
        private readonly DurationsService _service;
        public DurationsViewModel()
        {
            _service = (DurationsService)Unity.Container.Resolve(typeof(IDurationsService), "DurationsService");
            Items = _service.PaletteItems;
            Caption = Items[0].PaletteCaption;
            Id = Items[0].PaletteId;
            Hide(new object());
            SubscribeEvents();
            Margin = Infrastructure.Constants.Palette.DefaultPaletteMargin;
        }
        private void SubscribeEvents()
        {
            Ea.GetEvent<DurationPaletteClicked>().Unsubscribe(OnButtonClicked);
            Ea.GetEvent<DurationPaletteClicked>().Subscribe(OnButtonClicked, true);
        }
        public void OnButtonClicked(string target)
        {
            string Class = _Enum.VectorClass.None.ToString();

            EditorState.PaletteId = Id;
            string[] paramaters = target.Split(',');
            string vectorName = paramaters[1];
            string vectorClass = paramaters[0];
            switch (vectorClass)
            {
                case "Rest":
                    Class = _Enum.VectorClass.Rest.ToString();
                    EditorState.SetContext(vectorName, vectorClass, Class);
                    break;
                case "Note":
                    Class = _Enum.VectorClass.Note.ToString();
                    EditorState.SetContext(vectorName, vectorClass, Class);
                    break;
                case "Dot":
                    Class = _Enum.VectorClass.Dot.ToString();
                    EditorState.Dotted = (EditorState.Dotted == null) || (!((bool)EditorState.Dotted));
                    break;
                case "Accidental":
                    Class = _Enum.VectorClass.Accidental.ToString();
                    EditorState.Accidental = (EditorState.Accidental != vectorName) ? vectorName : null;
                    break;
            }

            if (Class != _Enum.VectorClass.Dot.ToString())
            {
                var vector = (from a in Vectors.VectorList
                              where (a.Class == EditorState.DurationType &&
                                  a.Name == EditorState.DurationCaption)
                              select a).FirstOrDefault();

                if (vector != null)
                {
                    EditorState.VectorId = vector.Id;
                }
                else
                {
					//TODO:Add checking for a = 0, a = 1  and a > 1, with approriate error level and message.
					//ie: if a > 1, a first vector is used and a warning given; whereas if a == 0 an error is thrown
                    var a = (
                                from b in Vectors.VectorList
                                where b.Class == Class && b.Name == vectorName
                                select b.Id
                            );

                    if (a.Any())
                    {
                        EditorState.VectorId = a.FirstOrDefault();
                    }
                }
            }
        }
    }
}
