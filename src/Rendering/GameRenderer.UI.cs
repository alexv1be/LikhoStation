using LikhoStation.src.Core;
using LikhoStation.src.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Rendering
{
    public partial class GameRenderer
    {
        // ИНТЕРФЕЙС И ДИАЛОГИ

        /// <summary>
        /// Главный метод интерфейса, определяющий, какой заголовок локации сейчас показать.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        private void DrawUI(Graphics g, GameController engine)
        {
            var level = engine.CurrentLevel;
            var p = engine.Player;
            var camX = engine.CameraOffsetX;
            var camY = engine.CameraOffsetY;

            var uiFont = new Font(pixelFont, 12);
            var screenWidth = GameController.VirtualWidth;
            var screenHeight = GameController.VirtualHeight;
            var locColor = Brushes.White;

            if (level.Name == "Kitchen") DrawKitchenUI(g, level, camX, camY);
            else if (level.Name == "Street") DrawOutlineText(g, "УЛИЦА", uiFont, locColor, 40, 40);
            else if (level.Name == "SubwayDescent") DrawOutlineText(g, "СПУСК В МЕТРО", uiFont, locColor, 40, 40);
            else if (level.Name == "AbandonedTrain") DrawOutlineText(g, "ЗАБРОШЕННЫЙ ПОЕЗД", uiFont, locColor, 40, 40);
            else if (level.Name == "AbandonedStation") DrawOutlineText(g, "ЗАБРОШЕННАЯ СТАНЦИЯ", uiFont, locColor, 40, 40);
            else if (level.Name == "LifelessStreet") DrawOutlineText(g, "БЕЗЖИЗНЕННАЯ УЛИЦА", uiFont, locColor, 40, 40);
            else if (level.Name == "LadnyForest") DrawOutlineText(g, "ЛАДНЫЙ ЛЕС", uiFont, locColor, 40, 40);

            if (!level.IsRealWorld) DrawOxygenUI(g, p);

            if (p.IsFocusMode)
                DrawOutlineText(g, "ЧУТЬЕ АКТИВНО", new Font(pixelFont, 8), Brushes.Cyan, 40, 80);

            if (engine.IsDevMode)
                DrawOutlineText(g, "РЕЖИМ РАЗРАБОТЧИКА (CTRL)", new Font(pixelFont, 8), Brushes.Orange, 40, 85);

            if (!level.IsItemPickedUp && level.IsNearItem && !level.IsViewingItem)
            {
                var itemX = level.ActiveItemRect.X - camX + level.ActiveItemRect.Width / 2;
                var itemY = level.ActiveItemRect.Y - camY - 20;

                DrawOutlineText(g, "Поднять предмет (E)", new Font(pixelFont, 10), Brushes.White, (int)itemX - 80, (int)itemY);
            }

            DrawDialogUI(g, level, p, screenWidth, screenHeight);
        }

        /// <summary>
        /// Отрисовка главного меню с подсветкой выбранного пункта.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void DrawMainMenu(Graphics g, GameController engine, int w, int h)
        {
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            if (menuBg != null) g.DrawImage(menuBg, 0, 0, w, h);
            else g.Clear(Color.Black);

            DrawOutlineText(g, "СТАНЦИЯ ЛИХО", new Font(pixelFont, 24), Brushes.White, w / 2 - 250, h / 4, true);

            string[] opts = engine.HasSaveFile ? new[] { "Новая игра", "Продолжить", "Выход" } : new[] { "Новая игра", "Выход" };
            var menuFont = new Font(pixelFont, 14);

            for (int i = 0; i < opts.Length; i++)
            {
                var color = i == engine.MenuIndex ? Brushes.Cyan : Brushes.White;
                var text = i == engine.MenuIndex ? "> " + opts[i] + " <" : opts[i];
                DrawOutlineText(g, text, menuFont, color, w / 2 + 150, h / 2 + i * 50);
            }
            DrawOutlineText(g, "Управление: WS - Навигация, Enter - Выбор", new Font(pixelFont, 8), Brushes.White, 20, h - 30);
        }

        /// <summary>
        /// Отрисовка меню паузы с подсветкой выбранного пункта.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void DrawPauseMenu(Graphics g, GameController engine, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, w, h);
            DrawOutlineText(g, "ПАУЗА", new Font(pixelFont, 24), Brushes.White, w / 2 - 100, h / 4, true);

            string[] options = { "Продолжить", "Сохранить и продолжить", "Сохранить и выйти в главное меню", "Выйти в главное меню" };
            var menuFont = new Font(pixelFont, 14);

            for (int i = 0; i < options.Length; i++)
            {
                var color = i == engine.MenuIndex ? Brushes.Cyan : Brushes.White;
                var text = i == engine.MenuIndex ? "> " + options[i] + " <" : options[i];

                DrawOutlineText(g, text, menuFont, color, w / 2 - 300, h / 2 + i * 50);
            }
        }

        /// <summary>
        /// Отрисовка полоски выносливости/кислорода.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="p"></param>
        private void DrawOxygenUI(Graphics g, Player p)
        {
            var textBrush = p.IsExhausted ? Brushes.Red : Brushes.White;
            var text = p.IsExhausted ? "ОДЫШКА!" : "ДЫХАНИЕ (Удерживай C)";

            var xPos = GameController.VirtualWidth - 450;
            var barOffset = 200;

            DrawOutlineText(g, text, new Font(pixelFont, 8), textBrush, xPos, 80);

            g.DrawRectangle(Pens.White, xPos + barOffset, 120, 200, 15);

            var barWidth = p.Oxygen / p.MaxOxygen * 198;
            g.FillRectangle(p.IsExhausted ? Brushes.Red : Brushes.LightSkyBlue, xPos + barOffset + 1, 121, barWidth, 13);
        }

        /// <summary>
        /// Логика появления окон с диалогами бабушки и Яны.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="level"></param>
        /// <param name="p"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void DrawDialogUI(Graphics g, Level level, Player p, int w, int h)
        {
            if (level.DialogStep == 0) return;

            Image img = null;
            var origSize = new Size(3464, 1350);

            if (level.DialogStep == 1) { img = dialog1; origSize = new Size(3464, 1687); }
            else if (level.DialogStep == 2) { img = dialog2; origSize = new Size(3464, 1350); }
            else if (level.DialogStep == 3) { img = dialog3; origSize = new Size(3464, 1354); }

            if (img == null) return;

            var finalW = (int)(origSize.Width * 0.15f);
            var finalH = (int)(origSize.Height * 0.15f);

            var rect = GetDialogRect(level, p, finalW, finalH);

            DrawImageWithAlpha(g, img, rect, level.DialogAlpha);
        }

        /// <summary>
        /// Логика позиционирования окон с диалогами бабушки и Яны.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="p"></param>
        /// <param name="drawW"></param>
        /// <param name="drawH"></param>
        /// <returns></returns>
        private Rectangle GetDialogRect(Level level, Player p, int drawW, int drawH)
        {
            int posX, posY;

            if (level.DialogStep == 2)
            {
                posX = (int)p.Pos.X + 80 + p.Size.Width / 4 - drawW / 4;
                posY = (int)p.Pos.Y - drawH - 0;
            }
            else
            {
                posX = 1500 - drawW / 2;
                posY = 600 - drawH;
            }

            return new Rectangle(posX, posY, drawW, drawH);
        }

        /// <summary>
        /// Специфические подсказки для кухни (например, «Взять сумку»).
        /// </summary>
        /// <param name="g"></param>
        /// <param name="level"></param>
        /// <param name="camX"></param>
        /// <param name="camY"></param>
        private void DrawKitchenUI(Graphics g, Level level, float camX, float camY)
        {
            DrawOutlineText(g, "Кухня бабушки", new Font(pixelFont, 12), Brushes.White, 40, 40);

            if (!level.IsItemPickedUp && level.IsNearItem)
            {
                var bagX = level.ActiveItemRect.X - camX;
                var bagY = level.ActiveItemRect.Y - camY;
                DrawOutlineText(g, "Взять сумку (E)", new Font(pixelFont, 10), Brushes.White, (int)bagX - 40, (int)bagY - 30);
            }
        }

        /// <summary>
        /// Метод для полноэкранного затемнения.
        /// </summary>
        private void DrawFullscreenItem(Graphics g, Level level, int w, int h)
        {
            if (level == null || !level.IsViewingItem) return;

            g.FillRectangle(new SolidBrush(Color.FromArgb(220, 0, 0, 0)), 0, 0, w, h);

            Image fullImg = null;
            string description = "";

            if (level.Name == "LifelessStreet")
            {
                fullImg = phoneFullImg;
                description = "Если б я знала, где...";
            }
            else if (level.Name == "LadnyForest")
            {
                fullImg = keysFullImg;
                description = "Почему мои ключи оказались тут?..";
            }
            else if (level.Name == "Garages")
            {
                fullImg = playerFullImg;
                description = "Батарейки давно сели, но я всё равно его храню.";
            }

            if (fullImg != null)
            {
                float scale = (h * 0.6f) / fullImg.Height;
                int drawW = (int)(fullImg.Width * scale);
                int drawH = (int)(fullImg.Height * scale);
                int x = (w - drawW) / 2;
                int y = (h - drawH) / 2;

                g.DrawImage(fullImg, x, y, drawW, drawH);

                if (!string.IsNullOrEmpty(description))
                {
                    var descFont = new Font(pixelFont, 10);

                    int textX = (w - (int)g.MeasureString(description, descFont).Width) / 2;
                    int textY = y + drawH + 50;

                    var transparentBrush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));

                    g.DrawString(description, descFont, transparentBrush, textX, textY);
                }
            }
        }

        private void DrawEndingCutscene(Graphics g, GameController engine, int w, int h)
        {
            int t = engine.CurrentLevel.DialogTimer;

            int fadeAlpha = (t <= 25) ? (int)((t / 25f) * 255) : 255;
            g.FillRectangle(new SolidBrush(Color.FromArgb(Math.Clamp(fadeAlpha, 0, 255), 0, 0, 0)), 0, 0, w, h);

            if (t <= 25) return;

            if (t > 25 && t <= 75)
            {
                int a = (t <= 40) ? (int)(((t - 25) / 15f) * 255) : 255;
                DrawEndingImage(g, ending1Img, w, h, Math.Clamp(a, 0, 255));
            }

            if (t > 75 && t <= 90)
            {
                int alpha1 = (int)(((90 - t) / 15f) * 255);
                int alpha2 = (int)(((t - 75) / 15f) * 255);
                DrawEndingImage(g, ending1Img, w, h, Math.Clamp(alpha1, 0, 255));
                DrawEndingImage(g, ending2Img, w, h, Math.Clamp(alpha2, 0, 255));
            }

            if (t > 90)
            {
                int a = (t > 135) ? (int)(((150 - t) / 15f) * 255) : 255;
                DrawEndingImage(g, ending2Img, w, h, Math.Clamp(a, 0, 255));
            }
        }

        /// <summary>
        /// Вспомогательный метод для отрисовки картинки без растягивания (с полями)
        /// </summary>
        private void DrawEndingImage(Graphics g, Image img, int w, int h, int alpha)
        {
            if (img == null || alpha <= 0) return;

            float ratio = Math.Min((float)w / img.Width, (float)h / img.Height);
            int drawW = (int)(img.Width * ratio);
            int drawH = (int)(img.Height * ratio);
            int posX = (w - drawW) / 2;
            int posY = (h - drawH) / 2;

            DrawImageWithAlpha(g, img, new Rectangle(posX, posY, drawW, drawH), alpha);
        }
    }
}
