using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Spalvos
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Graphics g => Graphics.FromImage(pictureBox2.Image);

        Color[] Pal = new Color[6]; // palete

        //int[,,] CI = new int[18, 18, 18]; // Color intervals

        Color2[,,] CI = new Color2[18, 18, 18];

        List<Color2> TopC = new List<Color2>();

        public int pbv; // progress bar value
        public int q; // quality
        public Image dImage;


        public struct Color2
        {
            public int r;
            public int g;
            public int b;
            public int n;

            public Color2(int r1, int g1, int b1, int n1)
            {
                r = r1;
                g = g1;
                b = b1;
                n = n1;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            /*
            openFileDialog1.Filter = "Image Files|*.jpg; *.png; *.jpeg; *.bmp";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Image img = Image.FromFile(openFileDialog1.FileName);
                progressBar1.Maximum = img.Width * img.Height;
                q = trackBar1.Value;
                button1.Enabled = false;
                trackBar1.Enabled = false;
                backgroundWorker1.RunWorkerAsync();
            }
            */
            if (pictureBox1.Image != Properties.Resources.pirm)
            {
                Image img = pictureBox1.Image;
                progressBar1.Maximum = img.Width * img.Height;
                q = trackBar1.Value;
                button1.Enabled = false;
                trackBar1.Enabled = false;
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("No image selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            pictureBox2.Image = new Bitmap(150, 150);
            BackgroundWorker worker = sender as BackgroundWorker;
            Image image = pictureBox1.Image;
            pictureBox1.Image = image;


            if (radioButton1.Checked)
            {
                Color col = Spalva(image, worker);
                Draw(col);
            }
            else if (radioButton2.Checked)
            {
                DC(image, worker);
                Draw1();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = pbv;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            trackBar1.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.AllowDrop = true;
            backgroundWorker1.WorkerReportsProgress = true;
            pictureBox2.Image = new Bitmap(150, 150);

            for (int j = 0; j < 18; j++)
            {
                for (int k = 0; k < 18; k++)
                {
                    for (int l = 0; l < 18; l++)
                    {
                        CI[j, k, l].r = j * 15;
                        CI[j, k, l].g = k * 15;
                        CI[j, k, l].b = l * 15;
                    }
                }
            }
        }

        private void Draw1()
        {
            for (int j = 0; j < 18; j++)
            {
                for (int k = 0; k < 18; k++)
                {
                    for (int l = 0; l < 18; l++)
                    {
                        if (CI[j, k, l].n > 0)
                        {
                            TopC.Add(CI[j, k, l]);
                        }
                    }
                }
            }
            
            Color2 u = new Color2(1, 2, 3, 4);

            while (TopC.Count > 10)
            {
                int t = 3000000;
                foreach (Color2 col in TopC)
                {
                    if (col.n < t)
                    {
                        t = col.n;
                        u = col;
                    }
                }
                TopC.Remove(u);
            }

            Color2[] TC = TopC.ToArray();
            TopC.Clear();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    Color2 tmp;
                    if (TC[j].n > TC[j - 1].n)
                    {
                        tmp = TC[j];
                        TC[j] = TC[j - 1];
                        TC[j - 1] = tmp;
                    }
                }

            }

            SolidBrush[] brush = new SolidBrush[10];

            for (int i = 0; i < 10; i++)
            {
                brush[i] = new SolidBrush(Color.FromArgb(TC[i].r, TC[i].g, TC[i].b));
                g.FillRectangle(brush[i], 0, i * 15, 150, 15);
            }
            pictureBox2.Invalidate();
        }

        private void DC(Image image, BackgroundWorker worker) // dominant color
        {
            for (int j = 0; j < 18; j++)
            {
                for (int k = 0; k < 18; k++)
                {
                    for (int l = 0; l < 18; l++)
                    {
                        CI[j, k, l].n = 0;
                    }
                }
            }

            Bitmap img = new Bitmap(image);
            for (int i = 0; i < img.Width; i += q)
            {
                for (int j = 0; j < img.Height; j += q)
                {
                    Color pixel = img.GetPixel(i, j);

                    CI[pixel.R / 15, pixel.G / 15, pixel.B / 15].n++;
                    pbv = i * img.Height + img.Width;
                }
                if (pbv % 1000 == 0)
                    worker.ReportProgress(pbv);
            }
            pbv = img.Height * img.Width;
            worker.ReportProgress(pbv);
        }

        private Color Spalva(Image image, BackgroundWorker worker)
        {
            int r0 = 0;
            int g0 = 0;
            int b0 = 0;
            int n = 0;

            Bitmap img = new Bitmap(image);
            for (int i = 0; i < img.Width; i+=q)
            {
                for (int j = 0; j < img.Height; j+=q)
                {
                    Color pixel = img.GetPixel(i, j);
                    r0 += pixel.R;
                    g0 += pixel.G;
                    b0 += pixel.B;
                    n++;
                    pbv = i * img.Height + img.Width;
                }
                if (pbv % 1000 == 0)
                    worker.ReportProgress(pbv);
            }
            pbv = img.Height * img.Width;
            worker.ReportProgress(pbv);

            r0 /= n;
            g0 /= n;
            b0 /= n;
            Color col = Color.FromArgb(Convert.ToInt32(r0), Convert.ToInt32(g0), Convert.ToInt32(b0));

            return col;
        }

        private void Draw(Color col)
        {
            Pal[2] = Color.FromArgb((col.R * 8) / 10, (col.G * 8) / 10, (col.B * 8) / 10);
            Pal[1] = Color.FromArgb((col.R * 5) / 10, (col.G * 5) / 10, (col.B * 5) / 10);
            Pal[0] = Color.FromArgb((col.R * 3) / 10, (col.G * 3) / 10, (col.B * 3) / 10);

            int sr, sg, sb, s1, s2, s3;

            sr = (255 - col.R);
            sg = (255 - col.G);
            sb = (255 - col.B);



            s1 = ((Maz(sr, sg, sb) * 2) / 10);
            s2 = ((Maz(sr, sg, sb) * 4) / 10);
            s3 = ((Maz(sr, sg, sb) * 6) / 10);

            Pal[3] = Color.FromArgb(col.R + s1, col.G + s1, col.B + s1);
            Pal[4] = Color.FromArgb(col.R + s2, col.G + s2, col.B + s2);
            Pal[5] = Color.FromArgb(col.R + s3, col.G + s3, col.B + s3);

            SolidBrush brush = new SolidBrush(col);


            for (int i = 5; i >= 0; i--)
            {
                SolidBrush brushT = new SolidBrush(Pal[i]);
                if (i >= 3)
                {
                    g.FillRectangle(brushT, 0, (5 - i) * 21, 150, 21);
                }
                else
                {
                    g.FillRectangle(brushT, 0, (5 - i) * 21 + 21, 150, 21);
                }

                if (i == 3)
                {
                    g.FillRectangle(brush, 0, 63, 150, 21);
                }
            }

            pictureBox2.Invalidate();
        }

        private int Maz(int x, int y, int z)
        {
            return (x < y) ? (x < z ? x : z) : (y < z ? y : z);
        }


        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            //pictureBox1.Image = Spalvos.Properties.Resources.antr;
        }

        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
            dImage = (Bitmap)e.Data.GetData(DataFormats.Bitmap, true);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            pictureBox1.Image = Spalvos.Properties.Resources.antr;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            //pictureBox1.Image = (Bitmap)e.Data.GetData(DataFormats.Bitmap, true);
            try
            {
                string[] bmp = e.Data.GetData(DataFormats.FileDrop) as string[];
                var img = bmp[0];
                pictureBox1.Image = Image.FromFile(img);
                Image imgT = pictureBox1.Image;

                pictureBox1.Image = imgT;
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox1.Image = Spalvos.Properties.Resources.safari;
            }
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            pictureBox1.Image = Spalvos.Properties.Resources.antr;
            e.Effect = DragDropEffects.Copy;
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {

        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Spalvos.Properties.Resources.pirm;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            UploadImage();
        }

        private void UploadImage()//Upload Image
        {
            openFileDialog1.Filter = "Image Files|*.jpg; *.png; *.jpeg; *.bmp";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Image img = Image.FromFile(openFileDialog1.FileName);
                pictureBox1.Image = img;
                /*
                progressBar1.Maximum = img.Width * img.Height;
                q = trackBar1.Value;
                button1.Enabled = false;
                trackBar1.Enabled = false;
                backgroundWorker1.RunWorkerAsync();
                */
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
