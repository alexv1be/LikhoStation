using Microsoft.VisualStudio.TestTools.UnitTesting;
using LikhoStation.src.Core;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LikhoStation.Tests
{
    [TestClass]
    public class PhysicsTests
    {
        [TestMethod]
        public void MoveRight_UpdatesPlayerPosition()
        {
            var controller = new GameController();
            controller.OnSingleKeyPress(Keys.Enter);
            controller.LoadScene("Street");
            var initialX = controller.Player.Pos.X;
            var keys = new HashSet<Keys> { Keys.D };

            controller.Update(keys);

            Assert.IsTrue(controller.Player.Pos.X > initialX);
            Assert.IsTrue(controller.Player.FacingRight);
            Assert.IsTrue(controller.Player.IsMoving);
        }

        [TestMethod]
        public void MoveLeft_UpdatesPlayerPosition()
        {
            // Arrange
            var controller = new GameController();
            // Используем лес, там точно широкое открытое пространство без стен на старте
            controller.LoadScene("LadnyForest");

            // Ставим Яну далеко от левого края (X = 1000) и четко на землю
            controller.Player.Pos = new System.Drawing.PointF(1000, controller.CurrentLevel.GroundY - controller.Player.Size.Height);
            controller.Player.IsGrounded = true;

            var initialX = controller.Player.Pos.X;
            var keys = new HashSet<Keys> { Keys.A };

            // Act
            controller.Update(keys);

            // Assert
            Assert.IsTrue(controller.Player.Pos.X < initialX, "Координата X должна была уменьшиться при движении влево");
            Assert.IsFalse(controller.Player.FacingRight, "Яна должна была повернуться влево");
        }

        [TestMethod]
        public void Jump_WhenGrounded_SetsVelocityY()
        {
            var controller = new GameController();
            controller.OnSingleKeyPress(Keys.Enter);
            controller.LoadScene("Street");
            controller.Player.IsGrounded = true;
            var keys = new HashSet<Keys> { Keys.Space };

            controller.Update(keys);

            Assert.AreEqual(controller.Player.JumpPower + 1.2f, controller.Player.VelocityY);
            Assert.IsFalse(controller.Player.IsGrounded);
        }
    }
}