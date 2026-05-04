using LikhoStation.src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Core
{
    public partial class GameController
    {
        // ИИ ВРАГОВ
        private void UpdateEnemies()
        {
            if (CurrentLevel == null) return;

            foreach (var enemy in CurrentLevel.Enemies)
            {
                PatrolEnemy(enemy);
                CheckEnemyCollision(enemy);
            }
        }

        // Генератор случайных чисел для непредсказуемости
        private static Random rnd = new Random();

        private void PatrolEnemy(Enemy enemy)
        {
            if (enemy.IsUnpredictable)
            {
                enemy.BehaviorTimer--;
                if (enemy.BehaviorTimer <= 0)
                {
                    enemy.MovingRight = rnd.Next(2) == 0;
                    enemy.BehaviorTimer = rnd.Next(50, 150);
                }
            }

            if (enemy.MovingRight)
            {
                enemy.Pos.X += enemy.Speed;
                if (enemy.Pos.X >= enemy.PatrolEndX)
                {
                    enemy.MovingRight = false;
                    if (enemy.IsUnpredictable) enemy.BehaviorTimer = rnd.Next(50, 150);
                }
            }
            else
            {
                enemy.Pos.X -= enemy.Speed;
                if (enemy.Pos.X <= enemy.PatrolStartX)
                {
                    enemy.MovingRight = true;
                    if (enemy.IsUnpredictable) enemy.BehaviorTimer = rnd.Next(50, 150);
                }
            }
        }

        private void CheckEnemyCollision(Enemy enemy)
        {
            var dx = Player.Pos.X + Player.Size.Width / 2 - (enemy.Pos.X + enemy.Size.Width / 2);
            var dy = Player.Pos.Y + Player.Size.Height / 2 - (enemy.Pos.Y + enemy.Size.Height / 2);
            var dist = Math.Sqrt(dx * dx + dy * dy);

            enemy.IsPlayerNear = dist < enemy.WarningRadius;

            if (dist < enemy.KillRadius && !Player.IsHoldingBreath)
                LoadScene(CurrentLevel.Name);
        }
    }
}
