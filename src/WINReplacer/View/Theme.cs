using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using WINReplacer.Properties;

namespace WINReplacer
{
    class Theme
    {
        static Theme()
        {
            LoadFont(ref pfc, Resources.GardensC);
            TextFont = new Font(pfc.Families[0], 13, FontStyle.Regular);
        }

        public static Color BackColor = Color.FromArgb(40, 46, 51);
        public static Color TextColor = Color.White;
        public static Color SelectBackColor = Color.FromArgb(165, 50, 90);
        public static Color SelectTextColor = Color.White;
        public static Color FormBackColor = BackColor;
        public static Font TextFont;
        private static PrivateFontCollection pfc = new PrivateFontCollection();

        private static void LoadFont(ref PrivateFontCollection pfc, byte[] res)
        {
            System.IntPtr data = Marshal.AllocCoTaskMem(res.Length);
            Marshal.Copy(res, 0, data, res.Length);
            pfc.AddMemoryFont(data, res.Length);

            Marshal.FreeCoTaskMem(data);
        }
    }
}
