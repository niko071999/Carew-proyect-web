using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Text;
using SistemaCajaRegistradora.Filters;
using System.Security.Claims;
using System.Threading;

namespace SistemaCajaRegistradora.Controllers
{
    [HandleError]
    public class UsuarioController : Controller
    {
        ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        [Autorizacion(idoperacion:12)]
        public ActionResult Listar()
        {
            Usuario usuario = (Usuario)Session["User"];
            ViewBag.nombreUser = usuario.nombreUsuario;
            var usuarios = db.Usuarios.Include(x => x.Role);
            return View(usuarios.ToList());
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        [Autorizacion(idoperacion: 13)]
        public PartialViewResult AgregarForms()
        {
            return PartialView("_formsUsuario");
        }

        [HttpPost]
        [ActionName("AgregarUsuario")]
        [Autorizacion(idoperacion: 13)]
        public JsonResult AgregarUsuario(Usuario usuario)
        {
            int n = 0;
            if (usuario.rolid == 1)
            {
                //Verificar si existe un administrador en el sistema
                var user = db.Usuarios.Where(u => u.rolid == 1).FirstOrDefault();
                if (user == null)
                {
                    usuario.rolid = 2; //Asignamos el rol de cajero
                    usuario.clave = Encrypt.GetSHA256(usuario.clave);
                    usuario.rutaImg = "./../Assets/images/blank-profile.png";
                    usuario.fecha_creacion = DateTime.UtcNow;
                    usuario.fecha_modificacion = usuario.fecha_creacion;
                    db.Usuarios.Add(usuario);
                    n = db.SaveChanges();
                    return Json(n, JsonRequestBehavior.AllowGet);
                }
                //Si existe un administrador, enviar -1 que significa error de creacion de usuario
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEditar")]
        [Autorizacion(idoperacion: 15)]
        public PartialViewResult formsEditar(int? id)
        {
            var usuario = db.Usuarios.Include(u => u.Role)
                            .Where(u => u.id == id).FirstOrDefault();
            var roles = db.Roles.ToList();
            quitarEspaciosVacios(usuario);
            ViewBag.rolesId = new SelectList(roles, "id", "rol", usuario.rolid);
            return PartialView("_formsUsuario", usuario);
        }

        [HttpPost]
        [ActionName("editarUsuario")]
        [Autorizacion(idoperacion: 15)]
        public JsonResult editarUsuario(Usuario usuario)
        {
            int n = 0;
            usuario.fecha_modificacion = DateTime.UtcNow;
            db.Entry(usuario).State = EntityState.Modified;
            n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEliminar")]
        [Autorizacion(idoperacion:16)]
        public PartialViewResult formsEliminar(int? id)
        {
            var usuario = db.Usuarios.Include(u=>u.Role).Where(u=>u.id == id).FirstOrDefault();
            quitarEspaciosVacios(usuario);
            return PartialView("_formsEliminar", usuario);
        }

        [HttpPost]
        [ActionName("eliminarUsuario")]
        [Autorizacion(idoperacion: 16)]
        public JsonResult eliminarUsuario(int? id)
        {
            int n = 0;
            var usuario = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuario);
            n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsCambiarPass")]
        public PartialViewResult formsCambiarPass()
        {
            if (Session["User"] != null)
            {
                Usuario user = (Usuario)Session["User"];
                return PartialView("_formsCambiarPass", user);
            }
            else
            {
                return PartialView("_formsRestablecerPass");
            }
        }

        [HttpPost]
        [ActionName("cambiarClave")]
        public JsonResult cambiarClave(Usuario usuario)
        {
            int n = 0;
            usuario.clave = Encrypt.GetSHA256(usuario.clave);
            var user = (Usuario)Session["User"];
            usuario.id = user.id;
            db.Usuarios.Attach(usuario);
            db.Entry(usuario).Property(u => u.clave).IsModified = true;
            n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsImagen")]
        [Autorizacion(idoperacion: 16)]
        public PartialViewResult formsImagen(int? id)
        {
            Session["idUser"] = id;
            return PartialView("_formsImagen");
        }

        [HttpPost]
        [ActionName("subirImagen")]
        [Autorizacion(idoperacion:16)]
        public JsonResult subirImagen(HttpPostedFileBase archivo)
        {
            try
            {
                if (archivo != null)
                {
                    string path = Server.MapPath("~/Assets/images/usuarios/");
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    archivo.SaveAs(path + System.IO.Path.GetFileName(archivo.FileName));

                    int iduser = (int)Session["idUser"];
                    var usuario = db.Usuarios.Find(iduser);
                    usuario.rutaImg = "./../Assets/images/usuarios/" + archivo.FileName;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { mensaje = "Archivo subido correctamente" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ActionName("RestablecerPass")]
        [Autorizacion(idoperacion: 20)]
        public ActionResult RestablecerPass(string user, string clave)
        {
            int n = 0;
            var usuario = db.Usuarios.Where(u => u.nombreUsuario == user).FirstOrDefault();
            usuario.solrespass = false;
            usuario.clave = clave;
            usuario.clave = Encrypt.GetSHA256(usuario.clave);
            db.Entry(usuario).State = EntityState.Modified;
            n = db.SaveChanges();
            return RedirectToAction("Listar", "Usuario");
        }

        [HttpPost]
        [ActionName("verificarNameUser")]
        public JsonResult verificarNameUser(Usuario usuario)
        {
            bool valid = false;

            usuario.nombreUsuario = Regex.Replace(usuario.nombreUsuario, @"\s", "");
            usuario.nombreUsuario = Regex.Replace(usuario.nombreUsuario.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
            usuario.nombreUsuario = usuario.nombreUsuario;
            var user = db.Usuarios.Where(u => u.nombreUsuario == usuario.nombreUsuario).FirstOrDefault();

            if (usuario.id == -1)
            {
                if (user == null)
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }

                return Json(new
                {
                    nombreUsuario = "",
                    createNameUser = usuario.nombreUsuario,
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                while (!valid)
                {
                    if (user == null)
                    {
                        valid = true;
                    }
                    else
                    {
                        int numero = new Random().Next(10, 100);
                        usuario.nombreUsuario = usuario.nombreUsuario + numero;
                        user = db.Usuarios.Where(u => u.nombreUsuario == usuario.nombreUsuario).FirstOrDefault();
                        valid = false;
                    }
                }
                return Json(new
                {
                    nombreUsuario = "",
                    createNameUser = usuario.nombreUsuario,
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ActionName("verificarClave")]
        public JsonResult verificarClave(Usuario usuario)
        {
            usuario.clave = Encrypt.GetSHA256(usuario.clave);
            var user = (Usuario)Session["User"];
            user.clave = user.clave.Trim();
            if (user.clave==usuario.clave)
            {
                return Json(new { success = true },JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [ActionName("getSesion")]
        public JsonResult getSesion()
        {
            Usuario user = (Usuario)Session["User"];
            if (user != null)
            {
                return Json(new
                {
                    nombreuser = user.nombre.Trim() + ' ' + user.apellido.Trim() + " (" + user.nombreUsuario.Trim() + ")",
                    imgruta = user.rutaImg.Trim()
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        private void quitarEspaciosVacios(Usuario usuario)
        {
            usuario.nombreUsuario = usuario.nombreUsuario.Trim();
            usuario.rutaImg = usuario.rutaImg.Trim();
            usuario.nombre = usuario.nombre.Trim();
            usuario.apellido = usuario.apellido.Trim();
        }
    }
}