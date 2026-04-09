using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Forms;
using LikhoStation;

namespace LikhoStation.Tests
{
    [TestClass]
    public class GameLogicTests
    {
        [TestMethod]
        public void LoadScene_Kitchen_SetsCorrectPlayerSize()
        {
            var controller = new GameController(1920, 1080);
            controller.LoadScene("Kitchen");

            Assert.AreEqual("Kitchen", controller.CurrentLevel.Name);
            Assert.AreEqual(100, controller.Player.Size.Width);
            Assert.AreEqual(200, controller.Player.Size.Height);
        }

        [TestMethod]
        public void Update_PressingRight_MovesPlayerRight()
        {
            var controller = new GameController(1920, 1080);
            controller.LoadScene("Street");
            controller.OnSingleKeyPress(Keys.Enter);

            float startX = controller.Player.Pos.X;
            var keys = new HashSet<Keys> { Keys.D };

            controller.Update(keys);

            Assert.AreEqual(startX + controller.Player.Speed, controller.Player.Pos.X);
        }

        [TestMethod]
        public void Update_HoldingCOnKitchen_DoesNotDecreaseOxygen()
        {
            var controller = new GameController(1920, 1080);
            controller.LoadScene("Kitchen");
            controller.OnSingleKeyPress(Keys.Enter);

            var keys = new HashSet<Keys> { Keys.C };
            float maxOxygen = controller.Player.MaxOxygen;

            controller.Update(keys);

            Assert.AreEqual(maxOxygen, controller.Player.Oxygen);
            Assert.IsFalse(controller.Player.IsHoldingBreath);
        }
    }
}