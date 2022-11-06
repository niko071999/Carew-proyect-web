using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmReporteProductosVendidos
    {
        public string codigobarra { get; set; }
        public string nombre_producto { get; set; }
        public string ruta_imagen { get; set; }
        public Nullable<int> precio { get; set; }
        public int cantidad_vendido { get; set; }
    }
}