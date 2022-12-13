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
            var result = db.Ventas.Include(v => v.MetodoPago).Include(v => v.MovimientosCaja.Usuario).ToArray();
            List<vmVenta> ventas = new List<vmVenta>();

            foreach (var item in result)
            {
                vmVenta venta = new vmVenta
                {
                    id = item.id,
                    cajero = item.MovimientosCaja.Usuario.nombre.Trim() + " " + item.MovimientosCaja.Usuario.apellido.Trim(),
                    metodoPago = item.MetodoPago.metodo_pago.Trim(),
                    numboleta = item.num_boleta,
                    totalVenta = item.total_venta,
                    fecha = item.fecha_creacion
                };
                ventas.Add(venta);
            }

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
            if (id != null)
            {
                var detailVentas = db.DetalleVentas
                                    .Include(vp => vp.Producto)
                                    .Include(vp => vp.Venta.MovimientosCaja.Usuario)
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
            Usuario user = (Usuario)Session["User"];
            var result = db.Ventas.Include(v => v.MetodoPago)
                                .Include(v => v.MovimientosCaja)
                                .Where(v => v.MovimientosCaja.cajeroid == user.id)
                                .OrderByDescending(v => v.fecha_creacion)
                                .FirstOrDefault();
            if (result == null)
            {
                string msgError = "No existe ninguna venta asociada al cajero";
                return Json(new
                {
                    data = 0,
                    msg = msgError
                }, JsonRequestBehavior.AllowGet);
            }
            string msgSuccess = "Ultima venta imprimiendo...";
            return Json(new
            {
                data = result.id,
                msg = msgSuccess
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("getMiVentas")]
        public JsonResult getMiVentas()
        {
            Usuario user = (Usuario)Session["User"];

            var result = db.Ventas.Include(v => v.MetodoPago).Include(v => v.MovimientosCaja).Where(v => v.MovimientosCaja.cajeroid == user.id).ToArray();

            List<vmVenta> ventas = new List<vmVenta>();

            foreach (var item in result)
            {
                vmVenta venta = new vmVenta
                {
                    id = item.id,
                    cajero = item.MovimientosCaja.Usuario.nombre.Trim() + " " + item.MovimientosCaja.Usuario.apellido.Trim(),
                    metodoPago = item.MetodoPago.metodo_pago.Trim(),
                    totalVenta = item.total_venta,
                    fecha = item.fecha_creacion
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
                using (ModelData db1 = new ModelData())
                {
                    string msgError = "Ocurrio un error inesperado, intentelo nuevamente o reinicie la pagina";
                    string msgErrorMC = "Al parecer no existe ningun movimiento de caja o no se ha cerrado aun correctamente";
                    string msgSuccess = "Venta creada correctamente";
                    try
                    {
                        if (ventaDetalle != null)
                        {
                            Usuario user = (Usuario)Session["User"];
                            long length = db.Ventas.ToArray().Length + 1;

                            //Buscar el ultimo movimiento de caja
                            var lastMC = db.MovimientosCajas.Include(mc => mc.Usuario)
                                    .Where(mc => mc.cajeroid == user.id)
                                    .OrderByDescending(mc => mc.fecha_apertura).FirstOrDefault();

                            if (lastMC == null)
                            {
                                throw new Exception(msgErrorMC);
                            }

                            //Insercion de los datos de la venta a la base de datos
                            ventaDetalle.venta.fecha_creacion = DateTime.Now;
                            ventaDetalle.venta.movimientocajaid = lastMC.id;
                            ventaDetalle.venta.num_boleta = numeroBoleta + length; //Generar el num de la boleta
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

                                //var producto = db.Productos.Find(idprod);
                                //producto.stock = producto.stock - stockProd;
                                //db.Entry(producto).State = EntityState.Modified;
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
                            msg = msgError,
                            idventa = 0
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

        [HttpGet]
        [Autorizacion(idoperacion: 24)]
        [ActionName("ReporteVentaDiaria")]
        public ActionResult ReporteVentaDiaria() => View();

        [HttpGet]
        [Autorizacion(idoperacion: 25)]
        [ActionName("getVentaDiaria")]
        public JsonResult getVentaDiaria()
        {
            double totalventadiaria = 0;
            int totalventadiariaAnterior = 0;
            int count = 0;
            DateTime fechaOld = new DateTime();

            List<vmReporteVentaDiaria> rvdList = new List<vmReporteVentaDiaria>();
            var ventas = db.Ventas.ToArray();
            string fechaAux = ventas.First().fecha_creacion.ToShortDateString();
            foreach (var venta in ventas)
            {
                vmReporteVentaDiaria rvd; //Reporte Venta Diaria
                double crecimiento;
                double porcentaje;
                if (fechaAux.Equals(venta.fecha_creacion.ToShortDateString()))
                {
                    totalventadiaria += venta.total_venta;
                    fechaOld = venta.fecha_creacion;
                    if (ventas.Last().fecha_creacion.Equals(venta.fecha_creacion))
                    {
                        crecimiento = totalventadiaria - totalventadiariaAnterior;
                        porcentaje = (crecimiento / totalventadiaria) * 100;
                        if (count > 0)
                        {
                            rvd = new vmReporteVentaDiaria()
                            {
                                fecha = fechaOld,
                                totalventa = Convert.ToInt32(totalventadiaria),
                                crecimiento = Convert.ToInt32(crecimiento),
                                porcentajeCrecimiento = porcentaje
                            };
                            rvdList.Add(rvd);
                            totalventadiariaAnterior = Convert.ToInt32(totalventadiaria);
                            totalventadiaria = 0;
                        }
                    }
                }
                else
                {
                    crecimiento = totalventadiaria - totalventadiariaAnterior;
                    porcentaje = (crecimiento / totalventadiaria) * 100;
                    if (count > 0)
                    {
                        rvd = new vmReporteVentaDiaria()
                        {
                            fecha = fechaOld,
                            totalventa = Convert.ToInt32(totalventadiaria),
                            crecimiento = Convert.ToInt32(crecimiento),
                            porcentajeCrecimiento = porcentaje
                        };
                        rvdList.Add(rvd);
                        totalventadiariaAnterior = Convert.ToInt32(totalventadiaria);
                        totalventadiaria = 0;
                    }
                    fechaOld = venta.fecha_creacion;
                    fechaAux = venta.fecha_creacion.ToShortDateString();
                    totalventadiaria += venta.total_venta;
                }
                count++;
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
            int count_pv = 0;
            Nullable<int> precioOld = 0;
            string codebarOld = string.Empty;
            string nombreProdOld = string.Empty;
            string rutaImgOld = string.Empty;
            List<vmReporteProductosVendidos> rpvList = new List<vmReporteProductosVendidos>();
            vmReporteProductosVendidos rpv;
            var detallesventas = db.DetalleVentas.Include(dv => dv.Producto.Imagen).OrderBy(dv => dv.productoid).ToArray();
            string codebar = detallesventas.First().Producto.codigo_barra.Trim();
            foreach (var dv in detallesventas)
            {
                if (codebar.Equals(dv.Producto.codigo_barra.Trim()))
                {
                    count_pv += dv.total_cantidad_producto;
                    codebarOld = dv.Producto.codigo_barra.Trim();
                    nombreProdOld = dv.Producto.nombre.Trim();
                    rutaImgOld = dv.Producto.Imagen.ruta.Trim();
                    precioOld = dv.Producto.precio;
                }
                else
                {
                    rpv = new vmReporteProductosVendidos()
                    {
                        codigobarra = codebarOld,
                        nombre_producto = nombreProdOld,
                        ruta_imagen = rutaImgOld,
                        precio = precioOld,
                        cantidad_vendido = count_pv
                    };
                    rpvList.Add(rpv);
                    count_pv = 0;
                    count_pv += dv.total_cantidad_producto; 
                }
                codebar = dv.Producto.codigo_barra.Trim();                
            }
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
                                    .Where(mc => mc.cajeroid == usuario.id)
                                    .OrderByDescending(mc => mc.fecha_apertura);
            
            //Verificar si no existen movimientos de caja del cajero
            if (contextMCUser.FirstOrDefault() == null)
            {
                return true;
            }

            var contextMCDate = contextMCUser.FirstOrDefault();

            //Revisar si la fecha no es la de hoy
            if (contextMCDate.fecha_apertura.Ticks != datetime.Ticks)
            {
                //Reviso si es que no ha realizado cierre de caja
                if (contextMCDate.fecha_cierre == null)
                {
                    return false;
                }
                return true;
            }

            if (contextMCDate.fecha_apertura.Ticks == datetime.Ticks)
            {
                return false;
            }

            if (contextMCDate.fecha_cierre == null)
            {
                return true;
            }

            return false;
        }

        private bool verificarCierreCaja()
        {
            DateTime hoy = DateTime.Now;
            var usuario = (Usuario)Session["User"];
            var contextMCUser = db.MovimientosCajas.Where(mc => mc.cajeroid == usuario.id)
                                    .OrderByDescending(mc => mc.fecha_apertura);

            var mov = contextMCUser.FirstOrDefault();
            if (mov == null) 
                return false;

            return hoy >= mov.fecha_apertura.AddDays(1);
        }
    }
}

