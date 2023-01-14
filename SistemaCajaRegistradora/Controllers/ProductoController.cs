using SistemaCajaRegistradora.Filters;
using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using System.Net;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

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
            List<vmProducto> productos = db.Productos.Include(p => p.Categoria)
                .Include(p => p.Prioridade)
                .Include(p => p.Imagen)
                .OrderBy(p => p.nombre)
                .Select(p => new vmProducto()
                {
                    id = p.id,
                    codigobarra = p.codigo_barra.Trim().ToString(),
                    nombre = p.nombre.Trim(),
                    categoria = p.Categoria.nombre.Trim(),
                    prioridad = p.Prioridade.prioridad.Trim(),
                    rutaimg = p.Imagen.ruta.Trim(),
                    precio = (int)p.precio,
                    stock = (int)p.stock,
                    stockmin = (int)p.stockmin,
                    stockmax = (int)p.stockmax,
                    fechacreacion = p.fecha_creacion
                }).ToList();

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
            producto.fecha_creacion = DateTime.Now;
            producto.fecha_modificacion = producto.fecha_creacion;
            db.Productos.Add(producto);
            int n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("SubirProductosCSV")]
        public PartialViewResult SubirProductosCSV() => PartialView("_formsLoadProdsCSV");

        [HttpPost]
        [ActionName("SubirProductosCSV")]
        public JsonResult SubirProductosCSV(string downloadURL)
        {
            List<string> errorProd = new List<string>();
            List<string> prodRepetidos = new List<string>();

            var codigosProd = db.Productos.Select(p => p.codigo_barra).ToArray();

            int n = 0;
            int row_count = 0;
            if (!string.IsNullOrEmpty(downloadURL))
            {
                WebClient client = new WebClient();
                string csvData = client.DownloadString(downloadURL);
                StringReader reader = new StringReader(csvData);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    int idPrioridad = 0;
                    int idCategoria = 0;
                    if (row_count > 0)
                    {
                        var producto = line.Split(';');
                        bool checkCod = verificarCodigoBarra(codigosProd, producto[0]);
                        if (!checkCod)//Si no existe el codigo de barra, se sigue como tal el proceso
                        {
                            if (isNumber(producto))
                            {
                                idPrioridad = obtenerPrioridadId(producto);
                                idCategoria = obtenerCategoriaId(producto);
                                Producto p = new Producto()
                                {
                                    codigo_barra = producto[0].ToString(),
                                    nombre = producto[1].ToString(),
                                    precio = Convert.ToInt32(producto[2]),
                                    stock = Convert.ToInt32(producto[3]),
                                    stockmin = Convert.ToInt32(producto[4]),
                                    stockmax = Convert.ToInt32(producto[5]),
                                    prioridadid = idPrioridad,
                                    categoriaid = idCategoria,
                                    imagenid = 1,
                                    fecha_creacion = DateTime.Now,
                                    fecha_modificacion = DateTime.Now
                                };
                                db.Productos.Add(p);
                            }
                            else
                            {
                                errorProd.Add(producto[0] + " - " + producto[1] + "\n");
                            }
                        }
                        else
                        {
                            prodRepetidos.Add(producto[0] + " - " + producto[1] + "\n");
                        }
                    }
                    row_count++;
                }
                n = db.SaveChanges();
                if (n == 0)
                {
                    string msgError = "No se guardo ningun cambio en la base de datos";
                    return Json(new
                    {
                        n,
                        msg = msgError,
                        status = "Error",
                        errorProd,
                        prodRepetidos
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            string msgSuccess = "Se han agregado con exito los productos";
            return Json(new
            {
                n,
                msg = msgSuccess,
                status = "success",
                errorProd,
                prodRepetidos
            }, JsonRequestBehavior.AllowGet);
        }

        private bool verificarCodigoBarra(string[] codigosProd, string producto) => codigosProd.Any(x => x.Equals(producto, StringComparison.OrdinalIgnoreCase));

        [HttpGet]
        [ActionName("formsEditar")]
        [Autorizacion(idoperacion: 4)]
        public PartialViewResult formsEditar(int? id)
        {   
            var producto = db.Productos.Include(p => p.Categoria)
                .Include(p => p.Prioridade).Include(p => p.Imagen).FirstOrDefault(p => p.id == id);
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
            producto.fecha_modificacion = DateTime.Now;
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
            var producto = db.Productos.FirstOrDefault(p => p.id == id);
            return PartialView("_formsImagen", producto);
        }

        [HttpPost]
        [ActionName("restablecerImagen")]
        public JsonResult restablecerImagen()
        {
            int n = 0;
            int id = (int)Session["idProducto"];
            var producto = db.Productos.Include(p => p.Imagen).FirstOrDefault(p => p.id == id);
            if (producto == null)
            {
                return Json(new
                {
                    n,
                    status = "error",
                    mensaje = "El dato es incorrecto"
                }, JsonRequestBehavior.AllowGet);
            }
            string nameFileOld = producto.Imagen.nombre;
            producto.imagenid = 1;
            db.Entry(producto).State = EntityState.Modified;
            n = db.SaveChanges();
            return Json(new
            {
                n,
                nameFileOld,
                status = "success",
                mensaje = "Se ha restablecido la imagen"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("subirImagen")]
        [Autorizacion(idoperacion: 6)] 
        public JsonResult subirImagen(string downloadURL, string nameFile)
        {
            using (TransactionScope scope = new TransactionScope())
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

                    db.Imagens.Add(img);
                    int n = db.SaveChanges();

                    if (n == 0)
                        throw new Exception(msgError);

                    int idProducto = (int)Session["idProducto"];

                    var producto = db.Productos.Find(idProducto);

                    string nombreImg = producto.Imagen.nombre;
                    long idimg = producto.imagenid;

                    producto.imagenid = img.id;
                    producto.fecha_modificacion = DateTime.Now;
                    db.Entry(producto).State = EntityState.Modified;
                    n = db.SaveChanges();
                    if (n == 0)
                        throw new Exception(msgError);

                    //Borrar registro de la otra imagen
                    if (idimg == 1)
                    {
                        scope.Complete();
                    }
                    var imagen = db.Imagens.Find(idimg);
                    if (imagen != null)
                    {
                        db.Imagens.Remove(imagen);
                        db.SaveChanges();
                    }
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

        [HttpGet]
        [ActionName("formsEliminar")]
        [Autorizacion(idoperacion: 5)]
        public PartialViewResult formsEliminar (int? id)
        {
            var producto = db.Productos.Include(p=>p.Categoria)
                .Include(p=>p.Prioridade).FirstOrDefault(p => p.id == id);
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
                if (producto == null)
                {
                    return Json(new
                    {
                        n,
                        nameFile,
                        idimg = 0
                    }, JsonRequestBehavior.AllowGet);
                }
                nameFile = producto.Imagen.nombre.Trim();
                var imagen = db.Imagens.Find(producto.imagenid);
                db.Productos.Remove(producto);
                if (producto.imagenid != 1) db.Imagens.Remove(imagen);
                n = db.SaveChanges();
                return Json(new
                {
                    n,
                    nameFile,
                    producto.imagenid
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
            if (producto == null)
            {
                return Json(new
                {
                    codigobarra = "",
                }, JsonRequestBehavior.AllowGet);
            }
            string codigo = producto.codigo_barra;
            var prod = db.Productos.FirstOrDefault(p => p.codigo_barra.Equals(codigo));
            if (prod == null)
            {
                return Json(new
                {
                    codigobarra = "",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                prod.id,
                codigobarra = prod.codigo_barra.Trim(),
                nombre = prod.nombre.Trim(),
                newstock = producto.stock,
                oldStock = prod.stock,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("aplicarExistencias")]
        [Autorizacion(idoperacion: 7)]
        public JsonResult aplicarExistencias(vmAddExistenciaProduct producto)
        {
            int n = 0;
            var resulProducto = db.Productos.FirstOrDefault(p => p.codigo_barra == producto.codigo_barra);
            if (resulProducto==null) return Json(n, JsonRequestBehavior.AllowGet);
            
            resulProducto.stock += producto.stock;
            resulProducto.fecha_modificacion = DateTime.Now;
            db.Entry(resulProducto).State = EntityState.Modified;
            n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
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

        private bool isNumber(string[] producto)
        {
            return !double.IsNaN(Convert.ToDouble(producto[2])) &&
                   !double.IsNaN(Convert.ToDouble(producto[3])) &&
                   !double.IsNaN(Convert.ToDouble(producto[4])) &&
                   !double.IsNaN(Convert.ToDouble(producto[5]));
        }

        private int obtenerPrioridadId(string[] produto)
        {
            switch (produto[6].ToString())
            {
                case ("Alta"):
                    return 3;
                case ("Medio"):
                    return 2;
                case ("Baja"):
                    return 1;
                default:
                    return 0;
            }
        }

        private int obtenerCategoriaId(string[] producto)
        {
            string categoria = producto[7].ToString()
                .Replace("\r", string.Empty);
            var categorias = db.Categorias.ToDictionary(c => c.nombre.Trim().ToUpper(),
                c => c);
            if (categorias.TryGetValue(categoria.ToUpper(), out Categoria cat)) return cat.id;
            Categoria newCategoria = new Categoria()
            {
                nombre = categoria,
            };
            db.Categorias.Add(newCategoria);
            db.SaveChanges();
            return newCategoria.id;
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