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
        // СИСТЕМЫ

        /// <summary>
        /// Сохраняет текущий уровень, координаты игрока и прогресс в текстовый файл.
        /// </summary>
        private void SaveGame()
        {
            var data = $"{CurrentLevel.Name}|{CurrentLevel.IsBagPickedUp}|{Player.Pos.X}|{Player.Pos.Y}";
            File.WriteAllText(saveFilePath, data);
            HasSaveFile = true;
        }

        /// <summary>
        /// Считывает данные из файла сохранения и восстанавливает состояние игры.
        /// </summary>
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

        // ДИАЛОГИ

        /// <summary>
        /// Проверяет условия для запуска кат-сцен с диалогами (например, когда игрок отходит от бабушки на кухне).
        /// </summary>
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

        /// <summary>
        /// Управляет таймером и плавностью появления/исчезновения (альфа-каналом) реплик диалога.
        /// </summary>
        private void ProcessDialogState()
        {
            CurrentLevel.DialogTimer++;
            var t = CurrentLevel.DialogTimer;
            var limit = CurrentLevel.DialogStep == 2 ? 50 : 70;

            if (t <= 15) CurrentLevel.DialogAlpha = t * 17;
            else if (t >= limit - 15 && t <= limit) CurrentLevel.DialogAlpha = (limit - t) * 17;
            else if (t > limit) CurrentLevel.DialogAlpha = 0;
            else CurrentLevel.DialogAlpha = 255;

            if (CurrentLevel.DialogAlpha > 255) CurrentLevel.DialogAlpha = 255;
            if (CurrentLevel.DialogAlpha < 0) CurrentLevel.DialogAlpha = 0;

            if (t > limit) AdvanceDialogPhase();
        }

        /// <summary>
        /// Переключает стадии диалога и завершает его, когда все реплики показаны.
        /// </summary>
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

        /// <summary>
        /// Рассчитывает текущий кадр спрайта ходьбы в зависимости от скорости движения игрока.
        /// </summary>
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
                var animSpeed = Player.IsHoldingBreath ? 600 : 300;
                Player.WalkFrame = (int)(DateTime.Now.TimeOfDay.TotalMilliseconds / animSpeed % 4);
            }
            else
                Player.WalkFrame = 0;
        }

        /// <summary>
        /// Проверяет, находится ли игрок рядом с квестовыми предметами (например, сумкой) и обрабатывает их поднятие.
        /// </summary>
        /// <param name="keys"></param>
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
    }
}
