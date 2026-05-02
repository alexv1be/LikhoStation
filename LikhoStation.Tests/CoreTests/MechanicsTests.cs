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
            var engine = new GameController(1920, 1080);
            var keys = new HashSet<Keys> { Keys.ShiftKey };

            // Act 1: Проверяем реальный мир (Кухня)
            engine.LoadScene("Kitchen");
            engine.Update(keys);
            Assert.IsFalse(engine.Player.IsFocusMode, "Чутье НЕ должно работать в реальном мире");

            // Act 2: Проверяем изнанку (Заброшенный поезд)
            engine.LoadScene("AbandonedTrain");
            engine.Update(keys);
            Assert.IsTrue(engine.Player.IsFocusMode, "Чутье должно включаться в изнанке");
        }

        [TestMethod]
        public void UpdateInput_Exhaustion_ShouldTriggerWhenOxygenIsZero()
        {
            // Arrange
            var engine = new GameController(1920, 1080);
            engine.LoadScene("AbandonedTrain");

            // Искусственно выкачиваем кислород
            engine.Player.Oxygen = 0;
            var emptyKeys = new HashSet<Keys>(); // Ничего не нажимаем

            // Act
            engine.Update(emptyKeys); // Прогоняем один кадр логики

            // Assert
            Assert.IsTrue(engine.Player.IsExhausted, "Одышка должна появиться при нулевом кислороде");
        }
    }
}