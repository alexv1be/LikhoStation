using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LikhoStation.src.Models
{
    /// <summary>
    /// Класс данных главной героини (Яны). Хранит координаты, размеры коробки столкновений, физические параметры (скорость бега, сила прыжка), уровень кислорода, а также флаги состояний (задержка дыхания, чутье художника) и переменные для переключения кадров анимации.
    /// </summary>
    public class Player
    {
        public PointF Pos;
        public Size Size = new Size(60, 120);

        public float Speed = 8.0f;
        public float AirSpeed = 10.0f;

        public float VelocityY = 0;
        public float JumpPower = -24f;
        public bool IsGrounded = false;
        public float Oxygen = 100f;
        public float MaxOxygen = 100f;
        public bool IsHoldingBreath = false;
        public bool IsExhausted = false;
        public bool IsFocusMode = false;

        public bool FacingRight = true;
        public bool IsMoving = false;
        public int WalkFrame = 0;
        public int WalkTimer = 0;
    }
}
