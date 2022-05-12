namespace SistemaCajaRegistradora.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Usuario")]
    public partial class Usuario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Usuario()
        {
            Venta = new HashSet<Venta>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string nombreUsuario { get; set; }

        [Required]
        [StringLength(12)]
        public string nombreCajero { get; set; }

        [Required]
        [StringLength(24)]
        public string apellidoCajero { get; set; }

        [StringLength(100)]
        public string rutaImg { get; set; }

        [Required]
        [StringLength(100)]
        public string contraseña { get; set; }

        public int rolid { get; set; }

        public virtual Rol Rol { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Venta> Venta { get; set; }
    }
}
