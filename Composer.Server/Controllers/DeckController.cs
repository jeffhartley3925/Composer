using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Composer.Server.Controllers
{
    public class DeckController : Controller
    {
        public ActionResult Card(string id)
        {
            return View();
        }
    }
}
