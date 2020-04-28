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
using System.Drawing.Imaging;
using Microsoft.WindowsAPICodePack.Win32Native.GDI;
using Microsoft.WindowsAPICodePack.Win32Native;

namespace System.Drawing.IconLib
{
    [Author("Franco, Gustavo")]
    internal static class Tools
    {
        #region Methods
        public static bool CompareRGBQuadToColor(in RGBQuad RGBQuad, in Color color) => RGBQuad.rgbRed == color.R && RGBQuad.rgbGreen == color.G && RGBQuad.rgbBlue == color.B;

        public static unsafe void FlipYBitmap(in Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format1bppIndexed)

                return;

            // .Net bug.. it can't flip in the Y axis a 1bpp properly
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

            byte* pixelPtr = (byte*)bitmapData.Scan0.ToPointer();
            byte[] tmpbuffer = new byte[bitmapData.Stride];

            fixed (byte* lptmpbuffer = tmpbuffer)

                for (int i = 0; i < bitmap.Height / 2; i++)
                {
                    Core.CopyMemory(lptmpbuffer, pixelPtr + (i * bitmapData.Stride), (uint)bitmapData.Stride);
                    Core.CopyMemory(pixelPtr + (i * bitmapData.Stride), pixelPtr + (((bitmap.Height - 1) - i) * bitmapData.Stride), (uint)bitmapData.Stride);
                    Core.CopyMemory(pixelPtr + (((bitmap.Height - 1) - i) * bitmapData.Stride), lptmpbuffer, (uint)bitmapData.Stride);
                }

            bitmap.UnlockBits(bitmapData);
        }

        public static RGBQuad[] StandarizePalette(in RGBQuad[] palette)
        {
            var newPalette = new RGBQuad[256];

            for (int i = 0; i < palette.Length; i++)

                newPalette[i] = palette[i];

            return newPalette;
        }

        public static RGBQuad[] RGBQuadFromColorArray(in Bitmap bmp)
        {
            // Some programs as Axialis have problems with a reduced palette, so lets create a full palette
            int bits = BitsFromPixelFormat(bmp.PixelFormat);
            var rgbArray = new RGBQuad[bits <= 8 ? (1 << bits) : 0];
            Color[] entries = bmp.Palette.Entries;

            for (int i = 0; i < entries.Length; i++)
            {
                rgbArray[i].rgbRed = entries[i].R;
                rgbArray[i].rgbGreen = entries[i].G;
                rgbArray[i].rgbBlue = entries[i].B;
            }

            return rgbArray;
        }

        public static int BitsFromPixelFormat(in PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format4bppIndexed:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 16;
                case PixelFormat.Format24bppRgb:
                    return 24;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;
                default:
                    return 0;
            }
        }
        #endregion
    }
}
