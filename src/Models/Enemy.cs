using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LikhoStation.src.Models
{
    public class Enemy
    {
        public PointF Pos;
        public Size Size = new Size(150, 350);
        public float Speed = 3.5f;
        public float KillRadius = 250f;
        public float PatrolStartX;
        public float PatrolEndX;
        public bool MovingRight = true;
    }
}
