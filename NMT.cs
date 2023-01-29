using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace nis
{
    internal enum PFormat
    {
        Argb8888 = 0x02,
        Indexed4bpp = 0x0A,
        Indexed8bpp = 0x0E,
    }
    internal class NMT
    {
        public string Magic = "nismultitexform";
        public uint Zero1 { get; set; }
        public uint Size1 { get; set; }
        public uint Size2 { get; set; }
        public uint Zero2 { get; set; }
        public PFormat PixelFormat { get; set; }
        public byte Unk0x21 { get; set; }
        public ushort Unk0x22 { get; set; }
        public ushort PaletteColorsNum { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public ushort TextureNum { get; set; }
        public uint Stride { get; set; }

        public NISTexture Texture { get; set; }

        public void SaveToPNG(string nmtFile)
        {
            using(BinaryReader reader = new BinaryReader(File.OpenRead(nmtFile)))
            {
                if(Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0') != Magic)
                {
                    throw new Exception("Unknown magic.");
                }
                Zero1 = reader.ReadUInt32();
                Size1 = reader.ReadUInt32();
                Size2 = reader.ReadUInt32();
                Zero2 = reader.ReadUInt32();
                PixelFormat = (PFormat)reader.ReadByte();
                Unk0x21 = reader.ReadByte();
                Unk0x22 = reader.ReadUInt16();
                PaletteColorsNum = reader.ReadUInt16();
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                TextureNum = reader.ReadUInt16();
                Stride = reader.ReadUInt32();

                NISTexture texture = new NISTexture();
                if (PixelFormat != PFormat.Argb8888)
                {
                    texture.PaletteData = reader.ReadBytes(PaletteColorsNum * 4);
                }
                texture.Width = Width;
                texture.Height = Height;
                texture.PixelFormat = PixelFormat;
                texture.PixelData = reader.ReadBytes((int)(Stride * Height));
                texture.Palette = texture.GetPalette();
                texture.Image = texture.GetBitmap();
                texture.Image.Save(nmtFile + ".png", ImageFormat.Png);
            }
        }
    }

    internal class NISTexture
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public PFormat PixelFormat { get; set; }
        public Color[] Palette { get; set; }
        public Bitmap Image { get; set; }

        public byte[] PaletteData { get; set; }
        public byte[] PixelData { get; set; }

        public Bitmap GetBitmap()
        {
            Bitmap bmp = null;
            switch (PixelFormat)
            {
                case PFormat.Argb8888:
                    {
                        bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        break;
                    }
                case PFormat.Indexed4bpp:
                    {
                        bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
                        break;
                    }
                case PFormat.Indexed8bpp:
                    {
                        bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        break;
                    }
            }
            ColorPalette pal = bmp.Palette;
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            if(PixelFormat != PFormat.Argb8888 && Palette != null)
            {
                for (int i = 0; i < Palette.Length; i++)
                {
                    pal.Entries[i] = Palette[i];
                }
                bmp.Palette = pal;
            }
            IntPtr pNative = bmpData.Scan0;
            Marshal.Copy(PixelData, 0, pNative, PixelData.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }
        public Color[] GetPalette()
        {
            if (PaletteData == null)
                return null;

            int colorsNum = PaletteData.Length / 4;
            Color[] colors = new Color[colorsNum];

            for (int i = 0, l = 0; l < colorsNum; i += 4, l++)
            {
                colors[l] = Color.FromArgb(
                    PaletteData[i + 3],
                    PaletteData[i + 0],
                    PaletteData[i + 1],
                    PaletteData[i + 2]
                    );
            }

            return colors;
        }
    }
}
