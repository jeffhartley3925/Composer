using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Composer.Entities;

namespace Composer.Server.Api
{
    public class NoteController : ApiController
    {
        private CDataEntities db = new CDataEntities();

        public NoteController()
        {
            db.ContextOptions.ProxyCreationEnabled = false;
        }
        // GET api/Note
        public IEnumerable<Note> GetNotes()
        {
            var notes = db.Notes.Include(p => p.Chord);
            return notes.AsEnumerable();
        }

        // GET api/Note/5
        public Note GetNote(Guid id)
        {
            Note note = db.Notes.Single(n => n.Id == id);
            if (note == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return note;
        }

        // PUT api/Note/5
        public HttpResponseMessage PutNote(Guid id, Note note)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != note.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Notes.Attach(note);
            db.ObjectStateManager.ChangeObjectState(note, System.Data.EntityState.Modified);

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

        // POST api/Note
        public HttpResponseMessage PostNote(Note note)
        {
            if (ModelState.IsValid)
            {
                db.Notes.AddObject(note);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, note);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = note.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Note/5
        public HttpResponseMessage DeleteNote(Guid id)
        {
            Note note = db.Notes.Single(n => n.Id == id);
            if (note == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Notes.DeleteObject(note);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, note);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}