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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Drawing.IconLib
{
    [Author("Franco, Gustavo")]
    internal class Win32
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool UpdateResource(IntPtr hUpdate, uint lpType, ref string pName, ushort wLanguage, byte[] lpData, uint cbData);
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		public static extern bool UpdateResource(IntPtr hUpdate, uint lpType, IntPtr pName, ushort wLanguage, byte[] lpData, uint cbData);
		[DllImport("kernel32.dll", SetLastError = true)]
		public unsafe static extern bool UpdateResource(IntPtr hUpdate, uint lpType, byte[] pName, ushort wLanguage, byte[] lpData, uint cbData);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindResource(IntPtr hModule, string resourceID, IntPtr type);

		#region MACROS
		public static bool IS_INTRESOURCE(IntPtr value) =>
#if x64
			((ulong)value) <= ushort.MaxValue;
#elif x86
            ((uint)value) <= ushort.MaxValue;
#else
			Environment.Is64BitProcess ? ((ulong)value) <= ushort.MaxValue : ((uint)value) <= ushort.MaxValue;
#endif


		public static bool IS_INTRESOURCE(string value) => int.TryParse(value, out _);

        public static int MAKEINTRESOURCE(int resource) => 0x0000FFFF & resource;
        #endregion
    }
}
