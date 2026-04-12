using System.Collections.Generic;
using System.Drawing;

namespace LikhoStation
{
    public enum GameState { MainMenu, Playing, Paused }

    public class Player
    {
        public PointF Pos;
        public Size Size = new Size(60, 120);
        public float Speed = 10.0f;
        public float VelocityY = 0;
        public float JumpPower = -24f;
        public bool IsGrounded = false;
        public float Oxygen = 100f;
        public float MaxOxygen = 100f;
        public bool IsHoldingBreath = false;
        public bool IsExhausted = false;
        public bool IsFocusMode = false;
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
        public int DialogAlpha = 0; // Прозрачность (0-255)
    }
}