using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Forms;
using LikhoStation.src.Core;

namespace LikhoStation.Tests
{
    [TestClass]
    public class OxygenTests
    {
        [TestMethod]
        public void HoldingBreath_InKhmar_DecreasesOxygen()
        {
            var controller = new GameController();
            controller.LoadScene("AbandonedTrain");
            var initialOxygen = controller.Player.Oxygen;
            var keys = new HashSet<Keys> { Keys.C };

            controller.Update(keys);

            Assert.IsTrue(controller.Player.Oxygen < initialOxygen, "Кислород должен тратиться при зажатии С в изнанке");
            Assert.IsTrue(controller.Player.IsHoldingBreath, "Флаг задержки дыхания должен быть true");
        }

        [TestMethod]
        public void HoldingBreath_InRealWorld_DoesNotDecreaseOxygen()
        {
            var controller = new GameController();
            controller.LoadScene("Street");
            var keys = new HashSet<Keys> { Keys.C };

            controller.Update(keys);

            Assert.AreEqual(100f, controller.Player.Oxygen, "В реальном мире кислород не должен тратиться");
            Assert.IsFalse(controller.Player.IsHoldingBreath, "В реальном мире задержка дыхания не работает");
        }

        [TestMethod]
        public void Oxygen_Depleted_SetsExhaustedFlag()
        {
            var controller = new GameController();
            controller.LoadScene("AbandonedTrain");
            controller.Player.Oxygen = -1f;

            controller.Update(new HashSet<Keys>());

            Assert.IsTrue(controller.Player.IsExhausted, "Одышка должна появиться при нулевом кислороде");
        }
    }
}