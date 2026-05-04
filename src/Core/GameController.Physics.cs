using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Core
{
    public partial class GameController
    {
        //ФИЗИКА

        /// <summary>
        /// Обрабатывает движение игрока по оси X и проверяет столкновения с платформами по горизонтали.
        /// </summary>
        /// <param name="keys"></param>
        private void MovePlayerX(HashSet<Keys> keys)
        {
            Player.IsMoving = false;

            if (CurrentLevel.IsDialogActive) return;
            var currentSpeed = Player.IsGrounded ? Player.Speed : Player.AirSpeed;
            if (Player.IsHoldingBreath) currentSpeed /= 1.5f;
            var nextX = Player.Pos.X;

            if (keys.Contains(Keys.A) || keys.Contains(Keys.Left)) { nextX -= currentSpeed; Player.FacingRight = false; Player.IsMoving = true; }
            if (keys.Contains(Keys.D) || keys.Contains(Keys.Right)) { nextX += currentSpeed; Player.FacingRight = true; Player.IsMoving = true; }

            var nextPlayerX = new RectangleF(nextX, Player.Pos.Y, Player.Size.Width, Player.Size.Height);
            var canMoveX = true;
            foreach (var plat in CurrentLevel.Platforms)
                if (nextPlayerX.IntersectsWith(plat)) { canMoveX = false; break; }

            if (canMoveX) Player.Pos.X = nextX;
            if (CurrentLevel.Name == "SubwayDescent" && Player.Pos.X + Player.Size.Width >= CurrentLevel.WorldWidth)
                StartMetroCutscene();
        }

        /// <summary>
        /// Обрабатывает влияние гравитации, прыжки и проверяет столкновения с полом/потолком по оси Y.
        /// </summary>
        /// <param name="keys"></param>
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

        /// <summary>
        /// Не дает игроку выйти за левый край экрана и обрабатывает падение в пропасть (перезагрузка уровня).
        /// </summary>
        private void CheckBoundaries()
        {
            if (Player.Pos.X < 0) Player.Pos.X = 0;

            if (Player.Pos.X > CurrentLevel.WorldWidth - Player.Size.Width)
            {
                if (CurrentLevel.Name == "Kitchen")
                {
                    if (CurrentLevel.IsItemPickedUp) LoadScene("Street");
                    else Player.Pos.X = CurrentLevel.WorldWidth - Player.Size.Width;
                }
                else if (CurrentLevel.Name == "Street") LoadScene("SubwayDescent");
                else if (CurrentLevel.Name == "SubwayDescent") StartMetroCutscene();
                else if (CurrentLevel.Name == "AbandonedTrain") LoadScene("AbandonedStation");
                else if (CurrentLevel.Name == "AbandonedStation") LoadScene("LifelessStreet");
                else Player.Pos.X = CurrentLevel.WorldWidth - Player.Size.Width;
            }

            if (Player.Pos.Y > CurrentLevel.GroundY + 1000) LoadScene(CurrentLevel.Name);
        }

        /// <summary>
        /// Вычисляет смещение (офсет) камеры, чтобы она плавно следовала за игроком.
        /// </summary>
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
