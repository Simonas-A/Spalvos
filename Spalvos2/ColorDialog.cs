using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spalvos2
{
    public partial class ColorDialog : Form
    {
        public ColorDialog()
        {
            InitializeComponent();
        }

        public Image image { get; set; }
        public Image imageSystem { get; set; }

        public Image trueColor { get; set; }
        public Color tmpColor { get; set; }
        public string colorName { get; set; }
        public Dictionary<Point, string> ColorNames { get; set; } = new Dictionary<Point, string>();

        //public Dictionary<Point, string> ColorNames = new Dictionary<Point, string>();

        private void ColorDialog_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(imageSystem);
            pictureBox2.Image = image;
            pictureBox3.Image = trueColor;

            //label2.BackColor = Color.Transparent;
            //label2.ForeColor = Color.FromArgb((255 - tmpColor.R), (255 - tmpColor.G), (255 - tmpColor.B));
            label2.ForeColor = this.BackColor;
            label2.BackColor = tmpColor;
            label2.Text = colorName;

            /*
            pictureBox1.Image = new Bitmap(303, 303);
            Graphics g = Graphics.FromImage(pictureBox1.Image);

            int a = col.A;
            float h = col.GetHue();

            for (int i = 0; i <= 100; i++)
            {
                for (int j = 0; j <= 100; j++)
                {
                    Color c = HSBtoRGB.ColorFromAhsb(a, h, (i / 100f), (j / 100f));
                    SolidBrush brush = new SolidBrush(c);
                    g.FillRectangle(brush, i * 3, j * 3, 3, 3);
                }
            }
            */
            


            /*
            float b = col.GetBrightness();
            float s = col.GetSaturation();
            Color nColor = Color.FromArgb(255 - col.R, 255 - col.G, 255 - col.B);
            Pen pen = new Pen(nColor, 1);
            g.DrawRectangle(pen, s * 300 - 3, b * 300 - 3, 3, 3);
            */


            pictureBox2.Invalidate();
            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = new Point((e.X / 3), (e.Y / 3));
            string Text = ColorNames[point];
            label1.Text = Text;
            label1.Left = e.X + 25;
            label1.Top = e.Y + 25;

            //ReSelectColor(e.X, e.Y);

            /*
            ToolTip tt = new ToolTip();
            IWin32Window win = this;
            tt.Show(Text, win, point.X * 3, point.Y * 3, 10);
            */
        }

        private void ReSelectColor(int x, int y)
        {
            pictureBox1.Image = new Bitmap(imageSystem);
            //pictureBox1.Refresh();

            Bitmap bmp = (Bitmap)imageSystem;
            Color col = bmp.GetPixel(x, y);
            SolidBrush nBrush = new SolidBrush(Color.FromArgb((255 - col.R), (255 - col.G), (255 - col.B)));

            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                for (int i = 0; i < bmp.Height - 2; i += 3)
                {
                    for (int j = 0; j < bmp.Width - 2; j += 3)
                    {
                        //Color color = bmp.GetPixel(j, i);

                        if (IsBorder(bmp, j, i, col))
                        {
                            g.FillRectangle(nBrush, j, i, 3, 3);
                        }

                    }
                }
            }

            pictureBox1.Invalidate();

        }

        private bool IsBorder(Bitmap bmp, int x, int y, Color col)
        {
            if (bmp.GetPixel(x, y) != col)
            {
                if (x > 0)
                {
                    if (bmp.GetPixel(x - 1, y) == col)
                        return true;
                }

                if (y > 0)
                {
                    if (bmp.GetPixel(x, y - 1) == col)
                        return true;
                }

                if (x < bmp.Width - 3)
                {
                    if (bmp.GetPixel(x + 3, y) == col)
                        return true;
                }

                if (y < bmp.Height - 3)
                {
                    if (bmp.GetPixel(x, y + 3) == col)
                        return true;
                }

            }

            return false;
        }

        private void PictureBox1_MouseLeave(object sender, EventArgs e)
        {
            label1.Visible = false;
            //pictureBox1.Image = new Bitmap(imageSystem);
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void PictureBox1_MouseEnter(object sender, EventArgs e)
        {
            label1.Visible = true;
        }
    }
}
