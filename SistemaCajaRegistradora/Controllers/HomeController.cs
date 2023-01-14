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
            string cant_producto = db.Productos.Count().ToString("N0", CultureInfo.CurrentCulture);
            string cant_categoria = db.Categorias.Count().ToString("N0", CultureInfo.CurrentCulture);
            string cant_user = db.Usuarios.Count().ToString("N0", CultureInfo.CurrentCulture);
            string cant_ventas = db.Ventas.Count().ToString("N0", CultureInfo.CurrentCulture);
            string total_ventas = db.Ventas.Sum(v => v.total_venta).ToString("C0", CultureInfo.CurrentCulture);
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
    }
}