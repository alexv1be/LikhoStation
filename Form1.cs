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
        private AxWMPLib.AxWindowsMediaPlayer videoPlayer;

        public Form1()
        {
            InitializeComponent();

            videoPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            ((System.ComponentModel.ISupportInitialize)(this.videoPlayer)).BeginInit();
            this.Controls.Add(videoPlayer);
            videoPlayer.TabStop = false;
            videoPlayer.Enter += (s, e) => this.Focus();
            videoPlayer.Dock = DockStyle.Fill;
            videoPlayer.Visible = false;
            ((System.ComponentModel.ISupportInitialize)(this.videoPlayer)).EndInit();
           
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Станция Лихо";

            this.Shown += Form1_Shown;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Cursor.Hide();

            engine = new GameController(this.ClientSize.Width, this.ClientSize.Height);
            renderer = new GameRenderer();

            engine.OnPlayVideo = PlayCutsceneVideo;
            videoPlayer.uiMode = "none";
            videoPlayer.PlayStateChange += VideoPlayer_PlayStateChange;

            gameTimer.Interval = 20;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (engine.ShouldExit)
                Application.Exit();

            engine.Update(pressedKeys);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (engine == null || renderer == null) return;
            renderer.Draw(e.Graphics, engine, this.ClientSize.Width, this.ClientSize.Height);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Добавляем клавишу в список для непрерывного движения
            if (!pressedKeys.Contains(e.KeyCode)) pressedKeys.Add(e.KeyCode);

            // Отправляем одиночное нажатие в Контроллер (для меню и Esc)
            engine.OnSingleKeyPress(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (pressedKeys.Contains(e.KeyCode)) pressedKeys.Remove(e.KeyCode);
        }

        private void PlayCutsceneVideo(string path)
        {
            videoPlayer.Dock = DockStyle.None;
            videoPlayer.Location = new Point(0, 0);
            videoPlayer.Size = this.ClientSize;
            videoPlayer.stretchToFit = true;

            videoPlayer.URL = path;
            videoPlayer.Visible = true;
            videoPlayer.Ctlcontrols.play();
        }

        private void VideoPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            // 8 — это код состояния "MediaEnded" (Видео закончилось)
            if (e.newState == 8)
            {
                videoPlayer.Visible = false;
                engine.EndVideoCutscene();
            }
        }
    }
}