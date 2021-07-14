using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace Ral_Ncsc_Database
{
    class Ral
    {

        public string ColorRalReference { get; set; }
        public string ColorRAL { get; set; }
        public Color Rgb { get; set; }
        public CMYK Cmyk { get; set; }
        public string Hex { get; set; }

        public Ral()
        {
        
        }

        public Ral(string colorRAL, Color rgb, string colorReference, CMYK cmyk, string hex)
        {
            this.ColorRAL = colorRAL;
            this.Rgb = rgb;
            this.ColorRalReference = colorReference;
            this.Cmyk = cmyk;
            this.Hex = hex;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, Rgb: {2}, {3}, {4}", this.ColorRalReference, this.ColorRAL, this.Rgb.R, this.Rgb.G, this.Rgb.B);
        }
    }
}
