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

        private int screenWidth;
        private int screenHeight;
        private float gravity = 1.2f;
        private string saveFilePath = "save.txt";

        /// <summary>
        /// Конструктор: Инициализирует базовые параметры контроллера, создает игрока и проверяет наличие файла сохранения.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GameController(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            Player = new Player();
            HasSaveFile = File.Exists(saveFilePath);
        }

        /// <summary>
        /// Главный игровой цикл. Вызывается каждый кадр для обновления всех систем (ввода, физики, врагов, камеры).
        /// </summary>
        /// <param name="pressedKeys"></param>
        public void Update(HashSet<Keys> pressedKeys)
        {
            if (State != GameState.Playing) return;

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