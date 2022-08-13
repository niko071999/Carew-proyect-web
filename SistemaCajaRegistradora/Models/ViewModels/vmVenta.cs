using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmVenta
    {
        public int id { get; set; }
        public string cajero { get; set; }
        public string metodoPago { get; set; }
        public int totalVenta { get; set; }
        public String fecha { get; set; }
    }
}