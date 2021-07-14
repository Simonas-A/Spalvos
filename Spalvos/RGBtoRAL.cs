using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spalvos
{
    class RgbToRalStart
    {

        public List<Ral> duomenys;

        private RGB output;

        public RgbToRalStart(int red, int green, int blue)
        {
            output = new RGB(red, green, blue);

            FillDatabase();

        }//RgbToRal

        public Ral GetRal()
        {
            return RgbToRal(output);
        }

        public string GetRalString(int red, int green, int blue)
        {
            output = new RGB(red, green, blue);

            FillDatabase();
            return string.Format(output.ToString());
        }

        public void FillDatabase()
        {
            duomenys = ReadData();
        }

        public Ral RgbToRal(RGB input)
        {
            Ral returnValue = null;

            Dictionary<string, double> atstumas = new Dictionary<string, double>();

            foreach (Ral temp in duomenys)
            {
                double atstumasTarpSpalvu = input.DistanceBetween(temp.rgb);
                atstumas.Add(temp.colorRAL, atstumasTarpSpalvu);
                //Console.WriteLine("{0}:\t\t\t{1}, \tRGB: {2} \t CMYK: {3}", temp.colorReference, temp.colorRAL, temp.rgb.toString(), temp.cmyk.toString());
            }



            double min = atstumas.Values.Min();
            string minStr = null;

            foreach (KeyValuePair<string, double> temp in atstumas)
            {
                if (temp.Value == min)
                {
                    minStr = temp.Key;
                    //Console.WriteLine("{0}", temp.Value);
                    //Console.WriteLine("Maziausias atstumas: {0}", temp.Key);
                }
            }
            foreach (Ral temp in duomenys)
            {
                if (temp.colorRAL == minStr)
                {
                    //Console.WriteLine("{0}:\t\t\t{1}, \tRGB: {2} ", temp.colorReference, temp.colorRAL, temp.rgb.toString());
                    returnValue = temp;
                }
                    
            }
            return returnValue;
        }

        public List<Ral> ReadData()
        {
            List<Ral> temp = new List<Ral>();

            StreamReader file = new StreamReader("/C#/Spalvos/DealWithRalData/RalEverythingNewLine.txt");
            string line = null;

            while((line = file.ReadLine()) != null)
            {

                string[] modifiedLine = line.Split(new string[] { "\">" }, StringSplitOptions.None);//Dividing one line into parts

                for (int i = 0; i < modifiedLine.Length - 1; i++)
                {
                    modifiedLine[i] = modifiedLine[i].Replace("</li><li class=\"font-dark", "");
                }

                modifiedLine[5] = modifiedLine[5].Replace("</li>", ""); //makes hex system clean
                modifiedLine[5] = modifiedLine[5].Replace("#", "0x");

                int[] cmyk = ReturnArray(modifiedLine[3]);
                int[] rgb = ReturnArray(modifiedLine[4]);
                //Console.WriteLine("{0}", rgb[2]);
                temp.Add(new Ral {
                    colorReference = modifiedLine[1],
                    colorRAL = modifiedLine[2],
                    rgb = new RGB(rgb[0], rgb[1], rgb[2]),
                    cmyk = new CMYK(cmyk[0],cmyk[1], cmyk[2], cmyk[3]),
                    hex = modifiedLine[5],
                 });

            }//while


            return temp;
        }//ReadData()

        public int[] ReturnArray(string input)
        {

            string[] tempString = input.Split(new string[] { ", " }, StringSplitOptions.None);


            int[] tempInt = new int[20];

            for (int i = 0; i < tempString.Length; i++)
            {
                tempInt[i] = int.Parse(tempString[i]);
                
            }
            return tempInt;
        }
    }
}
