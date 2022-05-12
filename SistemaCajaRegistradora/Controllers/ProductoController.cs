using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ModelData db = new ModelData();

        // GET: Producto
        [HttpGet]
        public ActionResult Listar()
        {
            return View();
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        public PartialViewResult AgregarForms()
        {
            var categorias = db.Categoria.ToList();
            var prioridades = db.Prioridad.ToList();
            ViewBag.categoriaId = new SelectList(categorias,"id","nombre");
            ViewBag.prioridadId = new SelectList(prioridades, "id", "prioridad1");

            return PartialView("_formsProducto");
        }

        [HttpPost]
        [ActionName("AgregarProducto")]
        public JsonResult AgregarProducto(Producto producto)
        {
            //Se define una imagen por defecto
            producto.rutaImg = "./../Assets/images/img-product-test.jpg";
            producto.fecha_creacion = DateTime.Now;
            int n = 0;
            //db.Producto.Add(producto);
            //n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }
    }
}