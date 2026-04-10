using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace LikhoStation
{
    public class GameController
    {
        public Player Player { get; private set; }
        public Level CurrentLevel { get; private set; }
        public float CameraOffsetX { get; private set; }
        public float CameraOffsetY { get; private set; } // Новая ось камеры

        // МЕНЮ И СОСТОЯНИЯ
        public GameState State { get; private set; } = GameState.MainMenu;
        public int MenuIndex { get; private set; } = 0;
        public bool HasSaveFile { get; private set; }
        public bool ShouldExit { get; private set; } = false;

        private int screenWidth;
        private int screenHeight;
        private float gravity = 1.2f;
        private string saveFilePath = "save.txt";

        public GameController(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            Player = new Player();
            HasSaveFile = File.Exists(saveFilePath);
        }

        // ЛОГИКА ОДИНОЧНЫХ НАЖАТИЙ (ДЛЯ МЕНЮ И ПАУЗЫ)
        public void OnSingleKeyPress(Keys key)
        {
            if (State == GameState.MainMenu) HandleMainMenuInput(key);
            else if (State == GameState.Playing) HandlePlayingInput(key);
            else if (State == GameState.Paused) HandlePausedInput(key);
        }

        private void HandleMainMenuInput(Keys key)
        {
            if (key == Keys.Up || key == Keys.W) MenuIndex--;
            if (key == Keys.Down || key == Keys.S) MenuIndex++;

            int maxIndex = HasSaveFile ? 2 : 1;
            if (MenuIndex < 0) MenuIndex = maxIndex;
            if (MenuIndex > maxIndex) MenuIndex = 0;

            if (key == Keys.Enter)
            {
                if (MenuIndex == 0) StartNewGame();
                else if (MenuIndex == 1 && HasSaveFile) LoadGame();
                else if (MenuIndex == 1 && !HasSaveFile) ShouldExit = true;
                else if (MenuIndex == 2) ShouldExit = true;
            }
        }

        private void HandlePlayingInput(Keys key)
        {
            if (key == Keys.Escape)
            {
                State = GameState.Paused;
                MenuIndex = 0;
            }
        }

        private void HandlePausedInput(Keys key)
        {
            if (key == Keys.Up || key == Keys.W) MenuIndex--;
            if (key == Keys.Down || key == Keys.S) MenuIndex++;

            if (MenuIndex < 0) MenuIndex = 2;
            if (MenuIndex > 2) MenuIndex = 0;

            if (key == Keys.Enter)
            {
                if (MenuIndex == 0) State = GameState.Playing;
                else if (MenuIndex == 1)
                {
                    SaveGame();
                    State = GameState.MainMenu;
                    MenuIndex = 0;
                }
                else if (MenuIndex == 2) ShouldExit = true;
            }
            else if (key == Keys.Escape) State = GameState.Playing;
        }

        // СОХРАНЕНИЕ И ЗАГРУЗКА
        private void SaveGame()
        {
            string data = $"{CurrentLevel.Name},{CurrentLevel.IsBagPickedUp}";
            File.WriteAllText(saveFilePath, data);
            HasSaveFile = true;
        }

        private void LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                string[] data = File.ReadAllText(saveFilePath).Split(',');

                if (data.Length >= 2)
                {
                    string savedScene = data[0];
                    bool isBagPickedUp = bool.Parse(data[1]);

                    LoadScene(savedScene);
                    CurrentLevel.IsBagPickedUp = isBagPickedUp;
                    State = GameState.Playing;
                }
            }
        }

        private void StartNewGame()
        {
            LoadScene("Kitchen");
            State = GameState.Playing;
        }

        public void LoadScene(string sceneName)
        {
            CurrentLevel = new Level { Name = sceneName };
            CurrentLevel.GroundY = screenHeight * 0.75f;

            Player.Oxygen = Player.MaxOxygen;
            Player.IsExhausted = false;

            if (sceneName == "Kitchen") LoadKitchen();
            else if (sceneName == "Street") LoadStreet();
            else if (sceneName == "SubwayDescent") LoadSubwayDescent();
        }

        private void LoadKitchen()
        {
            CurrentLevel.IsRealWorld = true;
            CurrentLevel.HasKhmar = false;
            CurrentLevel.IsStaticCamera = true;
            CurrentLevel.WorldWidth = screenWidth;
            CurrentLevel.GroundY = screenHeight * 0.97f; // Опустили горизонт

            Player.Size = new Size(130, 480);
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Clear();
            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, CurrentLevel.WorldWidth, screenHeight - CurrentLevel.GroundY));

            // Сумка лежит на табуретке
            CurrentLevel.ItemBag = new RectangleF(700, CurrentLevel.GroundY - 250, 60, 60);
        }

        private void LoadStreet()
        {
            CurrentLevel.IsRealWorld = true;
            CurrentLevel.HasKhmar = false;
            CurrentLevel.IsStaticCamera = false;
            CurrentLevel.WorldWidth = 3500;

            Player.Size = new Size(110, 240);
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            // Сделали ямы меньше
            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, 1000, screenHeight));
            CurrentLevel.Platforms.Add(new RectangleF(1150, CurrentLevel.GroundY, 800, screenHeight));
            CurrentLevel.Platforms.Add(new RectangleF(2100, CurrentLevel.GroundY, 1400, screenHeight));

            CurrentLevel.Platforms.Add(new RectangleF(600, CurrentLevel.GroundY - 100, 120, 100));
            CurrentLevel.Platforms.Add(new RectangleF(1500, CurrentLevel.GroundY - 160, 150, 160));

            // Здание Метро в конце улицы (рисуется ПОВЕРХ Яны)
            float subwayHeight = 500f;
            CurrentLevel.ForegroundObject = new RectangleF(3200, CurrentLevel.GroundY - subwayHeight, 300, subwayHeight);
            CurrentLevel.HasForegroundObject = true;
        }

        private void LoadSubwayDescent()
        {
            CurrentLevel.IsRealWorld = true;
            CurrentLevel.HasKhmar = false;
            CurrentLevel.IsStaticCamera = false;
            CurrentLevel.FollowY = true; // Включаем вертикальную камеру
            CurrentLevel.WorldWidth = 4000;

            Player.Size = new Size(80, 160);

            // Начинаем спуск почти под потолком экрана
            float startY = screenHeight * 0.2f;
            Player.Pos = new PointF(100, startY - Player.Size.Height);

            // Входная площадка
            CurrentLevel.Platforms.Add(new RectangleF(0, startY, 400, screenHeight * 3));

            // Генерация Эскалатора (40 ступенек в цикле)
            int steps = 40;
            float stepW = 60;
            float stepH = 35;

            for (int i = 0; i < steps; i++)
            {
                CurrentLevel.Platforms.Add(new RectangleF(400 + (i * stepW), startY + (i * stepH), stepW, screenHeight * 3));
            }

            // Платформа станции в самом низу
            float bottomY = startY + (steps * stepH);
            CurrentLevel.GroundY = bottomY; // Задаем дно уровня

            CurrentLevel.Platforms.Add(new RectangleF(400 + (steps * stepW), bottomY, 2000, screenHeight * 3));

            // Настройка лимита камеры: горизонт внизу будет на 85% экрана
            CurrentLevel.MaxCameraOffsetY = bottomY - screenHeight * 0.85f;
            if (CurrentLevel.MaxCameraOffsetY < 0) CurrentLevel.MaxCameraOffsetY = 0;
        }

        // ОБНОВЛЕНИЕ КАДРА
        public void Update(HashSet<Keys> pressedKeys)
        {
            if (State != GameState.Playing) return;

            UpdateDialog(); // Проверяем, не началась ли кат-сцена

            UpdateInput(pressedKeys);
            MovePlayerX(pressedKeys);
            MovePlayerY(pressedKeys);
            CheckBoundaries();
            UpdateItems(pressedKeys);
            UpdateCamera();
        }

        private void UpdateDialog()
        {
            if (CurrentLevel.Name == "Kitchen" && !CurrentLevel.HasPlayedIntroDialog)
            {
                // Если Яна дошла до X=500 и диалог еще не начался
                if (Player.Pos.X >= 500 && !CurrentLevel.IsDialogActive)
                {
                    CurrentLevel.IsDialogActive = true;
                    CurrentLevel.DialogStep = 1; // Реплика Бабушки
                    CurrentLevel.DialogTimer = 0;
                }

                if (CurrentLevel.IsDialogActive)
                {
                    CurrentLevel.DialogTimer++; // Таймер тикает (50 тиков = 1 секунда)

                    // Через 125 тиков отвечает Яна
                    if (CurrentLevel.DialogStep == 1 && CurrentLevel.DialogTimer > 125)
                    {
                        CurrentLevel.DialogStep = 2;
                        CurrentLevel.DialogTimer = 0;
                    }
                    // Еще через 75 тиков диалог заканчивается
                    else if (CurrentLevel.DialogStep == 2 && CurrentLevel.DialogTimer > 75)
                    {
                        CurrentLevel.IsDialogActive = false;
                        CurrentLevel.HasPlayedIntroDialog = true;
                    }
                }
            }
        }

        private void UpdateInput(HashSet<Keys> keys)
        {
            // Отключаем чутье художника на кухне
            if (CurrentLevel.Name == "Kitchen") Player.IsFocusMode = false;
            else Player.IsFocusMode = keys.Contains(Keys.ShiftKey);

            if (Player.Oxygen <= 0) Player.IsExhausted = true;
            if (Player.Oxygen > Player.MaxOxygen * 0.3f) Player.IsExhausted = false;

            if (keys.Contains(Keys.C) && !Player.IsExhausted && !CurrentLevel.IsRealWorld)
            {
                Player.IsHoldingBreath = true;
                Player.Oxygen -= 0.3f;
            }
            else
            {
                Player.IsHoldingBreath = false;
                if (Player.Oxygen < Player.MaxOxygen) Player.Oxygen += 0.5f;
            }
        }

        private void MovePlayerX(HashSet<Keys> keys)
        {
            if (Player.IsHoldingBreath || CurrentLevel.IsDialogActive) return; // Яна замерла

            float nextX = Player.Pos.X;
            if (keys.Contains(Keys.A) || keys.Contains(Keys.Left)) nextX -= Player.Speed;
            if (keys.Contains(Keys.D) || keys.Contains(Keys.Right)) nextX += Player.Speed;

            RectangleF nextPlayerX = new RectangleF(nextX, Player.Pos.Y, Player.Size.Width, Player.Size.Height);
            bool canMoveX = true;
            foreach (var plat in CurrentLevel.Platforms)
            {
                if (nextPlayerX.IntersectsWith(plat)) { canMoveX = false; break; }
            }
            if (canMoveX) Player.Pos.X = nextX;
        }

        private void MovePlayerY(HashSet<Keys> keys)
        {
            if (CurrentLevel.IsDialogActive) return; // Физика Y тоже на паузе

            // Запрет прыжка на кухне
            bool canJump = CurrentLevel.Name != "Kitchen";
            if (keys.Contains(Keys.Space) && Player.IsGrounded && !Player.IsHoldingBreath && canJump)
            {
                Player.VelocityY = Player.JumpPower;
                Player.IsGrounded = false;
            }

            float nextY = Player.Pos.Y + Player.VelocityY;
            RectangleF nextPlayerY = new RectangleF(Player.Pos.X, nextY, Player.Size.Width, Player.Size.Height);
            bool hitGround = false;

            foreach (var plat in CurrentLevel.Platforms)
            {
                if (nextPlayerY.IntersectsWith(plat))
                {
                    if (Player.VelocityY > 0) { Player.Pos.Y = plat.Top - Player.Size.Height; Player.VelocityY = 0; Player.IsGrounded = true; hitGround = true; break; }
                    else if (Player.VelocityY < 0) { Player.Pos.Y = plat.Bottom; Player.VelocityY = 0; hitGround = true; break; }
                }
            }
            if (!hitGround) { Player.Pos.Y = nextY; Player.VelocityY += gravity; Player.IsGrounded = false; }
        }

        private void CheckBoundaries()
        {
            if (Player.Pos.X < 0) Player.Pos.X = 0;

            if (Player.Pos.X > CurrentLevel.WorldWidth - Player.Size.Width)
            {
                if (CurrentLevel.Name == "Kitchen")
                {
                    if (CurrentLevel.IsBagPickedUp) LoadScene("Street");
                    else Player.Pos.X = CurrentLevel.WorldWidth - Player.Size.Width;
                }
                else if (CurrentLevel.Name == "Street")
                {
                    LoadScene("SubwayDescent"); // Уходим в метро
                }
                else if (CurrentLevel.Name == "SubwayDescent")
                {
                    Player.Pos.X = CurrentLevel.WorldWidth - Player.Size.Width; // Тупик до след. этапа
                }
            }

            // Увеличили зону смерти, чтобы на глубоких уровнях Яна не умирала
            if (Player.Pos.Y > CurrentLevel.GroundY + 1000) LoadScene(CurrentLevel.Name);
        }

        private void UpdateItems(HashSet<Keys> keys)
        {
            if (CurrentLevel.Name == "Kitchen" && !CurrentLevel.IsBagPickedUp)
            {
                float centerX = Player.Pos.X + Player.Size.Width / 2;
                if (Math.Abs(centerX - (CurrentLevel.ItemBag.X + CurrentLevel.ItemBag.Width / 2)) < 150)
                {
                    CurrentLevel.IsNearBag = true;
                    if (keys.Contains(Keys.E)) CurrentLevel.IsBagPickedUp = true;
                }
                else CurrentLevel.IsNearBag = false;
            }
        }

        private void UpdateCamera()
        {
            if (CurrentLevel.IsStaticCamera)
            {
                CameraOffsetX = 0;
                CameraOffsetY = 0;
            }
            else
            {
                CameraOffsetX = Player.Pos.X - screenWidth / 2 + Player.Size.Width / 2;
                if (CameraOffsetX < 0) CameraOffsetX = 0;
                if (CameraOffsetX > CurrentLevel.WorldWidth - screenWidth) CameraOffsetX = CurrentLevel.WorldWidth - screenWidth;

                // Слежение по оси Y (для эскалатора)
                if (CurrentLevel.FollowY)
                {
                    CameraOffsetY = Player.Pos.Y - screenHeight * 0.5f; // Яна всегда по центру высоты во время спуска
                    if (CameraOffsetY < 0) CameraOffsetY = 0;
                    if (CameraOffsetY > CurrentLevel.MaxCameraOffsetY) CameraOffsetY = CurrentLevel.MaxCameraOffsetY; // Останавливаемся у пола станции
                }
                else
                {
                    CameraOffsetY = 0;
                }
            }
        }
    }
}