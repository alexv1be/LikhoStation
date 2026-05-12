using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Forms;
using LikhoStation.src.Core;

namespace LikhoStation.Tests.CoreTests
{
    [TestClass]
    public class MechanicsTests
    {
        [TestMethod]
        public void UpdateInput_FocusMode_ShouldTurnOnOnlyInUnderworld()
        {
            // Arrange
            var controller = new GameController();
            var keys = new HashSet<Keys> { Keys.ShiftKey };

            // Act 1: Проверяем реальный мир (Кухня)
            controller.LoadScene("Kitchen");
            controller.Update(keys);
            Assert.IsFalse(controller.Player.IsFocusMode, "Чутье НЕ должно работать в реальном мире");

            // Act 2: Проверяем изнанку (Заброшенный поезд)
            controller.LoadScene("AbandonedTrain");
            controller.Update(keys);
            Assert.IsTrue(controller.Player.IsFocusMode, "Чутье должно включаться в изнанке");
        }

        [TestMethod]
        public void UpdateInput_Exhaustion_ShouldTriggerWhenOxygenIsZero()
        {
            // Arrange
            var controller = new GameController();
            controller.LoadScene("AbandonedTrain");

            // Искусственно выкачиваем кислород
            controller.Player.Oxygen = 0;
            var emptyKeys = new HashSet<Keys>(); // Ничего не нажимаем

            // Act
            controller.Update(emptyKeys); // Прогоняем один кадр логики

            // Assert
            Assert.IsTrue(controller.Player.IsExhausted, "Одышка должна появиться при нулевом кислороде");
        }
    }
}