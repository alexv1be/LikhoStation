using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Forms;
using LikhoStation.src.Core;
using LikhoStation.src.Models;

namespace LikhoStation.Tests.CoreTests
{
    [TestClass]
    public class PhysicsTests
    {
        [TestMethod]
        public void Update_PressRight_ShouldMovePlayerAndFaceRight()
        {
            // Arrange
            var engine = new GameController(1920, 1080);
            engine.LoadScene("AbandonedTrain"); // Грузим уровень без стен в начале

            var initialX = engine.Player.Pos.X;
            var keys = new HashSet<Keys> { Keys.D }; // Жмем 'D'

            // Act
            engine.Update(keys);

            // Assert
            Assert.IsTrue(engine.Player.Pos.X > initialX, "Позиция X должна увеличиться при движении вправо");
            Assert.IsTrue(engine.Player.FacingRight, "Флаг FacingRight должен стать true");
            Assert.IsTrue(engine.Player.IsMoving, "Флаг IsMoving должен быть включен");
        }

        [TestMethod]
        public void Update_PressSpace_ShouldMakePlayerJump()
        {
            // Arrange
            var engine = new GameController(1920, 1080);
            engine.OnSingleKeyPress(Keys.Enter);
            engine.LoadScene("Street"); // На улице прыгать можно

            engine.Player.IsGrounded = true; // Искусственно ставим на землю
            var keys = new HashSet<Keys> { Keys.Space };

            // Act
            engine.Update(keys);

            // Assert
            Assert.IsFalse(engine.Player.IsGrounded, "Игрок должен оторваться от земли");
            Assert.AreEqual(engine.Player.JumpPower + 1.2f, engine.Player.VelocityY, "Скорость по Y должна стать равна силе прыжка с учетом одного кадра гравитации");
        }
    }
}