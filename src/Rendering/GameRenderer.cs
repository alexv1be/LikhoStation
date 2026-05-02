using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;
using LikhoStation.src.Core;
using LikhoStation.src.Models;

namespace LikhoStation.src.Rendering
{
    public partial class GameRenderer
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
        private Image abandonedTrainBg;

        private Image yanaHoodieIdle;
        private List<Image> yanaHoodieWalk = new List<Image>();

        private Image yanaCoatIdle;
        private List<Image> yanaCoatWalk = new List<Image>();

        private PrivateFontCollection customFonts = new PrivateFontCollection();
        public FontFamily pixelFont;

        /// <summary>
        /// Конструктор: загрузка всех файлов из Assets.
        /// </summary>
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
            if (File.Exists(@"Assets\Images\bg_abandoned_train.png")) abandonedTrainBg = Image.FromFile(@"Assets\Images\bg_abandoned_train.png");

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

        /// <summary>
        /// Главный метод, задающий порядок слоев: сначала фон, потом геометрия, потом игроки и UI.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="engine"></param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        public void Draw(Graphics g, GameController engine, int screenWidth, int screenHeight)
        {
            if (engine.State == GameState.MainMenu) 
            {
                DrawMainMenu(g, engine, screenWidth, screenHeight);
                return;
            }

            blinkCounter++;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            DrawBackground(g, engine, screenWidth, screenHeight);

            g.TranslateTransform(-engine.CameraOffsetX, -engine.CameraOffsetY);
            DrawGeometry(g, engine);

            DrawEnemies(g, engine);

            DrawPlayerAndForeground(g, engine);
            g.ResetTransform();

            if (engine.CurrentLevel.HasKhmar) 
                DrawKhmar(g, engine.Player, engine.CameraOffsetX, engine.CameraOffsetY, screenWidth, screenHeight);

            DrawUI(g, engine);

            if (engine.State == GameState.Paused) 
                DrawPauseMenu(g, engine, screenWidth, screenHeight);
        }
    }
}