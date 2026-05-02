using System.Collections.Generic;
using System.Drawing;

namespace LikhoStation
{
    public enum GameState { MainMenu, Playing, Paused, VideoPlaying }

    public class Player
    {
        public PointF Pos;
        public Size Size = new Size(60, 120);

        public float Speed = 8.0f;     // Медленная скорость шага по земле
        public float AirSpeed = 10.0f; // Быстрая скорость полета в прыжке (для дальности)

        public float VelocityY = 0;
        public float JumpPower = -24f;
        public bool IsGrounded = false;
        public float Oxygen = 100f;
        public float MaxOxygen = 100f;
        public bool IsHoldingBreath = false;
        public bool IsExhausted = false;
        public bool IsFocusMode = false;

        // АНИМАЦИЯ
        public bool FacingRight = true;
        public bool IsMoving = false;
        public int WalkFrame = 0;
        public int WalkTimer = 0;
    }

    public class Enemy
    {
        public PointF Pos;
        public Size Size = new Size(150, 350);
        public float Speed = 3.5f;
        public float KillRadius = 250f; // Зона, в которой Яна умрет
        public float PatrolStartX;
        public float PatrolEndX;
        public bool MovingRight = true;
    }

    public class Level
    {
        public string Name;
        public int WorldWidth;
        public float GroundY;

        public bool HasKhmar = false;
        public bool IsStaticCamera = false;
        public bool FollowY = false;
        public float MaxCameraOffsetY = 0f;
        public bool IsRealWorld = true;

        public List<RectangleF> Platforms = new List<RectangleF>();
        public List<RectangleF> Triggers = new List<RectangleF>();

        public RectangleF ItemBag;
        public bool IsBagPickedUp = false;
        public bool IsNearBag = false;

        public RectangleF ForegroundObject;
        public bool HasForegroundObject = false;

        // ДИАЛОГИ
        public bool HasPlayedIntroDialog = false;
        public bool IsDialogActive = false;
        public int DialogStep = 0;
        public int DialogTimer = 0;
        public int DialogAlpha = 0;

        public List<Enemy> Enemies = new List<Enemy>();
    }
}