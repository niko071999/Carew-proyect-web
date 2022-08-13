using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmProducto
    {
        public int id { get; set; }
        public string codigobarra { get; set; }
        public string nombre { get; set; }
        public int precio { get; set; }
        public int stock { get; set; }
        public int stockmin { get; set; }
        public int stockmax { get; set; }
        public string fechacreacion { get; set; }
        public string rutaimg { get; set; }
        public string prioridad { get; set; }
        public string categoria { get; set; }

    }
}