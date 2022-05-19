namespace SistemaCajaRegistradora.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Productos")]
    public partial class Producto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Producto()
        {
            venta_producto = new HashSet<venta_producto>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(15)]
        public string codigo_barra { get; set; }

        [Required]
        [StringLength(100)]
        public string nombre { get; set; }

        public int? precio { get; set; }

        public int? stock { get; set; }

        public int? stockmin { get; set; }

        public int? stockmax { get; set; }

        [Column(TypeName = "date")]
        public DateTime fecha_creacion { get; set; }

        [Required]
        [StringLength(255)]
        public string rutaImg { get; set; }

        public int prioridadid { get; set; }

        public int categoriaid { get; set; }

        public virtual Categoria Categoria { get; set; }

        public virtual Prioridade Prioridade { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<venta_producto> venta_producto { get; set; }
    }
}
