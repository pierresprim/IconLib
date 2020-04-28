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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing.IconLib.Exceptions;
using Microsoft.WindowsAPICodePack.Win32Native;
using System.Globalization;

namespace System.Drawing.IconLib.EncodingFormats
{
    [Author("Franco, Gustavo")]
    internal class PEFormat : ILibraryFormat
    {
        #region Variables Declaration
        private readonly object obj = new object();
        private static LinkedList<string> mIconsIDs;
        #endregion

        #region Methods
        public bool IsRecognizedFormat(in Stream stream)
        {
            stream.Position = 0;

            try
            {
                // Has a valid MS-DOS header?
                var dos_header = new IMAGE_DOS_HEADER(stream);

                if (dos_header.e_magic != (int)HeaderSignatures.IMAGE_DOS_SIGNATURE) //MZ

                    return false;

                //Lets position over the "PE" header
                _ = stream.Seek(dos_header.e_lfanew, SeekOrigin.Begin);

                //Lets read the NE header
                return new IMAGE_NT_HEADERS(stream).Signature == (int)HeaderSignatures.IMAGE_NT_SIGNATURE; //PE
            }
            catch (Exception) { }
            return false;
        }

        public unsafe MultiIcon Load(in Stream stream)
        {
            // LoadLibraryEx only can load files from File System, lets create a tmp file
            string tmpFile = null;

            IntPtr hLib = IntPtr.Zero;

            try
            {
                stream.Position = 0;

                // Find a tmp file where to dump the DLL stream, later we will remove this file
                tmpFile = Path.GetTempFileName();

                var fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write);
                byte[] buffer = new byte[stream.Length];

                _ = stream.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();

                if ((hLib = Core.LoadLibraryEx(tmpFile, IntPtr.Zero, (uint)LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE)) == IntPtr.Zero)

                    throw new InvalidFileException();

                LinkedList<string> iconsIDs;

                lock (obj)
                {
                    mIconsIDs = new LinkedList<string>();

                    bool bResult = Core.EnumResourceNames(hLib, (IntPtr)ResourceType.RT_GROUP_ICON, EnumResNameProc, IntPtr.Zero);

                    //if (bResult == false)
                    //{
                    // No Resources in this file
                    //}
                    iconsIDs = new LinkedList<string>(mIconsIDs);
                }

                var multiIcon = new MultiIcon();

                foreach (string id in iconsIDs)
                {
                    IntPtr hRsrc = IntPtr.Zero;

                    hRsrc = Win32.IS_INTRESOURCE(id)
                        ? Core.FindResource(hLib, (IntPtr)int.Parse(id, CultureInfo.InvariantCulture), (IntPtr)ResourceType.RT_GROUP_ICON)
                        : Win32.FindResource(hLib, id, (IntPtr)ResourceType.RT_GROUP_ICON);

                    if (hRsrc == IntPtr.Zero)

                        throw new InvalidFileException();

                    IntPtr hGlobal = Core.LoadResource(hLib, hRsrc);

                    if (hGlobal == IntPtr.Zero)

                        throw new InvalidFileException();

                    var pDirectory = (MEMICONDIR*)Core.LockResource(hGlobal);

                    if (pDirectory->wCount != 0)
                    {
                        MEMICONDIRENTRY* pEntry = &(pDirectory->arEntries);

                        var singleIcon = new SingleIcon(id);

                        for (int i = 0; i < pDirectory->wCount; i++)
                        {
                            IntPtr hIconInfo = Core.FindResource(hLib, (IntPtr)pEntry[i].wId, (IntPtr)ResourceType.RT_ICON);

                            if (hIconInfo == IntPtr.Zero)

                                throw new InvalidFileException();

                            IntPtr hIconRes = Core.LoadResource(hLib, hIconInfo);

                            if (hIconRes == IntPtr.Zero)

                                throw new InvalidFileException();

                            IntPtr dibBits = Core.LockResource(hIconRes);

                            if (dibBits == IntPtr.Zero)

                                throw new InvalidFileException();

                            buffer = new byte[Core.SizeofResource(hLib, hIconInfo)];

                            Marshal.Copy(dibBits, buffer, 0, buffer.Length);

                            var ms = new MemoryStream(buffer);
                            var iconImage = new IconImage(ms, buffer.Length);
                            _ = singleIcon.Add(iconImage);
                        }

                        multiIcon.Add(singleIcon);
                    }
                }

                // If everything went well then lets return the multiIcon.
                return multiIcon;
            }
            catch (Exception)
            {
                throw new InvalidFileException();
            }
            finally
            {
                if (hLib != null)

                    _ = Core.FreeLibrary(hLib);

                if (tmpFile is object)

                    File.Delete(tmpFile);
            }
        }

        public unsafe void Save(in MultiIcon multiIcon, in Stream stream)
        {
            // LoadLibraryEx only can load files from File System, lets create a tmp file
            string tmpFile = null;
            IntPtr hLib = IntPtr.Zero;
            MemoryStream ms;
            bool bResult;

            try
            {
                stream.Position = 0;

                // Find a tmp file where to dump the DLL stream, later we will remove this file
                tmpFile = Path.GetTempFileName();

                var fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write);
                byte[] buffer = Resource.EmptyDll;
                _ = stream.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();

                // Begin the injection process
                IntPtr updPtr = Core.BeginUpdateResource(tmpFile, false);

                if (updPtr == IntPtr.Zero)

                    throw new InvalidFileException();

                ushort iconIndex = 1;

                foreach (SingleIcon singleIcon in multiIcon)
                {
                    // Lets scan all groups
                    GRPICONDIR grpIconDir = GRPICONDIR.Initalizated;
                    grpIconDir.idCount = (ushort)singleIcon.Count;
                    grpIconDir.idEntries = new GRPICONDIRENTRY[grpIconDir.idCount];

                    for (int i = 0; i < singleIcon.Count; i++)
                    {
                        // Inside every Icon let update every image format
                        IconImage iconImage = singleIcon[i];
                        grpIconDir.idEntries[i] = iconImage.GRPICONDIRENTRY;
                        grpIconDir.idEntries[i].nID = iconIndex;

                        // Buffer creation with the same size of the icon to optimize write call
                        ms = new MemoryStream((int)grpIconDir.idEntries[i].dwBytesInRes);
                        iconImage.Write(ms);
                        buffer = ms.GetBuffer();

                        // Update resource but it doesn't write to disk yet
                        bResult = Core.UpdateResource(updPtr, (IntPtr)ResourceType.RT_ICON, (IntPtr)iconIndex, 0, buffer, (uint)ms.Length);

                        iconIndex++;

                        // For some reason Windows will fail if there are many calls to update resource and no call to endUpdateResource
                        // It is like there some internal buffer that gets full, after that all calls fail.
                        // This workaround will save the changes every 70 icons, for big files this slow the saving process significantly
                        // but I didn't find a way to make EndUpdateResource works without save frequently
                        if ((iconIndex % 70) == 0)
                        {
                            bResult = Core.EndUpdateResource(updPtr, false);
                            updPtr = Core.BeginUpdateResource(tmpFile, false);

                            if (updPtr == IntPtr.Zero)

                                throw new InvalidFileException();
                        }
                    }

                    // Buffer creation with the same size of the group to optimize write call
                    ms = new MemoryStream(grpIconDir.GroupDirSize);
                    grpIconDir.Write(ms);
                    buffer = ms.GetBuffer();

                    if (int.TryParse(singleIcon.Name, out int id))

                        // Write id as an integer
                        bResult = Win32.UpdateResource(updPtr, (int)ResourceType.RT_GROUP_ICON, (IntPtr)id, 0, buffer, (uint)ms.Length);

                    else
                    {
                        // Write id as string
                        IntPtr pName = Marshal.StringToHGlobalAnsi(singleIcon.Name.ToUpper(CultureInfo.CurrentCulture));
                        bResult = Win32.UpdateResource(updPtr, (int)ResourceType.RT_GROUP_ICON, pName, 0, buffer, (uint)ms.Length);
                        Marshal.FreeHGlobal(pName);
                    }
                }

                // Last call to update the file with the rest not that was not write before
                bResult = Core.EndUpdateResource(updPtr, false);

                // Because Windows Resource functions requiere a filepath, and we need to return an string then lets open
                // the temporary file and dump it to the stream received as parameter.
                fs = new FileStream(tmpFile, FileMode.Open, FileAccess.Read);
                buffer = new byte[fs.Length];
                _ = fs.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, buffer.Length);
                fs.Close();
            }
            catch (Exception)
            {
                throw new InvalidFileException();
            }
            finally
            {
                if (hLib != null)

                    _ = Core.FreeLibrary(hLib);

                if (tmpFile is object)

                    File.Delete(tmpFile);
            }
        }
        #endregion

        #region Private Methods
        private static unsafe bool EnumResNameProc(IntPtr hModule, IntPtr pType, IntPtr pName, IntPtr param)
        {
            _ = mIconsIDs.AddLast(Win32.IS_INTRESOURCE(pName) ? pName.ToString() : Marshal.PtrToStringUni(pName));

            return true;
        }
        #endregion
    }
}
