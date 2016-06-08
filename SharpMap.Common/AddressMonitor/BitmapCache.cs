using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Ptv.Controls.Map
{
    public class BitmapCache
    {
        private Dictionary<string, Bitmap> m_Bitmaps = new Dictionary<string, Bitmap>();

        public void AddBitmap(string name, string filename, System.Drawing.Color transparentColor)
        {
            name = name.ToUpper();

            if (m_Bitmaps.ContainsKey(name))
                return;

            if (System.IO.File.Exists(filename))
            {
                Bitmap bitmap = new Bitmap(filename);
                bitmap.MakeTransparent(transparentColor);
                m_Bitmaps[name] = bitmap;
            }
        }

        public void AddBitmap(string name, Bitmap bitmap)
        {
            name = name.ToUpper();

            if (m_Bitmaps.ContainsKey(name))
                return;

            m_Bitmaps[name] = bitmap;
        }

        public Bitmap GetBitmap(string name)
        {
            name = name.ToUpper();

            if (!m_Bitmaps.ContainsKey(name))
                return null;

            return m_Bitmaps[name];
        }
    }
}
