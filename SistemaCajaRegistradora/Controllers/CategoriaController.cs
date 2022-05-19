using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        public ActionResult Listar()
        {
            var categorias = db.Categorias;
            return View(categorias.ToList());
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        public PartialViewResult AgregarForms()
        {
            return PartialView("_formsCategoria");
        }
        [HttpPost]
        [ActionName("AgregarCategoria")]
        public JsonResult AgregarCategoria(Categoria categoria)
        {
            db.Categorias.Add(categoria);
            int n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }
    }
}