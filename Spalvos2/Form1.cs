using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ColorSystems;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
//using Spalvos2.HSBtoRGB;

namespace Spalvos2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public bool DebugLog = true;

        public bool Analysed = false;

        public bool dialogOpen = false;

        public uint Step;

        public uint AnalysedPixels
        {
            get
            {
                return _analysedPixels;
            }
            set
            {
                _analysedPixels = value;
                if (_analysedPixels / Step > progressBar1.Value)
                {
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action(() => UpdateProgress()));
                        return;
                    }
                    
                }
            }
        }

        private uint _analysedPixels;

        private readonly ColorBase[] systems = { new RalColors(), new NcsColors() };

        class ColorCounts
        {
            private Dictionary<string, int[]> countsBySystem = new Dictionary<string, int[]>();

            public int[] this[string name]
            {
                get { return countsBySystem[name]; }
            }

            public ColorCounts(params ColorBase[] colorSystems)
            {
                foreach (var system in colorSystems)
                {
                    countsBySystem.Add(system.Name, new int[system.Colors.Length]);
                }
            }

            public void Clear()
            {
                foreach (var counts in countsBySystem)
                {
                    for (int i = 0; i < counts.Value.Length; i++)
                    {
                        counts.Value[i] = 0;
                    }
                }
            }
        }

        class ImageData
        {
            public ColorCounts ColorSystems;

            public bool AddedToComparison = true;

            public ImageData(ColorCounts colorSystems)
            {
                ColorSystems = colorSystems;
            }
        }

        class TopCol
        {
            public int Index;
            public int Amount;

            public TopCol(int indx, int amt)
            {
                Index = indx;
                Amount = amt;
            }
        }

        class GroupComparer : IComparer
        {
            public int Compare(object objA, object objB)
            {
                return Convert.ToInt64(((ListViewGroup)objB).Header).CompareTo(Convert.ToInt64(((ListViewGroup)objA).Header));
            }
        }

        Dictionary<string, ImageData> Images = new Dictionary<string, ImageData>();

        Dictionary<string, ListView> listViewByColorSystem = new Dictionary<string, ListView>();
        Dictionary<string, Chart> pieChartByColorSystem = new Dictionary<string, Chart>();

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!DebugLog)
            {
                label5.Visible = false;
            }

            listView1.LargeImageList = imageList1;
            labelUpdate();
            foreach (var system in systems)
            {
                var tabPage = new TabPage(system.Name);
                var listView = new ListView();

                listView.GetType()
                    .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(listView, true, null);
                listView.FullRowSelect = true;
                listView.Dock = DockStyle.Top;
                listView.Height = 290;
                listView.View = View.Details;
                listView.MultiSelect = false;
                listView.OwnerDraw = true;
                listView.DrawColumnHeader += ListView_DrawColumnHeader;
                listView.DrawItem += ListView_DrawItem;
                listView.DrawSubItem += ListView_DrawSubItem;
                listView.Columns.Add("Preview", 300);
                listView.Columns.Add("Name", 100);
                listView.Columns.Add("%", 60, HorizontalAlignment.Right);
                listView.Columns.Add("Total", 100, HorizontalAlignment.Right);
                listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListView_ItemSelectionChanged);
                listView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseUp);

                var chart = new Chart();
                chart.Dock = DockStyle.Bottom;
                chart.ChartAreas.Add("ChartArea1");
                chart.Legends.Add("Legend1");
                chart.Name = "chart1";
                Series series = new Series();
                series.ChartArea = "ChartArea1";
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                chart.Size = new System.Drawing.Size(500, 180);
                chart.Series.Add(series);

                tabPage.Controls.Add(chart);
                tabPage.Controls.Add(listView);
                tabControl1.TabPages.Add(tabPage);

                listViewByColorSystem.Add(system.Name, listView);
                pieChartByColorSystem.Add(system.Name, chart);
            }
        }

        private void UpdateProgress()
        {
            progressBar1.Value = (int)(_analysedPixels / Step);
            if (progressBar1.Value == 100)
            {
                DrawTopColors();
            }
        }

        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                using (var brush = new SolidBrush(e.SubItem.BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                e.DrawDefault = false;
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                listView1.BackgroundImage = Properties.Resources._2;
            }
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                listView1.BackgroundImage = Properties.Resources._1;
            }
        }

        private void UploadImage(string[] names)
        {
            List<string> nameList = names.ToList();
            List<string> tmpList = new List<string>();

            Analysed = false;

            bool uploadOK = true;
            int uploadCount = Images.Count;

            foreach (string name in nameList)
            {
                try
                {
                    if (!Images.ContainsKey(name))
                    {
                        imageList1.Images.Add(FixedSize(Image.FromFile(name), 256, 144));
                        Images.Add(name, new ImageData(new ColorCounts(systems)));
                    }
                    else
                    {
                        tmpList.Add(name);
                    }
                }
                catch
                {
                    tmpList.Add(name);
                    uploadOK = false;
                }
            }

            foreach (string name in tmpList)
            {
                nameList.Remove(name);
            }
            

            if (uploadOK)
            {
                listView1.BackgroundImage = null;
            }
            else
            {
                MessageBox.Show("Some images were uploaded unsuccessfully");

                if (Images.Count == 0)
                    listView1.BackgroundImage = Properties.Resources._1;
                else
                    listView1.BackgroundImage = null;

            }

            for (int i = 0; i < nameList.Count; i++)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(nameList[i]), uploadCount + i);
                item.Name = nameList[i];
                listView1.Items.Add(item);
                item.Checked = true;
            }

            listView1.SelectedItems.Clear();

            labelUpdate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearList();
        }

        private void ClearList()
        {
            Images.Clear();
            listView1.Items.Clear();
            listView1.BackgroundImage = Properties.Resources._1;
            imageList1.Images.Clear();
            labelUpdate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Analyse();
        }

        private void Analyse()
        {
            progressBar1.Value = 0;

            label3.Text = "Status: Analysing";
            label3.Refresh();

            Thread analyseThread = new Thread(Analysation);
            analyseThread.Start();
        }

        private void Analysation()
        {
            uint totalPixels = 0;
            
            foreach (var imageData in Images)
            {
                totalPixels += (uint)(Image.FromFile(imageData.Key).Width * Image.FromFile(imageData.Key).Height);
            }

            Step = totalPixels / 100;
            AnalysedPixels = 0;

            var time = DateTime.Now;

            foreach (var img in Images)
            {
                AnalyseImage(img.Key);
            }

            var time2 = DateTime.Now;
            var deltaTime = time2 - time;

            UInt64 totalms = (uint)(deltaTime.Milliseconds + deltaTime.Seconds * 1000 + deltaTime.Minutes * 60000) + 1;
            double pixelsPermSecond = totalPixels / totalms;

            if (DebugLog)
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() => DebugText(deltaTime, pixelsPermSecond)));
                    return;
                }
            }
        }

        private void DrawTopColors()
        {
            label3.Text = "Status: Graphing";
            label3.Refresh();

            foreach (var system in systems)
            {
                List<TopCol> topC = new List<TopCol>();

                for (int i = 0; i < system.Colors.Count(); i++)
                {
                    topC.Add(new TopCol(i, 0));
                }

                foreach (var img in Images)
                {
                    if (img.Value.AddedToComparison)
                    {
                        var imageColorSystem = img.Value.ColorSystems[system.Name];
                        for (int i = 0; i < imageColorSystem.Length; i++)
                        {
                            topC[i].Amount += imageColorSystem[i];
                        }
                    }
                }

                var total = topC.Sum(q => (Int64)q.Amount);

                var listView = listViewByColorSystem[system.Name];

                var chart = pieChartByColorSystem[system.Name];

                listView.Items.Clear();
                listView.Groups.Clear();

                List<int> groupAmounts = new List<int>();

                int K = 30; // hue period
                for(int i = 0; i < 360; i += K)
                {
                    listView.Groups.Add(new ListViewGroup(""));
                    groupAmounts.Add(0);
                }
                chart.Series[0].Points.Clear();

                var minimumValue = (total / 1000);

                foreach (var item in topC.Where(q => q.Amount > minimumValue).OrderByDescending(q => q.Amount).Select((q, i) => new { ColorInfo = q, Num = i }))
                {
                    var colorInfo = system.Colors[item.ColorInfo.Index]; // order number
                    float hue = colorInfo.Color.GetHue();
                    if (hue == 360)
                    {
                        hue = 0;
                    }
                    int indx = Convert.ToInt32(Math.Floor(hue / K));
                    if (listView.Groups[indx].Items.Count <= 2)
                    {
                        var listViewItem = listView.Items.Add("");
                        listViewItem.ForeColor = Color.Black;
                        listViewItem.BackColor = colorInfo.Color;
                        listViewItem.Font = SystemFonts.DefaultFont;
                        listViewItem.Group = listView.Groups[indx];
                        groupAmounts[indx] += item.ColorInfo.Amount;

                        listViewItem.SubItems.Add(colorInfo.Name); // Color code/name
                        listViewItem.SubItems.Add($"{(item.ColorInfo.Amount * 100.0 / total):f2}"); // Percentage
                        listViewItem.SubItems.Add($"{item.ColorInfo.Amount}"); // Total pixels
                        listViewItem.SubItems.Add("", Color.Black, colorInfo.Color, SystemFonts.DefaultFont); // Preview
                    }
                }

                for (int i = 0; i < listView.Groups.Count; i++)
                {
                    listView.Groups[i].Header = $"{groupAmounts[i]}";
                }

                ListViewGroup[] groups = new ListViewGroup[listView.Groups.Count];

                listView.Groups.CopyTo(groups, 0);

                Array.Sort(groups, new GroupComparer());

                listView.BeginUpdate();
                listView.Groups.Clear();
                listView.Groups.AddRange(groups);
                listView.EndUpdate();

                for (int i = 0; i < listView.Groups.Count; i++)
                {
                    Int64 Amounts = Convert.ToInt64(listView.Groups[i].Header);
                    listView.Groups[i].Header = $"{(Amounts * 100.0 / total):f2}" + "%";
                }

                foreach (ListViewGroup group in listView.Groups)
                {
                    if (group.Items.Count > 0)
                    {
                        DataPoint dataPoint1 = new DataPoint();
                        ListViewItem item = group.Items[0];
                        dataPoint1.Color = item.BackColor;
                        dataPoint1.LegendText = item.SubItems[1].Text;
                        dataPoint1.SetValueY(Convert.ToInt32(item.SubItems[3].Text));
                        chart.Series[0].Points.Add(dataPoint1);
                    }
                }
            }

            label3.Text = "Status: Done";
            label3.Refresh();
        }

        private void AnalyseImage(string imageData)
        {
            Analysed = true;
            using (Bitmap bmp = new Bitmap(Image.FromFile(imageData)))
            {
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int byteCount = bitmapData.Stride * bmp.Height;
                IntPtr ptrFirstPixel = bitmapData.Scan0;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;

                Images[imageData].ColorSystems.Clear();
                
                for (int y = 0, offset = 0; y < bitmapData.Height; y++, offset += bitmapData.Stride)
                {
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        if (bytesPerPixel > 3 && Marshal.ReadByte(ptrFirstPixel, offset + x + 3) == 255)
                        {
                            byte b = Marshal.ReadByte(ptrFirstPixel, offset + x);
                            byte g = Marshal.ReadByte(ptrFirstPixel, offset + x + 1);
                            byte r = Marshal.ReadByte(ptrFirstPixel, offset + x + 2);
                            Color c = Color.FromArgb(r, g, b);

                            foreach (var system in systems)
                            {
                                var imageColorSystem = Images[imageData].ColorSystems[system.Name];
                                imageColorSystem[system.GetClosestColor(c)]++;
                            }
                            AnalysedPixels++;
                        }
                    }
                }
                bmp.UnlockBits(bitmapData);
            }
        }

        private void labelUpdate()
        {
            label1.Text = "Total images: " + Images.Count.ToString();
            int sel = 0;
            foreach(var img in Images)
            {
                if (img.Value.AddedToComparison)
                    sel++;
            }
            label2.Text = "Selected images: " + sel.ToString();
        }

        private void DebugText(TimeSpan lastTime, double lastPS)
        {
            label5.Text = "Analyse duration: " + lastTime + Environment.NewLine + "Pixels per milisecond: " + lastPS;
        }

        private void ListView_MouseUp(object sender, MouseEventArgs e)
        {
            dialogOpen = false;
        }

        private void ListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        { 
            ListView Tlist = listViewByColorSystem[tabControl1.SelectedTab.Text];
            //MessageBox.Show(Tlist.SelectedItems.Count.ToString());
            if (Tlist.SelectedItems.Count > 0 && !dialogOpen)
            {
                dialogOpen = true;
                ColorDialog form = new ColorDialog();
                var firstSelectedItem = Tlist.SelectedItems[0];
                Color col = firstSelectedItem.BackColor;
                Image tImg = new Bitmap(303, 303);
                Image tImgSys = new Bitmap(303, 303);
                Image imgColor = new Bitmap(612, 300);

                int a = col.A;
                float h = col.GetHue();
                Color mainColor = HSBtoRGB.ColorFromAhsb(a, h, 1, 0.5f);

                using (Graphics g = Graphics.FromImage(imgColor))
                {
                    using (SolidBrush brush = new SolidBrush(col))
                    {
                        g.FillRectangle(brush, 0, 0, 612, 300);
                    }
                }

                using (Graphics g = Graphics.FromImage(tImg), gS = Graphics.FromImage(tImgSys))
                {
                    foreach (var system in systems)
                    {
                        if (system.Name == tabControl1.SelectedTab.Text)
                        {
                            for (int i = 0; i <= 100; i++)
                            {
                                for (int j = 0; j <= 100; j++)
                                {
                                    Color c = HSBtoRGB.ColorFromAhsb(a, h, (i / 100f), (j / 100f));
                                    SolidBrush brush = new SolidBrush(c);
                                    var tmp = system.Colors[system.GetClosestColor(c)];
                                    string Name = tmp.Name;
                                    c = tmp.Color;

                                    form.ColorNames.Add(new Point(i, j), Name);

                                    SolidBrush brushS = new SolidBrush(c);
                                    gS.FillRectangle(brushS, i * 3, j * 3, 3, 3);
                                    g.FillRectangle(brush, i * 3, j * 3, 3, 3);
                                }
                            }

                            Color nColor = Color.FromArgb((255 - col.R), (255 - col.G), (255 - col.B));
                            Color nColor0 = Color.FromArgb((255 - mainColor.R), (255 - mainColor.G), (255 - mainColor.B));
                            SolidBrush nBrush = new SolidBrush(nColor0);
                            Bitmap bmp = (Bitmap)tImgSys;

                            for (int i = 0; i < bmp.Height - 2; i += 3)
                            {
                                for (int j = 0; j < bmp.Width - 2; j += 3)
                                {
                                    Color color = bmp.GetPixel(j, i);

                                    if (IsBorder(bmp, j, i, col))
                                    {
                                        gS.FillRectangle(nBrush, j, i, 3, 3);
                                        g.FillRectangle(nBrush, j, i, 3, 3);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                form.BackColor = Color.FromArgb((255 - mainColor.R), (255 - mainColor.G), (255 - mainColor.B));
                form.imageSystem = tImgSys;
                form.image = tImg;
                form.trueColor = imgColor;
                form.tmpColor = col;
                form.colorName = firstSelectedItem.SubItems[1].Text;
                form.ShowDialog();
            }

            if (Tlist.SelectedItems.Count > 0)
            {
                Tlist.SelectedItems.Clear();
            }
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

        static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg; *.png; *.jpeg; *.bmp";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UploadImage(openFileDialog1.FileNames);
            }
        }

        private void ListView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                listView1.BackgroundImage = Properties.Resources._1;
            }
        }

        private void ListView1_DragDrop(object sender, DragEventArgs e)
        {
            UploadImage(e.Data.GetData(DataFormats.FileDrop) as string[]);
        }

        private void ListView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            if (listView1.Items.Count == 0)
            {
                listView1.BackgroundImage = Properties.Resources._2;
            }
        }

        private void ListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                ImageDialog imageDialog = new ImageDialog();
                imageDialog.Image = Image.FromFile(item.Name);
                imageDialog.ShowDialog();
            }
        }

        private void ListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            foreach (var item in Images)
            {
                item.Value.AddedToComparison = false;
            }

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                Images[item.Name].AddedToComparison = true;
            }

            labelUpdate();
            if (Analysed)
            {
                DrawTopColors();
            }
        }

        private void ListView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                openFileDialog1.Filter = "Image Files|*.jpg; *.png; *.jpeg; *.bmp";
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    UploadImage(openFileDialog1.FileNames);
                }
            }
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            /*
            if (listViewByColorSystem[tabControl1.SelectedTab.Text].SelectedItems.Count > 0)
            {
                listViewByColorSystem[tabControl1.SelectedTab.Text].Items[0].Selected = true;
                listViewByColorSystem[tabControl1.SelectedTab.Text].Items[0].Selected = false;
            }
            
            listViewByColorSystem[tabControl1.SelectedTab.Text].SelectedItems.Clear();
            listViewByColorSystem[tabControl1.SelectedTab.Text].RedrawItems(0, listViewByColorSystem[tabControl1.SelectedTab.Text].Items.Count - 1, false);
            listViewByColorSystem[tabControl1.SelectedTab.Text].Refresh();
            listViewByColorSystem[tabControl1.SelectedTab.Text].Invalidate();
            */
        }
    }
}
