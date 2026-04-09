using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LikhoStation
{
    public partial class Form1 : Form
    {
        private GameController engine;
        private GameRenderer renderer;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Станция Лихо";

            // Ждем загрузки формы, чтобы получить точные размеры экрана на весь монитор
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            engine = new GameController(this.ClientSize.Width, this.ClientSize.Height);
            renderer = new GameRenderer();

            gameTimer.Interval = 20;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            engine.Update(pressedKeys); // Контроллер обновляет физику
            Invalidate();               // Просим форму перерисоваться
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (engine == null || renderer == null) return;
            renderer.Draw(e.Graphics, engine, this.ClientSize.Width, this.ClientSize.Height); // Рендерер рисует картинку
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