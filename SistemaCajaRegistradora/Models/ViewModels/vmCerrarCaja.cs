using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmCerrarCaja
    {
        public int totalVentaDia { get; set; }
        public int montoAperturaCaja { get; set; }
        public int total { get; set; }
        public DateTime fecha_cc { get; set; }
        public DateTime fecha_ac { get; set; }
    }
}