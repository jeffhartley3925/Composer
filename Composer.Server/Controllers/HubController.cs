using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Composer.Server.Controllers
{
    public class HubController : Controller
    {
        //
        // GET: /Hub/

        public ActionResult HubUi()
        {
            return View();
        }

    }
}
