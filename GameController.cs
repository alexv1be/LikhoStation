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
        public float CameraOffsetY { get; private set; }
        public Action<string> OnPlayVideo;
        public Action OnStopVideo;

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
            else if (State == GameState.VideoPlaying) HandleVideoInput(key);
        }

        private void HandleMainMenuInput(Keys key)
        {
            if (key == Keys.Up || key == Keys.W) MenuIndex--;
            if (key == Keys.Down || key == Keys.S) MenuIndex++;

            var maxIndex = HasSaveFile ? 2 : 1;
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

            // Теперь у нас 4 пункта (0, 1, 2, 3)
            if (MenuIndex < 0) MenuIndex = 3;
            if (MenuIndex > 3) MenuIndex = 0;

            if (key == Keys.Enter)
            {
                if (MenuIndex == 0)
                {
                    State = GameState.Playing;
                }
                else if (MenuIndex == 1)
                {
                    SaveGame();
                    State = GameState.Playing;
                }
                else if (MenuIndex == 2)
                {
                    SaveGame();
                    State = GameState.MainMenu;
                    MenuIndex = 0;
                }
                else if (MenuIndex == 3)
                {
                    State = GameState.MainMenu;
                    MenuIndex = 0;
                }
            }
            else if (key == Keys.Escape) State = GameState.Playing;
        }

        private void HandleVideoInput(Keys key)
        {
            // Если нажали пробел — скипаем
            if (key == Keys.Space)
            {
                OnStopVideo?.Invoke();
                EndVideoCutscene();
            }
        }

        // СОХРАНЕНИЕ И ЗАГРУЗКА
        private void SaveGame()
        {
            var data = $"{CurrentLevel.Name}|{CurrentLevel.IsBagPickedUp}|{Player.Pos.X}|{Player.Pos.Y}";
            File.WriteAllText(saveFilePath, data);
            HasSaveFile = true;
        }

        private void LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                var data = File.ReadAllText(saveFilePath).Split('|');

                if (data.Length >= 2)
                {
                    var savedScene = data[0];
                    var isBagPickedUp = bool.Parse(data[1]);

                    LoadScene(savedScene);
                    CurrentLevel.IsBagPickedUp = isBagPickedUp;

                    if (data.Length >= 4)
                    {
                        var savedX = float.Parse(data[2]);
                        var savedY = float.Parse(data[3]);
                        Player.Pos = new PointF(savedX, savedY);
                    }

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
            else if (sceneName == "AbandonedTrain") LoadAbandonedTrain();
            else if (sceneName == "AbandonedStation") LoadAbandonedStation();
        }

        private void LoadKitchen()
        {
            CurrentLevel.IsRealWorld = true;
            CurrentLevel.HasKhmar = false;
            CurrentLevel.IsStaticCamera = true;
            CurrentLevel.WorldWidth = screenWidth;
            CurrentLevel.GroundY = screenHeight * 0.97f;

            Player.Speed = 10.0f;
            Player.Size = new Size(325, 650);
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Clear();
            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, CurrentLevel.WorldWidth, screenHeight - CurrentLevel.GroundY));

            CurrentLevel.ItemBag = new RectangleF(630, CurrentLevel.GroundY - 407, 200, 200);
        }

        private void LoadStreet()
        {
            CurrentLevel.IsRealWorld = true;
            CurrentLevel.HasKhmar = false;
            CurrentLevel.IsStaticCamera = false;
            CurrentLevel.WorldWidth = 3500;
            CurrentLevel.GroundY = screenHeight * 0.91f;

            Player.Size = new Size(125, 250);
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Clear();

            CurrentLevel.Platforms.Add(new RectangleF(400, CurrentLevel.GroundY - 40, 40, 40));
            CurrentLevel.Platforms.Add(new RectangleF(440, CurrentLevel.GroundY - 90, 50, 90));
            CurrentLevel.Platforms.Add(new RectangleF(490, CurrentLevel.GroundY - 150, 60, 150));
            CurrentLevel.Platforms.Add(new RectangleF(550, CurrentLevel.GroundY - 80, 50, 80));

            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, 1100, screenHeight));
            CurrentLevel.Platforms.Add(new RectangleF(1450, CurrentLevel.GroundY, 950, screenHeight));
            CurrentLevel.Platforms.Add(new RectangleF(2700, CurrentLevel.GroundY, 1400, screenHeight));

            CurrentLevel.ForegroundObject = new RectangleF(3200, CurrentLevel.GroundY - 500, 300, 500);
            CurrentLevel.HasForegroundObject = true;
        }

        private void CheckLevelTriggers()
        {
            if (CurrentLevel.Name == "Street" && Player.Pos.X >= 3000)
                LoadSubwayDescent();

            if (CurrentLevel.Name == "SubwayDescent" && Player.Pos.X >= 2800)
                StartMetroCutscene();

            if (CurrentLevel.Name == "AbandonedTrain" && Player.Pos.X >= 1400)
                LoadScene("AbandonedStation");
        }

        private void LoadSubwayDescent()
        {
            CurrentLevel = new Level { Name = "SubwayDescent" };
            CurrentLevel.IsRealWorld = true;
            CurrentLevel.HasKhmar = false;
            CurrentLevel.FollowY = true;
            CurrentLevel.WorldWidth = 3500;

            Player.Speed = 10.0f;
            Player.Size = new Size(250, 500);

            var startY = 1040f;

            Player.Pos = new PointF(300, startY - Player.Size.Height - 50);

            CurrentLevel.Platforms.Clear();

            CurrentLevel.Platforms.Add(new RectangleF(0, startY, 1130, 2000));

            var steps = 31;
            var stepW = 37;
            var stepH = 49;

            for (int i = 0; i < steps; i++)
            {
                var x = 1130 + (i * stepW);
                var y = startY + (i * stepH);
                CurrentLevel.Platforms.Add(new RectangleF(x, y, stepW, 2000));
            }

            var bottomY = startY + (steps * stepH);
            CurrentLevel.GroundY = bottomY;

            CurrentLevel.Platforms.Add(new RectangleF(1130 + (steps * stepW), bottomY, 2000, 2000));

            CurrentLevel.MaxCameraOffsetY = bottomY - screenHeight * 0.85f;
            if (CurrentLevel.MaxCameraOffsetY < 0) CurrentLevel.MaxCameraOffsetY = 0;
        }

        private void StartMetroCutscene()
        {
            State = GameState.VideoPlaying;

            OnPlayVideo?.Invoke(@"Assets\Video\metro_cutscene.mp4");
        }

        public void EndVideoCutscene()
        {
            State = GameState.Playing;
            LoadScene("AbandonedTrain");
        }

        private void LoadAbandonedTrain()
        {
            State = GameState.Playing;

            CurrentLevel = new Level { Name = "AbandonedTrain", IsRealWorld = false, HasKhmar = true, WorldWidth = screenWidth };
            CurrentLevel.IsStaticCamera = true;

            Player.Size = new Size(440, 880);
            CurrentLevel.GroundY = screenHeight * 0.95f;

            Player.Speed = 15.0f;

            var startX = (screenWidth / 2f) - (Player.Size.Width / 2f);
            Player.Pos = new PointF(startX, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, screenWidth, 500));
        }

        private void LoadAbandonedStation()
        {
            State = GameState.Playing;
            CurrentLevel = new Level { Name = "AbandonedStation", IsRealWorld = false, HasKhmar = true, WorldWidth = 3500 };
            Player.Size = new Size(110, 240);
            CurrentLevel.GroundY = screenHeight * 0.8f;
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);
            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, 3500, 500));

            var auka = new Enemy();
            auka.Pos = new PointF(1500, CurrentLevel.GroundY - 350);
            auka.PatrolStartX = 1500f;
            auka.PatrolEndX = 2500f;
            CurrentLevel.Enemies.Add(auka);
        }

        private void UpdateEnemies()
        {
            if (CurrentLevel == null) return;

            foreach (var enemy in CurrentLevel.Enemies)
            {
                PatrolEnemy(enemy);
                CheckEnemyCollision(enemy);
            }
        }

        private void PatrolEnemy(Enemy enemy)
        {
            if (enemy.MovingRight)
            {
                enemy.Pos.X += enemy.Speed;
                if (enemy.Pos.X >= enemy.PatrolEndX) enemy.MovingRight = false;
            }
            else
            {
                enemy.Pos.X -= enemy.Speed;
                if (enemy.Pos.X <= enemy.PatrolStartX) enemy.MovingRight = true;
            }
        }

        private void CheckEnemyCollision(Enemy enemy)
        {
            var dx = (Player.Pos.X + Player.Size.Width / 2) - (enemy.Pos.X + enemy.Size.Width / 2);
            var dy = (Player.Pos.Y + Player.Size.Height / 2) - (enemy.Pos.Y + enemy.Size.Height / 2);
            var dist = Math.Sqrt(dx * dx + dy * dy);

            // Если Яна в зоне и дышит — загружаем уровень заново (смерть)
            if (dist < enemy.KillRadius && !Player.IsHoldingBreath)
            {
                LoadScene(CurrentLevel.Name);
            }
        }

        // ОБНОВЛЕНИЕ КАДРА
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

        private void UpdateDialog()
        {
            if (CurrentLevel.Name != "Kitchen" || CurrentLevel.HasPlayedIntroDialog) return;

            if (Player.Pos.X >= 400 && CurrentLevel.DialogStep == 0)
            {
                CurrentLevel.DialogStep = 1;
                CurrentLevel.IsDialogActive = true;
            }

            if (CurrentLevel.DialogStep > 0) ProcessDialogState();
        }

        private void ProcessDialogState()
        {
            CurrentLevel.DialogTimer++;
            var t = CurrentLevel.DialogTimer;
            var limit = (CurrentLevel.DialogStep == 2) ? 50 : 70;

            if (t <= 15) CurrentLevel.DialogAlpha = t * 17;
            else if (t >= (limit - 15) && t <= limit) CurrentLevel.DialogAlpha = (limit - t) * 17;
            else if (t > limit) CurrentLevel.DialogAlpha = 0;
            else CurrentLevel.DialogAlpha = 255;

            if (CurrentLevel.DialogAlpha > 255) CurrentLevel.DialogAlpha = 255;
            if (CurrentLevel.DialogAlpha < 0) CurrentLevel.DialogAlpha = 0;

            if (t > limit) AdvanceDialogPhase();
        }

        private void AdvanceDialogPhase()
        {
            CurrentLevel.DialogTimer = 0;
            CurrentLevel.DialogStep++;

            if (CurrentLevel.DialogStep == 3) CurrentLevel.IsDialogActive = false;

            if (CurrentLevel.DialogStep > 3)
            {
                CurrentLevel.DialogStep = 0;
                CurrentLevel.HasPlayedIntroDialog = true;
            }
        }

        private void UpdateInput(HashSet<Keys> keys)
        {
            // Отключаем чутье художника на кухне (!!добавить еще локи)
            if (CurrentLevel.Name == "Kitchen") Player.IsFocusMode = false;
            else Player.IsFocusMode = keys.Contains(Keys.ShiftKey);

            if (Player.Oxygen <= 0) Player.IsExhausted = true;
            if (Player.Oxygen > Player.MaxOxygen * 0.3f) Player.IsExhausted = false;

            if (keys.Contains(Keys.C) && !Player.IsExhausted && !CurrentLevel.IsRealWorld)
            {
                Player.IsHoldingBreath = true;
                Player.Oxygen -= 0.6f;
            }
            else
            {
                Player.IsHoldingBreath = false;
                if (Player.Oxygen < Player.MaxOxygen) Player.Oxygen += 0.4f;
            }
        }

        private void UpdateAnimation()
        {
            if (CurrentLevel.IsDialogActive)
            {
                Player.IsMoving = false;
                Player.WalkFrame = 0;
                return;
            }

            if (Player.IsMoving && Player.IsGrounded)
            {
                // Если задерживает дыхание, анимация станет в 2 раза медленнее
                var animSpeed = Player.IsHoldingBreath ? 600 : 300;
                Player.WalkFrame = (int)((DateTime.Now.TimeOfDay.TotalMilliseconds / animSpeed) % 4);
            }
            else
                Player.WalkFrame = 0;
        }

        private void MovePlayerX(HashSet<Keys> keys)
        {
            Player.IsMoving = false;

            if (CurrentLevel.IsDialogActive) return;
            var currentSpeed = Player.IsGrounded ? Player.Speed : Player.AirSpeed;
            if (Player.IsHoldingBreath) currentSpeed /= 3.0f;
            var nextX = Player.Pos.X;

            if (keys.Contains(Keys.A) || keys.Contains(Keys.Left)) { nextX -= currentSpeed; Player.FacingRight = false; Player.IsMoving = true; }
            if (keys.Contains(Keys.D) || keys.Contains(Keys.Right)) { nextX += currentSpeed; Player.FacingRight = true; Player.IsMoving = true; }

            var nextPlayerX = new RectangleF(nextX, Player.Pos.Y, Player.Size.Width, Player.Size.Height);
            var canMoveX = true;
            foreach (var plat in CurrentLevel.Platforms)
                if (nextPlayerX.IntersectsWith(plat)) { canMoveX = false; break; }

            if (canMoveX) Player.Pos.X = nextX;
            if (CurrentLevel.Name == "SubwayDescent" && (Player.Pos.X + Player.Size.Width) >= CurrentLevel.WorldWidth)
                StartMetroCutscene();
        }

        private void MovePlayerY(HashSet<Keys> keys)
        {
            if (CurrentLevel.IsDialogActive) return;

            var canJump = CurrentLevel.Name != "Kitchen" && CurrentLevel.Name != "SubwayDescent" && CurrentLevel.Name != "AbandonedTrain";
            if (keys.Contains(Keys.Space) && Player.IsGrounded && !Player.IsHoldingBreath && canJump)
            {
                Player.VelocityY = Player.JumpPower;
                Player.IsGrounded = false;
            }

            Player.VelocityY += gravity;

            var nextY = Player.Pos.Y + Player.VelocityY;
            var nextPlayerY = new RectangleF(Player.Pos.X, nextY, Player.Size.Width, Player.Size.Height);
            var hitGround = false;

            foreach (var plat in CurrentLevel.Platforms)
            {
                if (nextPlayerY.IntersectsWith(plat))
                {
                    if (Player.VelocityY > 0) { Player.Pos.Y = plat.Top - Player.Size.Height; Player.VelocityY = 0; Player.IsGrounded = true; hitGround = true; break; }
                    else if (Player.VelocityY < 0) { Player.Pos.Y = plat.Bottom; Player.VelocityY = 0; hitGround = true; break; }
                }
            }
            if (!hitGround) { Player.Pos.Y = nextY; Player.IsGrounded = false; }
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
                else if (CurrentLevel.Name == "Street") LoadScene("SubwayDescent");
                else if (CurrentLevel.Name == "SubwayDescent") StartMetroCutscene();
                else if (CurrentLevel.Name == "AbandonedTrain") LoadScene("AbandonedStation");
                else Player.Pos.X = CurrentLevel.WorldWidth - Player.Size.Width;
            }

            if (Player.Pos.Y > CurrentLevel.GroundY + 1000) LoadScene(CurrentLevel.Name);
        }

        private void UpdateItems(HashSet<Keys> keys)
        {
            if (CurrentLevel.Name == "Kitchen" && !CurrentLevel.IsBagPickedUp)
            {
                var centerX = Player.Pos.X + Player.Size.Width / 2;
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

                if (CurrentLevel.FollowY)
                {
                    CameraOffsetY = Player.Pos.Y - screenHeight * 0.5f;
                    if (CameraOffsetY < 0) CameraOffsetY = 0;   
                    if (CameraOffsetY > CurrentLevel.MaxCameraOffsetY) CameraOffsetY = CurrentLevel.MaxCameraOffsetY;
                }
                else
                {
                    CameraOffsetY = 0;
                }
            }
        }
    }
}