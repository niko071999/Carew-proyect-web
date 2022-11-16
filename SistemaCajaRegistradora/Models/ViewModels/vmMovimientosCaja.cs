using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmMovimientosCaja
    {
        public int id { get; set; }
        public int num_caja { get; set; }
        public int monto_apertura { get; set; }
        public DateTime fecha_apertura { get; set; }
        public Nullable<DateTime> fecha_cierre { get; set; }
        public Nullable<int> total_venta_diaria { get; set; }
        public Nullable<int> total_real_efectivo { get; set; }
        public Nullable<int> total_real_transferencia { get; set; }
        public Nullable<int> diferencia { get; set; }
    }
}