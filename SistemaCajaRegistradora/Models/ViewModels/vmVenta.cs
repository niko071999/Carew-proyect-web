﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmVenta
    {
        public long id { get; set; }
        public string cajero { get; set; }
        public string metodoPago { get; set; }
        public long numboleta { get; set; }
        public int totalVenta { get; set; }
        public DateTime fecha { get; set; }
    }
}