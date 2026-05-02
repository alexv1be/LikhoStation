using LikhoStation.src.Core;
using LikhoStation.src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Rendering
{
    public partial class GameRenderer
    {
        // СУЩЕСТВА

        /// <summary>
        /// Выбор нужного кадра анимации персонажа в зависимости от её состояния.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        private void DrawPlayerAndForeground(Graphics g, GameController engine)
        {
            var level = engine.CurrentLevel;
            var p = engine.Player;
            var isK = level.Name == "Kitchen";
            var idleImg = isK ? yanaHoodieIdle : yanaCoatIdle;
            var s = idleImg;

            if (!p.IsGrounded && !isK)
            {
                if (yanaCoatWalk.Count > 1) s = yanaCoatWalk[1];
            }
            else if (p.IsMoving && p.IsGrounded)
            {
                var walkList = isK ? yanaHoodieWalk : yanaCoatWalk;
                if (p.WalkFrame == 0 && walkList.Count > 0) s = walkList[0];
                else if (p.WalkFrame == 2 && walkList.Count > 1) s = walkList[1];
            }

            if (s != null) DrawEntitySprite(g, s, p);

            if (level.Name == "SubwayDescent" && subwayFg != null && !p.IsFocusMode && !engine.IsDevMode)
                g.DrawImage(subwayFg, 0, 0, level.WorldWidth, 2700);
        }

        /// <summary>
        /// Вспомогательный метод для отражения спрайта (ScaleTransform -1), если персонаж идет влево.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="s"></param>
        /// <param name="p"></param>
        private void DrawEntitySprite(Graphics g, Image s, Player p)
        {
            var state = g.Save();

            if (!p.FacingRight)
            {
                g.TranslateTransform(p.Pos.X * 2 + p.Size.Width, 0);
                g.ScaleTransform(-1, 1);
            }

            g.DrawImage(s, p.Pos.X, p.Pos.Y, p.Size.Width, p.Size.Height);

            g.Restore(state);
        }

        /// <summary>
        /// Цикл отрисовки всех врагов на уровне.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        private void DrawEnemies(Graphics g, GameController engine)
        {
            if (engine.CurrentLevel.Enemies == null) return;

            foreach (var enemy in engine.CurrentLevel.Enemies)
            {
                var enemyBrush = new SolidBrush(Color.FromArgb(200, 10, 10, 20));
                g.FillRectangle(enemyBrush, enemy.Pos.X, enemy.Pos.Y, enemy.Size.Width, enemy.Size.Height);

                if (engine.Player.IsFocusMode)
                {
                    DrawEnemyHearingRadius(g, enemy);
                }
            }
        }

        /// <summary>
        /// Отрисовка красного круга «зоны смерти» вокруг врага в режиме чутья.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="enemy"></param>
        private void DrawEnemyHearingRadius(Graphics g, Enemy enemy)
        {
            var dangerColor = Color.FromArgb(100, 255, 0, 0); // Красная зона смерти

            using (var p = new Pen(dangerColor, 3))
            {
                var x = enemy.Pos.X + enemy.Size.Width / 2 - enemy.KillRadius;
                var y = enemy.Pos.Y + enemy.Size.Height / 2 - enemy.KillRadius;
                var diameter = enemy.KillRadius * 2;

                g.DrawEllipse(p, x, y, diameter, diameter);
            }
        }
    }
}
