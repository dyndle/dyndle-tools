using System;
using Dyndle.Tools.Installer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dyndle.Tools.Installer.Test
{
    [TestClass]
    public class ModelGeneratorTest
    {
        [TestMethod]
        public void GenerateModel()
        {
            var metadataSchema = new ImportItem()
            {
                Name = "Dyndle Metadata Schema",
                ItemType = ItemType.Schema,
                ContentResourceName = "DyndleMetadata.xsd"
            };

        }
    }
}
