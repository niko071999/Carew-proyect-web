using SistemaCajaRegistradora.Controllers;
using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Filters
{
    public class VerificarSesion : ActionFilterAttribute
    {
        private Usuario usuario;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                base.OnActionExecuting(filterContext);
                usuario = (Usuario)HttpContext.Current.Session["User"];
                if (usuario == null)
                {
                    if (filterContext.Controller is SesionController == false)
                    {
                        filterContext.HttpContext.Response.Redirect("/Sesion/Login");
                    }
                }
            }
            catch (Exception)
            {
                filterContext.Result = new RedirectResult("~/Sesion/Login");
            }
        }
    }
}