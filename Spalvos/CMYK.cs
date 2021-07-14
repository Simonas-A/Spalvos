using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spalvos
{
    class CMYK
    {
        public int cyan { get; set; }
        public int magenta { get; set; }
        public int yellow { get; set; }
        public int key { get; set; }

        public CMYK()
        {
        }

        public CMYK(int cyan, int magenta, int yellow, int key)
        {
            this.cyan = cyan;
            this.magenta = magenta;
            this.yellow = yellow;
            this.key = key;
        }

        public string toString()
        {
            return string.Format("{0}, {1}, {2}, {3}", cyan, magenta, yellow, key);
        }
    }
}
