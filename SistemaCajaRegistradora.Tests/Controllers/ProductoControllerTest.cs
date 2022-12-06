using SistemaCajaRegistradora.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;

namespace SistemaCajaRegistradora.Controllers.Tests
{
    [TestClass]
    public class ProductoControllerTest
    {
        [TestMethod]
        public void ListarTest()
        {
            ProductoController pc = new ProductoController();
            ViewResult r = pc.Listar() as ViewResult;

            Assert.IsNotNull(r);
        }
    }
}
