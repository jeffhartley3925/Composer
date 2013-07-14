using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.ServiceModel.Web;
using System.Web;
using Composer.Entities;
using System.Data;
using System.Data.Objects;

namespace Composer.Server
{
    public class DataService : DataService<CDataEntities>
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            // Examples:
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.UseVerboseErrors = true;
        }

        protected override CDataEntities CreateDataSource()
        {
            var context = new CDataEntities();
            context.ContextOptions.LazyLoadingEnabled = true;
            context.ContextOptions.ProxyCreationEnabled = false;
            context.SavingChanges += new EventHandler(dataSource_Validate);
            return context;
        }

        void dataSource_Validate(object sender, EventArgs e)
        {
            string PreferredNoteForeground = "#000000";
            var context = sender as ObjectContext;
            foreach (ObjectStateEntry entry in context.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
            {
                if (!entry.IsRelationship && (entry.Entity.GetType() == typeof(Composer.Entities.Composition)))
                {
                    Composer.Entities.Composition composition = entry.Entity as Composer.Entities.Composition;

                    if (composition.Provenance.TitleLine == "Discard") //TODO: re-purposed property. do we need to do this?
                        composition = null;
                }
                if (!entry.IsRelationship && (entry.Entity.GetType() == typeof(Composer.Entities.Note)))
                {
                    Composer.Entities.Note note = entry.Entity as Composer.Entities.Note;

                    if (note.Foreground == null)
                        note.Foreground = PreferredNoteForeground;
                }

                if (!entry.IsRelationship && (entry.Entity.GetType() == typeof(Composer.Entities.Verse)))
                {
                    Composer.Entities.Verse verse = entry.Entity as Composer.Entities.Verse;

                    if (verse.UIHelper != null)
                        verse.UIHelper = null;
                }
            }
        }

        [WebGet]
        public IQueryable<Composer.Entities.Composition> HubCompositions(string audit_author_id, string friendIds, string id)
        {
            if (id.Length == 0)
            {
                int SAMPLE_COMPOSITION_FLAG_INDEX = 0;
                string IS_SET = "1";
                List<string> f = friendIds.Split(',').ToList();
                return from c in this.CurrentDataSource.Compositions
                       where
                           c.Audit.Author_Id == audit_author_id
                           || f.Contains(c.Audit.Author_Id)
                           || c.Flags.Substring(SAMPLE_COMPOSITION_FLAG_INDEX, 1) == IS_SET
                       select c;
            }
            else
            {
                Guid guid = Guid.Parse(id);
                return from c in this.CurrentDataSource.Compositions
                       where
                           c.Id == guid
                       select c;
            }
        }
    }
}
