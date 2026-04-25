using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;

namespace LikhoStation
{
    public class GameRenderer
    {
        private int visibilityRadius = 350;
        private int blinkCounter = 0;

        private Image menuBg;
        private Image kitchenBg;
        private Image bagImg;
        private Image streetBg;
        private Image dialog1, dialog2, dialog3;
        private Image subwayBg;
        private Image subwayFg;

        private Image yanaHoodieIdle;
        private List<Image> yanaHoodieWalk = new List<Image>();

        private Image yanaCoatIdle;
        private List<Image> yanaCoatWalk = new List<Image>();

        private PrivateFontCollection customFonts = new PrivateFontCollection();
        private FontFamily pixelFont;

        public GameRenderer()
        {
            if (File.Exists(@"Assets\Images\menu.png")) menuBg = Image.FromFile(@"Assets\Images\menu.png");
            if (File.Exists(@"Assets\Images\kitchen.png")) kitchenBg = Image.FromFile(@"Assets\Images\kitchen.png");
            if (File.Exists(@"Assets\Images\bag.png")) bagImg = Image.FromFile(@"Assets\Images\bag.png");
            if (File.Exists(@"Assets\Images\dialogue_kitchen_grandma_1.png")) dialog1 = Image.FromFile(@"Assets\Images\dialogue_kitchen_grandma_1.png");
            if (File.Exists(@"Assets\Images\dialogue_kitchen_yana_1.png")) dialog2 = Image.FromFile(@"Assets\Images\dialogue_kitchen_yana_1.png");
            if (File.Exists(@"Assets\Images\dialogue_kitchen_grandma_2.png")) dialog3 = Image.FromFile(@"Assets\Images\dialogue_kitchen_grandma_2.png");
            if (File.Exists(@"Assets\Images\street_bg.png")) streetBg = Image.FromFile(@"Assets\Images\street_bg.png");
            if (File.Exists(@"Assets\Images\subway_bg.png")) subwayBg = Image.FromFile(@"Assets\Images\subway_bg.png");
            if (File.Exists(@"Assets\Images\subway_fg.png")) subwayFg = Image.FromFile(@"Assets\Images\subway_fg.png");

            var p = @"Assets\Images\";
            if (File.Exists(p + "yana_hoodie_idle.png")) yanaHoodieIdle = Image.FromFile(p + "yana_hoodie_idle.png");
            if (File.Exists(p + "yana_hoodie_w1.png")) yanaHoodieWalk.Add(Image.FromFile(p + "yana_hoodie_w1.png"));
            if (File.Exists(p + "yana_hoodie_w2.png")) yanaHoodieWalk.Add(Image.FromFile(p + "yana_hoodie_w2.png"));
            if (File.Exists(p + "yana_coat_idle.png")) yanaCoatIdle = Image.FromFile(p + "yana_coat_idle.png");
            if (File.Exists(p + "yana_coat_w1.png")) yanaCoatWalk.Add(Image.FromFile(p + "yana_coat_w1.png"));
            if (File.Exists(p + "yana_coat_w2.png")) yanaCoatWalk.Add(Image.FromFile(p + "yana_coat_w2.png"));
            if (File.Exists(@"Assets\Fonts\LCD40x2Display-Regular.otf"))
            {
                customFonts.AddFontFile(@"Assets\Fonts\LCD40x2Display-Regular.otf");
                pixelFont = customFonts.Families[0];
            }
            else pixelFont = new FontFamily("Courier New");
        }

        public void Draw(Graphics g, GameController engine, int screenWidth, int screenHeight)
        {
            if (engine.State == GameState.MainMenu) { DrawMainMenu(g, engine, screenWidth, screenHeight); return; }

            blinkCounter++;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            DrawBackground(g, engine, screenWidth, screenHeight);

            g.TranslateTransform(-engine.CameraOffsetX, -engine.CameraOffsetY);
            DrawGeometry(g, engine);
            DrawPlayerAndForeground(g, engine);
            g.ResetTransform();

            if (engine.CurrentLevel.HasKhmar) DrawKhmar(g, engine.Player, engine.CameraOffsetX, engine.CameraOffsetY, screenWidth, screenHeight);
            DrawUI(g, engine.CurrentLevel, engine.Player, engine.CameraOffsetX, engine.CameraOffsetY);

            if (engine.State == GameState.Paused) DrawPauseMenu(g, engine, screenWidth, screenHeight);
        }

        private void DrawImageWithAlpha(Graphics g, Image img, Rectangle dest, int alpha)
        {
            if (img == null) return;
            var a = alpha / 255f;
            var cm = new ColorMatrix();
            cm.Matrix33 = a;
            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
            }
        }

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

        private void DrawMainMenu(Graphics g, GameController engine, int w, int h)
        {
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            if (menuBg != null) g.DrawImage(menuBg, 0, 0, w, h);
            else g.Clear(Color.Black);

            DrawOutlineText(g, "СТАНЦИЯ ЛИХО", new Font(pixelFont, 24), Brushes.White, w / 2 - 250, h / 4, true);

            string[] opts = engine.HasSaveFile ? new[] { "Новая игра", "Продолжить", "Выход" } : new[] { "Новая игра", "Выход" };
            Font menuFont = new Font(pixelFont, 14);

            for (int i = 0; i < opts.Length; i++)
            {
                var color = (i == engine.MenuIndex) ? Brushes.Cyan : Brushes.White;
                var text = (i == engine.MenuIndex) ? "> " + opts[i] + " <" : opts[i];
                DrawOutlineText(g, text, menuFont, color, w / 2 + 150, h / 2 + (i * 50));
            }
            DrawOutlineText(g, "Управление: WS - Навигация, Enter - Выбор", new Font(pixelFont, 8), Brushes.White, 20, h - 30);
        }

        private void DrawPauseMenu(Graphics g, GameController engine, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, w, h);
            DrawOutlineText(g, "ПАУЗА", new Font(pixelFont, 24), Brushes.White, w / 2 - 100, h / 4, true);

            string[] options = { "Продолжить", "Сохранить", "Сохранить и выйти", "Выход из игры" };
            var menuFont = new Font(pixelFont, 14);

            for (int i = 0; i < options.Length; i++)
            {
                var color = (i == engine.MenuIndex) ? Brushes.Cyan : Brushes.White;
                var text = (i == engine.MenuIndex) ? "> " + options[i] + " <" : options[i];
                DrawOutlineText(g, text, menuFont, color, w / 2 - 180, h / 2 + (i * 50));
            }
        }

        private void DrawBackground(Graphics g, GameController engine, int w, int h)
        {
            Level level = engine.CurrentLevel;
            if (engine.Player.IsFocusMode) g.Clear(Color.FromArgb(5, 5, 10));
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
        }

        private void DrawGeometry(Graphics g, GameController engine)
        {
            Level level = engine.CurrentLevel;
            foreach (var plat in level.Platforms)
            {
                // Если включено чутьё - рисуем контуры
                if (engine.Player.IsFocusMode)
                    g.DrawRectangle(new Pen(Color.FromArgb(50, 255, 255, 255), 2), plat.X, plat.Y, plat.Width, plat.Height);
                else if (level.Name != "Kitchen" && level.Name != "Street" && level.Name != "SubwayDescent")
                    g.FillRectangle(Brushes.Gray, plat);
            }

            if (level.Name == "Kitchen" && !level.IsBagPickedUp)
            {
                if (bagImg != null) g.DrawImage(bagImg, level.ItemBag);
                else g.FillRectangle(Brushes.SaddleBrown, level.ItemBag);

                if (engine.Player.IsFocusMode) g.DrawRectangle(new Pen(Color.Yellow, 4), level.ItemBag.X, level.ItemBag.Y, level.ItemBag.Width, level.ItemBag.Height);
            }
        }

        private void DrawPlayerAndForeground(Graphics g, GameController engine)
        {
            Level level = engine.CurrentLevel;
            Player p = engine.Player;
            bool isK = level.Name == "Kitchen";
            Image idleImg = isK ? yanaHoodieIdle : yanaCoatIdle;
            Image s = idleImg;

            if (!p.IsGrounded && !isK) // Если в прыжке, то используем yana_coat_w2.png
                if (yanaCoatWalk.Count > 1) s = yanaCoatWalk[1];

            else if (p.IsMoving && p.IsGrounded)
            {
                var walkList = isK ? yanaHoodieWalk : yanaCoatWalk;
                if (p.WalkFrame == 0 && walkList.Count > 0) s = walkList[0];
                else if (p.WalkFrame == 2 && walkList.Count > 1) s = walkList[1];
            }

            if (s != null) DrawEntitySprite(g, s, p);

            if (level.Name == "SubwayDescent" && subwayFg != null && !p.IsFocusMode)
                g.DrawImage(subwayFg, 0, 0, level.WorldWidth, 2700);
        }

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

        private void DrawKhmar(Graphics g, Player p, float camX, float camY, int width, int height)
        {
            using (GraphicsPath wallPath = new GraphicsPath())
            {
                wallPath.AddRectangle(new Rectangle(0, 0, width, height));
                var centerX = (p.Pos.X - camX) + p.Size.Width / 2;
                var centerY = (p.Pos.Y - camY) + p.Size.Height / 3;
                wallPath.AddEllipse(centerX - visibilityRadius, centerY - visibilityRadius, visibilityRadius * 2, visibilityRadius * 2);
                g.FillPath(new SolidBrush(Color.FromArgb(250, 5, 5, 10)), wallPath);
            }
        }

        private void DrawUI(Graphics g, Level level, Player p, float camX, float camY)
        {
            var uiFont = new Font(pixelFont, 12);

            if (level.Name == "Kitchen") DrawKitchenUI(g, level, camX, camY);
            else if (level.Name == "Street") DrawOutlineText(g, "УЛИЦА", uiFont, Brushes.LightSteelBlue, 40, 40);
            else if (level.Name == "SubwayDescent") DrawOutlineText(g, "СПУСК В МЕТРО", uiFont, Brushes.DarkGray, 40, 40);

            if (!level.IsRealWorld) DrawOxygenUI(g, p);
            if (p.IsFocusMode) DrawOutlineText(g, "ЧУТЬЕ АКТИВНО", new Font(pixelFont, 8), Brushes.Cyan, 40, 130);

            DrawDialogUI(g, level, p, 1920, 1080);
        }

        private void DrawKitchenUI(Graphics g, Level level, float camX, float camY)
        {
            DrawOutlineText(g, "Кухня бабушки", new Font(pixelFont, 12), Brushes.White, 40, 40);

            if (!level.IsBagPickedUp && level.IsNearBag)
            {
                var bagX = level.ItemBag.X - camX;
                var bagY = level.ItemBag.Y - camY;
                DrawOutlineText(g, "Взять сумку (E)", new Font(pixelFont, 10), Brushes.White, (int)bagX - 40, (int)bagY - 30);
            }
        }

        private void DrawOxygenUI(Graphics g, Player p)
        {
            var textBrush = p.IsExhausted ? Brushes.Red : Brushes.White;
            var text = p.IsExhausted ? "ОДЫШКА!" : "ДЫХАНИЕ (Удерживай C)";

            DrawOutlineText(g, text, new Font(pixelFont, 8), textBrush, 40, 80);
            g.DrawRectangle(Pens.White, 40, 120, 200, 15);
            g.FillRectangle(p.IsExhausted ? Brushes.Red : Brushes.LightSkyBlue, 41, 121, (p.Oxygen / p.MaxOxygen) * 198, 13);
        }

        private void DrawDialogUI(Graphics g, Level level, Player p, int w, int h)
        {
            if (level.DialogStep == 0) return;

            Image img = null;
            var origSize = new Size(3464, 1350); // Дефолт

            if (level.DialogStep == 1) { img = dialog1; origSize = new Size(3464, 1687); }
            else if (level.DialogStep == 2) { img = dialog2; origSize = new Size(3464, 1350); }
            else if (level.DialogStep == 3) { img = dialog3; origSize = new Size(3464, 1354); }

            if (img == null) return;

            var finalW = (int)(origSize.Width * 0.15f);
            var finalH = (int)(origSize.Height * 0.15f);

            var rect = GetDialogRect(level, p, finalW, finalH);

            DrawImageWithAlpha(g, img, rect, level.DialogAlpha);
        }

        private Rectangle GetDialogRect(Level level, Player p, int drawW, int drawH)
        {
            int posX, posY;

            if (level.DialogStep == 2)
            {
                posX = (int)p.Pos.X + 80 + (p.Size.Width / 4) - (drawW / 4);
                posY = (int)p.Pos.Y - drawH - 0;
            }
            else
            {
                posX = 1500 - (drawW / 2);
                posY = 600 - drawH;
            }

            return new Rectangle(posX, posY, drawW, drawH);
        }
    }
}