using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Autorizacion : AuthorizeAttribute
    {
        private Usuario usuario;
        private ModelData db = new ModelData();
        private int idoperacion;

        public Autorizacion(int idoperacion = 0)
        {
            this.idoperacion = idoperacion;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                usuario = (Usuario)HttpContext.Current.Session["User"];
                var user = db.rol_operacion.Where(ro => ro.idrol == usuario.rolid 
                            && ro.idoperacion == idoperacion).ToList();
                if (user.Count() == 0)
                {
                    filterContext.Result = new RedirectResult("~/Error/ErrorAutorizacion");
                }
                if (user == null)
                {
                    filterContext.Result = new RedirectResult("~/Sesion/Login");
                }
            }
            catch (Exception)
            {
                filterContext.Result = new RedirectResult("~/Error/ErrorAutorizacion");
            }
        }
    }
}