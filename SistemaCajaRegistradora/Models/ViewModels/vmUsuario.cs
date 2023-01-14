using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaCajaRegistradora.Models.ViewModels
{
    public class vmUsuario
    {
        public int id { get; set; }
        public string nombreCajero { get; set; }
        public string nombreUsuario { get; set; }
        public string rutaImg { get; set; }
        public string rol { get; set; }
        public bool? stateConexion { get; set; }
        public bool? solrespass { get; set; }
        
        public vmUsuario()
        {
        }

        public vmUsuario(int id, string nombreCajero)
        {
            this.id = id;
            this.nombreCajero = nombreCajero;
        }
    }
}