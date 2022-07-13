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
                if (usuario.solrespass == true)
                {
                    //Dar error cuando se pidio una solicitud de restablecimiento de contraseña
                    ViewBag.Error = "Este usuario tiene una solicitud de restablecimiento de contraseña pendiente";
                    return View();
                }
                Session["User"] = usuario;

                //Reedireccionamiento de los usuarios a los diferentes modulos
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
            TempData["logout"] = "Sesion finalizada";
            return Redirect("~/Sesion/Login");
        }

        [HttpGet]
        [ActionName("SolicitarResPass")]
        public ActionResult SolicitarResPass()
        {
            return View();
        }

        [HttpPost]
        [ActionName("SolicitarResPass")]
        public ActionResult SolicitarResPass(string user)
        {
            user = user.Trim();
            if (user != "")
            {
                var usuario = db.Usuarios.Where(u => u.nombreUsuario == user).FirstOrDefault();
                if (usuario != null)
                {
                    if (usuario.rolid != 1)
                    {
                        int n = 0; 
                        usuario.solrespass = true;
                        db.Entry(usuario).State = EntityState.Modified;
                        n = db.SaveChanges();
                        TempData["Success"] = "Se solicito al administrador restablecimiento de contraseña";
                        return RedirectToAction("Login","Sesion");
                    }
                    else
                    {
                        ViewBag.Error = "El usuario es un administrador";
                        return View();
                    }
                }
            }
            else
            {
                ViewBag.Error = "El campo de texto esta vacio";
                return View();
            }
            return View();
        }
    }
}