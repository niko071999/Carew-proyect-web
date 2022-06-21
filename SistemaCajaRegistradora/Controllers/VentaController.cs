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
        [Autorizacion(idoperacion:21)]
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
            vmVenta venta = new vmVenta();

            foreach (var item in result)
            {
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
    }
}