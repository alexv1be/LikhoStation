using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikhoStation.src.Rendering
{
    public partial class GameRenderer
    {
        // ИНСТРУМЕНТЫ

        /// <summary>
        /// Метод для рисования полупрозрачных картинок (используется в диалогах) через ColorMatrix.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="img"></param>
        /// <param name="dest"></param>
        /// <param name="alpha"></param>
        private void DrawImageWithAlpha(Graphics g, Image img, Rectangle dest, int alpha)
        {
            if (img == null) return;
            var a = alpha / 255f;
            var cm = new ColorMatrix();
            cm.Matrix33 = a;
            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
            }
        }

        /// <summary>
        /// Метод, который делает текст читаемым, рисуя черный контур вокруг белых букв.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="f"></param>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isStrong"></param>
        private void DrawOutlineText(Graphics g, string text, Font f, Brush c, int x, int y, bool isStrong = false)
        {
            var ob = Brushes.Black;
            var t = isStrong ? 4 : 2;
            g.DrawString(text, f, ob, x - t, y);
            g.DrawString(text, f, ob, x + t, y);
            g.DrawString(text, f, ob, x, y - t);
            g.DrawString(text, f, ob, x, y + t);

            if (isStrong)
            {
                g.DrawString(text, f, ob, x - t, y - t);
                g.DrawString(text, f, ob, x + t, y - t);
                g.DrawString(text, f, ob, x - t, y + t);
                g.DrawString(text, f, ob, x + t, y + t);
            }
            g.DrawString(text, f, c, x, y);
        }
    }
}
