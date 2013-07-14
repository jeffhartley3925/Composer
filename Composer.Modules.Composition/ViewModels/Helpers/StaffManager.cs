using System;
using System.Linq;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class StaffManager
    {
        private static readonly DataServiceRepository<Repository.DataService.Composition> Repository;

        public static int CurrentDensity;

        static StaffManager()
        {
            Repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            SubscribeEvents();
        }

        public static bool IsEmpty(Repository.DataService.Staff staff)
        {
            var isEmpty = true;
            foreach (var measure in staff.Measures)
            {
                isEmpty = isEmpty && MeasureManager.IsEmpty(measure);
            }
            return isEmpty;
        }

        private static void SubscribeEvents()
        {
        }

        public static Repository.DataService.Staff Create(Guid parentId, int sequence)
        {
            var obj = Repository.Create<Repository.DataService.Staff>();
            obj.Id = Guid.NewGuid();
			obj.Key_Id = Infrastructure.Dimensions.Keys.Key.Id;
            obj.Staffgroup_Id = parentId;
            obj.Index = (short)Cache.Staffs.Count();
            obj.Sequence = sequence;
            obj.Clef_Id = Preferences.DefaultClefId;
            obj.TimeSignature_Id = Infrastructure.Dimensions.TimeSignatures.TimeSignature.Id;
            obj.Bar_Id = Infrastructure.Dimensions.Bars.Bar.Id;
            obj.Audit = Common.GetAudit();
            obj.Status = CollaborationManager.GetBaseStatus();
            Cache.AddStaff(obj);
            return obj;
        }

        public static Repository.DataService.Staff Clone(Guid parentId, Repository.DataService.Staff source)
        {
            Repository.DataService.Staff obj = Create(parentId, source.Sequence);
            obj.Sequence = source.Sequence;
            obj.Bar_Id = source.Bar_Id;
            obj.Clef_Id = source.Clef_Id;
            obj.TimeSignature_Id = source.TimeSignature_Id;
            obj.Status = CollaborationManager.GetBaseStatus();
            Cache.AddStaff(obj);
            return obj;
        }
    }
}
