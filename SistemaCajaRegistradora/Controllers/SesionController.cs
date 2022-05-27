using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SistemaCajaRegistradora.Controllers
{
    public class SesionController : Controller
    {
        ModelData db = new ModelData();
        public ActionResult Login()
        {

            Usuario user;
            if (Session["User"] != null)
            {
                user = (Usuario)Session["User"];
                if (user.rolid == 1)
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (user.rolid == 2)
                {
                    return RedirectToAction("POS", "Venta");
                }
                else
                {
                    return Content("Error, Rol de id no existe");
                }
            }
            return View();
        }
        [HttpPost]
        [ActionName("Login")]
        public ActionResult Login(string user, string clave)
        {
            user = user.Trim();
            clave = clave.Trim();
            clave = Encrypt.GetSHA256(clave);

            try
            {
                var usuario = db.Usuarios.Where(u => u.nombreUsuario == user && u.clave == clave).FirstOrDefault();
                if (usuario == null)
                {
                    ViewBag.Error = "El nombre de usuario y/o contraseña son erroneos";
                    return View();
                }
                Session["User"] = usuario;
                if (usuario.rolid == 1)
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (usuario.rolid == 2)    
                {
                    return RedirectToAction("POS", "Venta");
                }
                else
                {
                    return Content("Error, Rol de id no existe");
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        [ActionName("SignOut")]
        public ActionResult SignOut()
        {
            Session["User"] = null;
            return Redirect("~/Sesion/Login");
        }
    }
}