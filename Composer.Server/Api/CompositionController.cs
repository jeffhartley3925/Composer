using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Composer.Entities;

namespace Composer.Server.Api
{
    public class CompositionController : ApiController
    {
        private CDataEntities db = new CDataEntities();

        // GET api/Composition
        public IEnumerable<Composition> GetCompositions()
        {
            return db.Compositions.AsEnumerable();
        }

        // GET api/Composition/5
        public Composition GetComposition(Guid id)
        {
            var composition = db.Compositions.Single(c => c.Id == id);
            if (composition == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return composition;
        }

        // PUT api/Composition/5
        public HttpResponseMessage PutComposition(Guid id, Composition composition)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != composition.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Compositions.Attach(composition);
            db.ObjectStateManager.ChangeObjectState(composition, System.Data.EntityState.Modified);

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

        // POST api/Composition
        public HttpResponseMessage PostComposition(Composition composition)
        {
            if (ModelState.IsValid)
            {
                db.Compositions.AddObject(composition);
                db.SaveChanges();

                var response = Request.CreateResponse(HttpStatusCode.Created, composition);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = composition.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Composition/5
        public HttpResponseMessage DeleteComposition(Guid id)
        {
            var composition = db.Compositions.Single(c => c.Id == id);
            if (composition == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Compositions.DeleteObject(composition);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, composition);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}