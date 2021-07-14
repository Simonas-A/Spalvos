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
    public partial class ImageDialog : Form
    {
        public ImageDialog()
        {
            InitializeComponent();
        }

        public Image Image { get; set; }

        private void ImageDialog_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = Image;
        }
    }
}
