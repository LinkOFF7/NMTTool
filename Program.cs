using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace nis
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            string ext = Path.GetExtension(args[0]);
            NMT nmt = new NMT();
            switch (ext)
            {
                case ".nmt":
                    {
                        nmt.SaveToPNG(args[0]);
                        break;
                    }
                case ".png":
                    {
                        //not implemented yet
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Not supported.");
                        return;
                    }
            }
        }
    }
}
