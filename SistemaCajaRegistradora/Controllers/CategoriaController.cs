﻿using SistemaCajaRegistradora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SistemaCajaRegistradora.Filters;

namespace SistemaCajaRegistradora.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ModelData db = new ModelData();

        [HttpGet]
        [ActionName("Listar")]
        [Autorizacion(idoperacion:8)]
        public ActionResult Listar()
        {
            ViewData["CategoriasLength"] = db.Categorias.ToArray().Length;
            return View();
        }

        [HttpGet]
        [ActionName("getCategorias")]
        public JsonResult getCategorias()
        {
            db.Configuration.LazyLoadingEnabled = false;
            var result = db.Categorias.ToArray();
            return Json(new
            {
                data = result
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [ActionName("AgregarForms")]
        [Autorizacion(idoperacion: 9)]
        public PartialViewResult AgregarForms() => PartialView("_formsCategoria");

        [HttpPost]
        [ActionName("AgregarCategoria")]
        [Autorizacion(idoperacion:9)]
        public JsonResult AgregarCategoria(Categoria categoria)
        {
            db.Categorias.Add(categoria);
            int n = db.SaveChanges();
            return Json(n,JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [ActionName("formsEditar")]
        [Autorizacion(idoperacion:10)]
        public PartialViewResult formsEditar(int? id)
        {
            var categoria = db.Categorias.Find(id);
            if (categoria != null)
            {
                categoria.nombre = categoria.nombre.Trim();
                categoria.descripcion = categoria.descripcion.Trim();
            }
            return PartialView("_formsCategoria", categoria);
        }
        [HttpPost]
        [ActionName("editarCategoria")]
        [Autorizacion(idoperacion: 10)]
        public JsonResult editarCategoria(Categoria categoria)
        {
            db.Entry(categoria).State = EntityState.Modified;
            int n = db.SaveChanges();
            return Json(n, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [ActionName("formsEliminar")]
        [Autorizacion(idoperacion: 11)]
        public PartialViewResult formsEliminar(int? id)
        {
            var categoria = db.Categorias.Find(id);
            if (categoria != null) categoria.nombre.Trim();
            return PartialView("_formsEliminar", categoria);
        }
        [HttpPost]
        [ActionName("eliminarCategoria")]
        [Autorizacion(idoperacion: 11)]
        public JsonResult eliminarCategoria(int? id)
        {
            var categoria = db.Categorias.Find(id);
            int n = 0;
            if (categoria != null)
            {
                db.Categorias.Remove(categoria);
                n = db.SaveChanges();
            }
            return Json(n, JsonRequestBehavior.AllowGet);
        }
    }
}