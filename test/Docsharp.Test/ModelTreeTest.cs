using Docsharp.Core;
using NUnit.Framework;
using System;

namespace Docsharp.Test
{
    internal class ModelTreeTest : BaseTest
    {      
        
        [Test]
        public void InterfacesExist()
        {
            Assert.AreEqual(
                "IPowerable",
                Docs.Models.Root
                .Namespaces["Test"]
                .Namespaces["Data"]
                .Namespaces["Interfaces"]
                .Types["IPowerable"]
                .GetName());
        }

        [Test]
        public void EnumerationsExist()
        {
            Assert.AreEqual(
                "EngineSize",
                Docs.Models.Root
                .Namespaces["Test"]
                .Namespaces["Data"]
                .Namespaces["Enumerations"]
                .Types["EngineSize"]
                .GetName());
        }        
    }
}