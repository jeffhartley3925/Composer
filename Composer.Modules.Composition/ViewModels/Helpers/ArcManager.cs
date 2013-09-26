using Composer.Infrastructure;
using System;
using System.Linq;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
	public static class ArcManager
	{
		private static readonly DataServiceRepository<Repository.DataService.Composition> repository;

	    public static Guid SelectedArcId { get; set; }

	    public static Arc Arc { get; set; }

	    static ArcManager()
		{
			if (repository == null)
				repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
			SubscribeEvents();
		}

		private static void SubscribeEvents()
		{

		}

        public static Arc Create(Guid parentId, Guid staffId, _Enum.ArcType type)
		{
            var obj = repository.Create<Arc>();
			obj.Id = Guid.NewGuid();
			obj.Note_Id1 = Infrastructure.Support.Selection.Notes[0].Id;
			obj.Note_Id2 = Infrastructure.Support.Selection.Notes[1].Id;
			obj.Chord_Id1 = (from a in Cache.Chords where a.Id == Infrastructure.Support.Selection.Notes[0].Chord_Id select a.Id).SingleOrDefault();
			obj.Chord_Id2 = (from a in Cache.Chords where a.Id == Infrastructure.Support.Selection.Notes[1].Chord_Id select a.Id).SingleOrDefault();
			obj.Audit.CreateDate = DateTime.Now;
			obj.Audit.ModifyDate = DateTime.Now;
			obj.Audit.Author_Id = Current.User.Id;
            obj.ArcSweep = "";
            obj.FlareSweep = "";
            obj.Top = 0;
            obj.Staff_Id = staffId;
			obj.Composition_Id = parentId;
			obj.Type = (short)type;
            obj.Status = CollaborationManager.GetBaseStatus();
			return obj;
		}

        public static Arc Create(Guid parentId, _Enum.ArcType type, Guid note_Id1, Guid note_Id2, Guid chord_Id1, Guid chord_Id2)
        {
            var obj = repository.Create<Arc>();
            obj.Id = Guid.NewGuid();
            obj.Note_Id1 = note_Id1;
            obj.Note_Id2 = note_Id2;
            obj.Chord_Id1 = chord_Id1;
            obj.Chord_Id2 = chord_Id2;
            obj.Audit.CreateDate = DateTime.Now;
            obj.Audit.ModifyDate = DateTime.Now;
            obj.Audit.Author_Id = Current.User.Id;
            obj.ArcSweep = "";
            obj.FlareSweep = "";
            obj.Top = 0;
            obj.Composition_Id = parentId;
            obj.Type = (short)type;
            obj.Status = CollaborationManager.GetBaseStatus();
            return obj;
        }

		public static Arc Clone(Arc source)
		{
            var obj = Create(source.Id, (_Enum.ArcType)source.Type, source.Note_Id1, source.Note_Id2, source.Chord_Id1, source.Chord_Id2);
            obj.ArcSweep = source.ArcSweep;
            obj.Composition_Id = source.Id;
            obj.Id = Guid.NewGuid();
            obj.FlareSweep = source.FlareSweep;
            obj.Top = source.Top + ((EditorState.IsPrinting) ? Defaults.PrintingHeight : 0);
            obj.Type = source.Type;
            obj.Staff_Id = source.Staff_Id;
			return obj;
		}

		public static void Delete()
		{
		}

		public static bool Validate(_Enum.ArcType type)
		{
			bool result = false;
			switch (type)
			{
				case _Enum.ArcType.Slur:
					result = Infrastructure.Support.Selection.Notes[0].StartTime != Infrastructure.Support.Selection.Notes[1].StartTime;
					break;
				case _Enum.ArcType.Tie:
					result = Infrastructure.Support.Selection.Notes[0].StartTime != Infrastructure.Support.Selection.Notes[1].StartTime &&
							Infrastructure.Support.Selection.Notes[0].Pitch == Infrastructure.Support.Selection.Notes[1].Pitch &&
							AreContiguous();
					break;
			}
			return result;
		}

		public static bool AreContiguous()
		{
			Infrastructure.Support.Selection.Notes.Sort(
			    (x, y) => x.StartTime.Value.CompareTo(y.StartTime.Value));

			return Infrastructure.Support.Selection.Notes[0].StartTime + 
                (double)Infrastructure.Support.Selection.Notes[0].Duration == Infrastructure.Support.Selection.Notes[1].StartTime;
		}
	}
}