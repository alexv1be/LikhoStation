using System.Drawing;
using System.Drawing.Drawing2D;

namespace LikhoStation
{
    public class GameRenderer
    {
        private int visibilityRadius = 350;
        private int blinkCounter = 0;

        public void Draw(Graphics g, GameController engine, int screenWidth, int screenHeight)
        {
            if (engine.State == GameState.MainMenu)
            {
                DrawMainMenu(g, engine, screenWidth, screenHeight);
                return;
            }

            blinkCounter++;
            Level level = engine.CurrentLevel;
            Player p = engine.Player;

            if (p.IsFocusMode) g.Clear(Color.FromArgb(5, 5, 10));
            else if (level.Name == "Kitchen") g.Clear(Color.FromArgb(50, 40, 40));
            else if (level.Name == "Street") g.Clear(Color.FromArgb(20, 25, 35));
            else if (level.Name == "SubwayDescent") g.Clear(Color.FromArgb(30, 30, 35)); // Под землей темнее

            // Сдвигаем камеру теперь по двум осям (X и Y)
            g.TranslateTransform(-engine.CameraOffsetX, -engine.CameraOffsetY);

            // Отрисовка геометрии уровня
            foreach (var plat in level.Platforms)
            {
                if (p.IsFocusMode) g.DrawRectangle(new Pen(Color.FromArgb(50, 255, 255, 255), 2), plat.X, plat.Y, plat.Width, plat.Height);
                else if (level.Name == "Kitchen") g.FillRectangle(new SolidBrush(Color.FromArgb(100, 80, 80)), plat);
                else if (level.Name == "SubwayDescent") g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 45)), plat); // Цвет эскалатора
                else g.FillRectangle(Brushes.Gray, plat);
            }

            if (level.Name == "Kitchen" && !level.IsBagPickedUp)
            {
                g.FillRectangle(Brushes.SaddleBrown, level.ItemBag);
                if (p.IsFocusMode) g.DrawRectangle(new Pen(Color.Yellow, 4), level.ItemBag.X, level.ItemBag.Y, level.ItemBag.Width, level.ItemBag.Height);
            }

            // Отрисовка Яны
            Brush playerBrush = Brushes.DarkRed;
            if (!level.IsRealWorld)
            {
                if (p.IsExhausted) playerBrush = (blinkCounter % 16 < 8) ? Brushes.Red : Brushes.White;
                else playerBrush = p.IsHoldingBreath ? Brushes.MediumPurple : Brushes.DarkRed;
            }
            g.FillRectangle(playerBrush, p.Pos.X, p.Pos.Y, p.Size.Width, p.Size.Height);

            // Отрисовка объектов ПЕРЕДНЕГО ПЛАНА
            if (level.HasForegroundObject)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(15, 15, 20)), level.ForegroundObject);
                g.DrawString("МЕТРО", new Font("Arial", 24, FontStyle.Bold), Brushes.DarkRed, level.ForegroundObject.X + 30, level.ForegroundObject.Y + 150);
            }

            g.ResetTransform();

            // Передаем CameraOffsetY для корректной отрисовки Хмари и UI
            if (level.HasKhmar) DrawKhmar(g, p, engine.CameraOffsetX, engine.CameraOffsetY, screenWidth, screenHeight);
            DrawUI(g, level, p, engine.CameraOffsetX, engine.CameraOffsetY);

            if (engine.State == GameState.Paused)
            {
                DrawPauseMenu(g, engine, screenWidth, screenHeight);
            }
        }

        private void DrawMainMenu(Graphics g, GameController engine, int w, int h)
        {
            g.Clear(Color.Black);
            g.DrawString("СТАНЦИЯ ЛИХО", new Font("Georgia", 40, FontStyle.Bold), Brushes.DarkRed, w / 2 - 220, h / 4);

            string[] options = engine.HasSaveFile ?
                new string[] { "Новая игра", "Продолжить", "Выход" } :
                new string[] { "Новая игра", "Выход" };

            for (int i = 0; i < options.Length; i++)
            {
                Brush color = (i == engine.MenuIndex) ? Brushes.Yellow : Brushes.Gray;
                string text = (i == engine.MenuIndex) ? "> " + options[i] + " <" : options[i];
                g.DrawString(text, new Font("Arial", 20), color, w / 2 - 100, h / 2 + (i * 50));
            }

            g.DrawString("Управление: Стрелочки/WASD - Навигация, Enter - Выбор", new Font("Arial", 12), Brushes.DimGray, 20, h - 50);
        }

        private void DrawPauseMenu(Graphics g, GameController engine, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, w, h);
            g.DrawString("ПАУЗА", new Font("Georgia", 40, FontStyle.Bold), Brushes.White, w / 2 - 100, h / 4);

            string[] options = { "Продолжить", "Сохранить и выйти в меню", "Выход из игры" };

            for (int i = 0; i < options.Length; i++)
            {
                Brush color = (i == engine.MenuIndex) ? Brushes.Yellow : Brushes.White;
                string text = (i == engine.MenuIndex) ? "> " + options[i] + " <" : options[i];
                g.DrawString(text, new Font("Arial", 20), color, w / 2 - 180, h / 2 + (i * 50));
            }
        }

        private void DrawKhmar(Graphics g, Player p, float camX, float camY, int width, int height)
        {
            using (GraphicsPath wallPath = new GraphicsPath())
            {
                wallPath.AddRectangle(new Rectangle(0, 0, width, height));

                // Корректировка позиций Хмари с учетом CameraOffsetY
                float screenPlayerX = p.Pos.X - camX;
                float screenPlayerY = p.Pos.Y - camY;

                float centerX = screenPlayerX + p.Size.Width / 2;
                float centerY = screenPlayerY + p.Size.Height / 3;

                wallPath.AddEllipse(centerX - visibilityRadius, centerY - visibilityRadius, visibilityRadius * 2, visibilityRadius * 2);

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(250, 5, 5, 10)))
                {
                    g.FillPath(brush, wallPath);
                }
            }
        }

        private void DrawUI(Graphics g, Level level, Player p, float camX, float camY)
        {
            if (level.Name == "Kitchen")
            {
                g.DrawString("КУХНЯ БАБУШКИ (Реальный мир)", new Font("Arial", 20), Brushes.LightGray, 40, 40);
                if (!level.IsBagPickedUp && level.IsNearBag)
                {
                    float bagScreenX = level.ItemBag.X - camX;
                    float bagScreenY = level.ItemBag.Y - camY; // UI тоже должен учитывать камеру
                    g.DrawString("Взять (E)", new Font("Arial", 16, FontStyle.Bold), Brushes.Yellow, bagScreenX - 10, bagScreenY - 30);
                }
            }
            else if (level.Name == "Street")
            {
                g.DrawString("УЛИЦА (Путь к метро)", new Font("Arial", 20), Brushes.LightSteelBlue, 40, 40);
            }
            else if (level.Name == "SubwayDescent")
            {
                g.DrawString("СПУСК В МЕТРО", new Font("Arial", 20), Brushes.DarkGray, 40, 40);
            }

            if (!level.IsRealWorld)
            {
                Brush textBrush = p.IsExhausted ? Brushes.Red : Brushes.White;
                string text = p.IsExhausted ? "ОДЫШКА!" : "ДЫХАНИЕ (Удерживай C)";

                g.DrawString(text, new Font("Arial", 10, FontStyle.Bold), textBrush, 40, 80);
                g.DrawRectangle(Pens.White, 40, 100, 200, 15);
                g.FillRectangle(p.IsExhausted ? Brushes.Red : Brushes.LightSkyBlue, 41, 101, (p.Oxygen / p.MaxOxygen) * 198, 13);
            }

            if (p.IsFocusMode) g.DrawString("ЧУТЬЕ ХУДОЖНИКА АКТИВНО", new Font("Arial", 12, FontStyle.Italic), Brushes.Cyan, 40, 130);
        }
    }
}