using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SistemaCajaRegistradora.Filters;

namespace SistemaCajaRegistradora.Controllers
{
    public class VentaController : Controller
    {
        private readonly ModelData db = new ModelData();
     
        // GET: Venta
        [HttpGet]
        [Autorizacion(idoperacion: 21)]
        [ActionName("Listar")]
        public ActionResult Listar()
        {
            return View();
        }

        [HttpGet]
        [Autorizacion(idoperacion: 22)]
        [ActionName("getVentas")]
        public JsonResult getVentas()
        {
            //db.Configuration.LazyLoadingEnabled = false;
            var result = db.Ventas.Include(v => v.MetodoPago).Include(v => v.Usuario).ToArray();

            List<vmVenta> ventas = new List<vmVenta>();

            foreach (var item in result)
            {
                vmVenta venta = new vmVenta();
                venta.id = item.id;
                venta.cajero = item.Usuario.nombre.Trim() + " " + item.Usuario.apellido.Trim();
                venta.metodoPago = item.MetodoPago.metodo_pago.Trim();
                venta.totalVenta = item.total_venta;
                venta.fecha = item.fecha_creacion.ToShortDateString();

                ventas.Add(venta);
            }

            return Json(new
            {
                data = ventas
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("getMoreDetail")]
        public PartialViewResult getMoreDetail(long? id)
        {
            if (id != null)
            {
                var detailVentas = db.DetalleVentas
                                    .Include(vp => vp.Producto)
                                    .Include(vp => vp.Venta)
                                    .Where(vp => vp.ventaid == id);
                return PartialView("_detailVenta", detailVentas.ToList());
            }
            else
            {
                return PartialView(null);
            }
        }

        [HttpGet]
        [ActionName("POS")]
        public ActionResult POS()
        {
            return View();
        }

        [HttpGet]
        [ActionName("getMiVentas")]
        public JsonResult getMiVentas()
        {
            Usuario user = (Usuario)Session["User"];

            var result = db.Ventas.Include(v => v.MetodoPago).Include(v => v.Usuario).Where(v => v.cajeroid == user.id).ToArray();

            List<vmVenta> ventas = new List<vmVenta>();

            foreach (var item in result)
            {
                vmVenta venta = new vmVenta();
                venta.id = item.id;
                venta.cajero = item.Usuario.nombre.Trim() + " " + item.Usuario.apellido.Trim();
                venta.metodoPago = item.MetodoPago.metodo_pago.Trim();
                venta.totalVenta = item.total_venta;
                venta.fecha = item.fecha_creacion.ToShortDateString();

                ventas.Add(venta);
            }

            return Json(new
            {
                data = ventas
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ActionName("cargarProducto")]
        public JsonResult cargarProducto(Producto producto)
        {
            if (producto != null)
            {
                string codigo = producto.codigo_barra;
                var prod = db.Productos.Where(p => p.codigo_barra.Equals(codigo)).FirstOrDefault();
                if (prod != null)
                {
                    if (prod.stock >= producto.stock)
                    {
                        return Json(new
                        {
                            id = prod.id,
                            codigobarra = prod.codigo_barra.Trim(),
                            nombre = prod.nombre.Trim(),
                            precio = prod.precio,
                            cantidadmax = prod.stock,
                            cantidad = producto.stock,
                            preciototal = 0,
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new
                        {
                            codigobarra = "",
                            mensaje = "Stock insuficiente",
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new
                    {
                        codigobarra = "",
                        mensaje = "No existe ningun producto asociado a este codigo de barra",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new
                {
                    codigobarra = "",
                    mensaje = "Existio un error de conexion, reinicie la pagina",
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [ActionName("finalizarVenta")]
        public JsonResult finalizarVenta(Venta venta)
        {
            DbContextTransaction transaction = db.Database.BeginTransaction();
            transaction.Commit();
            bool error;
            try
            {
                if (venta != null)
                {
                    Usuario user = (Usuario)Session["User"];
                    long length = db.Ventas.ToArray().Length + 1;

                    venta.fecha_creacion = DateTime.Now;
                    venta.cajeroid = user.id;
                    venta.num_boleta = length; //Generar el num de la boleta
                    db.Ventas.Add(venta);
                    db.SaveChanges();
                    Session["ventaid"] = venta.id;
                    error = false;
                }
                else
                {
                    error = true;

                }
            }
            catch (Exception) { error = true; }

            if (!error)
            {
                transaction.Commit();
                return Json(new
                {
                    data = venta
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                transaction.Rollback();
                return Json(new
                {
                    data = "",
                    mensaje = "Ocurrio un error inesperado, intentelo nuevamente o reinicie la pagina"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ActionName("agregarDetalleVenta")]
        public JsonResult agregarDetalleVenta(DetalleVenta detalle)
        {
            DbContextTransaction transaction = db.Database.BeginTransaction();
            transaction.Commit();
            int n = 0;
            bool error;
            try
            {
                if (detalle != null)
                {
                    detalle.ventaid = (long)Session["ventaid"];
                    db.DetalleVentas.Add(detalle);
                    n = db.SaveChanges();
                    throw new Exception("Error");
                    if (n > 0)
                    {
                        var producto = db.Productos.Where(p => p.id == detalle.productoid).FirstOrDefault();
                        if (producto!=null)
                        {
                            producto.stock = producto.stock - detalle.total_cantidad_producto;
                            db.Entry(producto).State = EntityState.Modified;
                            n = db.SaveChanges();
                            error = false;
                            throw new Exception("Error");
                        }
                    }
                    else { error = true; }    
                }
                else { error = true; }
            }
            catch (Exception) { error = true; }
            
            if (!error)
            {
                transaction.Commit();
                return Json(n, JsonRequestBehavior.AllowGet);
            }
            else
            {
                transaction.Rollback();
                long id = (long)Session["ventaid"];
                var venta = db.Ventas.Find(id);
                db.Ventas.Remove(venta);
                return Json(n, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [ActionName("viewBoletaVenta")]
        public PartialViewResult viewBoletaVenta(long? id)
        {
            DbContextTransaction transaction = db.Database.BeginTransaction();
            transaction.Commit();
            try
            {
                if (id != null)
                {
                    if (id == 0)
                    {
                        long ventaid = (long)Session["ventaid"];
                        var detailVentas = db.DetalleVentas
                                                .Include(vp => vp.Producto)
                                                .Include(vp => vp.Venta)
                                                .Where(vp => vp.ventaid == ventaid).ToList();
                        return PartialView("_boletaVenta", detailVentas);
                    }
                    else
                    {
                        var detailVentas = db.DetalleVentas
                                            .Include(vp => vp.Producto)
                                            .Include(vp => vp.Venta)
                                            .Where(vp => vp.ventaid == id);
                        return PartialView("_boletaVenta", detailVentas.ToList());
                    }
                    
                }
                else
                {
                    return PartialView(null);
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                return PartialView(null);
            }

        }
    }
}

