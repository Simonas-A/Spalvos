using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ral_Ncsc_Database
{
    class CMYK
    {
        public int Cyan { get; set; }
        public int Magenta { get; set; }
        public int Yellow { get; set; }
        public int Key { get; set; }

        public CMYK()
        {
        }

        public CMYK(int cyan, int magenta, int yellow, int key)
        {
            this.Cyan = cyan;
            this.Magenta = magenta;
            this.Yellow = yellow;
            this.Key = key;
        }

        public override string ToString()
        {
            return string.Format("Cyan: {0}, Magenta: {1},Yellow: {2},Key: {3}", Cyan, Magenta, Yellow, Key);
        }
    }
}
