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

        public static ObservableCollection<Repository.DataService.Chord> getAllChordsInStaffgroupByMeasureSequence(Repository.DataService.Measure measure)
        {
            ObservableCollection<Repository.DataService.Chord> chords = new ObservableCollection<Repository.DataService.Chord>();
            try
            {
                var pars = (from a in Cache.Staffs where a.Id == measure.Staff_Id select a).First();
                var parsg = (from a in Cache.Staffgroups where a.Id == pars.Staffgroup_Id select a).First();
                foreach (var s in parsg.Staffs)
                {
                    foreach (var m in s.Measures)
                    {
                        if (m.Sequence == measure.Sequence)
                        {
                            foreach (var c in m.Chords)
                            {
                                if (CollaborationManager.IsActive(c))
                                {
                                    var a = (from b in chords where c.StartTime == b.StartTime select b);
                                    var e = a as List<Repository.DataService.Chord> ?? a.ToList();
                                    if (!e.Any())
                                    {
                                        chords.Add(c);
                                    }
                                }
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
