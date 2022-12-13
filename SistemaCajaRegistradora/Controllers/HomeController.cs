using SistemaCajaRegistradora.Models;
using SistemaCajaRegistradora.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers
{
    public class HomeController : Controller
    {   
        ModelData db = new ModelData();
        [HttpGet]
        public ActionResult Index()
        {
            string cant_producto = db.Productos.ToArray().Length.ToString("N0", CultureInfo.CurrentCulture);
            string cant_categoria = db.Categorias.ToArray().Length.ToString("N0", CultureInfo.CurrentCulture);
            string cant_user = db.Usuarios.ToArray().Length.ToString("N0", CultureInfo.CurrentCulture);
            string cant_ventas = db.Ventas.ToArray().Length.ToString("N0", CultureInfo.CurrentCulture);
            string total_ventas = calcularTotalVentas();
            vmAmountItems ai = new vmAmountItems()
            {
                amountProd = cant_producto,
                amountCategoria = cant_categoria,
                amountUsuarios = cant_user,
                amountVentas = cant_ventas,
                totalVentas = total_ventas
            };
            return View(ai);
        }

        private string calcularTotalVentas()
        {
            int totalventas = 0;
            var ventas = db.Ventas.ToArray();
            foreach (var venta in ventas)
            {
                totalventas += venta.total_venta;
            }
            return totalventas.ToString("C0", CultureInfo.CurrentCulture);
        }
    }
}