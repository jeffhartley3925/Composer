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
    public class MeasureController : ApiController
    {
        private CDataEntities db = new CDataEntities();

        // GET api/Measure
        public IEnumerable<Measure> GetMeasures()
        {
            var measures = db.Measures.Include("Staff");
            return measures.AsEnumerable();
        }

        // GET api/Measure/5
        public Measure GetMeasure(Guid id)
        {
            var measure = db.Measures.Single(m => m.Id == id);
            if (measure == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return measure;
        }

        // PUT api/Measure/5
        public HttpResponseMessage PutMeasure(Guid id, Measure measure)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != measure.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Measures.Attach(measure);
            db.ObjectStateManager.ChangeObjectState(measure, System.Data.EntityState.Modified);

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

        // POST api/Measure
        public HttpResponseMessage PostMeasure(Measure measure)
        {
            if (ModelState.IsValid)
            {
                db.Measures.AddObject(measure);
                db.SaveChanges();

                var response = Request.CreateResponse(HttpStatusCode.Created, measure);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = measure.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Measure/5
        public HttpResponseMessage DeleteMeasure(Guid id)
        {
            var measure = db.Measures.Single(m => m.Id == id);
            if (measure == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Measures.DeleteObject(measure);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, measure);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}