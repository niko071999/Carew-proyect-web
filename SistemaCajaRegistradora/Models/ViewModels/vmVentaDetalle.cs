using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmVentaDetalle
    {
        public List<DetalleVenta> dV { get; set; }
        public Venta venta { get; set; }
        //List<DetalleVenta> dV = new List<DetalleVenta>();
        //Venta venta = new Venta();
    }
}