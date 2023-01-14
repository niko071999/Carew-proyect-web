using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult ErrorAutorizacion() => View();
        public ActionResult Error404() => View();
    }
}