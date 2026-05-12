using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LikhoStation.src.Models;

namespace LikhoStation.src.Core
{
    public partial class GameController
    {
        public const int VirtualWidth = 1920;
        public const int VirtualHeight = 1200;

        public Player Player { get; private set; }
        public Level CurrentLevel { get; private set; }
        public float CameraOffsetX { get; private set; }
        public float CameraOffsetY { get; private set; }
        public Action<string> OnPlayVideo;
        public Action OnStopVideo;

        // МЕНЮ И СОСТОЯНИЯ
        public GameState State { get; private set; } = GameState.MainMenu;
        public int MenuIndex { get; private set; } = 0;
        public bool HasSaveFile { get; private set; }
        public bool ShouldExit { get; private set; } = false;
        public bool IsDevMode { get; private set; } = false;

        private int screenWidth = VirtualWidth;
        private int screenHeight = VirtualHeight;
        private float gravity = 1.2f;
        private string saveFilePath = "save.txt";

        public GameController()
        {
            Player = new Player();
            HasSaveFile = File.Exists(saveFilePath);
        }

        /// <summary>
        /// Главный игровой цикл. Вызывается каждый кадр для обновления всех систем (ввода, физики, врагов, камеры).
        /// </summary>
        /// <param name="pressedKeys"></param>
        public void Update(HashSet<Keys> pressedKeys)
        {
            if (State == GameState.EndingCutscene)
            {
                CurrentLevel.DialogTimer++;
                if (CurrentLevel.DialogTimer > 150)
                {
                    State = GameState.MainMenu;
                    MenuIndex = 0;
                }
                return;
            }

            if (State != GameState.Playing) return;

            if (CurrentLevel != null && CurrentLevel.IsViewingItem)
            {
                UpdateItems(pressedKeys);
                return;
            }

            UpdateDialog();
            CheckLevelTriggers();
            UpdateInput(pressedKeys);
            MovePlayerX(pressedKeys);
            MovePlayerY(pressedKeys);
            UpdateEnemies();
            CheckBoundaries();
            UpdateItems(pressedKeys);
            UpdateCamera();
            UpdateAnimation();
        }
    }
}