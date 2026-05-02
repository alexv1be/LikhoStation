using LikhoStation.src.Core;
using LikhoStation.src.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Rendering
{
    public partial class GameRenderer
    {
        // ОКРУЖЕНИЕ

        /// <summary>
        /// Отрисовка фоновых изображений и заливка цветом в режиме разработчика.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void DrawBackground(Graphics g, GameController engine, int w, int h)
        {
            var level = engine.CurrentLevel;

            if (engine.Player.IsFocusMode || engine.IsDevMode)
            {
                g.Clear(Color.FromArgb(5, 5, 10));
                return;
            }

            else if (level.Name == "Kitchen")
            {
                if (kitchenBg != null) g.DrawImage(kitchenBg, 0, 0, w, h);
                else g.Clear(Color.FromArgb(50, 40, 40));
            }
            else if (level.Name == "Street")
            {
                if (streetBg != null) g.DrawImage(streetBg, -engine.CameraOffsetX, 0, level.WorldWidth, h);
                else g.Clear(Color.FromArgb(20, 25, 35));
            }
            else if (level.Name == "SubwayDescent")
            {
                if (subwayBg != null) g.DrawImage(subwayBg, -engine.CameraOffsetX, -engine.CameraOffsetY, level.WorldWidth, 2700);
                else g.Clear(Color.FromArgb(30, 30, 35));
            }
            else if (level.Name == "AbandonedTrain")
            {
                if (abandonedTrainBg != null) g.DrawImage(abandonedTrainBg, 0, 0, w, h);
                else g.Clear(Color.Black);
            }
        }

        /// <summary>
        /// Отрисовка черных блоков платформ и триггеров с белыми рамками.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        private void DrawGeometry(Graphics g, GameController engine)
        {
            var level = engine.CurrentLevel;

            var allVisualBlocks = new List<RectangleF>();
            allVisualBlocks.AddRange(level.Platforms);
            allVisualBlocks.AddRange(level.Triggers);

            foreach (var plat in allVisualBlocks)
            {
                if (engine.IsDevMode)
                {
                    g.FillRectangle(Brushes.Black, plat);
                    g.DrawRectangle(Pens.White, plat.X, plat.Y, plat.Width, plat.Height);
                }
                else if (engine.Player.IsFocusMode)
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(50, 255, 255, 255), 2), plat.X, plat.Y, plat.Width, plat.Height);
                }
                else if (level.Name != "Kitchen" && level.Name != "Street" && level.Name != "SubwayDescent" && level.Name != "AbandonedTrain")
                {
                    g.FillRectangle(Brushes.Gray, plat);
                }
            }

            if (level.Name == "Kitchen" && !level.IsBagPickedUp)
            {
                if (engine.IsDevMode)
                {
                    g.FillRectangle(Brushes.Black, level.ItemBag);
                    g.DrawRectangle(Pens.White, level.ItemBag.X, level.ItemBag.Y, level.ItemBag.Width, level.ItemBag.Height);
                }
                else
                {
                    if (bagImg != null && !engine.Player.IsFocusMode)
                        g.DrawImage(bagImg, level.ItemBag);
                    else if (!engine.Player.IsFocusMode)
                        g.FillRectangle(Brushes.SaddleBrown, level.ItemBag);

                    if (engine.Player.IsFocusMode)
                    {
                        g.DrawRectangle(new Pen(Color.Yellow, 4), level.ItemBag.X, level.ItemBag.Y, level.ItemBag.Width, level.ItemBag.Height);
                    }
                }
            }
        }

        /// <summary>
        /// Математика и отрисовка радиального градиента тумана вокруг персонажа.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="p"></param>
        /// <param name="camX"></param>
        /// <param name="camY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void DrawKhmar(Graphics g, Player p, float camX, float camY, int width, int height)
        {
            var currentRadius = p.Size.Height > 500 ? 800 : 450;

            var centerX = p.Pos.X - camX + p.Size.Width / 2;
            var centerY = p.Pos.Y - camY + p.Size.Height / 3;
            var fogColor = Color.FromArgb(250, 5, 5, 10);

            using (GraphicsPath wallPath = new GraphicsPath())
            {
                wallPath.AddRectangle(new Rectangle(0, 0, width, height));
                wallPath.AddEllipse(centerX - currentRadius, centerY - currentRadius, currentRadius * 2, currentRadius * 2);
                g.FillPath(new SolidBrush(fogColor), wallPath);
            }

            using (GraphicsPath gradientPath = new GraphicsPath())
            {
                gradientPath.AddEllipse(centerX - currentRadius, centerY - currentRadius, currentRadius * 2, currentRadius * 2);
                using (PathGradientBrush pgb = new PathGradientBrush(gradientPath))
                {
                    pgb.CenterColor = Color.Transparent;
                    pgb.SurroundColors = new Color[] { fogColor };

                    g.FillPath(pgb, gradientPath);
                }
            }
        }
    }
}
