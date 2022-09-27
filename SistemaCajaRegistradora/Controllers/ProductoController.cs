using SistemaCajaRegistradora.Filters;
using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        [ActionName("Listar")]
        [Autorizacion(idoperacion:1)]
        public ActionResult Listar()
        {
            ViewData["ProductosLength"] = db.Productos.Include(p => p.Categoria).Include(p => p.Prioridade).ToArray().Length;
            return View();
        }

        [HttpGet]
        [ActionName("getProductos")]
        public JsonResult getProductos()
        {
            db.Configuration.LazyLoadingEnabled = false;
            var result = db.Productos.Include(p => p.Categoria).Include(p => p.Prioridade).ToArray();

            List<vmProducto> productos = new List<vmProducto>();

            foreach (var item in result)
            {
                vmProducto producto = new vmProducto();
                producto.id = item.id;
                producto.codigobarra = item.codigo_barra.Trim();
                producto.nombre = item.nombre.Trim();
                producto.categoria = item.Categoria.nombre.Trim();
                producto.prioridad = item.Prioridade.prioridad.Trim();
                producto.rutaimg = item.rutaImg.Trim();
                producto.precio = (int)item.precio;
                producto.stock = (int)item.stock;
                producto.stockmin = (int)item.stockmin;
                producto.stockmax = (int)item.stockmax;
                producto.fechacreacion = item.fecha_creacion;

                productos.Add(producto);
            }

            return Json(new
            {
                data = productos
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        [Autorizacion(idoperacion: 3)]
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
        [Autorizacion(idoperacion: 3)]
        public JsonResult AgregarProducto(Producto producto)
        {
            validarValoresNull(producto);
            int n = 0;
            //Se define una imagen por defecto
            producto.rutaImg = "./../Assets/images/productos/default-product-image.png";
            producto.fecha_creacion = DateTime.UtcNow;
            db.Productos.Add(producto);
            n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEditar")]
        [Autorizacion(idoperacion: 4)]
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
        [Autorizacion(idoperacion: 4)]
        public JsonResult editarProducto(Producto producto)
        {
            validarValoresNull(producto);
            db.Entry(producto).State = EntityState.Modified;
            int n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsImagen")]
        [Autorizacion(idoperacion: 6)]
        public PartialViewResult formsImagen(int? id)
        {
            Session["idProducto"] = id;
            var producto = db.Productos.Where(p=>p.id==id).FirstOrDefault();
            return PartialView("_formsImagen", producto);
        }

        [HttpPost]
        [ActionName("subirImagen")]
        [Autorizacion(idoperacion: 6)]
        public JsonResult subirImagen(HttpPostedFileBase archivo)
        {
            try
            {
                if (archivo != null)
                {
                    string path = Server.MapPath("~/Assets/images/productos/");
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    };
                    archivo.SaveAs(path + System.IO.Path.GetFileName(archivo.FileName));

                    int idProducto = (int)Session["idProducto"];
                    var producto = db.Productos.Find(idProducto);
                    producto.rutaImg = "./../Assets/images/productos/" + archivo.FileName;
                    db.Entry(producto).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(
                    new { mensaje = "Imagen subida correctamente" }, JsonRequestBehavior.AllowGet); ;
                }
                else
                {
                    return Json(
                    new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(
                    new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [ActionName("formsEliminar")]
        [Autorizacion(idoperacion: 5)]
        public PartialViewResult formsEliminar (int? id)
        {
            var producto = db.Productos.Include(p=>p.Categoria)
                .Include(p=>p.Prioridade).Where(p=>p.id == id).FirstOrDefault();
            producto.nombre.Trim();
            return PartialView("_formsEliminar",producto);
        }

        [HttpPost]
        [ActionName("eliminarProducto")]
        [Autorizacion(idoperacion: 5)]
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
                return Json(n, JsonRequestBehavior.AllowGet);
            }
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("addExistencias")]
        [Autorizacion(idoperacion: 7)]
        public JsonResult addExistencias(Producto producto)
        {
            if (producto != null)
            {
                string codigo = producto.codigo_barra;
                var prod = db.Productos.Where(p => p.codigo_barra.Equals(codigo)).FirstOrDefault();
                if (prod != null)
                {
                    
                    return Json(new
                    {
                        id = prod.id,
                        codigobarra = prod.codigo_barra.Trim(),
                        nombre = prod.nombre.Trim(),
                        newstock = producto.stock,
                        oldStock = prod.stock,
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        codigobarra = "",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [HttpPost]
        [ActionName("aplicarExistencias")]
        [Autorizacion(idoperacion: 7)]
        public JsonResult aplicarExistencias(Producto producto)
        {
            int n = 0;
            var resulProducto = db.Productos.Include(p => p.Categoria).Include(p => p.Prioridade)
                .Where(p => p.codigo_barra == producto.codigo_barra).FirstOrDefault();
            if (resulProducto!=null)
            {
                resulProducto.stock += producto.stock;
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