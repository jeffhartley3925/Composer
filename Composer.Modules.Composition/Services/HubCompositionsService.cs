using Composer.Modules.Composition.EventArgs;
using Composer.Repository;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;

namespace Composer.Modules.Composition.Service
{
    public class HubCompositionsService : IHubCompositionsService
    {
        public void GetHubCompositionsAsync()
        {
            DataServiceRepository<Composer.Repository.DataService.Composition> repository =
            new DataServiceRepository<Composer.Repository.DataService.Composition>();

            DataServiceQuery<Composer.Repository.DataService.Composition> qry =
                repository.RetrieveAll<Composer.Repository.DataService.Composition>()
                as DataServiceQuery<Composer.Repository.DataService.Composition>;

            qry = qry.Expand(composition =>
                composition.Verses);

            qry = qry.Expand(composition =>
                composition.Collaborations);

            qry.BeginExecute(new AsyncCallback(a =>
            {
                try
                {
                    IEnumerable<Composer.Repository.DataService.Composition> results = qry.EndExecute(a);
                    if (HubCompositionsLoadingComplete != null)
                    {
                        HubCompositionsLoadingComplete(this, new HubCompositionsLoadingEventArgs(results));
                    }
                }
                catch (Exception ex)
                {
                    if (HubCompositionsLoadingError != null)
                    {
                        HubCompositionsLoadingError(this, new HubCompositionsErrorEventArgs(ex));
                    }
                }
            }), null);
        }

        public event EventHandler<HubCompositionsLoadingEventArgs> HubCompositionsLoadingComplete;

        public event EventHandler<HubCompositionsErrorEventArgs> HubCompositionsLoadingError;
    }
 }
