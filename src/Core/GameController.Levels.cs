using LikhoStation.src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Core
{
    public partial class GameController
    {
        // УРОВНИ И ТРИГГЕРЫ

        /// <summary>
        /// Запускает новую игру, загружая стартовую локацию (Кухня).
        /// </summary>
        private void StartNewGame()
        {
            LoadScene("Kitchen");
            State = GameState.Playing;
        }

        /// <summary>
        /// Базовый метод-маршрутизатор. Сбрасывает характеристики игрока и вызывает метод генерации конкретного уровня по его названию.
        /// </summary>
        /// <param name="sceneName"></param>
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
            else if (sceneName == "LifelessStreet") LoadLifelessStreet();
            else if (sceneName == "LadnyForest") LoadLadnyForest();
        }

        /// <summary>
        /// Генерирует локацию "Кухня бабушки" (реальный мир, статичная камера, обучение).
        /// </summary>
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

            CurrentLevel.ActiveItemRect = new RectangleF(630, CurrentLevel.GroundY - 407, 200, 200);
        }

        /// <summary>
        /// Генерирует локацию "Улица" (платформинг в реальном мире).
        /// </summary>
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

            CurrentLevel.Triggers.Add(new RectangleF(3000 + Player.Size.Width, CurrentLevel.GroundY - 450, 150, 450));
        }

        /// <summary>
        /// Проверяет координаты игрока для активации переходов между уровнями или запуска кат-сцен.
        /// </summary>
        private void CheckLevelTriggers()
        {
            if (CurrentLevel.Name == "Street" && Player.Pos.X >= 3000)
                LoadSubwayDescent();

            if (CurrentLevel.Name == "SubwayDescent" && Player.Pos.X >= 2780)
                StartMetroCutscene();

            if (CurrentLevel.Name == "AbandonedTrain" && Player.Pos.X >= 1400)
                LoadScene("AbandonedStation");
            if (CurrentLevel.Name == "LifelessStreet" && Player.Pos.X >= (CurrentLevel.WorldWidth - 550))
                LoadScene("LadnyForest");
        }

        /// <summary>
        /// Генерирует локацию "Спуск в метро" (длинная лестница вниз, камера следует по оси Y).
        /// </summary>
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
                var x = 1130 + i * stepW;
                var y = startY + i * stepH;
                CurrentLevel.Platforms.Add(new RectangleF(x, y, stepW, 2000));
            }

            var bottomY = startY + steps * stepH;
            CurrentLevel.GroundY = bottomY;

            CurrentLevel.Platforms.Add(new RectangleF(1130 + steps * stepW, bottomY, 2000, 2000));

            CurrentLevel.MaxCameraOffsetY = bottomY - screenHeight * 0.85f;
            if (CurrentLevel.MaxCameraOffsetY < 0) CurrentLevel.MaxCameraOffsetY = 0;

            CurrentLevel.Triggers.Add(new RectangleF(2780 + Player.Size.Width, CurrentLevel.GroundY - 450, 150, 450));
        }

        /// <summary>
        /// Запускает видеоролик перехода персонажа в Изнанку.
        /// </summary>
        private void StartMetroCutscene()
        {
            State = GameState.VideoPlaying;

            OnPlayVideo?.Invoke(@"Assets\Video\metro_cutscene.mp4");
        }

        /// <summary>
        /// Завершает показ видеоролика и загружает первый уровень Изнанки.
        /// </summary>
        public void EndVideoCutscene()
        {
            State = GameState.Playing;
            LoadScene("AbandonedTrain");
        }

        /// <summary>
        /// Генерирует локацию "Заброшенный поезд" (первое появление хмари и чутья).
        /// </summary>
        private void LoadAbandonedTrain()
        {
            State = GameState.Playing;

            CurrentLevel = new Level { Name = "AbandonedTrain", IsRealWorld = false, HasKhmar = true, WorldWidth = screenWidth };
            CurrentLevel.IsStaticCamera = true;

            Player.Size = new Size(440, 880);
            CurrentLevel.GroundY = screenHeight * 0.95f;

            Player.Speed = 15.0f;

            var startX = screenWidth / 2f - Player.Size.Width / 2f;
            Player.Pos = new PointF(startX, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, screenWidth, 500));

            CurrentLevel.Triggers.Add(new RectangleF(1400 + Player.Size.Width, CurrentLevel.GroundY - 450, 150, 450));
        }

        /// <summary>
        /// Генерирует локацию "Заброшенная станция" (появление первых врагов).
        /// </summary>
        private void LoadAbandonedStation()
        {
            State = GameState.Playing;

            var correctWidth = (int)(screenHeight * (21f / 9f));

            CurrentLevel = new Level { Name = "AbandonedStation", IsRealWorld = false, HasKhmar = true, WorldWidth = correctWidth };

            CurrentLevel.IsStaticCamera = false;

            Player.Size = new Size(220, 480);
            CurrentLevel.GroundY = screenHeight * 0.75f;
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Clear();

            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, 550, 500));
            CurrentLevel.Platforms.Add(new RectangleF(1000, CurrentLevel.GroundY, 2400, 500));

            var likho = new Enemy();
            likho.Size = new Size(280, 650);
            likho.Pos = new PointF(1500, CurrentLevel.GroundY - likho.Size.Height);
            likho.Speed = 6.5f;
            likho.KillRadius = 400f;
            likho.WarningRadius = 750f;

            likho.PatrolStartX = 1500f;
            likho.PatrolEndX = correctWidth - 300f;
            CurrentLevel.Enemies.Add(likho);
        }

        /// <summary>
        /// Генерирует локацию "Безжизненная улица".
        /// </summary>
        private void LoadLifelessStreet()
        {
            State = GameState.Playing;

            var correctWidth = (int)(screenHeight * (30f / 9f));

            CurrentLevel = new Level { Name = "LifelessStreet", IsRealWorld = false, HasKhmar = true, WorldWidth = correctWidth };
            CurrentLevel.IsStaticCamera = false;

            Player.Size = new Size(180, 400);

            CurrentLevel.GroundY = screenHeight * 0.95f;
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Clear();
            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, correctWidth, 500));

            var auka = new Enemy();
            auka.Size = new Size(150, 260);
            auka.Pos = new PointF(1500, CurrentLevel.GroundY - auka.Size.Height);

            auka.Speed = 20.0f;
            auka.KillRadius = 250f;
            auka.WarningRadius = 500f;
            auka.IsUnpredictable = true;
            auka.PatrolStartX = 1000f;
            auka.PatrolEndX = correctWidth - 100f;

            CurrentLevel.Enemies.Add(auka);
        }

        /// <summary>
        /// Генерирует локацию "Ладный лес".
        /// </summary>
        private void LoadLadnyForest()
        {
            State = GameState.Playing;

            var correctWidth = (int)(screenHeight * (30f / 9f));

            CurrentLevel = new Level { Name = "LadnyForest", IsRealWorld = false, HasKhmar = true, WorldWidth = correctWidth };
            CurrentLevel.IsStaticCamera = false;

            Player.Speed = 18.0f;
            Player.AirSpeed = 22.0f;

            Player.Size = new Size(180, 400);

            CurrentLevel.GroundY = screenHeight * 0.85f;
            Player.Pos = new PointF(100, CurrentLevel.GroundY - Player.Size.Height);

            CurrentLevel.Platforms.Clear();

            CurrentLevel.Platforms.Add(new RectangleF(0, CurrentLevel.GroundY, 1050, 500));
            CurrentLevel.Platforms.Add(new RectangleF(1500, CurrentLevel.GroundY, 1250, 500));
            CurrentLevel.Platforms.Add(new RectangleF(3150, CurrentLevel.GroundY, correctWidth - 3150, 500));

            // Предмет на пне (доделать)
            CurrentLevel.ActiveItemRect = new RectangleF(2100, CurrentLevel.GroundY - 120, 80, 80);
            CurrentLevel.IsItemPickedUp = false;
        }
    }
}
