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
            if (State == GameState.MainMenu)
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
                    else if (MenuIndex == 1 && !HasSaveFile) ShouldExit = true; // Если нет сейва, 1 - это Выход
                    else if (MenuIndex == 2) ShouldExit = true;
                }
            }
            else if (State == GameState.Playing)
            {
                if (key == Keys.Escape)
                {
                    State = GameState.Paused;
                    MenuIndex = 0; // Сбрасываем курсор на "Продолжить"
                }
            }
            else if (State == GameState.Paused)
            {
                if (key == Keys.Up || key == Keys.W) MenuIndex--;
                if (key == Keys.Down || key == Keys.S) MenuIndex++;

                if (MenuIndex < 0) MenuIndex = 2;
                if (MenuIndex > 2) MenuIndex = 0;

                if (key == Keys.Enter)
                {
                    if (MenuIndex == 0) State = GameState.Playing; // Продолжить
                    else if (MenuIndex == 1) // Сохранить и выйти в меню
                    {
                        SaveGame();
                        State = GameState.MainMenu;
                        MenuIndex = 0;
                    }
                    else if (MenuIndex == 2) ShouldExit = true; // Выйти из игры
                }
                else if (key == Keys.Escape) State = GameState.Playing; // Снять с паузы
            }
        }

        // --- СОХРАНЕНИЕ И ЗАГРУЗКА ---
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
                // Проверяем, что в массиве есть хотя бы 2 элемента, чтобы избежать вылета
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

            if (sceneName == "Kitchen")
            {
                CurrentLevel.IsRealWorld = true;
                CurrentLevel.HasKhmar = false;
                CurrentLevel.IsStaticCamera = true;
                CurrentLevel.WorldWidth = screenWidth;

                Player.Size = new Size(100, 200);
                Player.Pos = new PointF(150, CurrentLevel.GroundY - Player.Size.Height);

                CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, CurrentLevel.WorldWidth, screenHeight - CurrentLevel.GroundY));
                CurrentLevel.ItemBag = new RectangleF(700, CurrentLevel.GroundY - 60, 60, 60);
            }
            else if (sceneName == "Street")
            {
                CurrentLevel.IsRealWorld = true;
                CurrentLevel.HasKhmar = false;
                CurrentLevel.IsStaticCamera = false;
                CurrentLevel.WorldWidth = 3500;

                Player.Size = new Size(60, 120);
                Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

                CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, 800, screenHeight - CurrentLevel.GroundY));
                CurrentLevel.Platforms.Add(new RectangleF(1100, CurrentLevel.GroundY, 600, screenHeight - CurrentLevel.GroundY));
                CurrentLevel.Platforms.Add(new RectangleF(2000, CurrentLevel.GroundY, 1500, screenHeight - CurrentLevel.GroundY));
                CurrentLevel.Platforms.Add(new RectangleF(500, CurrentLevel.GroundY - 80, 100, 80));
                CurrentLevel.Platforms.Add(new RectangleF(1400, CurrentLevel.GroundY - 140, 120, 140));
            }
        }

        // ОБНОВЛЕНИЕ КАДРА
        public void Update(HashSet<Keys> pressedKeys)
        {
            if (State != GameState.Playing) return; // Физика не работает на паузе и в меню

            UpdateInput(pressedKeys);
            MovePlayerX(pressedKeys);
            MovePlayerY(pressedKeys);
            CheckBoundaries();
            UpdateItems(pressedKeys);
            UpdateCamera();
        }

        private void UpdateInput(HashSet<Keys> keys)
        {
            Player.IsFocusMode = keys.Contains(Keys.ShiftKey);

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
            if (Player.IsHoldingBreath) return;

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
            if (keys.Contains(Keys.Space) && Player.IsGrounded && !Player.IsHoldingBreath)
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
                    Player.Pos.X = CurrentLevel.WorldWidth - Player.Size.Width;
                }
            }

            if (Player.Pos.Y > screenHeight + 200) LoadScene(CurrentLevel.Name);
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
            if (CurrentLevel.IsStaticCamera) CameraOffsetX = 0;
            else
            {
                CameraOffsetX = Player.Pos.X - screenWidth / 2 + Player.Size.Width / 2;
                if (CameraOffsetX < 0) CameraOffsetX = 0;
                if (CameraOffsetX > CurrentLevel.WorldWidth - screenWidth) CameraOffsetX = CurrentLevel.WorldWidth - screenWidth;
            }
        }
    }
}