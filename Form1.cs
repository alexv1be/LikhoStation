using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LikhoStation
{
    public partial class Form1 : Form
    {
        private PointF playerPos = new PointF(100, 450);
        private float playerSpeed = 5.0f;
        private Size playerSize = new Size(40, 80);

        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private int visibilityRadius = 200;

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
            if (pressedKeys.Contains(Keys.A) || pressedKeys.Contains(Keys.Left)) playerPos.X -= playerSpeed;
            if (pressedKeys.Contains(Keys.D) || pressedKeys.Contains(Keys.Right)) playerPos.X += playerSpeed;

            if (playerPos.X < 0) playerPos.X = 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.DrawLine(Pens.Gray, 0, 530, this.Width, 530);
            g.FillRectangle(Brushes.DarkRed, playerPos.X, playerPos.Y, playerSize.Width, playerSize.Height);

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