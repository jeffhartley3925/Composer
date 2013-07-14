using System;
using System.Collections.ObjectModel;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public static class VerseManager
    {
        public static Repository.DataService.Measure Measure;
        public static decimal[] ChordStartTimes;
        public static Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;

        private static readonly DataServiceRepository<Repository.DataService.Composition> Repository;

        public static ObservableCollection<Word> Words;

        static VerseManager()
        {
            Repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            Words = new ObservableCollection<Word>();
            SubscribeEvents();
        }

        public static Repository.DataService.Verse Create(Guid parentId, int sequence)
        {
            var obj = Repository.Create<Repository.DataService.Verse>();
            obj.Id = Guid.NewGuid();
            obj.Composition_Id = parentId;
            obj.Sequence = sequence;
            obj.Index = (short)((sequence / Defaults.SequenceIncrement) + 1);
            obj.Text = string.Empty;
            obj.Disposition = 1;
            obj.UIHelper = null;
            obj.Status = CollaborationManager.GetBaseStatus();
            obj.Audit.CreateDate = DateTime.Now;
            obj.Audit.ModifyDate = DateTime.Now;
            obj.Audit.Author_Id = Current.User.Id;
            obj.Status = CollaborationManager.GetBaseStatus();
            return obj;
        }

        public static Repository.DataService.Verse Clone(Guid parentId, Repository.DataService.Verse source)
        {
            Repository.DataService.Verse obj = Create(parentId, source.Sequence);
            obj.Sequence = source.Sequence;
            obj.Index = source.Index;
            obj.Text = source.Text;
            obj.Status = CollaborationManager.GetBaseStatus();
            return obj;
        }

        private static void SubscribeEvents()
        {

        }
    }
}
