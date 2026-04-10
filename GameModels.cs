using System.Collections.Generic;
using System.Drawing;

namespace LikhoStation
{
    // Состояния игры
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused
    }

    // Модель Игрока
    public class Player
    {
        public PointF Pos;
        public Size Size = new Size(60, 120); // Базовый размер
        public float Speed = 8.0f;
        public float VelocityY = 0;
        public float JumpPower = -22f;
        public bool IsGrounded = false;

        public float Oxygen = 100f;
        public float MaxOxygen = 100f;
        public bool IsHoldingBreath = false;
        public bool IsExhausted = false;
        public bool IsFocusMode = false;
    }

    // Модель Уровня
    public class Level
    {
        public string Name;
        public int WorldWidth;
        public float GroundY;

        // Настройки атмосферы и камеры
        public bool HasKhmar = false;
        public bool IsStaticCamera = false;
        public bool FollowY = false;          // Следить ли за Яной по вертикали
        public float MaxCameraOffsetY = 0f;   // Ограничитель спуска камеры

        public bool IsRealWorld = true; // Если true - врагов нет

        public List<RectangleF> Platforms = new List<RectangleF>();
        public RectangleF ItemBag;
        public bool IsBagPickedUp = false;
        public bool IsNearBag = false;

        // Объект переднего плана (рисуется ПОВЕРХ героини)
        public RectangleF ForegroundObject;
        public bool HasForegroundObject = false;

        // Переменные для диалогов
        public bool HasPlayedIntroDialog = false;
        public bool IsDialogActive = false;
        public int DialogStep = 0;
        public int DialogTimer = 0;
    }
}