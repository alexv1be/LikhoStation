using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LikhoStation.src.Models
{
    /// <summary>
    /// Перечисление, определяющее текущее глобальное состояние приложения (Главное меню, Игровой процесс, Пауза, Воспроизведение видео).
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        VideoPlaying
    }
}
