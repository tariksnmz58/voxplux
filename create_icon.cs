using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

class Program {
    static void Main() {
        using (Bitmap bmp = new Bitmap(256, 256))
        using (Graphics g = Graphics.FromImage(bmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            
            using (Brush b = new SolidBrush(Color.FromArgb(255, 6, 182, 212))) {
                g.FillEllipse(b, 0, 0, 256, 256);
            }
            
            using (Font f = new Font("Segoe UI", 120, FontStyle.Bold))
            using (Brush tb = new SolidBrush(Color.White)) {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("V", f, tb, new RectangleF(0, 0, 256, 256), sf);
            }
            
            // To create a valid ICO, we need an ICO header.
            using (FileStream fs = new FileStream(@"src\Voxplux.App\icon.ico", FileMode.Create)) {
                // write ICO header
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write((short)0); // reserved
                bw.Write((short)1); // image type (1 = icon)
                bw.Write((short)1); // number of images
                bw.Write((byte)256); // width
                bw.Write((byte)256); // height
                bw.Write((byte)0); // colors
                bw.Write((byte)0); // reserved
                bw.Write((short)1); // color planes
                bw.Write((short)32); // bits per pixel
                
                using (MemoryStream ms = new MemoryStream()) {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    bw.Write((int)ms.Length); // size of image
                    bw.Write((int)22); // offset of image data
                    bw.Write(ms.ToArray());
                }
            }
        }
    }
}
