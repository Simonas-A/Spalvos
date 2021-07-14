using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Spalvos
{
    class Ral
    {
        public string colorReference { get; set; }
        public string colorRAL { get; set; }
        public RGB rgb { get; set; }
        public CMYK cmyk { get; set; }
        public string hex { get; set; }
        public Ral()
        {
        }

        public Ral(string colorRAL, RGB rgb, string colorReference, CMYK cmyk, string hex)
        {
            this.colorRAL = colorRAL;
            this.rgb = rgb;
            this.colorReference = colorReference;
            this.cmyk = cmyk;
            this.hex = hex;

        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, RGB:{2}",this.colorReference, this.colorRAL, this.rgb.toString());
        }

        
    }
}
