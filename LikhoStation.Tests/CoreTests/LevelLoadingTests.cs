using Microsoft.VisualStudio.TestTools.UnitTesting;
using LikhoStation.src.Core;

namespace LikhoStation.Tests
{
    [TestClass]
    public class LevelLoadingTests
    {
        [TestMethod]
        public void LoadScene_LadnyForest_SetsCorrectParameters()
        {
            var controller = new GameController();

            controller.LoadScene("LadnyForest");

            Assert.AreEqual("LadnyForest", controller.CurrentLevel.Name);
            Assert.IsTrue(controller.CurrentLevel.HasKhmar);
            Assert.IsFalse(controller.CurrentLevel.IsRealWorld);
            Assert.AreEqual(18.0f, controller.Player.Speed);
            Assert.AreEqual(22.0f, controller.Player.AirSpeed);
        }

        [TestMethod]
        public void LoadScene_ResetsPlayerStats()
        {
            var controller = new GameController();
            controller.Player.Oxygen = 10f;
            controller.Player.IsExhausted = true;

            controller.LoadScene("Street");

            Assert.AreEqual(100f, controller.Player.Oxygen);
            Assert.IsFalse(controller.Player.IsExhausted);
            Assert.AreEqual(-24.0f, controller.Player.JumpPower);
        }
    }
}