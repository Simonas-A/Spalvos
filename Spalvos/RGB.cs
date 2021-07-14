using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spalvos
{
    class RGB
    {
        public int red { get; set; }
        public int green { get; set; }
        public int blue { get; set; }

        public RGB()
        {

        }
        public RGB(int red, int green, int blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public string toString()
        {
            return string.Format("{0}, {1}, {2}", red, green, blue);
        }

        public double DistanceBetween(RGB comparing)
        {
            double temp = Math.Pow((this.red - comparing.red), 2) + Math.Pow((this.green - comparing.green),2) + Math.Pow((this.blue - comparing.blue), 2);
            temp = Math.Sqrt(temp);
            return temp;
        }
    }

}
