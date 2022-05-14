﻿using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SistemaCajaRegistradora.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ModelDatabase db = new ModelDatabase();

        // GET: Producto
        [HttpGet]
        public ActionResult Listar()
        {
            var producto = db.Producto.Include(p => p.Categoria).Include(p => p.Prioridad);
            return View(producto.ToList());
        }

        [HttpGet]
        [ActionName("AgregarForms")]
        public PartialViewResult AgregarForms()
        {
            var categorias = db.Categoria.ToList();
            var prioridades = db.Prioridad.ToList();
            ViewBag.categoriaId = new SelectList(categorias,"id","nombre");
            ViewBag.prioridadId = new SelectList(prioridades, "id", "prioridad1");

            return PartialView("_formsProducto");
        }

        [HttpPost]
        [ActionName("AgregarProducto")]
        public JsonResult AgregarProducto(Producto producto)
        {
            validarValoresNull(producto);

            //Se define una imagen por defecto
            producto.rutaImg = "./../Assets/images/productos/default-product-image.png";
            producto.fecha_creacion = DateTime.Now;
            db.Producto.Add(producto);
            int n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsEditar")]
        public PartialViewResult formsEditar(int? id)
        {   
            var producto = db.Producto.Include(p => p.Categoria)
                .Include(p => p.Prioridad).Where(p => p.id == id).FirstOrDefault();
            var categorias = db.Categoria.ToList();
            var prioridades = db.Prioridad.ToList();
            ViewBag.categoriaId = new SelectList(categorias, "id", "nombre",producto.categoriaid);
            ViewBag.prioridadId = new SelectList(prioridades, "id", "prioridad1",producto.prioridadid);

            return PartialView("_formsProducto",producto);
        }

        [HttpPost]
        [ActionName("editarProducto")]
        public JsonResult editarProducto(Producto producto)
        {
            validarValoresNull(producto);
            db.Entry(producto).State = EntityState.Modified;
            int n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("formsImagen")]
        public PartialViewResult formsImagen(int? id)
        {
            Session["idProducto"] = id;
            return PartialView("_formsImagen");
        }

        [HttpGet]
        [ActionName("formsEliminar")]
        public PartialViewResult formsEliminar (int? id)
        {
            var producto = db.Producto.Include(p=>p.Categoria)
                .Include(p=>p.Prioridad).Where(p=>p.id == id).FirstOrDefault();
            return PartialView("_formsEliminar",producto);
        }

        [HttpPost]
        [ActionName("eliminarProducto")]
        public JsonResult eliminarProducto(int? id)
        {
            var producto = db.Producto.Find(id);
            db.Producto.Remove(producto);
            db.SaveChanges();
            return Json(producto, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("subirImagen")]
        public JsonResult subirImagen(HttpPostedFileBase archivo)
        {
            try
            {
                if(archivo != null)
                {
                    string path = Server.MapPath("~/Assets/images/productos/");
                    if (!System.IO.Directory.Exists(path)){
                        System.IO.Directory.CreateDirectory(path);
                    };
                    archivo.SaveAs(path + System.IO.Path.GetFileName(archivo.FileName));
                    
                    int idProducto = (int)Session["idProducto"];
                    var producto = db.Producto.Find(idProducto);
                    producto.rutaImg = "./../Assets/images/productos/" + archivo.FileName;
                    db.Entry(producto).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(
                    new { mensaje = "Archivo subido correctamente" }, JsonRequestBehavior.AllowGet); ;
                }
                else
                {
                    return Json(
                    new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
                }
            }catch (Exception){
                return Json(
                    new { mensaje = "Error de subida de archivo" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ActionName("addExistencias")]
        public JsonResult addExistencias(Producto producto)
        {   
            if (producto != null)
            {
                int codigo = producto.codigo_barra;
                var prod = db.Producto.Where(p => p.codigo_barra == codigo).FirstOrDefault();
                if (prod != null)
                {
                    prod.stock++;
                    return Json(new
                    {
                        id = prod.id,
                        codigobarra = prod.codigo_barra,
                        nombre = prod.nombre.Trim(),
                        newstock = prod.stock,
                        oldStock = prod.stock - 1,
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        codigobarra = "",
                        nombre = "",
                        stock = "",
                        increment = "",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        public JsonResult aplicarExistencias(Producto producto)
        {
            int n = 0;
            var resulProducto = db.Producto.Include(p => p.Categoria).Include(p => p.Prioridad)
                .Where(p => p.codigo_barra == producto.codigo_barra).FirstOrDefault();
            if (resulProducto!=null)
            {
                resulProducto.stock = producto.stock;
                db.Entry(resulProducto).State = EntityState.Modified;
                n = db.SaveChanges();
                return Json(n,JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(n, JsonRequestBehavior.AllowGet); ;
            }
        }

        private void validarValoresNull(Producto producto)
        {
            if (producto.precio == null)
            {
                producto.precio = 0;
            }
            if (producto.stock == null)
            {
                producto.stock = 0;
            }
            if (producto.stockmin == null)
            {
                producto.stockmin = 0;
            }
            if (producto.stockmax == null)
            {
                producto.stockmax = 0;
            }
        }


    }
}