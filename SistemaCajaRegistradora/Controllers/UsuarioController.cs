using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers
{
    public class UsuarioController : Controller
    {
        ModelData db = new ModelData();
        
        [HttpGet]
        [ActionName("Listar")]
        public ActionResult Listar()
        {
            var usuarios = db.Usuarios;
            return View(usuarios.ToList());
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        public PartialViewResult AgregarForms()
        {
            return PartialView("_formsUsuario");
        }
    }
}