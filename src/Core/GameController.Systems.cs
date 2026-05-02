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

        // ДИАЛОГИ
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
            var limit = CurrentLevel.DialogStep == 2 ? 50 : 70;

            if (t <= 15) CurrentLevel.DialogAlpha = t * 17;
            else if (t >= limit - 15 && t <= limit) CurrentLevel.DialogAlpha = (limit - t) * 17;
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

        //
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
