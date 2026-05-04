using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LikhoStation.src.Models
{
    /// <summary>
    /// Класс данных противника (например, Ауки) [cite: 6]. Хранит позицию, скорость, радиус смертельной зоны (KillRadius) и координаты точек, между которыми враг патрулирует территорию (PatrolStartX, PatrolEndX).
    /// </summary>
    public class Enemy
    {
        public PointF Pos;
        public Size Size = new Size(150, 350);
        public float Speed = 3.5f;
        public float KillRadius = 250f;
        public float WarningRadius = 450f;
        public bool IsPlayerNear = false;
        public float PatrolStartX;
        public float PatrolEndX;
        public bool MovingRight = true;
        public bool IsUnpredictable = false;
        public int BehaviorTimer = 0;
    }
}
