using BBot.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.IO;

namespace BBot.Test
{
    
    
    /// <summary>
    ///This is a test class for PlayingStateTest and is intended
    ///to contain all PlayingStateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PlayingStateTest
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
        ///A test for ScanGrid
        ///</summary>
        [TestMethod()]
        [DeploymentItem("BBot.exe")]
        public void ScanGridTest()
        {
            PlayingState_Accessor target = new PlayingState_Accessor(); // TODO: Initialize to an appropriate value
            target.bmpBoard = GetBitmap();
            
            target.ScanGrid();
            
        }

        public Bitmap GetBitmap()
        {
            return (Bitmap)Bitmap.FromFile(@"C:\Users\mrsheen\github\bbot\BBot\Assets\raw source.bmp");
        }
    }
}
