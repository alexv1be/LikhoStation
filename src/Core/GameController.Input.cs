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
        // ОБРАБОТКА ВВОДА

        /// <summary>
        /// Главный обработчик одиночных нажатий клавиш. Перенаправляет ввод в зависимости от текущего состояния игры (меню, игра, пауза).
        /// </summary>
        /// <param name="key"></param>
        public void OnSingleKeyPress(Keys key)
        {
            if (State == GameState.MainMenu) HandleMainMenuInput(key);
            else if (State == GameState.Playing) HandlePlayingInput(key);
            else if (State == GameState.Paused) HandlePausedInput(key);
            else if (State == GameState.VideoPlaying) HandleVideoInput(key);
        }

        /// <summary>
        /// Обрабатывает навигацию и выбор пунктов в Главном меню.
        /// </summary>
        /// <param name="key"></param>
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

        /// <summary>
        /// Обрабатывает системные нажатия во время геймплея (например, выход в меню паузы по Escape или включение режима разработчика).
        /// </summary>
        /// <param name="key"></param>
        private void HandlePlayingInput(Keys key)
        {
            if (key == Keys.Escape)
            {
                State = GameState.Paused;
                MenuIndex = 0;
            }
            if (key == Keys.ControlKey)
            {
                IsDevMode = !IsDevMode;
            }
        }

        /// <summary>
        /// Обрабатывает навигацию и выбор пунктов в меню Паузы (продолжить, сохранить, выйти).
        /// </summary>
        /// <param name="key"></param>
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

        /// <summary>
        /// Обрабатывает пропуск кат-сцен (например, по нажатию пробела).
        /// </summary>
        /// <param name="key"></param>
        private void HandleVideoInput(Keys key)
        {
            if (key == Keys.Space)
            {
                OnStopVideo?.Invoke();
                EndVideoCutscene();
            }
        }

        /// <summary>
        /// Обрабатывает зажатые клавиши (Shift для чутья, C для дыхания) и управляет запасом кислорода игрока.
        /// </summary>
        /// <param name="keys"></param>
        private void UpdateInput(HashSet<Keys> keys)
        {
            if (!CurrentLevel.IsRealWorld) Player.IsFocusMode = keys.Contains(Keys.ShiftKey);
            else Player.IsFocusMode = false;

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
                if (Player.Oxygen < Player.MaxOxygen) Player.Oxygen += 1.0f;
            }
        }
    }
}
