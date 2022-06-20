using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers
{
    public class VentaController : Controller
    {
        // GET: Venta
        public ActionResult Listar()
        {
            return View();
        }
    }
}