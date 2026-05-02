using Microsoft.VisualStudio.TestTools.UnitTesting;
using LikhoStation.src.Models;

namespace LikhoStation.Tests.ModelsTests
{
    [TestClass]
    public class PlayerTests
    {
        [TestMethod]
        public void Player_ShouldHaveCorrectDefaultStats()
        {
            // Arrange & Act
            var player = new Player();

            // Assert
            Assert.AreEqual(100f, player.Oxygen, "Начальный кислород должен быть 100");
            Assert.AreEqual(100f, player.MaxOxygen, "Максимальный кислород должен быть 100");
            Assert.IsFalse(player.IsExhausted, "Игрок не должен рождаться с одышкой");
            Assert.IsTrue(player.FacingRight, "По умолчанию игрок должен смотреть вправо");
            Assert.AreEqual(-24f, player.JumpPower, "Сила прыжка должна быть -24f");
        }
    }
}