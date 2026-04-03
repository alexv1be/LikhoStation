using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LikhoStation
{
    public partial class Form1 : Form
    {
        // --- ПАРАМЕТРЫ ИГРОКА (ЯНЫ) ---
        private PointF playerPos = new PointF(100, 450);
        private float playerSpeed = 5.0f;
        private Size playerSize = new Size(40, 80);
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();

        // --- ХМАРЬ ---
        private int visibilityRadius = 200;

        // --- ВЗАИМОДЕЙСТВИЕ (СУМКА БАБУШКИ) ---
        private RectangleF itemBag = new RectangleF(700, 500, 30, 30); // Сумка лежит на x=700
        private bool isBagPickedUp = false; // Состояние: подобрали или нет
        private bool isNearBag = false;    // Состояние: стоим ли мы достаточно близко

        // --- ТАЙМЕР ---
        private System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.ClientSize = new Size(1000, 600);
            this.Text = "Станция Лихо";

            gameTimer.Interval = 20;
            gameTimer.Tick += UpdateGame;
            gameTimer.Start();
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            // 1. Движение
            if (pressedKeys.Contains(Keys.A) || pressedKeys.Contains(Keys.Left)) playerPos.X -= playerSpeed;
            if (pressedKeys.Contains(Keys.D) || pressedKeys.Contains(Keys.Right)) playerPos.X += playerSpeed;

            // Ограничение экрана слева
            if (playerPos.X < 0) playerPos.X = 0;

            // 2. Логика проверки близости к сумке
            if (!isBagPickedUp)
            {
                float centerX = playerPos.X + playerSize.Width / 2;
                float bagCenterX = itemBag.X + itemBag.Width / 2;

                // Если расстояние до сумки меньше 70 пикселей
                if (Math.Abs(centerX - bagCenterX) < 70)
                {
                    isNearBag = true;
                    // Если нажата клавиша E — подбираем
                    if (pressedKeys.Contains(Keys.E))
                    {
                        isBagPickedUp = true;
                    }
                }
                else
                {
                    isNearBag = false;
                }
            }

            Invalidate(); // Перерисовать экран
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем пол
            g.DrawLine(Pens.Gray, 0, 530, this.Width, 530);

            // Рисуем сумку (если не подобрана)
            if (!isBagPickedUp)
            {
                g.FillRectangle(Brushes.SaddleBrown, itemBag);

                // Рисуем подсказку "Нажми E"
                if (isNearBag)
                {
                    g.DrawString("Нажми E", new Font("Arial", 12, FontStyle.Bold), Brushes.Yellow, itemBag.X - 10, itemBag.Y - 30);
                }
            }

            // Рисуем игрока (Яну)
            g.FillRectangle(Brushes.DarkRed, playerPos.X, playerPos.Y, playerSize.Width, playerSize.Height);

            // Рисуем Хмарь поверх всего
            DrawKhmar(g);
        }

        private void DrawKhmar(Graphics g)
        {
            using (GraphicsPath wallPath = new GraphicsPath())
            {
                wallPath.AddRectangle(this.ClientRectangle);
                float centerX = playerPos.X + playerSize.Width / 2;
                float centerY = playerPos.Y + playerSize.Height / 2;
                wallPath.AddEllipse(centerX - visibilityRadius, centerY - visibilityRadius, visibilityRadius * 2, visibilityRadius * 2);

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(245, 10, 10, 15)))
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