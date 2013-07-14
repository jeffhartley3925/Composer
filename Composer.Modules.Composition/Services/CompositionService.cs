using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using Microsoft.Practices.ServiceLocation;
using Composer.Repository.DataService;
using Composer.Modules.Composition.EventArgs;
using Composer.Repository;
using Composer.Modules.Composition.ViewModels.Helpers;
namespace Composer.Modules.Composition.Service
{
    public class CompositionService : ICompositionService
    {
        public string CompositionId { get; set; }

        private CDataEntities context;
        private CDataEntities Context
        {
            get
            {
                if (context == null)
                {
                    context = ServiceLocator.Current.GetInstance<CDataEntities>();
                }
                return context;
            }
        }
        #region ICompositionService Members

        public Repository.DataService.Composition Composition { get; set; }
        public CompositionService()
        {

        }
        public void GetCompositionAsync()
        {
            DataServiceRepository<Composer.Repository.DataService.Composition> repository =
            new DataServiceRepository<Composer.Repository.DataService.Composition>();
            Guid guid;
            Guid.TryParse(this.CompositionId, out guid);

            DataServiceQuery<Composer.Repository.DataService.Composition> qry =
                repository.RetrieveAll<Composer.Repository.DataService.Composition>().Where(c => c.Id == guid)
                as DataServiceQuery<Composer.Repository.DataService.Composition>;

            qry = qry.Expand(composition =>
                composition.Staffgroups.SubExpand(staffgroup =>
                    staffgroup.Staffs.SubExpand(staff =>
                        staff.Measures).SubExpand(measure =>
                            measure.Chords).SubExpand(chord =>
                                chord.Notes)));

            qry = qry.Expand(composition =>
                composition.Verses);

            qry = qry.Expand(composition =>
                composition.Collaborations);

            qry = qry.Expand(composition =>
                  composition.Arcs);

            qry.BeginExecute(new AsyncCallback(a =>
            {
                try
                {
                    IEnumerable<Composer.Repository.DataService.Composition> results = qry.EndExecute(a);

                    if (CompositionLoadingComplete != null)
                    {
                        CompositionLoadingComplete(this, new CompositionLoadingEventArgs(results));
                    }
                }
                catch (Exception ex)
                {
                    if (CompositionLoadingError != null)
                    {
                        CompositionLoadingError(this, new CompositionErrorEventArgs(ex));
                    }
                }
            }), null);
        }
        public event EventHandler<CompositionLoadingEventArgs> CompositionLoadingComplete;
        public event EventHandler<CompositionErrorEventArgs> CompositionLoadingError;

        #endregion
    }
}
