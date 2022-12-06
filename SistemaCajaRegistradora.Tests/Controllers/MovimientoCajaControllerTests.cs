using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaCajaRegistradora.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaCajaRegistradora.Controllers.Tests
{
    [TestClass]
    public class MovimientoCajaControllerTests
    {
        [TestMethod]
        public void verificarCajaUsoTest()
        {
            int id_entrada = 1;
            MovimientoCajaController mc = new MovimientoCajaController();
            bool result = mc.verificarCajaUso(id_entrada);

            if (id_entrada > 0)
            {
                Assert.IsTrue(result);
            }
            else
            {
                Assert.Fail("ID <= 0");
            }
        }
    }
}