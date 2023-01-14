using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

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
                List<vmUsuario> cajeros = db.Usuarios.Select(x => new vmUsuario()
                {
                    id = x.id,
                    nombreCajero = x.nombre.Trim() + " " + x.apellido.Trim()
                }).ToList();

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
            vmMovimientosCaja[] mcList = db.MovimientosCajas.Include(mc => mc.Caja)
                .Where(mc => mc.cajeroid == id)
                .Select(mc => new vmMovimientosCaja()
                {
                    id = mc.id,
                    diferencia = mc.diferencia_caja,
                    fecha_apertura = mc.fecha_apertura,
                    fecha_cierre = mc.fecha_cierre,
                    monto_apertura = mc.monto_apertura,
                    num_caja = mc.Caja.num_caja,
                    total_real_efectivo = mc.total_caja_real_efectivo,
                    total_real_transferencia = mc.total_caja_real_transferencia,
                    total_venta_diaria = mc.total_venta_diaria
                })
                .ToArray();
            return Json(new
            {
                movimientos = mcList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("getFechaApertura")]
        public JsonResult getFechaApertura()
        {
            Usuario user = (Usuario)Session["User"];
            var lastMC = db.MovimientosCajas.Include(mc => mc.Usuario)
                                    .OrderByDescending(mc => mc.fecha_apertura)
                                    .FirstOrDefault(mc => mc.cajeroid == user.id);
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
                var cajero = db.Usuarios.FirstOrDefault(u => u.id == user.id);
                if (cajero == null) throw new Exception("No existe el cajero");

                string nombreCajero = cajero.nombre.Trim() + " " + cajero.apellido.Trim();
                var cajas = db.Cajas.ToList();
                ViewBag.cajero = nombreCajero;
                ViewBag.cajas = new SelectList(cajas, "id", "num_caja");
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("AbrirCaja");
            }
            
        }

        [HttpPost]
        [ActionName("AbrirCaja")]
        public ActionResult AbrirCaja(DateTime fecha, string monto, int cajas)
        {
            //Verificar valores
            if (fecha == null && string.IsNullOrEmpty(monto)) return RedirectToAction("AbrirCaja");
            // Obtener usuario actual
            Usuario user = (Usuario)Session["User"];
            //Verifico si la caja esta en uso
            if (verificarCajaUso(cajas))
            {
                TempData["msgError"] = "La caja seleccionada esta siendo utilizada por otro cajero";
                return RedirectToAction("AbrirCaja");
            }
            //Verifico si se debe realizar apertura de caja
            if (!verificarAperturaCaja())
            {
                return RedirectToAction("POS", "Venta");
            }
            using (TransactionScope scope = new TransactionScope())
            {
                using (ModelData db1 = new ModelData())
                {
                    try
                    {
                        //Convierto el monto a entero, remplazando los puntos a vacio
                        int montoapertura = Convert.ToInt32(monto.Replace(".", ""));

                        MovimientosCaja mc = new MovimientosCaja()
                        {
                            fecha_apertura = fecha,
                            monto_apertura = montoapertura,
                            cajaid = cajas,
                            cajeroid = user.id
                        };
                        db.MovimientosCajas.Add(mc);
                        int n = db.SaveChanges();
                        //Si no se guardaron los cambios, lo mando a la vista nuevamente
                        if (n == 0)
                            throw new Exception("No se guardo ningun cambio, asegurese de que los datos ingresados son correctos");
                        //Transaccion completada
                        scope.Complete();

                        return RedirectToAction("POS", "Venta");
                    }
                    catch (Exception e)
                    {
                        TempData["msgError"] = e == null ? 
                            "Ha ocurrido un error inesperado, asegurese de que los datos ingresados son correctos"
                            :e.ToString();
                        return RedirectToAction("AbrirCaja");
                    }
                }
            }

        }

        [HttpGet]
        [ActionName("CerrarCaja")]
        public ActionResult CerrarCaja()
        {
            DateTime lastDate = DateTime.Now;
            Usuario user = (Usuario)Session["User"];
            //Se consigue los datos del cajero y ultimo movimiento de caja
            var cajero_mc_data = db.Usuarios.Include(u => u.MovimientosCajas)
                .Where(u => u.id == user.id)
                .Select(u => new
                    {
                        nombreCajero = u.nombre.Trim() + " " + u.apellido.Trim(),
                        lastMC = u.MovimientosCajas.OrderByDescending(mc => mc.fecha_apertura)
                                .FirstOrDefault()
                    })
                .FirstOrDefault();
            //Se verifica si existen los datos del usuario
            if (cajero_mc_data == null || string.IsNullOrEmpty(cajero_mc_data.nombreCajero))
            {
                TempData["msgError"] = "No existe ningun cajero, asegurese que este iniciado de sesion antes de realizar el proceso, o reinicie el sistema";
                return RedirectToAction("POS", "Venta");
            }

            var lastMC = cajero_mc_data.lastMC;

            //Calcular el total de venta
            int totalVentasDia = db.Ventas.Include(v => v.MovimientosCaja)
                .Any(v =>
                    v.MovimientosCaja.cajeroid == user.id
                    && v.fecha_creacion >= lastMC.fecha_apertura
                    && v.fecha_creacion <= lastDate)
                ? db.Ventas.Where(v =>
                    v.MovimientosCaja.cajeroid == user.id
                    && v.fecha_creacion >= lastMC.fecha_apertura
                    && v.fecha_creacion <= lastDate).Sum(v => v.total_venta) : 0;

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
            ViewBag.cajero = cajero_mc_data.nombreCajero;
            return View(cc);
        }

        [HttpPost]
        [ActionName("CerrarCaja")]
        public ActionResult CerrarCaja(DateTime fecha_cc, int total, int montoRealEfectivo,
                                    int montoRealTransferencia, int diferencia)
        {
            Usuario user = (Usuario)Session["User"];
            var lastMC = db.MovimientosCajas.Include(mc => mc.Usuario)
                .OrderByDescending(mc => mc.fecha_apertura)
                .FirstOrDefault(mc => mc.cajeroid == user.id);

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
            return RedirectToAction("POS", "Venta");
        }

        private bool verificarCajaUso(int cajas)
        {
            var movimientosCajas = db.MovimientosCajas.Where(mc => mc.fecha_cierre == null).ToArray();

            if (movimientosCajas.Length == 0)
            {
                return false;
            }
            return movimientosCajas.Select(mc => mc.cajaid == cajas).SingleOrDefault();
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
    }
}