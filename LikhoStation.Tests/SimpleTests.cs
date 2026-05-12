using Microsoft.VisualStudio.TestTools.UnitTesting;
using LikhoStation.src.Models;

namespace LikhoStation.Tests
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public void NewPlayer_HasFullOxygen()
        {
            var player = new Player();

            Assert.AreEqual(100f, player.Oxygen, "При старте у игрока должно быть 100 кислорода");
            Assert.AreEqual(100f, player.MaxOxygen, "Максимальный запас должен быть 100");
        }

        [TestMethod]
        public void NewPlayer_IsNotMovingByDefault()
        {
            var player = new Player();

            Assert.IsFalse(player.IsMoving, "Игрок не должен двигаться при спавне");
            Assert.IsFalse(player.IsExhausted, "Игрок не должен быть уставшим при спавне");
        }

        [TestMethod]
        public void Enemy_HasCorrectDefaultStats()
        {
            var enemy = new Enemy();

            Assert.AreEqual(3.5f, enemy.Speed, "Стандартная скорость врага должна быть 3.5");
            Assert.IsTrue(enemy.MovingRight, "По умолчанию враг должен идти вправо");
        }
    }
}