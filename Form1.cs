using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LikhoStation
{
    public partial class Form1 : Form
    {
        // СОСТОЯНИЕ ИГРЫ
        private string currentScene = "";
        private int worldWidth = 1000; // Ширина текущего уровня
        private float cameraOffsetX = 0; // Смещение камеры

        // УРОВЕНЬ (Платформы и стены)
        private List<RectangleF> platforms = new List<RectangleF>();

        // ПАРАМЕТРЫ ИГРОКА (ЯНЫ)
        private PointF playerPos;
        private float playerSpeed = 8.0f;
        private Size playerSize = new Size(60, 120);
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();

        // ФИЗИКА
        private float velocityY = 0;
        private float gravity = 1.2f;
        private float jumpPower = -22f;
        private bool isGrounded = false;

        // ОБЪЕКТЫ И КОЛЛИЗИИ
        private RectangleF itemBag;
        private bool isBagPickedUp = false;
        private bool isNearBag = false;

        // --- ВИЗУАЛ ---
        private int visibilityRadius = 350;
        private System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Станция Лихо";

            // Чтобы получить правильные размеры экрана сразу
            this.ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            LoadScene("Kitchen");

            gameTimer.Interval = 20;
            gameTimer.Tick += UpdateGame;
            gameTimer.Start();
        }

        // ГЕНЕРАТОР УРОВНЕЙ
        private void LoadScene(string sceneName)
        {
            currentScene = sceneName;
            platforms.Clear();
            float groundY = this.ClientSize.Height * 0.8f; // Уровень земли (80% экрана)

            if (sceneName == "Kitchen")
            {
                worldWidth = this.ClientSize.Width; // Кухня размером в 1 экран
                playerPos = new PointF(100, groundY - playerSize.Height);

                // Пол кухни
                platforms.Add(new RectangleF(0, groundY, worldWidth, this.ClientSize.Height - groundY));

                // Сумка
                itemBag = new RectangleF(600, groundY - 45, 45, 45);
            }
            else if (sceneName == "Street")
            {
                worldWidth = 3500; // Улица длинная (3.5 экрана)
                playerPos = new PointF(100, groundY - playerSize.Height);

                // Пол с ямами (собираем из кусков)
                platforms.Add(new RectangleF(0, groundY, 800, this.ClientSize.Height - groundY));    // Старт
                platforms.Add(new RectangleF(1100, groundY, 600, this.ClientSize.Height - groundY)); // Островок после ямы
                platforms.Add(new RectangleF(2000, groundY, 1500, this.ClientSize.Height - groundY));// Дорога к метро

                // Препятствия
                platforms.Add(new RectangleF(500, groundY - 80, 100, 80));  // Маленький ящик
                platforms.Add(new RectangleF(1400, groundY - 140, 120, 140)); // Высокий ящик (надо запрыгивать)
            }
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            // 1. ДВИЖЕНИЕ ПО ОСИ X
            float nextX = playerPos.X;
            if (pressedKeys.Contains(Keys.A) || pressedKeys.Contains(Keys.Left)) nextX -= playerSpeed;
            if (pressedKeys.Contains(Keys.D) || pressedKeys.Contains(Keys.Right)) nextX += playerSpeed;

            // Проверка столкновений со стенами (все объекты в platforms)
            RectangleF nextPlayerX = new RectangleF(nextX, playerPos.Y, playerSize.Width, playerSize.Height);
            bool canMoveX = true;
            foreach (var plat in platforms)
            {
                if (nextPlayerX.IntersectsWith(plat))
                {
                    canMoveX = false;
                    break;
                }
            }
            if (canMoveX) playerPos.X = nextX;

            // 2. ДВИЖЕНИЕ ПО ОСИ Y (ПРЫЖКИ И ПАДЕНИЕ)
            if (pressedKeys.Contains(Keys.Space) && isGrounded)
            {
                velocityY = jumpPower;
                isGrounded = false;
            }

            float nextY = playerPos.Y + velocityY;
            RectangleF nextPlayerY = new RectangleF(playerPos.X, nextY, playerSize.Width, playerSize.Height);
            bool hitGroundObject = false;

            foreach (var plat in platforms)
            {
                if (nextPlayerY.IntersectsWith(plat))
                {
                    if (velocityY > 0) // Падаем на платформу
                    {
                        playerPos.Y = plat.Top - playerSize.Height;
                        velocityY = 0;
                        isGrounded = true;
                        hitGroundObject = true;
                        break;
                    }
                    else if (velocityY < 0) // Ударились головой снизу
                    {
                        playerPos.Y = plat.Bottom;
                        velocityY = 0;
                        hitGroundObject = true;
                        break;
                    }
                }
            }

            if (!hitGroundObject)
            {
                playerPos.Y = nextY;
                velocityY += gravity;
                isGrounded = false;
            }

            // 3. ГРАНИЦЫ МИРА И ПЕРЕХОДЫ
            if (playerPos.X < 0) playerPos.X = 0; // Не пускаем налево за экран

            if (playerPos.X > worldWidth - playerSize.Width)
            {
                if (currentScene == "Kitchen")
                {
                    if (isBagPickedUp) LoadScene("Street"); // Переход!
                    else playerPos.X = worldWidth - playerSize.Width; // Блок, если нет сумки
                }
                else if (currentScene == "Street")
                {
                    playerPos.X = worldWidth - playerSize.Width; // Конец уровня (здесь потом будет метро)
                }
            }

            // СМЕРТЬ В ЯМЕ
            if (playerPos.Y > this.ClientSize.Height + 200)
            {
                // Если упали ниже экрана — возвращаем в начало сцены
                playerPos.X = 100;
                playerPos.Y = 100;
                velocityY = 0;
            }

            // 4. ЛОГИКА ПРЕДМЕТОВ
            if (currentScene == "Kitchen" && !isBagPickedUp)
            {
                float centerX = playerPos.X + playerSize.Width / 2;
                if (Math.Abs(centerX - (itemBag.X + itemBag.Width / 2)) < 100)
                {
                    isNearBag = true;
                    if (pressedKeys.Contains(Keys.E)) isBagPickedUp = true;
                }
                else isNearBag = false;
            }

            // 5. КАМЕРА
            // Камера следит за Яной, чтобы она была по центру
            cameraOffsetX = playerPos.X - this.ClientSize.Width / 2 + playerSize.Width / 2;

            // Не даем камере уйти за левый или правый край мира
            if (cameraOffsetX < 0) cameraOffsetX = 0;
            if (cameraOffsetX > worldWidth - this.ClientSize.Width) cameraOffsetX = worldWidth - this.ClientSize.Width;
            if (worldWidth <= this.ClientSize.Width) cameraOffsetX = 0; // Для кухни (маленьких сцен) не двигаем

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Фон
            if (currentScene == "Kitchen") g.Clear(Color.FromArgb(30, 20, 20));
            else g.Clear(Color.FromArgb(10, 10, 25));

            // === ОТРИСОВКА ИГРОВОГО МИРА (С УЧЕТОМ КАМЕРЫ) ===
            g.TranslateTransform(-cameraOffsetX, 0);

            // Отрисовка всех платформ (полов, ящиков, стен)
            foreach (var plat in platforms)
            {
                if (currentScene == "Kitchen") g.FillRectangle(new SolidBrush(Color.FromArgb(80, 60, 60)), plat);
                else g.FillRectangle(Brushes.Gray, plat);
            }

            // Предметы (Сумка)
            if (currentScene == "Kitchen" && !isBagPickedUp)
            {
                g.FillRectangle(Brushes.SaddleBrown, itemBag);
            }

            // Яна
            g.FillRectangle(Brushes.DarkRed, playerPos.X, playerPos.Y, playerSize.Width, playerSize.Height);

            // Сбрасываем сдвиг камеры, чтобы UI и Хмарь рисовались ровно на экране
            g.ResetTransform();

            // ОТРИСОВКА ИНТЕРФЕЙСА (UI) И ХМАРИ
            if (currentScene == "Kitchen")
            {
                g.DrawString("КУХНЯ БАБУШКИ", new Font("Arial", 20), Brushes.Gray, 40, 40);
                if (!isBagPickedUp && isNearBag)
                {
                    // Подсказка E рисуется поверх экрана, надо вычислить её позицию
                    float bagScreenX = itemBag.X - cameraOffsetX;
                    g.DrawString("Взять (E)", new Font("Arial", 12, FontStyle.Bold), Brushes.Yellow, bagScreenX - 10, itemBag.Y - 30);
                }
            }
            else if (currentScene == "Street")
            {
                g.DrawString("УЛИЦА", new Font("Arial", 20), Brushes.SteelBlue, 40, 40);
            }

            DrawKhmar(g);
        }

        private void DrawKhmar(Graphics g)
        {
            using (GraphicsPath wallPath = new GraphicsPath())
            {
                wallPath.AddRectangle(new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height));

                // Центр Хмари учитывает камеру
                float screenPlayerX = playerPos.X - cameraOffsetX;
                float centerX = screenPlayerX + playerSize.Width / 2;
                float centerY = playerPos.Y + playerSize.Height / 3;

                wallPath.AddEllipse(centerX - visibilityRadius, centerY - visibilityRadius, visibilityRadius * 2, visibilityRadius * 2);

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(250, 5, 5, 10)))
                {
                    g.FillPath(brush, wallPath);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!pressedKeys.Contains(e.KeyCode)) pressedKeys.Add(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (pressedKeys.Contains(e.KeyCode)) pressedKeys.Remove(e.KeyCode);
        }
    }
}