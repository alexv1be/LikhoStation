using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace LikhoStation
{
    public class GameRenderer
    {
        private int visibilityRadius = 350;
        private int blinkCounter = 0;

        private Image menuBackground;
        private Image kitchenBackground;

        // Инструменты для своего шрифта
        private PrivateFontCollection customFonts = new PrivateFontCollection();
        private FontFamily pixelFont;

        public GameRenderer()
        {
            if (File.Exists(@"images\menu.png")) menuBackground = Image.FromFile(@"images\menu.png");
            if (File.Exists(@"images\kitchen.png")) kitchenBackground = Image.FromFile(@"images\kitchen.png");

            // Подгружаем кастомный пиксельный шрифт
            if (File.Exists(@"images\LCD40x2Display-Regular.otf"))
            {
                customFonts.AddFontFile(@"images\LCD40x2Display-Regular.otf");
                pixelFont = customFonts.Families[0];
            }
            else pixelFont = new FontFamily("Courier New"); // Резервный, если файл не найден
        }

        // ГЛАВНЫЙ МЕТОД ОТРИСОВКИ
        public void Draw(Graphics g, GameController engine, int screenWidth, int screenHeight)
        {
            if (engine.State == GameState.MainMenu) { DrawMainMenu(g, engine, screenWidth, screenHeight); return; }

            blinkCounter++;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit; // Отключаем сглаживание для пиксель-арта

            DrawBackground(g, engine, screenWidth, screenHeight);

            g.TranslateTransform(-engine.CameraOffsetX, -engine.CameraOffsetY);
            DrawGeometry(g, engine);
            DrawPlayerAndForeground(g, engine);
            g.ResetTransform();

            if (engine.CurrentLevel.HasKhmar) DrawKhmar(g, engine.Player, engine.CameraOffsetX, engine.CameraOffsetY, screenWidth, screenHeight);
            DrawUI(g, engine.CurrentLevel, engine.Player, engine.CameraOffsetX, engine.CameraOffsetY);

            if (engine.State == GameState.Paused) DrawPauseMenu(g, engine, screenWidth, screenHeight);
        }

        // УНИВЕРСАЛЬНАЯ ОБВОДКА ТЕКСТА
        private void DrawOutlineText(Graphics g, string text, Font f, Brush c, int x, int y, bool isStrong = false)
        {
            Brush ob = Brushes.Black;
            int t = isStrong ? 4 : 2;

            g.DrawString(text, f, ob, x - t, y);
            g.DrawString(text, f, ob, x + t, y);
            g.DrawString(text, f, ob, x, y - t);
            g.DrawString(text, f, ob, x, y + t);

            if (isStrong)
            {
                g.DrawString(text, f, ob, x - t, y - t);
                g.DrawString(text, f, ob, x + t, y - t);
                g.DrawString(text, f, ob, x - t, y + t);
                g.DrawString(text, f, ob, x + t, y + t);
            }
            g.DrawString(text, f, c, x, y);
        }

        // МЕНЮ
        private void DrawMainMenu(Graphics g, GameController engine, int w, int h)
        {
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            if (menuBackground != null) g.DrawImage(menuBackground, 0, 0, w, h);
            else g.Clear(Color.Black);

            DrawOutlineText(g, "СТАНЦИЯ ЛИХО", new Font(pixelFont, 24), Brushes.White, w / 2 - 250, h / 4, true);

            string[] opts = engine.HasSaveFile ? new[] { "Новая игра", "Продолжить", "Выход" } : new[] { "Новая игра", "Выход" };
            Font menuFont = new Font(pixelFont, 14);

            for (int i = 0; i < opts.Length; i++)
            {
                Brush color = (i == engine.MenuIndex) ? Brushes.Cyan : Brushes.White;
                string text = (i == engine.MenuIndex) ? "> " + opts[i] + " <" : opts[i];
                DrawOutlineText(g, text, menuFont, color, w / 2 + 150, h / 2 + (i * 50));
            }

            DrawOutlineText(g, "Управление: WASD - Навигация, Enter - Выбор", new Font(pixelFont, 8), Brushes.White, 20, h - 30);
        }

        private void DrawPauseMenu(Graphics g, GameController engine, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, w, h);

            DrawOutlineText(g, "ПАУЗА", new Font(pixelFont, 24), Brushes.White, w / 2 - 100, h / 4, true);

            string[] options = { "Продолжить", "Сохранить и выйти", "Выход из игры" };
            Font menuFont = new Font(pixelFont, 14);

            for (int i = 0; i < options.Length; i++)
            {
                Brush color = (i == engine.MenuIndex) ? Brushes.Cyan : Brushes.White;
                string text = (i == engine.MenuIndex) ? "> " + options[i] + " <" : options[i];
                DrawOutlineText(g, text, menuFont, color, w / 2 - 180, h / 2 + (i * 50));
            }
        }

        // КОМПОНЕНТЫ МИРА
        private void DrawBackground(Graphics g, GameController engine, int w, int h)
        {
            Level level = engine.CurrentLevel;
            if (engine.Player.IsFocusMode) g.Clear(Color.FromArgb(5, 5, 10));
            else if (level.Name == "Kitchen")
            {
                if (kitchenBackground != null) g.DrawImage(kitchenBackground, 0, 0, w, h);
                else g.Clear(Color.FromArgb(50, 40, 40));
            }
            else if (level.Name == "Street") g.Clear(Color.FromArgb(20, 25, 35));
            else if (level.Name == "SubwayDescent") g.Clear(Color.FromArgb(30, 30, 35));
        }

        private void DrawGeometry(Graphics g, GameController engine)
        {
            Level level = engine.CurrentLevel;
            foreach (var plat in level.Platforms)
            {
                if (engine.Player.IsFocusMode) g.DrawRectangle(new Pen(Color.FromArgb(50, 255, 255, 255), 2), plat.X, plat.Y, plat.Width, plat.Height);
                else if (level.Name == "SubwayDescent") g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 45)), plat);
                else if (level.Name != "Kitchen") g.FillRectangle(Brushes.Gray, plat);
            }

            if (level.Name == "Kitchen" && !level.IsBagPickedUp)
            {
                g.FillRectangle(Brushes.SaddleBrown, level.ItemBag);
                if (engine.Player.IsFocusMode) g.DrawRectangle(new Pen(Color.Yellow, 4), level.ItemBag.X, level.ItemBag.Y, level.ItemBag.Width, level.ItemBag.Height);
            }
        }

        private void DrawPlayerAndForeground(Graphics g, GameController engine)
        {
            Level level = engine.CurrentLevel;
            Player p = engine.Player;
            Brush pBrush = p.IsExhausted ? ((blinkCounter % 16 < 8) ? Brushes.Red : Brushes.White) : (p.IsHoldingBreath ? Brushes.MediumPurple : Brushes.DarkRed);

            g.FillRectangle(level.IsRealWorld ? Brushes.DarkRed : pBrush, p.Pos.X, p.Pos.Y, p.Size.Width, p.Size.Height);

            if (level.HasForegroundObject)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(15, 15, 20)), level.ForegroundObject);
                DrawOutlineText(g, "МЕТРО", new Font(pixelFont, 24), Brushes.DarkRed, (int)level.ForegroundObject.X + 30, (int)level.ForegroundObject.Y + 150);
            }
        }

        private void DrawKhmar(Graphics g, Player p, float camX, float camY, int width, int height)
        {
            using (GraphicsPath wallPath = new GraphicsPath())
            {
                wallPath.AddRectangle(new Rectangle(0, 0, width, height));
                float centerX = (p.Pos.X - camX) + p.Size.Width / 2;
                float centerY = (p.Pos.Y - camY) + p.Size.Height / 3;
                wallPath.AddEllipse(centerX - visibilityRadius, centerY - visibilityRadius, visibilityRadius * 2, visibilityRadius * 2);
                g.FillPath(new SolidBrush(Color.FromArgb(250, 5, 5, 10)), wallPath);
            }
        }

        // ИНТЕРФЕЙС
        private void DrawUI(Graphics g, Level level, Player p, float camX, float camY)
        {
            Font uiFont = new Font(pixelFont, 12);

            if (level.Name == "Kitchen") DrawKitchenUI(g, level, camX, camY);
            else if (level.Name == "Street") DrawOutlineText(g, "УЛИЦА", uiFont, Brushes.LightSteelBlue, 40, 40);
            else if (level.Name == "SubwayDescent") DrawOutlineText(g, "СПУСК В МЕТРО", uiFont, Brushes.DarkGray, 40, 40);

            if (!level.IsRealWorld) DrawOxygenUI(g, p);
            if (p.IsFocusMode) DrawOutlineText(g, "ЧУТЬЕ АКТИВНО", new Font(pixelFont, 12), Brushes.Cyan, 40, 130);
        }

        private void DrawKitchenUI(Graphics g, Level level, float camX, float camY)
        {
            DrawOutlineText(g, "Кухня бабушки", new Font(pixelFont, 12), Brushes.White, 40, 40);

            if (!level.IsBagPickedUp && level.IsNearBag)
            {
                float bagX = level.ItemBag.X - camX;
                float bagY = level.ItemBag.Y - camY;
                DrawOutlineText(g, "Взять сумку (E)", new Font(pixelFont, 10), Brushes.White, (int)bagX - 40, (int)bagY - 30);
            }
        }

        private void DrawOxygenUI(Graphics g, Player p)
        {
            Brush textBrush = p.IsExhausted ? Brushes.Red : Brushes.White;
            string text = p.IsExhausted ? "ОДЫШКА!" : "ДЫХАНИЕ (Удерживай C)";

            DrawOutlineText(g, text, new Font(pixelFont, 8), textBrush, 40, 80);
            g.DrawRectangle(Pens.White, 40, 100, 200, 15);
            g.FillRectangle(p.IsExhausted ? Brushes.Red : Brushes.LightSkyBlue, 41, 101, (p.Oxygen / p.MaxOxygen) * 198, 13);
        }
    }
}