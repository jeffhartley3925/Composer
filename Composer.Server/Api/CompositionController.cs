﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Composer.Entities;

namespace Composer.Server.Api
{
    [RoutePrefix("api")]
    public class CompositionController : ApiController
    {
        private readonly CDataEntities db = new CDataEntities();

        public CompositionController()
        {
            db.ContextOptions.ProxyCreationEnabled = false;
        }

        [HttpGet]
        [Route("composition")]
        public IEnumerable<Composition> GetCompositions()
        {
            return db.Compositions.AsEnumerable();
        }

        [HttpGet]
        [Route("composition/id")]
        public Composition GetComposition(Guid id)
        {
            var composition = db.Compositions.Single(c => c.Id == id);
            if (composition == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return composition;
        }

        [HttpPut]
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
                db.Compositions.Add(composition);
                db.SaveChanges();

                var response = Request.CreateResponse(HttpStatusCode.Created, composition);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = composition.Id }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // DELETE api/Composition/5
        public IHttpActionResult DeleteComposition(Guid id)
        {
            var composition = db.Compositions.Single(c => c.Id == id);
            if (composition == null)
            {
                return NotFound();
            }

            db.Compositions.DeleteObject(composition);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return NotFound();
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}