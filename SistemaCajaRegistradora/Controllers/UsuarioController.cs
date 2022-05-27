using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Text;

namespace SistemaCajaRegistradora.Controllers
{
    public class UsuarioController : Controller
    {
        ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        public ActionResult Listar()
        {
            var usuarios = db.Usuarios.Include(x => x.Role);
            return View(usuarios.ToList());
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        public PartialViewResult AgregarForms()
        {
            var roles = db.Roles.ToList();
            ViewBag.rolesId = new SelectList(roles, "id", "rol");
            return PartialView("_formsUsuario");
        }

        [HttpPost]
        [ActionName("AgregarUsuario")]
        public JsonResult AgregarUsuario(Usuario usuario)
        {
            int n = 0;
            usuario.clave = Encrypt.GetSHA256(usuario.clave);
            usuario.rutaImg = "./../Assets/images/blank-profile.png";
            db.Usuarios.Add(usuario);
            n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
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
    }
}