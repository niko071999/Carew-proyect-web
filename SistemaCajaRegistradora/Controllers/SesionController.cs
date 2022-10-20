﻿using SistemaCajaRegistradora.Models;
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
        private readonly CarewEntities db = new CarewEntities();
        public ActionResult Login()
        {
            var usuarios = db.Usuarios.ToArray();
            //Verificar si existen usuarios en el sistema
            if (usuarios.Length == 0)
            {
                //Si no existen enviar a la vista de bienvenida y creacion de usuarios administrador
                return RedirectToAction("AgregarUsuarioAdmin");
            }
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
        [ActionName("AgregarUsuarioAdmin")]
        public ActionResult AgregarUsuarioAdmin()
        {
            return View();
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
                fecha_modificacion = DateTime.UtcNow
            };
            db.Usuarios.Add(usuario);
            int n = db.SaveChanges();
            
            if (n > 0)
            {
                Session["User"] = usuario;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Ocurrio un error al guardar el usuario, intentelo de nuevo o mas tarde";
                return View();
            }
        }
    }
}