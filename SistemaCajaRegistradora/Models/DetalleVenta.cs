//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SistemaCajaRegistradora.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DetalleVenta
    {
        public long id { get; set; }
        public int productoid { get; set; }
        public long ventaid { get; set; }
        public int total_precio_producto { get; set; }
        public int total_cantidad_producto { get; set; }
    
        public virtual Producto Producto { get; set; }
        public virtual Venta Venta { get; set; }
    }
}
