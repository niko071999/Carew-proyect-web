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

namespace SistemaCajaRegistradora.Controllers
{
    public class VentaController : Controller
    {
        private readonly ModelData db = new ModelData();
        private readonly int numeroBoleta = 0; //Numero de boletas
        
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
            db.Configuration.LazyLoadingEnabled = false;

            List<vmVenta> ventas = db.Ventas.Include(v => v.MetodoPago)
                .Include(v => v.MovimientosCaja.Usuario)
                .Select(v => new vmVenta()
                {
                    id = v.id,
                    cajero = v.MovimientosCaja.Usuario.nombre.Trim() + " " + v.MovimientosCaja.Usuario.apellido.Trim(),
                    metodoPago = v.MetodoPago.metodo_pago.Trim(),
                    numboleta = v.num_boleta,
                    totalVenta = v.total_venta,
                    fecha = v.fecha_creacion
                }).ToList();
            return Json(new
            {
                data = ventas
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Autorizacion(idoperacion: 23)]
        [ActionName("getMoreDetail")]
        public PartialViewResult getMoreDetail(long? id)
        {
            if (id == null) return PartialView(null);

            var detailVentas = db.DetalleVentas
                                    .Include(vp => vp.Producto)
                                    .Include(vp => vp.Venta.MovimientosCaja.Usuario)
                                    .Where(vp => vp.ventaid == id).ToList();
            return PartialView("_detailVenta", detailVentas);
        }

        [HttpGet]
        [ActionName("POS")]
        public ActionResult POS()
        {
            if (verificarAperturaCaja())
            {
                return RedirectToAction("AbrirCaja","MovimientoCaja");
            }
            if (verificarCierreCaja())
            {
                TempData["mensaje"] = "Han pasado 24 horas desde la apertura de caja, se debe cerrar obligatoriamente";
                return RedirectToAction("CerrarCaja", "MovimientoCaja");
            }
            return View();
        }

        [HttpGet]
        [ActionName("ultimaVenta")]
        public JsonResult ultimaVenta()
        {
            string msgError = "No existe ninguna venta asociada al cajero";
            string msgSuccess = "Ultima venta imprimiendo...";
            
            Usuario user = (Usuario)Session["User"];
            var result = db.Ventas.Include(v => v.MetodoPago)
                                .Include(v => v.MovimientosCaja)
                                .OrderByDescending(v => v.fecha_creacion)
                                .FirstOrDefault(v => v.MovimientosCaja.cajeroid == user.id);
            if (result == null)
            {
                return Json(new
                {
                    data = 0,
                    msg = msgError
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                data = result.id,
                msg = msgSuccess
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("cargarProducto")]
        public JsonResult cargarProducto(Producto producto)
        {
            if (producto == null)
            {
                return Json(new
                {
                    codigobarra = "",
                    mensaje = "Existe un error de conexion, reinicie la página",
                }, JsonRequestBehavior.AllowGet);
            }
            string codigo = producto.codigo_barra;
            int stock = (int)producto.stock;
            var prod = db.Productos.FirstOrDefault(p => p.codigo_barra.Equals(codigo));
            if (prod == null)
            {
                return Json(new
                {
                    codigobarra = "",
                    mensaje = "No existe ningún producto asociado a este código de barra",
                }, JsonRequestBehavior.AllowGet);
            }
            if (prod.stock <= stock)
            {
                return Json(new
                {
                    codigobarra = "",
                    mensaje = "Stock insuficiente",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                prod.id,
                codigobarra = prod.codigo_barra.Trim(),
                nombre = prod.nombre.Trim(),
                prod.precio,
                cantidadmax = prod.stock,
                cantidad = producto.stock,
                preciototal = 0,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("finalizarVenta")]
        public JsonResult finalizarVenta(vmVentaDetalle ventaDetalle)
        {
            if (verificarAperturaCaja() || verificarCierreCaja())
            {
                return Json(new
                {
                    data = "",
                    idventa = -1
                }, JsonRequestBehavior.AllowGet);
            }
            using (TransactionScope scope = new TransactionScope())
            {
                string msgError = "Ocurrio un error inesperado, intentelo nuevamente o reinicie la pagina";
                string msgErrorMC = "Al parecer no existe ningun movimiento de caja o no se ha cerrado aun correctamente";
                string msgSuccess = "Venta creada correctamente";
                int n = 0;
                try
                {
                    //Se verifica si no viene vacio el objeto
                    if (ventaDetalle == null)
                    {
                        throw new Exception(msgError);
                    }
                    Usuario user = (Usuario)Session["User"];
                    var ventas_mc_data = db.Ventas.Include(v => v.MovimientosCaja).ToArray();

                    //Se verifica si no esta vacio
                    long length = ventas_mc_data.Length + 1;
                    if (length == 0)
                    {
                        throw new Exception(msgErrorMC);
                    }

                    //Buscar el ultimo movimiento de caja del cajero
                    var lastMC = ventas_mc_data
                        .OrderByDescending(v => v.MovimientosCaja.fecha_apertura)
                        .FirstOrDefault(v => v.MovimientosCaja.cajeroid == user.id).MovimientosCaja;

                    //Insercion de los datos de la venta a la base de datos
                    ventaDetalle.venta.fecha_creacion = DateTime.Now;
                    ventaDetalle.venta.movimientocajaid = lastMC.id;
                    ventaDetalle.venta.num_boleta = numeroBoleta + length; //Generar el num de la boleta
                    db.Ventas.Add(ventaDetalle.venta);

                    //Insercion de los datos del detalle a la base de datos
                    long idVenta = ventaDetalle.venta.id;
                    for (int i = 0; i < ventaDetalle.dV.Count; i++)
                    {
                        ventaDetalle.dV[i].ventaid = idVenta;
                        db.DetalleVentas.Add(ventaDetalle.dV[i]);
                        int stockProd = ventaDetalle.dV[i].total_cantidad_producto;
                        int idprod = ventaDetalle.dV[i].productoid;

                        //Se modifica las existencias de los productos afectados en la venta
                        var producto = db.Productos.Find(idprod);
                        producto.stock = producto.stock - stockProd;
                        db.Entry(producto).State = EntityState.Modified;
                    }
                    n = db.SaveChanges();
                    //Si ocurre un error al guardar los cambios, realizar un rollback
                    if (n == 0)
                    {
                        throw new Exception(msgError);
                    }

                    scope.Complete();

                    //Proceso finalizado correctamente
                    return Json(new
                    {
                        data = "OK",
                        msg = msgSuccess,
                        idventa = ventaDetalle.venta.id
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        data = "",
                        msg = msgError,
                        idventa = 0
                    }, JsonRequestBehavior.AllowGet);
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
                    var detailVentas_noid = db.DetalleVentas
                                            .Include(vp => vp.Producto)
                                            .Include(vp => vp.Venta)
                                            .Where(vp => vp.ventaid == ventaid).ToList();
                    return PartialView("_boletaVenta", detailVentas_noid);
                }
                var detailVentas_id = db.DetalleVentas
                                        .Include(vp => vp.Producto)
                                        .Include(vp => vp.Venta)
                                        .Where(vp => vp.ventaid == id).ToList();
                return PartialView("_boletaVenta", detailVentas_id);
            }
            return PartialView(null);

        }

        [HttpGet]
        [Autorizacion(idoperacion: 24)]
        [ActionName("ReporteVentaDiaria")]
        public ActionResult ReporteVentaDiaria() => View();

        [HttpGet]
        [Autorizacion(idoperacion: 25)]
        [ActionName("getVentaDiaria")]
        public JsonResult getVentaDiaria()
        {
            int totalventadiariaAnterior = 0;

            Dictionary<DateTime, double> rvdDicc = new Dictionary<DateTime, double>();
            var ventas = db.Ventas.ToArray();

            foreach (var venta in ventas)
            {
                if (rvdDicc.ContainsKey(venta.fecha_creacion.Date))
                {
                    rvdDicc[venta.fecha_creacion.Date] += venta.total_venta;
                }
                else
                {
                    rvdDicc.Add(venta.fecha_creacion.Date, venta.total_venta);
                }
            }

            List<vmReporteVentaDiaria> rvdList = new List<vmReporteVentaDiaria>();
            foreach (var venta in rvdDicc)
            {
                vmReporteVentaDiaria rvd; //Reporte Venta Diaria
                double crecimiento;
                double porcentaje;
                crecimiento = venta.Value - totalventadiariaAnterior;
                porcentaje = (crecimiento / venta.Value) * 100;
                rvd = new vmReporteVentaDiaria()
                {
                    fecha = venta.Key,
                    totalventa = Convert.ToInt32(venta.Value),
                    crecimiento = Convert.ToInt32(crecimiento),
                    porcentajeCrecimiento = porcentaje
                };
                rvdList.Add(rvd);
                totalventadiariaAnterior = Convert.ToInt32(venta.Value);
            }
            return Json(new
            {
                data = rvdList.ToArray(),
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Autorizacion(idoperacion: 26)]
        [ActionName("ReporteProductosVendidos")]
        public ActionResult ReporteProductosVendidos() => View();

        [HttpGet]
        [Autorizacion(idoperacion: 30)]
        [ActionName("getProductosVendidos")]
        public JsonResult getProductosVendidos()
        {
            vmReporteProductosVendidos rpv;
            Dictionary<int, vmReporteProductosVendidos> productoVendidosDicc = new Dictionary<int, vmReporteProductosVendidos>();
            var detallesventas = db.DetalleVentas.Include(dv => dv.Producto.Imagen)
                .OrderBy(dv => dv.productoid)
                .ToArray();
            foreach (var dv in detallesventas)
            {
                if (productoVendidosDicc.TryGetValue(dv.productoid, out rpv))
                {
                    rpv.cantidad_vendido += dv.total_cantidad_producto;
                }
                else
                {
                    rpv = new vmReporteProductosVendidos()
                    {
                        codigobarra = dv.Producto.codigo_barra.Trim(),
                        nombre_producto = dv.Producto.nombre.Trim(),
                        ruta_imagen = dv.Producto.Imagen.ruta.Trim(),
                        precio = dv.Producto.precio,
                        cantidad_vendido = dv.total_cantidad_producto
                    };
                    productoVendidosDicc.Add(dv.productoid, rpv);
                }
            }
            List<vmReporteProductosVendidos> rpvList = productoVendidosDicc.Values.ToList();
            return Json(new
            {
                data = rpvList.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        private bool verificarAperturaCaja()
        {
            DateTime datetime = DateTime.Now;
            var usuario = (Usuario)Session["User"];
            var contextMCUser = db.MovimientosCajas.Include(mc => mc.Usuario)
                                    .OrderByDescending(mc => mc.fecha_apertura)
                                    .FirstOrDefault(mc => mc.cajeroid == usuario.id);
            //Verificar si no existen movimientos de caja del cajero
            if (contextMCUser == null) return true;

            var contextMCDate = contextMCUser;

            //Revisar si la fecha no es la de hoy y si es que no ha realizado cierre de caja
            if (contextMCDate.fecha_apertura != datetime) return contextMCDate.fecha_cierre != null;

            return contextMCDate.fecha_cierre != null;
        }

        private bool verificarCierreCaja()
        {
            DateTime hoy = DateTime.Now;
            var usuario = (Usuario)Session["User"];
            var mov = db.MovimientosCajas
                .OrderByDescending(mc => mc.fecha_apertura)
                .FirstOrDefault(mc => mc.cajeroid == usuario.id);

            if (mov == null) return false;

            return hoy >= mov.fecha_apertura.AddDays(1);
        }
    }
}

