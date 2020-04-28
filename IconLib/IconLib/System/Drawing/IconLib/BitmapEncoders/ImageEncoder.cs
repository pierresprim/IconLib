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
using static System.Drawing.IconLib.Util;

namespace System.Drawing.IconLib.BitmapEncoders
{
    [Author("Franco, Gustavo")]
    internal abstract class ImageEncoder
    {
        #region Variables Declaration
        protected BitmapInfoHeader mHeader;
        protected RGBQuad[] mColors;
        protected byte[] mXOR;
        protected byte[] mAND;
        #endregion

        #region Constructors
        protected ImageEncoder() { }
        #endregion

        #region Properties
        public unsafe virtual Icon Icon
        {
            get
            {
                using (var ms = new MemoryStream())
                {

                    // ICONDIR
                    ICONDIR iconDir = ICONDIR.Initalizated;
                    iconDir.idCount = 1;
                    Write<ICONDIR>(iconDir, ms);

                    // ICONDIRENTRY 
                    var iconEntry = new ICONDIRENTRY
                    {
                        bColorCount = (byte)mHeader.biClrUsed,
                        bHeight = (byte)(mHeader.biHeight / 2),
                        bReserved = 0,
                        bWidth = (byte)mHeader.biWidth,
                        dwBytesInRes = (uint)(Marshal.SizeOf<BitmapInfoHeader>() +
                                                (Marshal.SizeOf<RGBQuad>() * ColorsInPalette) +
                                                mXOR.Length + mAND.Length),
                        dwImageOffset = (uint)(sizeof(ICONDIR) + sizeof(ICONDIRENTRY)),
                        wBitCount = mHeader.biBitCount,
                        wPlanes = mHeader.biPlanes
                    };
                    Write<ICONDIRENTRY>(iconEntry, ms);

                    // Image Info Header
                    _ = ms.Seek(iconEntry.dwImageOffset, SeekOrigin.Begin);
                    Write<BitmapInfoHeader>(mHeader, ms);

                    // Image Palette
                    byte[] buffer = new byte[Marshal.SizeOf<RGBQuad>() * ColorsInPalette];
                    var handle = GCHandle.Alloc(mColors, GCHandleType.Pinned);
                    Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, buffer.Length);
                    handle.Free();
                    ms.Write(buffer, 0, buffer.Length);

                    // Image XOR Image
                    ms.Write(mXOR, 0, mXOR.Length);

                    // Image AND Image
                    ms.Write(mAND, 0, mAND.Length);

                    // Rewind the stream
                    ms.Position = 0;

                    return new Icon(ms, iconEntry.bWidth, iconEntry.bHeight);
                }
            }
        }

        public virtual BitmapInfoHeader Header
        {
            get => mHeader;
            set => mHeader = value;
        }

        public virtual RGBQuad[] Colors
        {
            get => mColors;
            set => mColors = value;
        }

        public virtual byte[] XOR
        {
            get => mXOR;
            set
            {
                mHeader.biSizeImage = (ushort)value.Length;
                mXOR = value;
            }
        }

        public virtual byte[] AND
        {
            get => mAND;
            set => mAND = value;
        }

        public unsafe virtual int ColorsInPalette => (int)(mHeader.biClrUsed != 0 ?
                                    mHeader.biClrUsed :
                                    mHeader.biBitCount <= 8 ?
                                        (uint)(1 << mHeader.biBitCount) : 0);

        public unsafe virtual int ImageSize => Marshal.SizeOf<BitmapInfoHeader>() + (Marshal.SizeOf<RGBQuad>() * ColorsInPalette) + mXOR.Length + mAND.Length;

        public abstract IconImageFormat IconImageFormat { get; }
        #endregion

        #region Abstract Methods
        public abstract void Read(in Stream stream, in int resourceSize);
        public abstract void Write(in Stream stream);
        #endregion

        #region Methods
        public void CopyFrom(in ImageEncoder encoder)
        {
            mHeader = encoder.mHeader;
            mColors = encoder.mColors;
            mXOR = encoder.mXOR;
            mAND = encoder.mAND;
        }
        #endregion
    }
}
