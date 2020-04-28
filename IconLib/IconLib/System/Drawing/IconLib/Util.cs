using Microsoft.WindowsAPICodePack.Win32Native.GDI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.IconLib
{
    internal static class Util
    {

        #region Methods
        public unsafe static void Read<T>(ref T @struct, in Stream stream) where T : unmanaged
        {
            byte[] array = new byte[Marshal.SizeOf<T>()];
            _ = stream.Read(array, 0, array.Length);
            fixed (byte* pData = array)
                @struct = *(T*)pData;
        }

        public unsafe static void Write<T>(in T @struct, in Stream stream) where T : unmanaged
        {
            byte[] array = new byte[Marshal.SizeOf<T>()];
            fixed (T* ptr = &@struct)
                Marshal.Copy((IntPtr)ptr, array, 0, Marshal.SizeOf<T>());
            stream.Write(array, 0, sizeof(T));
        }
        #endregion

        public static void Write<T>( in T[] structs, in Stream stream) where T : unmanaged
        {
            foreach (T @struct in structs)
                Write(@struct, stream);
        }

        #region BitmapInfoHeader

        #region Constructors
        public static BitmapInfoHeader GetBitmapInfoHeader(Stream stream)
        {
            var header = new BitmapInfoHeader();

            Read(ref header, stream);

            return header;
        }
        #endregion
        #endregion

        #region RGBQuad
        #region Methods
        public static void Set(this RGBQuad RGBQuad, byte r, byte g, byte b)
        {
            RGBQuad.rgbRed = r;
            RGBQuad.rgbGreen = g;
            RGBQuad.rgbBlue = b;
        }
        #endregion
        #endregion

    }
}
