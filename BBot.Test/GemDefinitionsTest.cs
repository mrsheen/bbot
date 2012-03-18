using BBot.GamePieces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BBot.Test
{
    
    
    /// <summary>
    ///This is a test class for GemDefinitionsTest and is intended
    ///to contain all GemDefinitionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GemDefinitionsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for SerializeDefinition
        ///</summary>
        [TestMethod()]
        [DeploymentItem("BBot.exe")]
        public void SerializeDefinitionTest()
        {
            
            //RedMeanB    GreenMeanB    BlueMeanB
            List<Gem> listGemColorStats = new List<Gem>();

            listGemColorStats.Add(new Gem { Name = GemColor.White, Modifiers = GemModifier.None, Color = Color.FromArgb(165, 165, 165) });
            listGemColorStats.Add(new Gem { Name = GemColor.Purple, Modifiers = GemModifier.None, Color = Color.FromArgb(135, 28, 129) });
            listGemColorStats.Add(new Gem { Name = GemColor.Orange, Modifiers = GemModifier.None, Color = Color.FromArgb(192, 104, 37) });
            listGemColorStats.Add(new Gem { Name = GemColor.Red, Modifiers = GemModifier.None, Color = Color.FromArgb(204, 18, 38) });
            listGemColorStats.Add(new Gem { Name = GemColor.Yellow, Modifiers = GemModifier.None, Color = Color.FromArgb(160, 136, 30) });
            listGemColorStats.Add(new Gem { Name = GemColor.Green, Modifiers = GemModifier.None, Color = Color.FromArgb(30, 180, 58) });
            listGemColorStats.Add(new Gem { Name = GemColor.Blue, Modifiers = GemModifier.None, Color = Color.FromArgb(20, 90, 155) });


            GemDefinitions_Accessor.SerializeDefinition(listGemColorStats);
            
           List<Gem> reSerialized = GemDefinitions_Accessor.DeserializeDefinition();

            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
