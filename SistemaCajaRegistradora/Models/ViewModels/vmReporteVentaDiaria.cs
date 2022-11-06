using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmReporteVentaDiaria
    {
        public DateTime fecha { get; set; }
        public int totalventa { get; set; }
        public int crecimiento { get; set; }
        public double porcentajeCrecimiento { get; set; }
    }
}