//  Copyright (c) 2006, Gustavo Franco
//  Email:  gustavo_franco@hotmail.com
//  All rights reserved.

//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:

//  Redistributions of source code must retain the above copyright notice, 
//  this list of conditions and the following disclaimer. 
//  Redistributions in binary form must reproduce the above copyright notice, 
//  this list of conditions and the following disclaimer in the documentation 
//  and/or other materials provided with the distribution. 

//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
using System;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing.IconLib.Exceptions;
using System.Drawing.IconLib.BitmapEncoders;
using System.Drawing.IconLib.EncodingFormats;

namespace System.Drawing.IconLib
{
    [Author("Franco, Gustavo")]
    public sealed class IconImage
    {
        #region Constructors
        internal IconImage() => Encoder = new BMPEncoder();

        internal IconImage(Stream stream, int resourceSize) => Read(stream, resourceSize);
        #endregion

        #region Properties
        public unsafe int ColorsInPalette => (int)(Encoder.Header.biClrUsed != 0 ?
                                    Encoder.Header.biClrUsed :
                                    Encoder.Header.biBitCount <= 8 ?
                                        (uint)(1 << Encoder.Header.biBitCount) : 0);

        public Size Size => new Size((int)Encoder.Header.biWidth, (int)(Encoder.Header.biHeight / 2));

        public PixelFormat PixelFormat
        {
            get
            {
                switch (Encoder.Header.biBitCount)
                {
                    case 1:
                        return PixelFormat.Format1bppIndexed;
                    case 4:
                        return PixelFormat.Format4bppIndexed;
                    case 8:
                        return PixelFormat.Format8bppIndexed;
                    case 16:
                        return PixelFormat.Format16bppRgb565;
                    case 24:
                        return PixelFormat.Format24bppRgb;
                    case 32:
                        return PixelFormat.Format32bppArgb;
                    default:
                        return PixelFormat.Undefined;
                }
            }
        }

        public Icon Icon => Encoder.Icon;

        public unsafe Bitmap Transparent => Icon.ToBitmap();

        public Bitmap Image
        {
            get
            {
                IntPtr hDCScreen = Win32.GetDC(IntPtr.Zero);

                // Image
                BITMAPINFO bitmapInfo;
                IntPtr bits;
                bitmapInfo.icHeader = Encoder.Header;
                bitmapInfo.icHeader.biHeight /= 2;
                bitmapInfo.icColors = Tools.StandarizePalette(Encoder.Colors);
                IntPtr hDCScreenOUTBmp = Win32.CreateCompatibleDC(hDCScreen);
                IntPtr hBitmapOUTBmp = Win32.CreateDIBSection(hDCScreenOUTBmp, ref bitmapInfo, 0, out bits, IntPtr.Zero, 0);
                Marshal.Copy(Encoder.XOR, 0, bits, Encoder.XOR.Length);
                Bitmap OutputBmp = Bitmap.FromHbitmap(hBitmapOUTBmp);

                Win32.ReleaseDC(IntPtr.Zero, hDCScreen);
                Win32.DeleteObject(hBitmapOUTBmp);
                Win32.DeleteDC(hDCScreenOUTBmp);

                //// GDI+ returns a PixelFormat.Format32bppRgb for 32bits objects, 
                //// we have to recreate it to PixelFormat.Format32bppARgb
                //if (OutputBmp.PixelFormat == PixelFormat.Format32bppRgb)
                //{
                //    BitmapData bmpData = OutputBmp.LockBits(new Rectangle(0, 0, OutputBmp.Width, OutputBmp.Height), ImageLockMode.ReadOnly, OutputBmp.PixelFormat);
                //    Bitmap bmp = new Bitmap(OutputBmp.Width, OutputBmp.Height, bmpData.Stride, PixelFormat.Format32bppArgb, bmpData.Scan0);
                //    OutputBmp.UnlockBits(bmpData);
                //    // I can't dispose the OutputBmp, because the data in bmpData.Scan0 become invalid
                //    // and operations over the new bitmap fail, later take a look if this brings memory leak
                //    // OutputBmp.Dispose();
                //    OutputBmp = bmp;
                //}

                return OutputBmp;
            }
        }

        public Bitmap Mask
        {
            get
            {
                IntPtr hDCScreen = Win32.GetDC(IntPtr.Zero);

                // Image
                BITMAPINFO bitmapInfo;
                IntPtr bits;
                bitmapInfo.icHeader = Encoder.Header;
                bitmapInfo.icHeader.biHeight /= 2;
                bitmapInfo.icHeader.biBitCount = 1;
                bitmapInfo.icColors = new RGBQUAD[256];
                bitmapInfo.icColors[0].Set(0, 0, 0);
                bitmapInfo.icColors[1].Set(255, 255, 255);
                IntPtr hDCScreenOUTBmp = Win32.CreateCompatibleDC(hDCScreen);
                IntPtr hBitmapOUTBmp = Win32.CreateDIBSection(hDCScreenOUTBmp, ref bitmapInfo, 0, out bits, IntPtr.Zero, 0);
                Marshal.Copy(Encoder.AND, 0, bits, Encoder.AND.Length);
                Bitmap OutputBmp = Bitmap.FromHbitmap(hBitmapOUTBmp);

                Win32.ReleaseDC(IntPtr.Zero, hDCScreen);
                Win32.DeleteObject(hBitmapOUTBmp);
                Win32.DeleteDC(hDCScreenOUTBmp);

                return OutputBmp;
            }
        }

        public IconImageFormat IconImageFormat
        {
            get => Encoder.IconImageFormat;
            set
            {
                if (value == IconImageFormat.UNKNOWN)
                    throw new InvalidIconFormatSelectionException();

                if (value == Encoder.IconImageFormat)
                    return;

                ImageEncoder newEncoder = null;
                switch (value)
                {
                    case IconImageFormat.BMP:
                        newEncoder = new BMPEncoder();
                        break;
                    case IconImageFormat.PNG:
                        newEncoder = new PNGEncoder();
                        break;
                }
                newEncoder.CopyFrom(Encoder);
                Encoder = newEncoder;
            }
        }
        #endregion

        #region Internal Properties
        internal ImageEncoder Encoder { get; private set; }

        internal unsafe int IconImageSize => Encoder.ImageSize;

        internal unsafe ICONDIRENTRY ICONDIRENTRY
        {
            get
            {
                ICONDIRENTRY iconDirEntry;
                iconDirEntry.bColorCount = (byte)Encoder.Header.biClrUsed;
                iconDirEntry.bHeight = (byte)Encoder.Header.biHeight;
                iconDirEntry.bReserved = 0;
                iconDirEntry.bWidth = (byte)Encoder.Header.biWidth;
                iconDirEntry.dwBytesInRes = (uint)(sizeof(BITMAPINFOHEADER) +
                                                sizeof(RGBQUAD) * ColorsInPalette +
                                                Encoder.XOR.Length + Encoder.AND.Length);
                iconDirEntry.dwImageOffset = 0;
                iconDirEntry.wBitCount = Encoder.Header.biBitCount;
                iconDirEntry.wPlanes = Encoder.Header.biPlanes;
                return iconDirEntry;
            }
        }

        internal unsafe GRPICONDIRENTRY GRPICONDIRENTRY
        {
            get
            {
                GRPICONDIRENTRY groupIconDirEntry;
                groupIconDirEntry.bColorCount = (byte)Encoder.Header.biClrUsed;
                groupIconDirEntry.bHeight = (byte)Encoder.Header.biHeight;
                groupIconDirEntry.bReserved = 0;
                groupIconDirEntry.bWidth = (byte)Encoder.Header.biWidth;
                groupIconDirEntry.dwBytesInRes = (uint)IconImageSize;
                groupIconDirEntry.nID = 0;
                groupIconDirEntry.wBitCount = Encoder.Header.biBitCount;
                groupIconDirEntry.wPlanes = Encoder.Header.biPlanes;
                return groupIconDirEntry;
            }
        }
        #endregion

        #region Methods
        public unsafe void Set(Bitmap bitmap, Bitmap bitmapMask, Color transparentColor)
        {
            // We need to rotate the images, but we don't want to mess with the source image, lets create a clone
            Bitmap image = (Bitmap)bitmap.Clone();
            Bitmap mask = bitmapMask != null ? (Bitmap)bitmapMask.Clone() : null;
            try
            {
                //.NET has a bug flipping in the Y axis for 1bpp images, let do it ourself
                if (image.PixelFormat != PixelFormat.Format1bppIndexed)
                    image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                else
                    Tools.FlipYBitmap(image);

                if (mask != null)
                    Tools.FlipYBitmap(mask);

                if (mask != null && (image.Size != mask.Size || mask.PixelFormat != PixelFormat.Format1bppIndexed))
                    throw new InvalidMultiIconMaskBitmap();

                // Palette
                // Some icons programs like Axialis have program with a reduce palette, so lets create a complete palette instead
                RGBQUAD[] palette = Tools.RGBQUADFromColorArray(image);

                // Bitmap Header
                BITMAPINFOHEADER infoHeader = new BITMAPINFOHEADER();
                infoHeader.biSize = (uint)sizeof(BITMAPINFOHEADER);
                infoHeader.biWidth = (uint)image.Width;
                infoHeader.biHeight = (uint)image.Height * 2;
                infoHeader.biPlanes = 1;
                infoHeader.biBitCount = (ushort)Tools.BitsFromPixelFormat(image.PixelFormat);
                infoHeader.biCompression = IconImageFormat.BMP;
                infoHeader.biXPelsPerMeter = 0;
                infoHeader.biYPelsPerMeter = 0;
                infoHeader.biClrUsed = (uint)palette.Length;
                infoHeader.biClrImportant = 0;

                // IconImage
                Encoder.Header = infoHeader;
                Encoder.Colors = palette;

                // XOR Image
                BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
                IntPtr scanColor = bmpData.Scan0;
                Encoder.XOR = new byte[Math.Abs(bmpData.Stride) * bmpData.Height];
                Marshal.Copy(scanColor, Encoder.XOR, 0, Encoder.XOR.Length);
                image.UnlockBits(bmpData);
                infoHeader.biSizeImage = (uint)Encoder.XOR.Length;

                // AND Image
                if (mask == null)
                {
                    // Lets create the AND Image from the Color Image
                    Bitmap bmpBW = new Bitmap(image.Width, image.Height, PixelFormat.Format1bppIndexed);
                    BitmapData bmpBWData = bmpBW.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, bmpBW.PixelFormat);
                    IntPtr scanBW = bmpBWData.Scan0;
                    Encoder.AND = new byte[Math.Abs(bmpBWData.Stride) * bmpBWData.Height];

                    //Let extract the AND image from the XOR image
                    int strideC = Math.Abs(bmpData.Stride);
                    int strideB = Math.Abs(bmpBWData.Stride);
                    int bpp = Tools.BitsFromPixelFormat(image.PixelFormat);
                    int posCY;
                    int posCX;
                    int posBY;
                    int color;
                    Color tColor;
                    RGBQUAD paletteColor;

                    //If the image is 24 bits, then lets make sure alpha channel is 0
                    if (bpp == 24)
                        transparentColor = Color.FromArgb(0, transparentColor.R, transparentColor.G, transparentColor.B);

                    for (int y = 0; y < bmpData.Height; y++)
                    {
                        posBY = strideB * y;
                        posCY = strideC * y;
                        for (int x = 0; x < bmpData.Width; x++)
                        {
                            switch (bpp)
                            {
                                case 1:
                                    Encoder.AND[(x >> 3) + posCY] = (byte)Encoder.XOR[(x >> 3) + posCY];
                                    break;
                                case 4:
                                    color = Encoder.XOR[(x >> 1) + posCY];
                                    paletteColor = Encoder.Colors[(x & 1) == 0 ? color >> 4 : color & 0x0F];
                                    if (Tools.CompareRGBQUADToColor(paletteColor, transparentColor))
                                    {
                                        Encoder.AND[(x >> 3) + posBY] |= (byte)(0x80 >> (x & 7));
                                        Encoder.XOR[(x >> 1) + posCY] &= (byte)((x & 1) == 0 ? 0x0F : 0xF0);
                                    }
                                    break;
                                case 8:
                                    color = Encoder.XOR[x + posCY];
                                    paletteColor = Encoder.Colors[color];
                                    if (Tools.CompareRGBQUADToColor(paletteColor, transparentColor))
                                    {
                                        Encoder.AND[(x >> 3) + posBY] |= (byte)(0x80 >> (x & 7));
                                        Encoder.XOR[x + posCY] = 0;
                                    }
                                    break;
                                case 16:
                                    throw new NotSupportedException("16 bpp images are not supported for Icons");
                                case 24:
                                    posCX = x * 3;
                                    tColor = Color.FromArgb(0, Encoder.XOR[posCX + posCY + 0],
                                                                Encoder.XOR[posCX + posCY + 1],
                                                                Encoder.XOR[posCX + posCY + 2]);
                                    if (tColor == transparentColor)
                                        Encoder.AND[(x >> 3) + posBY] |= (byte)(0x80 >> (x & 7));
                                    break;
                                case 32:
                                    if (transparentColor == Color.Transparent)
                                    {
                                        if (Encoder.XOR[(x << 2) + posCY + 3] == 0)
                                            Encoder.AND[(x >> 3) + posBY] |= (byte)(0x80 >> (x & 7));
                                    }
                                    else if (Encoder.XOR[(x << 2) + posCY + 0] == transparentColor.B &&
     Encoder.XOR[(x << 2) + posCY + 1] == transparentColor.G &&
     Encoder.XOR[(x << 2) + posCY + 2] == transparentColor.R)
                                    {
                                        Encoder.AND[(x >> 3) + posBY] |= (byte)(0x80 >> (x & 7));
                                        Encoder.XOR[(x << 2) + posCY + 0] = 0;
                                        Encoder.XOR[(x << 2) + posCY + 1] = 0;
                                        Encoder.XOR[(x << 2) + posCY + 2] = 0;
                                    }
                                    else

                                        Encoder.XOR[(x << 2) + posCY + 3] = 255;
                                    break;
                            }
                        }
                    }
                    bmpBW.UnlockBits(bmpBWData);
                }
                else
                {
                    // Mask is coming by parameter, so we don't need to create it
                    BitmapData bmpBWData = mask.LockBits(new Rectangle(0, 0, mask.Width, mask.Height), ImageLockMode.ReadOnly, mask.PixelFormat);
                    IntPtr scanBW = bmpBWData.Scan0;
                    Encoder.AND = new byte[Math.Abs(bmpBWData.Stride) * bmpBWData.Height];
                    Marshal.Copy(scanBW, Encoder.AND, 0, Encoder.AND.Length);
                    mask.UnlockBits(bmpBWData);
                }
            }
            finally
            {
                if (image != null)
                    image.Dispose();
                if (mask != null)
                    mask.Dispose();
            }

        }
        #endregion

        #region Internal Methods
        internal unsafe void Read(Stream stream, int resourceSize)
        {
            switch (GetIconImageFormat(stream))
            {
                case IconImageFormat.BMP:
                    {
                        Encoder = new BMPEncoder();
                        Encoder.Read(stream, resourceSize);
                        break;
                    }
                case IconImageFormat.PNG:
                    {
                        Encoder = new PNGEncoder();
                        Encoder.Read(stream, resourceSize);
                        break;
                    }
            }
        }

        internal unsafe void Write(Stream stream)
        {
            Encoder.Write(stream);
        }
        #endregion

        #region Private Methods
        private unsafe IconImageFormat GetIconImageFormat(Stream stream)
        {
            long streamPos = stream.Position;

            try
            {
                BinaryReader br = new BinaryReader(stream);
                byte[] array = new byte[sizeof(BITMAPINFOHEADER)];
                byte bSignature = br.ReadByte();
                switch (bSignature)
                {
                    case 40: // BMP ?
                        return IconImageFormat.BMP;
                    case 0x89: // PNG ?
                        if (br.ReadInt16() == 0x4E50)
                            return IconImageFormat.PNG;
                        break;
                }
                return IconImageFormat.UNKNOWN;
            }
            finally
            {
                stream.Position = streamPos;
            }
        }
        #endregion
    }
}
