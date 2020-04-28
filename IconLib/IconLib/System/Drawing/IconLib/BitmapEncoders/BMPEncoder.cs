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
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Win32Native.GDI;

namespace System.Drawing.IconLib.BitmapEncoders
{
    [Author("Franco, Gustavo")]
    internal class BMPEncoder : ImageEncoder
    {
        #region Constructors
        public BMPEncoder() { }
        #endregion

        #region Properties
        public override IconImageFormat IconImageFormat => IconImageFormat.BMP;
        #endregion

        #region Methods
        public unsafe override void Read(in Stream stream, in int resourceSize)
        {
            // BitmapInfoHeader
            Util.Read(ref mHeader, stream);

            // Palette
            mColors = new RGBQuad[ColorsInPalette];
            byte[] colorBuffer = new byte[mColors.Length * Marshal.SizeOf<RGBQuad>()];
            _ = stream.Read(colorBuffer, 0, colorBuffer.Length);
            var handle = GCHandle.Alloc(mColors, GCHandleType.Pinned);
            Marshal.Copy(colorBuffer, 0, handle.AddrOfPinnedObject(), colorBuffer.Length);
            handle.Free();

            // XOR Image
            int stride = (((mHeader.biWidth * mHeader.biBitCount) + 31) & ~31) >> 3;
            mXOR = new byte[stride * (mHeader.biHeight / 2)];
            _ = stream.Read(mXOR, 0, mXOR.Length);

            // AND Image
            stride = (((mHeader.biWidth * 1) + 31) & ~31) >> 3;
            mAND = new byte[stride * (mHeader.biHeight / 2)];
            _ = stream.Read(mAND, 0, mAND.Length);
        }

        public unsafe override void Write(in Stream stream)
        {
            // BinaryReader br = new BinaryReader(stream);

            // BitmapInfoHeader
            Util.Write(in mHeader, stream);

            // Palette
            byte[] buffer = new byte[ColorsInPalette * Marshal.SizeOf<RGBQuad>()];
            var handle = GCHandle.Alloc(mColors, GCHandleType.Pinned);
            Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, buffer.Length);
            handle.Free();
            stream.Write(buffer, 0, buffer.Length);

            // XOR Image
            stream.Write(mXOR, 0, mXOR.Length);

            // AND Image
            stream.Write(mAND, 0, mAND.Length);
        }
        #endregion
    }
}
