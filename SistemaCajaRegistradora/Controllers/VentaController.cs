using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SistemaCajaRegistradora.Filters;
using System.Transactions;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web.WebPages;

namespace SistemaCajaRegistradora.Controllers
{
    public class VentaController : Controller
    {
        private readonly CarewEntities db = new CarewEntities();

        // GET: Venta
        [HttpGet]
        [Autorizacion(idoperacion: 21)]
        [ActionName("Listar")]
        public ActionResult Listar() => View();

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
                vmVenta venta = new vmVenta
                {
                    id = item.id,
                    cajero = item.Usuario.nombre.Trim() + " " + item.Usuario.apellido.Trim(),
                    metodoPago = item.MetodoPago.metodo_pago.Trim(),
                    totalVenta = item.total_venta,
                    fecha = item.fecha_creacion.ToShortDateString()
                };

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
            int day = DateTime.UtcNow.Day;
            int month = DateTime.UtcNow.Month;
            int year = DateTime.UtcNow.Year;
            var usuario = (Usuario)Session["User"];
            var movcaja = db.MovimientosCajas.Include(mc => mc.Usuario)
                    .Where(mc => mc.cajeroid == usuario.id 
                                && mc.fecha_apertura.Day == day
                                && mc.fecha_apertura.Month == month
                                && mc.fecha_apertura.Year == year).ToArray();

            if (movcaja.Length == 0)
            {
                return RedirectToAction("AbrirCaja");
            }
            return View();
        }

        [HttpGet]
        [ActionName("AbrirCaja")]
        public ActionResult AbrirCaja()
        {
            Usuario user = (Usuario)Session["User"];
            var cajero = db.Usuarios.Where(u => u.id == user.id).FirstOrDefault();
            string nombreCajero = cajero.nombre.Trim() + " " + cajero.apellido.Trim();
            var cajas = db.Cajas.ToList();
            ViewBag.cajero = nombreCajero;
            ViewBag.cajas = new SelectList(cajas, "id", "num_caja");
            return View();
        }

        [HttpPost]
        [ActionName("AbrirCaja")]
        public ActionResult AbrirCaja(DateTime fecha, string monto, int cajas)
        {
            if (fecha == null && monto.IsEmpty())
            {
                return RedirectToAction("AbrirCajas");
            }
            Usuario user = (Usuario)Session["User"];
            monto = monto.Replace(".","");
            int montoapertura = Convert.ToInt32(monto);

            MovimientosCaja mc = new MovimientosCaja()
            {
                fecha_apertura = fecha,
                monto_apertura = montoapertura,
                cajaid = cajas,
                cajeroid = user.id
            };

            db.MovimientosCajas.Add(mc);
            int n = db.SaveChanges();
            if (n==0)
            {
                return RedirectToAction("AbrirCajas");
            }
            return RedirectToAction("POS");
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
                vmVenta venta = new vmVenta
                {
                    id = item.id,
                    cajero = item.Usuario.nombre.Trim() + " " + item.Usuario.apellido.Trim(),
                    metodoPago = item.MetodoPago.metodo_pago.Trim(),
                    totalVenta = item.total_venta,
                    fecha = item.fecha_creacion.ToShortDateString()
                };

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
                            prod.id,
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
        public JsonResult finalizarVenta(vmVentaDetalle ventaDetalle)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                using (CarewEntities db1 = new CarewEntities())
                {
                    string msgError = "Ocurrio un error inesperado, intentelo nuevamente o reinicie la pagina";
                    string msgSuccess = "Venta creada correctamente";
                    try
                    {
                        if (ventaDetalle != null)
                        {
                            Usuario user = (Usuario)Session["User"];
                            long length = db.Ventas.ToArray().Length + 1;

                            //Insercion de los datos de la venta a la base de datos
                            ventaDetalle.venta.fecha_creacion = DateTime.Now;
                            ventaDetalle.venta.cajeroid = user.id;
                            ventaDetalle.venta.num_boleta = length; //Generar el num de la boleta
                            db.Ventas.Add(ventaDetalle.venta);
                            int n = db.SaveChanges();
                            //Si ocurre un error realizar un rollback
                            if (n == 0)
                            {
                                throw new Exception(msgError);
                            }

                            //Insercion de los datos del detalle a la base de datos
                            long idVenta = ventaDetalle.venta.id;
                            for (int i = 0; i < ventaDetalle.dV.Count; i++)
                            {
                                ventaDetalle.dV[i].ventaid = idVenta;
                                db.DetalleVentas.Add(ventaDetalle.dV[i]);
                                int stockProd = ventaDetalle.dV[i].total_cantidad_producto;
                                int idprod = ventaDetalle.dV[i].productoid;

                                var producto = db.Productos.Find(idprod);
                                producto.stock = producto.stock - stockProd;
                                db.Entry(producto).State = EntityState.Modified;
                            }
                            n = db.SaveChanges();
                            
                            //throw new Exception(msgError);//EXCEPCION
                                                        
                            if (n == 0)
                            {
                                throw new Exception(msgError);
                            }

                            scope.Complete();//transaction commit 

                            //Proceso finalizado correctamente
                            return Json(new
                            {
                                data = "OK",
                                msg = msgSuccess,
                                idventa = ventaDetalle.venta.id
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            throw new Exception(msgError);
                        }
                    }
                    catch (Exception)
                    {
                        return Json(new
                        {
                            data = "",
                            msg = msgError
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpGet]
        [ActionName("viewBoletaVenta")]
        public PartialViewResult viewBoletaVenta(long? id)
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
    }
}

