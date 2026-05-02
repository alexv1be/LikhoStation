using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LikhoStation.src.Models
{
    /// <summary>
    /// Класс данных конкретной локации [cite: 6]. Содержит физические размеры уровня, уровень пола (GroundY), списки всех платформ, зон-триггеров и врагов. Также хранит настройки поведения камеры, флаг наличия "хмари", параметры квестовых предметов (сумка) и переменные для проигрывания диалогов.
    /// </summary>
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

        public bool HasPlayedIntroDialog = false;
        public bool IsDialogActive = false;
        public int DialogStep = 0;
        public int DialogTimer = 0;
        public int DialogAlpha = 0;

        public List<Enemy> Enemies = new List<Enemy>();
    }
}
