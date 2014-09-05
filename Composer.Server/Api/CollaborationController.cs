using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Composer.Entities;

namespace Composer.Server.Api
{
    public class CollaborationController : ApiController
    {
        private CDataEntities db = new CDataEntities();

        public CollaborationController()
        {
            db.ContextOptions.ProxyCreationEnabled = false;
        }

        // GET api/Collaboration
        public IEnumerable<Collaboration> GetCollaborations()
        {
            var collaborations = db.Collaborations.Include("Composition");
            return collaborations.AsEnumerable();
        }

        // GET api/Collaboration/5
        public Collaboration GetCollaboration(Guid id)
        {
            var collaboration = db.Collaborations.Single(c => c.Id == id);
            if (collaboration == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return collaboration;
        }

        // PUT api/Collaboration/5
        public HttpResponseMessage PutCollaboration(Guid id, Collaboration collaboration)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != collaboration.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Collaborations.Attach(collaboration);
            db.ObjectStateManager.ChangeObjectState(collaboration, System.Data.EntityState.Modified);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Collaboration
        public HttpResponseMessage PostCollaboration(Collaboration collaboration)
        {
            if (ModelState.IsValid)
            {
                db.Collaborations.Add(collaboration);
                db.SaveChanges();

                var response = Request.CreateResponse(HttpStatusCode.Created, collaboration);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = collaboration.Id }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // DELETE api/Collaboration/5
        public HttpResponseMessage DeleteCollaboration(Guid id)
        {
            var collaboration = db.Collaborations.Single(c => c.Id == id);
            if (collaboration == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Collaborations.DeleteObject(collaboration);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, collaboration);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}