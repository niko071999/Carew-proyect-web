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
using System.Web.WebPages;
using System.Transactions;
using System.Web.Helpers;
using SistemaCajaRegistradora.Models.ViewModels;
using System.Collections;

namespace SistemaCajaRegistradora.Controllers
{
    [HandleError]
    public class UsuarioController : Controller
    {
        private readonly ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        [Autorizacion(idoperacion:12)]
        public ActionResult Listar()
        {
            return View();
        }

        [HttpGet]
        [ActionName("getUsuarios")]
        public JsonResult getUsuarios()
        {
            db.Configuration.LazyLoadingEnabled = false;
            List<vmUsuario> usuariosList = new List<vmUsuario>();
            vmUsuario vmUsuario;
            var usuarios = db.Usuarios.Include(u => u.Imagen).Include(u => u.Role).ToArray();
            foreach (var usuario in usuarios)
            {
                vmUsuario = new vmUsuario()
                {
                    id = usuario.id,
                    nombreCajero = usuario.nombre.Trim() + " " + usuario.apellido.Trim(),
                    nombreUsuario = usuario.nombreUsuario.Trim(),
                    rutaImg = usuario.Imagen.ruta,
                    rol = usuario.Role.rol.Trim(),
                    stateConexion = usuario.conectado,
                    solrespass = usuario.solrespass
                };
                usuariosList.Add(vmUsuario);
            }

            return Json(new
            {
                data = usuariosList
            }, JsonRequestBehavior.AllowGet);
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
            if (usuario.rolid != 1)
            {
                usuario.rolid = 2; //Asignamos el rol de cajero
                usuario.clave = Encrypt.GetSHA256(usuario.clave);
                usuario.imagenid = 2; //Se define una imagen por defecto
                usuario.fecha_creacion = DateTime.UtcNow;
                usuario.fecha_modificacion = usuario.fecha_creacion;
                usuario.conectado = false;
                db.Usuarios.Add(usuario);
                int n = db.SaveChanges();
                return Json(n, JsonRequestBehavior.AllowGet);
            }
            //Si existe un administrador, enviar -1 que significa error de creacion de usuario
            return Json(-1, JsonRequestBehavior.AllowGet);
            
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
            usuario.fecha_modificacion = DateTime.UtcNow;
            db.Entry(usuario).State = EntityState.Modified;
            int n = db.SaveChanges();
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
            string nameFile = string.Empty;

            try
            {
                var usuario = db.Usuarios.Include(u => u.Imagen)
                .Where(u => u.id == id).FirstOrDefault();
                nameFile = usuario.Imagen.nombre.Trim();
                long idimg = usuario.imagenid;
                db.Imagens.Find(idimg);
                db.Usuarios.Remove(usuario);
                if (idimg != 2)
                {
                    db.Usuarios.Remove(usuario);
                }
                n = db.SaveChanges();
                return Json(new
                {
                    n,
                    nameFile,
                    idimg
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    n,
                    nameFile,
                    idimg = 0
                }, JsonRequestBehavior.AllowGet);
            }
            
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
            usuario.fecha_modificacion = DateTime.Now;
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
            var usuario = db.Usuarios.Where(u => u.id == id).FirstOrDefault();
            return PartialView("_formsImagenUsuario", usuario);
        }

        [HttpPost]
        [ActionName("subirImagen")]
        [Autorizacion(idoperacion:16)]
        public JsonResult subirImagen(string downloadURL, string nameFile)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                using (ModelData db1 = new ModelData())
                {
                    string msgSuccess = "Imagen subida correctamente";
                    string msgError = "Error de subida de archivo";
                    try
                    {
                        if (downloadURL.Equals(string.Empty) && nameFile.Equals(string.Empty))
                            throw new Exception(msgError);

                        Imagen img = new Imagen()
                        {
                            nombre = nameFile,
                            ruta = downloadURL,
                        };
                        db1.Imagens.Add(img);
                        int n = db1.SaveChanges();

                        if (n == 0)
                            throw new Exception(msgError);

                        int idUsuario = (int)Session["idUser"];

                        //Cambiar la imagen a la variable de session
                        var user = (Usuario)Session["User"];
                        if (user.id == idUsuario)
                        {
                            user.imagenid = img.id;
                            Session["User"] = user;
                        }

                        var usuario = db1.Usuarios.Find(idUsuario);
                        string nombreImg = usuario.Imagen.nombre;
                        long idimg = usuario.imagenid;

                        usuario.imagenid = img.id;
                        usuario.fecha_modificacion = DateTime.Now;
                        db1.Entry(usuario).State = EntityState.Modified;
                        n = db1.SaveChanges();
                        if (n == 0)
                            throw new Exception(msgError);

                        //Borrar registro de la otra imagen
                        if (idimg != 2)
                        {
                            var imagen = db1.Imagens.Find(idimg);
                            db1.Imagens.Remove(imagen);
                            db1.SaveChanges();
                        }

                        scope.Complete();

                        return Json(new
                        {
                            status = "success",
                            msg = msgSuccess,
                            idimg,
                            nombreImg
                        },JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception)
                    {
                        return Json(new
                        {
                            status = "error",
                            msg = msgError,
                            idimg = 0,
                            nombreImg = ""
                        },JsonRequestBehavior.AllowGet);
                    }
                }
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
            usuario.fecha_modificacion = DateTime.Now;
            db.Entry(usuario).State = EntityState.Modified;
            n = db.SaveChanges();
            return RedirectToAction("Listar");
        }

        [HttpGet]
        [ActionName("obtenerBarcodeUser")]
        public PartialViewResult obtenerBarcodeUser(int? id)
        {
            if (id != null)
            {
                var usuario = db.Usuarios.Where(u => u.id == id).FirstOrDefault();
                if (usuario != null)
                {
                    ViewBag.code = Encrypt.GetSHA256(usuario.fecha_creacion.Value.Ticks.ToString());
                    return PartialView("_formObtenerBarcode", usuario);
                }
                return PartialView(null);
            }
            return PartialView(null);
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
                        usuario.nombreUsuario += numero;
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
            var user = (Usuario)Session["User"];
            if (user != null)
            {
                return Json(new
                {
                    nombreuser = user.nombre.Trim() + ' ' + user.apellido.Trim() + " (" + user.nombreUsuario.Trim() + ")",
                    imgruta = user.Imagen.ruta.Trim()
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
            usuario.Imagen.ruta = usuario.Imagen.ruta.Trim();
            usuario.nombre = usuario.nombre.Trim();
            usuario.apellido = usuario.apellido.Trim();
        }
    }
}