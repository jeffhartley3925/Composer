using System;
using System.Linq;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class StaffgroupManager
    {
        private static readonly DataServiceRepository<Repository.DataService.Composition> Repository;

        public static int CurrentDensity = Defaults.DefaultStaffgroupDensity;

        static StaffgroupManager()
        {
            Repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
        }

        public static bool IsEmpty(Repository.DataService.Staffgroup staffgroup)
        {
            var isEmpty = true;
            foreach (var staff in staffgroup.Staffs)
            {
                isEmpty = isEmpty && StaffManager.IsEmpty(staff);
            }
            return isEmpty;
        }

        public static Repository.DataService.Staffgroup Create(Guid parentId, int sequence)
        {
            var obj = Repository.Create<Repository.DataService.Staffgroup>();
            obj.Id = Guid.NewGuid();
			obj.Key_Id = Infrastructure.Dimensions.Keys.Key.Id;
            obj.Sequence = sequence;
            obj.Index = (short)Cache.Staffgroups.Count();
            obj.Composition_Id = parentId;
            obj.Audit = Common.GetAudit();
            obj.Status = CollaborationManager.GetBaseStatus();
            Cache.AddStaffgroup(obj);
            return obj;
        }
        public static Repository.DataService.Staffgroup Clone(Guid parentId, Repository.DataService.Staffgroup source)
        {
            Repository.DataService.Staffgroup obj = Create(parentId, source.Sequence);
            obj.Sequence = source.Sequence;
            obj.Status = CollaborationManager.GetBaseStatus();
            Cache.AddStaffgroup(obj);
            return obj;
        }

        public static ObservableCollection<Repository.DataService.Chord> GetAllChordsInStaffgroupByMeasureSequence(Repository.DataService.Measure m)
        {
            var chords = new ObservableCollection<Repository.DataService.Chord>();
            try
            {
                var mStaff = Utils.GetStaff(m.Staff_Id);
                var mStaffgroup = Utils.GetStaffgroup(mStaff.Staffgroup_Id);

                foreach (var staff in mStaffgroup.Staffs)
                {
                    foreach (var measure in staff.Measures)
                    {
                        if (measure.Sequence != m.Sequence) continue;
                        foreach (var ch in measure.Chords)
                        {
                            if (!CollaborationManager.IsActive(ch, null)) continue;
                            var a = (from b in chords where ch.StartTime == b.StartTime select b);
                            var e = a as List<Repository.DataService.Chord> ?? a.ToList();
                            if (!e.Any())
                            {
                                chords.Add(ch);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "Exception in getAllChordsInStaffgroupByMeasureSequence");
            }
            return chords;
        }
    }
}
