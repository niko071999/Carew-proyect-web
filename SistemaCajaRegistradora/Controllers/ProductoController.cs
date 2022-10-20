using Firebase.Storage;
using Firebase.Auth;
using SistemaCajaRegistradora.Filters;
using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Threading;
using System.Transactions;

namespace SistemaCajaRegistradora.Controllers
{
    public class ProductoController : Controller
    {
        private readonly CarewEntities db = new CarewEntities();

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
            var result = db.Productos.Include(p => p.Categoria)
                                    .Include(p => p.Prioridade)
                                    .Include(p => p.Imagen)
                                    .ToArray();

            List<vmProducto> productos = new List<vmProducto>();

            foreach (var item in result)
            {
                vmProducto producto = new vmProducto
                {
                    id = item.id,
                    codigobarra = item.codigo_barra.Trim(),
                    nombre = item.nombre.Trim(),
                    categoria = item.Categoria.nombre.Trim(),
                    prioridad = item.Prioridade.prioridad.Trim(),
                    rutaimg = item.Imagen.ruta.Trim(),
                    precio = (int)item.precio,
                    stock = (int)item.stock,
                    stockmin = (int)item.stockmin,
                    stockmax = (int)item.stockmax,
                    fechacreacion = item.fecha_creacion
                };
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
            //Se define una imagen por defecto
            producto.imagenid = 1;
            producto.fecha_creacion = DateTime.UtcNow;
            producto.fecha_ultima_edicion = producto.fecha_creacion;
            db.Productos.Add(producto);
            int n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEditar")]
        [Autorizacion(idoperacion: 4)]
        public PartialViewResult formsEditar(int? id)
        {   
            var producto = db.Productos.Include(p => p.Categoria)
                .Include(p => p.Prioridade).Include(p => p.Imagen).Where(p => p.id == id).FirstOrDefault();
            producto.codigo_barra = producto.codigo_barra.Trim();
            producto.nombre = producto.nombre.Trim();
            producto.Imagen.ruta = producto.Imagen.ruta.Trim();
            producto.Imagen.nombre = producto.Imagen.nombre.Trim();
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
            producto.fecha_ultima_edicion = DateTime.UtcNow;
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
        public JsonResult subirImagen(string downloadURL, string nameFile)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                using (CarewEntities db1 = new CarewEntities())
                {
                    string msgSuccess = "Imagen subida correctamente";
                    string msgError = "Error de subida de archivo";
                    try
                    {
                        if (downloadURL.Equals(string.Empty) && nameFile.Equals(string.Empty))
                            throw new Exception(msgError);

                        Imagen img = new Imagen()
                        {
                            nombre = nameFile,
                            ruta = downloadURL,
                        };

                        db1.Imagens.Add(img);
                        int n = db1.SaveChanges();

                        if (n == 0)
                            throw new Exception(msgError);

                        int idProducto = (int)Session["idProducto"];

                        var producto = db1.Productos.Find(idProducto);

                        string nombreImg = producto.Imagen.nombre;
                        long idimg = producto.imagenid;
                        
                        producto.imagenid = img.id;
                        db1.Entry(producto).State = EntityState.Modified;
                        n = db1.SaveChanges();
                        if (n == 0)
                            throw new Exception(msgError);

                        //Borrar registro de la otra imagen
                        if (idimg != 1)
                        {
                            var imagen = db1.Imagens.Find(idimg);
                            db1.Imagens.Remove(imagen);
                            db1.SaveChanges();
                        }

                        scope.Complete();

                        return Json(new 
                        { 
                            status = "success", 
                            mensaje = msgSuccess,
                            idimg,
                            nombreImg
                        }, 
                            JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception)
                    {
                        return Json(new
                        {
                            status = "error",
                            msg = msgError,
                            idimg = 0,
                            nombreImg = ""
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
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
            string nameFile = string.Empty;

            try
            {
                var producto = db.Productos.Find(id);
                nameFile = producto.Imagen.nombre.Trim();
                long idimg = producto.imagenid;
                var imagen = db.Imagens.Find(idimg);
                db.Productos.Remove(producto);
                db.Imagens.Remove(imagen);
                n = db.SaveChanges();
                return Json(new
                {
                    n,
                    nameFile,
                    idimg
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    n,
                    nameFile,
                    idimg = 0
                }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost]
        [ActionName("addExistencias")]
        [Autorizacion(idoperacion: 7)]
        public JsonResult addExistencias(vmAddExistenciaProduct producto)
        {
            if (producto != null)
            {
                string codigo = producto.codigo_barra;
                var prod = db.Productos.Where(p => p.codigo_barra.Equals(codigo)).FirstOrDefault();
                if (prod != null)
                {
                    
                    return Json(new
                    {
                        prod.id,
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
        public JsonResult aplicarExistencias(vmAddExistenciaProduct producto)
        {
            int n = 0;
            var resulProducto = db.Productos.Include(p => p.Categoria).Include(p => p.Prioridade).Include(p=>p.Imagen)
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

//Subir imagen de forma local
//string path = Server.MapPath("~/Assets/images/productos/");
//if (!System.IO.Directory.Exists(path))
//{
//    System.IO.Directory.CreateDirectory(path);
//};
//archivo.SaveAs(path + System.IO.Path.GetFileName(archivo.FileName));

//Stream stream = archivo.InputStream;
//string nombre = DateTime.UtcNow.ToString() + "_producto";
//string urlimagen = subirImagenStorage(stream,nombre).Result;