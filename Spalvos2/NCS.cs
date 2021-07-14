using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ral_Ncsc_Database
{
    class NCS
    {
        public string NcsReference { get; set; }
        public Color Rgb { get; set; }

        public NCS()
        {
        }

        public NCS(string reference, Color rgbInput)
        {
            this.NcsReference = reference;
            this.Rgb = rgbInput;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Rgb: {1}, {2}, {3}", this.NcsReference, this.Rgb.R, this.Rgb.G, this.Rgb.B);
        }
    }
}
