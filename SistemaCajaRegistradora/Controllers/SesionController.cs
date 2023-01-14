using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;

namespace SistemaCajaRegistradora.Controllers
{
    public class SesionController : Controller
    {
        private readonly ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Login")]
        public ActionResult Login()
        {
            try
            {
                var contextUsuarios = db.Usuarios.Include(u => u.Role).ToArray();
                var usuarios = contextUsuarios;
                //Verificar si existen usuarios en el sistema
                if (usuarios.Length == 0)
                {
                    //Si no existen enviar a la vista de bienvenida y creacion de usuarios administrador
                    return RedirectToAction("AgregarUsuarioAdmin");
                }
                if (Session["User"] == null)
                {
                    return View();
                }
                Usuario user = (Usuario)Session["User"];
                //CAMBIAR ATRIBUTO CONECTADO A TRUE
                var usuario = contextUsuarios.FirstOrDefault(u => u.id == user.id);
                if (!usuario.conectado)
                {
                    usuario.conectado = true;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();
                }

                string msgError = "Error, Rol de id no existe o el usuario no existe";
                switch (user.rolid)
                {
                    case 1:
                        if (usuario == null) return Content(msgError);
                        return RedirectToAction("Index", "Home");
                    case 2:
                        if (usuario == null) return Content(msgError);
                        return RedirectToAction("POS", "Venta");
                    default:
                        Session["User"] = null;
                        ViewBag.Error = "Hubo un error al validar el rol del usuario";
                        return RedirectToAction("Login", "Sesion");
                }
            }
            catch (Exception)
            {
                return View();
            }
        }

        [HttpPost]
        [ActionName("Login")]
        public ActionResult Login(string user, string clave)
        {
            user = user.Trim();
            clave = Encrypt.GetSHA256(clave.Trim());
            try
            {
                var usuario = db.Usuarios.Include(u => u.Imagen).Where(u => u.nombreUsuario == user && u.clave == clave).FirstOrDefault();
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
                if (!usuario.conectado)
                {
                    //Cambiamos la variable conectado a true
                    usuario.conectado = true;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //Reedireccionamiento de los usuarios a los diferentes modulos
                switch (usuario.rolid)
                {
                    case 1:
                        return RedirectToAction("Index", "Home");
                    case 2:
                        return RedirectToAction("POS", "Venta");
                    default:
                        Session["User"] = null;
                        ViewBag.Error = "Hubo un error al validar el rol del usuario";
                        return RedirectToAction("Login", "Sesion");
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        [ActionName("AgregarUsuarioAdmin")]
        public ActionResult AgregarUsuarioAdmin() => View();

        [HttpGet]
        [ActionName("SignOut")]
        public ActionResult SignOut()
        {
            try
            {
                var user = (Usuario)Session["User"];
                var usuario = db.Usuarios.Include(u => u.Role).FirstOrDefault(u => u.id == user.id);
                if (usuario != null && usuario.conectado)
                {
                    //Cambiamos la variable conectado a false
                    usuario.conectado = false;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();
                }

                Session["User"] = null;
                TempData["logout"] = "Sesion finalizada";
                return Redirect("~/Sesion/Login");
            }
            catch (Exception)
            {
                Session["User"] = null;
                TempData["logout"] = "Sesion finalizada";
                return Redirect("~/Sesion/Login");
            }
        }

        [HttpGet]
        [ActionName("SolicitarResPass")]
        public ActionResult SolicitarResPass() => View();

        [HttpPost]
        [ActionName("SolicitarResPass")]
        public ActionResult SolicitarResPass(string user)
        {
            user = user.Trim();
            if (user == "")
            {
                ViewBag.Error = "El campo de texto esta vacio";
                return View();
            }
            var usuario = db.Usuarios.FirstOrDefault(u => u.nombreUsuario == user);
            if (usuario == null) return View();

            if (usuario.rolid == 1)
            {
                ViewBag.Error = "El usuario es un administrador";
                return View();
            }

            usuario.solrespass = true;
            db.Entry(usuario).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Se solicito al administrador restablecimiento de contraseña";
            return RedirectToAction("Login", "Sesion");
        }

        //Metodo para agregar el usuario administrador del sistema
        [HttpPost]
        [ActionName("AgregarUsuarioAdmin")]
        public ActionResult AgregarUsuarioAdmin(string firstname, string secondname, string nameuser, string clave, string clave2)
        {
            if (!clave.Equals(clave2))
            {
                ViewBag.Error = "Asegurese que las claves ingresadas sean las mismas";
                return View();
            }

            Usuario usuario = new Usuario()
            {
                nombre = firstname,
                apellido = secondname,
                nombreUsuario = nameuser,
                clave = Encrypt.GetSHA256(clave),
                rolid = 1,
                imagenid = 2,
                solrespass = false,
                fecha_creacion = DateTime.UtcNow,
                fecha_modificacion = DateTime.UtcNow,
                conectado = true

            };
            db.Usuarios.Add(usuario);
            int n = db.SaveChanges();
            if (n == 0)
            {
                ViewBag.Error = "Ocurrio un error al guardar el usuario, intentelo de nuevo o mas tarde";
                return View();                
            }
            Session["User"] = usuario;
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ActionName("verificarSession")]
        public JsonResult verificarSession(string user)
        {
            var usuario = db.Usuarios.FirstOrDefault(u => u.nombreUsuario.Equals(user));
            if (usuario == null)
            {
                return Json(new
                {
                    data = "error"
                }, JsonRequestBehavior.AllowGet);
            }

            if (usuario.conectado == true)
            {
                usuario.conectado = false;
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new
                {
                    data = "success"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                data = "not change"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}