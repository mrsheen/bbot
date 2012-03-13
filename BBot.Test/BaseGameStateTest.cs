using BBot.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BBot;
using System.Drawing;

namespace BBot.Test
{
    
    
    /// <summary>
    ///This is a test class for BaseGameStateTest and is intended
    ///to contain all BaseGameStateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BaseGameStateTest
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
        ///A test for FindStateFromScreen
        ///</summary>
        [TestMethod()]
        public void FindStateFromScreenTest()
        {
            GameEngine game = new GameEngine(new Rectangle(1920,0,1440,900));

            game.GameExtents = new Rectangle(2326, 333 ,525 ,435);
            //game.GameExtents = new Rectangle(1920 + 203, 333, 523, 432);

            PlayNowState target = new PlayNowState(); // TODO: Initialize to an appropriate value
            bool quickCheck = true; // TODO: Initialize to an appropriate value
            MatchingPoint expected = new MatchingPoint(); // TODO: Initialize to an appropriate value
            expected.Certainty = 151380;
            expected.X = 203;
            expected.Y = 333;
            expected.MaxCertaintyDelta = 92.580321039088346;
            expected.MinimumCertainty = 200000;
            expected.Resolution = 1;
            expected.Confident = true;

            
            MatchingPoint actual;
            target.Init(game);
            actual = target.FindStateFromScreen(quickCheck);
            Assert.AreEqual(expected.Certainty, actual.Certainty);
            
        }
    }
}
