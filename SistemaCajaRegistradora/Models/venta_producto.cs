namespace SistemaCajaRegistradora.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class venta_producto
    {
        public int id { get; set; }

        public int productoid { get; set; }

        public int ventaid { get; set; }

        public int total_precio { get; set; }

        public int total_cantidad { get; set; }

        public virtual Producto Producto { get; set; }

        public virtual Venta Venta { get; set; }
    }
}
