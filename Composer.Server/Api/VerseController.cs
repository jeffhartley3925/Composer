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
    public class VerseController : ApiController
    {
        private CDataEntities db = new CDataEntities();

        public VerseController()
        {
            db.ContextOptions.ProxyCreationEnabled = false;
        }

        // GET api/Verse
        public IEnumerable<Verse> GetVerses()
        {
            var verses = db.Verses.Include("Composition");
            return verses.AsEnumerable();
        }

        // GET api/Verse/5
        public Verse GetVerse(Guid id)
        {
            var verse = db.Verses.Single(v => v.Id == id);
            if (verse == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return verse;
        }

        // PUT api/Verse/5
        public HttpResponseMessage PutVerse(Guid id, Verse verse)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != verse.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Verses.Attach(verse);
            db.ObjectStateManager.ChangeObjectState(verse, System.Data.EntityState.Modified);

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

        // POST api/Verse
        public HttpResponseMessage PostVerse(Verse verse)
        {
            if (ModelState.IsValid)
            {
                db.Verses.Add(verse);
                db.SaveChanges();

                var response = Request.CreateResponse(HttpStatusCode.Created, verse);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = verse.Id }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // DELETE api/Verse/5
        public HttpResponseMessage DeleteVerse(Guid id)
        {
            var verse = db.Verses.Single(v => v.Id == id);
            if (verse == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Verses.(verse);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, verse);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}