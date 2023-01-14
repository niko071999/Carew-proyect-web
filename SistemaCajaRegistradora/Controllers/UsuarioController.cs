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
using System.Transactions;
using SistemaCajaRegistradora.Models.ViewModels;

namespace SistemaCajaRegistradora.Controllers
{
    [HandleError]
    public class UsuarioController : Controller
    {
        private readonly ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        [Autorizacion(idoperacion: 12)]
        public ActionResult Listar() => View();

        [HttpGet]
        [ActionName("getUsuarios")]
        public JsonResult getUsuarios()
        {
            db.Configuration.LazyLoadingEnabled = false;
            List<vmUsuario> usuariosList = db.Usuarios.Include(u => u.Imagen).Include(u => u.Role).Select(u => new vmUsuario()
            {
                id = u.id,
                nombreCajero = u.nombre.Trim() + " " + u.apellido.Trim(),
                nombreUsuario = u.nombreUsuario.Trim(),
                rutaImg = u.Imagen.ruta.Trim(),
                rol = u.Role.rol.Trim(),
                stateConexion = u.conectado,
                solrespass = u.solrespass
            }).ToList();

            return Json(new
            {
                data = usuariosList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        [Autorizacion(idoperacion: 13)]
        public PartialViewResult AgregarForms() => PartialView("_formsUsuario");

        [HttpPost]
        [ActionName("AgregarUsuario")]
        [Autorizacion(idoperacion: 13)]
        public JsonResult AgregarUsuario(Usuario usuario)
        {
            if (usuario.rolid == 1)
            {
                //Si existe un administrador, enviar -1 que significa error de creacion de usuario
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
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

        [HttpGet]
        [ActionName("formsEditar")]
        [Autorizacion(idoperacion: 15)]
        public PartialViewResult formsEditar(int? id)
        {
            var user_rol_data = db.Usuarios.Include(u => u.Role).ToArray();

            var usuario = user_rol_data.FirstOrDefault(u => u.id == id);
            var roles = user_rol_data.Select(r => new Role()
            {
                id = r.Role.id,
                rol = r.Role.rol,
                rol_operacion = r.Role.rol_operacion
            }).ToList();
            ViewBag.rolesId = new SelectList(roles, "id", "rol", usuario.rolid);

            quitarEspaciosVacios(usuario);
            return PartialView("_formsUsuario", usuario);
        }

        [HttpPost]
        [ActionName("editarUsuario")]
        [Autorizacion(idoperacion: 15)]
        public JsonResult editarUsuario(Usuario usuario)
        {
            Usuario u = (Usuario)Session["User"];
            if (usuario.id == u.id)
            {
                usuario.conectado = true;
            }
            usuario.fecha_modificacion = DateTime.Now;
            db.Entry(usuario).State = EntityState.Modified;
            int n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEliminar")]
        [Autorizacion(idoperacion:16)]
        public PartialViewResult formsEliminar(int? id)
        {
            var usuario = db.Usuarios.Include(u=>u.Role).FirstOrDefault(u => u.id == id);
            if (usuario == null) return PartialView(null);

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
                var usuario_imagen_data = db.Usuarios.Include(u => u.Imagen).ToArray();
                var imagenes = usuario_imagen_data.Select(i => new Imagen()
                {
                    id = i.Imagen.id,
                    nombre = i.Imagen.nombre.Trim(),
                    ruta = i.Imagen.ruta.Trim(),
                }).ToArray();

                var usuario = usuario_imagen_data.FirstOrDefault(u => u.id == id);

                if (usuario == null)
                {
                    return Json(new
                    {
                        n,
                        nameFile,
                        idimg = 0
                    }, JsonRequestBehavior.AllowGet);
                }
                db.Usuarios.Remove(usuario);

                nameFile = usuario.Imagen.nombre.Trim();
                long idimg = usuario.imagenid;
                var img = imagenes.FirstOrDefault(i => i.id == idimg);
                if (img != null && idimg != 2) 
                    db.Imagens.Remove(img);

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
            if (Session["User"] == null) return PartialView("_formsRestablecerPass");

            Usuario user = (Usuario)Session["User"];
            return PartialView("_formsCambiarPass", user);
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
            var usuario = db.Usuarios.FirstOrDefault(u => u.id == id);
            if (usuario == null) return PartialView(null);

            return PartialView("_formsImagenUsuario", usuario);
        }

        [HttpPost]
        [ActionName("subirImagen")]
        [Autorizacion(idoperacion:16)]
        public JsonResult subirImagen(string downloadURL, string nameFile)
        {
            using (TransactionScope scope = new TransactionScope())
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
                    db.Imagens.Add(img);
                    int n = db.SaveChanges();

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

                    var usuario = db.Usuarios.Find(idUsuario);
                    string nombreImg = usuario.Imagen.nombre;
                    long idimg = usuario.imagenid;

                    usuario.imagenid = img.id;
                    usuario.fecha_modificacion = DateTime.Now;
                    db.Entry(usuario).State = EntityState.Modified;
                    n = db.SaveChanges();
                    if (n == 0)
                        throw new Exception(msgError);

                    //Borrar registro de la otra imagen
                    if (idimg != 2)
                    {
                        var imagen = db.Imagens.Find(idimg);
                        db.Imagens.Remove(imagen);
                        db.SaveChanges();
                    }

                    scope.Complete();

                    return Json(new
                    {
                        status = "success",
                        msg = msgSuccess,
                        idimg,
                        nombreImg
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        status = "error",
                        msg = msgError,
                        idimg = 0,
                        nombreImg = ""
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ActionName("restablecerImagen")]
        public JsonResult restablecerImagen()
        {
            int n = 0;
            int id = (int)Session["idUser"];
            var usuario = db.Usuarios.Include(u => u.Imagen).FirstOrDefault(u => u.id == id);
            if (usuario == null)
            {
                return Json(new
                {
                    n,
                    status = "error",
                    mensaje = "El dato es incorrecto"
                }, JsonRequestBehavior.AllowGet);
            }
            string nameFileOld = usuario.Imagen.nombre;
            usuario.imagenid = 2;
            db.Entry(usuario).State = EntityState.Modified;
            n = db.SaveChanges();
            return Json(new
            {
                n,
                nameFileOld,
                status = "success",
                mensaje = "Se ha restablecido la imagen"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("RestablecerPass")]
        [Autorizacion(idoperacion: 20)]
        public ActionResult RestablecerPass(string user, string clave)
        {
            var usuario = db.Usuarios.FirstOrDefault(u => u.nombreUsuario == user);
            if (usuario == null)
            {
                return View();
            }
            usuario.solrespass = false;
            usuario.clave = clave;
            usuario.clave = Encrypt.GetSHA256(usuario.clave);
            usuario.fecha_modificacion = DateTime.Now;
            db.Entry(usuario).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Listar");
        }

        [HttpGet]
        [ActionName("obtenerBarcodeUser")]
        public PartialViewResult obtenerBarcodeUser(int? id)
        {
            if (id != null)
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.id == id);
                if (usuario == null) return PartialView(null);

                ViewBag.code = Encrypt.GetSHA256(usuario.fecha_creacion.Value.Ticks.ToString());
                return PartialView("_formObtenerBarcode", usuario);
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
                valid = user == null ? true : false;

                return Json(new
                {
                    nombreUsuario = "",
                    createNameUser = usuario.nombreUsuario,
                }, JsonRequestBehavior.AllowGet);
            }
            while (!valid)
            {
                valid = user == null ? true : false;

                if (!valid)
                {
                    int numero = new Random().Next(10, 100);
                    usuario.nombreUsuario += numero;
                    user = db.Usuarios.FirstOrDefault(u => u.nombreUsuario == usuario.nombreUsuario);
                }
            }
            return Json(new
            {
                nombreUsuario = "",
                createNameUser = usuario.nombreUsuario,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ActionName("verificarClave")]
        public JsonResult verificarClave(Usuario usuario)
        {
            usuario.clave = Encrypt.GetSHA256(usuario.clave);
            var user = (Usuario)Session["User"];
            user.clave = user.clave.Trim();
            return user.clave == usuario.clave ? 
                Json(new { success = true }, JsonRequestBehavior.AllowGet) :
                Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("getSesion")]
        public JsonResult getSesion()
        {
            var user = (Usuario)Session["User"];
            if (user == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                nombreuser = user.nombre.Trim() + ' ' + user.apellido.Trim() + " (" + user.nombreUsuario.Trim() + ")",
                imgruta = user.Imagen.ruta.Trim()
            }, JsonRequestBehavior.AllowGet);
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