using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SistemaCajaRegistradora.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ModelData db = new ModelData();

        // GET: Producto
        [HttpGet]
        [ActionName("Listar")]
        public ActionResult Listar()
        {
            var producto = db.Productos.Include(p => p.Categoria).Include(p => p.Prioridade);  
            return View(producto.ToList());
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        public PartialViewResult AgregarForms()
        {
            var categorias = db.Categorias.ToList();
            var prioridades = db.Prioridades.ToList();
            ViewBag.categoriaId = new SelectList(categorias,"id","nombre");
            ViewBag.prioridadId = new SelectList(prioridades, "id", "prioridad");

            return PartialView("_formsProducto");
        }

        [HttpPost]
        [ActionName("AgregarProducto")]
        public JsonResult AgregarProducto(Producto producto)
        {
            validarValoresNull(producto);

            //Se define una imagen por defecto
            producto.rutaImg = "./../Assets/images/productos/default-product-image.png";
            producto.fecha_creacion = DateTime.Now;
            db.Productos.Add(producto);
            int n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEditar")]
        public PartialViewResult formsEditar(int? id)
        {   
            var producto = db.Productos.Include(p => p.Categoria)
                .Include(p => p.Prioridade).Where(p => p.id == id).FirstOrDefault();
            producto.codigo_barra = producto.codigo_barra.Trim();
            producto.nombre = producto.nombre.Trim();
            var categorias = db.Categorias.ToList();
            var prioridades = db.Prioridades.ToList();
            ViewBag.categoriaId = new SelectList(categorias, "id", "nombre",producto.categoriaid);
            ViewBag.prioridadId = new SelectList(prioridades, "id", "prioridad",producto.prioridadid);

            return PartialView("_formsProducto",producto);
        }

        [HttpPost]
        [ActionName("editarProducto")]
        public JsonResult editarProducto(Producto producto)
        {
            validarValoresNull(producto);
            db.Entry(producto).State = EntityState.Modified;
            int n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsImagen")]
        public PartialViewResult formsImagen(int? id)
        {
            Session["idProducto"] = id;
            return PartialView("_formsImagen");
        }

        [HttpGet]
        [ActionName("formsEliminar")]
        public PartialViewResult formsEliminar (int? id)
        {
            var producto = db.Productos.Include(p=>p.Categoria)
                .Include(p=>p.Prioridade).Where(p=>p.id == id).FirstOrDefault();
            producto.nombre.Trim();
            return PartialView("_formsEliminar",producto);
        }

        [HttpPost]
        [ActionName("eliminarProducto")]
        public JsonResult eliminarProducto(int? id)
        {
            int n = 0;

            var producto = db.Productos.Find(id);
            try
            {
                db.Productos.Remove(producto);
                n = db.SaveChanges();
            }
            catch (Exception)
            {

            }
            return Json(producto, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("subirImagen")]
        public JsonResult subirImagen(HttpPostedFileBase archivo)
        {
            try
            {
                if(archivo != null)
                {
                    string path = Server.MapPath("~/Assets/images/productos/");
                    if (!System.IO.Directory.Exists(path)){
                        System.IO.Directory.CreateDirectory(path);
                    };
                    archivo.SaveAs(path + System.IO.Path.GetFileName(archivo.FileName));
                    
                    int idProducto = (int)Session["idProducto"];
                    var producto = db.Productos.Find(idProducto);
                    producto.rutaImg = "./../Assets/images/productos/" + archivo.FileName;
                    db.Entry(producto).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(
                    new { mensaje = "Archivo subido correctamente" }, JsonRequestBehavior.AllowGet); ;
                }
                else
                {
                    return Json(
                    new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
                }
            }catch (Exception){
                return Json(
                    new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ActionName("addExistencias")]
        public JsonResult addExistencias(Producto producto)
        {
            if (producto != null)
            {
                string codigo = producto.codigo_barra;
                var prod = db.Productos.Where(p => p.codigo_barra.Equals(codigo)).FirstOrDefault();
                if (prod != null)
                {
                    prod.stock++;
                    return Json(new
                    {
                        id = prod.id,
                        codigobarra = prod.codigo_barra.Trim(),
                        nombre = prod.nombre.Trim(),
                        newstock = prod.stock,
                        oldStock = prod.stock - 1,
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        codigobarra = "",
                        nombre = "",
                        stock = "",
                        increment = "",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [HttpPost]
        [ActionName("aplicarExistencias")]
        public JsonResult aplicarExistencias(Producto producto)
        {
            int n = 0;
            var resulProducto = db.Productos.Include(p => p.Categoria).Include(p => p.Prioridade)
                .Where(p => p.codigo_barra == producto.codigo_barra).FirstOrDefault();
            if (resulProducto!=null)
            {
                resulProducto.stock = producto.stock;
                db.Entry(resulProducto).State = EntityState.Modified;
                n = db.SaveChanges();
                return Json(n,JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(n, JsonRequestBehavior.AllowGet); ;
            }
        }

        private void validarValoresNull(Producto producto)
        {
            if (producto.precio == null)
            {
                producto.precio = 0;
            }
            if (producto.stock == null)
            {
                producto.stock = 0;
            }
            if (producto.stockmin == null)
            {
                producto.stockmin = 0;
            }
            if (producto.stockmax == null)
            {
                producto.stockmax = 0;
            }
        }


    }
}