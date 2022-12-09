using Firebase.Auth;
using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace SistemaCajaRegistradora.Controllers
{
    public class MovimientoCajaController : Controller
    {
        private readonly ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        public ActionResult Listar()
        {
            try
            {
                List<vmUsuario> cajeros = new List<vmUsuario>();
                var userArray = db.Usuarios.ToArray();
                foreach (var item in userArray)
                {
                    cajeros.Add(new vmUsuario()
                    {
                        id = item.id,
                        nombreCajero = item.nombre.Trim() + " " + item.apellido,
                        nombreUsuario = string.Empty,
                        rol = string.Empty,
                        rutaImg = string.Empty,
                        solrespass = false,
                        stateConexion = false
                    });
                }

                ViewBag.cajerosid = new SelectList(cajeros, "id", "nombreCajero");
                return View();
            }
            catch (Exception)
            {
                return View();
            }
            
        }

        [HttpGet]
        [ActionName("getMovCaja")]
        public JsonResult getMovCaja(int? id)
        {
            List<vmMovimientosCaja> mcList = new List<vmMovimientosCaja>();
            var movimientos = db.MovimientosCajas.Include(mc => mc.Caja).Where(mc => mc.cajeroid == id).ToArray();
            foreach (var movimiento in movimientos)
            {
                vmMovimientosCaja mc = new vmMovimientosCaja()
                {
                    id = movimiento.id,
                    diferencia = movimiento.diferencia_caja,
                    fecha_apertura = movimiento.fecha_apertura,
                    fecha_cierre = movimiento.fecha_cierre,
                    monto_apertura = movimiento.monto_apertura,
                    num_caja = movimiento.Caja.num_caja,
                    total_real_efectivo = movimiento.total_caja_real_efectivo,
                    total_real_transferencia = movimiento.total_caja_real_transferencia,
                    total_venta_diaria = movimiento.total_venta_diaria
                };
                mcList.Add(mc);
            }
            return Json(new
            {
                movimientos = mcList.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("getFechaApertura")]
        public JsonResult getFechaApertura()
        {
            Usuario user = (Usuario)Session["User"];
            var lastMC = db.MovimientosCajas.Include(mc => mc.Usuario)
                                    .Where(mc => mc.cajeroid == user.id)
                                    .OrderByDescending(mc => mc.fecha_apertura).FirstOrDefault();
            if (lastMC == null) return Json(null, JsonRequestBehavior.AllowGet);
            return Json(lastMC.fecha_apertura.AddDays(1).ToString("MM-dd-yyyy HH:mm:ss"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("AbrirCaja")]
        public ActionResult AbrirCaja()
        {
            try
            {
                Usuario user = (Usuario)Session["User"];
                var cajero = db.Usuarios.Where(u => u.id == user.id).FirstOrDefault();
                string nombreCajero = cajero.nombre.Trim() + " " + cajero.apellido.Trim();
                var cajas = db.Cajas.ToList();
                ViewBag.cajero = nombreCajero;
                ViewBag.cajas = new SelectList(cajas, "id", "num_caja");
                return View();
            }
            catch (Exception)
            {
                return View();
            }
            
        }

        [HttpPost]
        [ActionName("AbrirCaja")]
        public ActionResult AbrirCaja(DateTime fecha, string monto, int cajas)
        {
            //Verificar valores
            if (fecha == null && monto.IsEmpty())
            {
                return RedirectToAction("AbrirCaja");
            }
            if (!verificarCajaUso(cajas))
            {
                if (verificarAperturaCaja())
                {
                    Usuario user = (Usuario)Session["User"];
                    monto = monto.Replace(".", "");
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
                    if (n == 0)
                    {
                        return RedirectToAction("AbrirCaja");
                    }
                }
            }
            else
            {
                TempData["msgError"] = "La caja seleccionada esta siendo utilizada por otro cajero";
                return RedirectToAction("AbrirCaja");
            }

            return RedirectToAction("POS", "Venta");
        }

        [HttpGet]
        [ActionName("CerrarCaja")]
        public ActionResult CerrarCaja()
        {
            int totalVentasDia = 0;
            DateTime lastDate = DateTime.Now;
            Usuario user = (Usuario)Session["User"];

            var cajero = db.Usuarios.Where(u => u.id == user.id).FirstOrDefault();
            string nombreCajero = cajero.nombre.Trim() + " " + cajero.apellido.Trim();
            ViewBag.cajero = nombreCajero;

            var lastMC = db.MovimientosCajas.Include(mc => mc.Usuario)
                                    .Where(mc => mc.cajeroid == user.id)
                                    .OrderByDescending(mc => mc.fecha_apertura).FirstOrDefault();
            var ventaDia = db.Ventas.Where(v => v.MovimientosCaja.cajeroid == user.id).ToArray();

            foreach (var item in ventaDia.ToArray())
            {
                if (item.fecha_creacion.Ticks >= lastMC.fecha_apertura.Ticks && item.fecha_creacion.Ticks <= lastDate.Ticks)
                {
                    totalVentasDia += item.total_venta;
                }
            }

            if (totalVentasDia == 0)
            {
                TempData["msgError"] = "No existe ninguna venta, al menos debe haber una venta realizada para el cierre de caja";
                return RedirectToAction("POS","Venta");
            }

            vmCerrarCaja cc = new vmCerrarCaja()
            {
                totalVentaDia = totalVentasDia,
                montoAperturaCaja = lastMC.monto_apertura,
                total = totalVentasDia + lastMC.monto_apertura,
                fecha_cc = lastDate,
                fecha_ac = lastMC.fecha_apertura
            };

            return View(cc);
        }

        [HttpPost]
        [ActionName("CerrarCaja")]
        public ActionResult CerrarCaja(DateTime fecha_cc, int total, int montoRealEfectivo,
                                    int montoRealTransferencia, int diferencia)
        {

            Usuario user = (Usuario)Session["User"];
            var lastMC = db.MovimientosCajas.Include(mc => mc.Usuario)
                                    .Where(mc => mc.cajeroid == user.id)
                                    .OrderByDescending(mc => mc.fecha_apertura).FirstOrDefault();

            if (lastMC.fecha_cierre == null)
            {
                lastMC.total_caja_real_efectivo = montoRealEfectivo;
                lastMC.total_caja_real_transferencia = montoRealTransferencia;
                lastMC.total_venta_diaria = total;
                lastMC.diferencia_caja = diferencia;
                lastMC.fecha_cierre = fecha_cc;

                db.Entry(lastMC).State = EntityState.Modified;
                int n = db.SaveChanges();

                if (n != 0)
                {
                    return RedirectToAction("POS", "Venta");
                }
                return View();
            }

            return RedirectToAction("POS","Venta");

        }

        private bool verificarCajaUso(int cajas)
        {
            var movimientosCajas = db.MovimientosCajas.Where(mc => mc.fecha_cierre == null).ToArray();

            if (movimientosCajas.Length == 0)
            {
                return false;
            }

            foreach (var mc in movimientosCajas)
            {
                if (mc.cajaid == cajas)
                {
                    return true;
                }
            }

            return false;
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
    }
}