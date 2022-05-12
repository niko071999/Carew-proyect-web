namespace SistemaCajaRegistradora.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Venta")]
    public partial class Venta
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Venta()
        {
            venta_producto = new HashSet<venta_producto>();
        }

        public int id { get; set; }

        public int total_venta { get; set; }

        public int metodo_pagoid { get; set; }

        [Column(TypeName = "date")]
        public DateTime fecha_creacion { get; set; }

        public int cajeroid { get; set; }

        public int vuelto { get; set; }

        public virtual MetodoPago MetodoPago { get; set; }

        public virtual Usuario Usuario { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<venta_producto> venta_producto { get; set; }
    }
}
