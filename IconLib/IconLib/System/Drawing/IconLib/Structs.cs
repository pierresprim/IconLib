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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static System.Drawing.IconLib.Util;
using WinCopies.Collections;

namespace System.Drawing.IconLib
{

    #region SEGMENT_ENTRY
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct SEGMENT_ENTRY
    {
        public ushort wOffset;
        public ushort wLength;
        public ushort wFlag;
        public ushort wMinSize;
    }
    #endregion

    #region SEGMENT_TABLE
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct SEGMENT_TABLE
    {
        public SEGMENT_ENTRY[] seg_entries;

        #region Methods
        public void Write(in Stream stream) => Write<SEGMENT_ENTRY>(seg_entries, stream);
        #endregion
    }
    #endregion

    #region IMAGE_DOS_HEADER
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct IMAGE_DOS_HEADER // DOS .EXE header
    {
        public ushort e_magic;           // Magic number
        public ushort e_cblp;            // Bytes on last page of file
        public ushort e_cp;              // Pages in file
        public ushort e_crlc;            // Relocations
        public ushort e_cparhdr;         // Size of header in paragraphs
        public ushort e_minalloc;        // Minimum extra paragraphs needed
        public ushort e_maxalloc;        // Maximum extra paragraphs needed
        public ushort e_ss;              // Initial (relative) SS value
        public ushort e_sp;              // Initial SP value
        public ushort e_csum;            // Checksum
        public ushort e_ip;              // Initial IP value
        public ushort e_cs;              // Initial (relative) CS value
        public ushort e_lfarlc;          // File address of relocation table
        public ushort e_ovno;            // Overlay number
        public fixed short e_res[4];    // Reserved words
        public ushort e_oemid;           // OEM identifier (for e_oeminfo)
        public ushort e_oeminfo;         // OEM information; e_oemid specific
        public fixed short e_res2[10];   // Reserved words
        public uint e_lfanew;           // File address of new exe header

        #region Constructors
        public IMAGE_DOS_HEADER(Stream stream)
        {
            this = new IMAGE_DOS_HEADER();
            Read(ref this, stream);
        }
        #endregion
    }
    #endregion

    #region IMAGE_FILE_HEADER
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct IMAGE_FILE_HEADER
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }
    #endregion

    #region IMAGE_NT_HEADERS
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal struct IMAGE_DATA_DIRECTORY
    {
        public uint VirtualAddress;
        public uint Size;
    }
    #endregion

    #region IMAGE_NT_HEADERS
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct IMAGE_OPTIONAL_HEADER
    {
        //
        // Standard fields.
        //
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;

        //
        // NT additional fields.
        //
        public uint ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public IMAGE_DATA_DIRECTORY DataDirectory1;
        public IMAGE_DATA_DIRECTORY DataDirectory2;
        public IMAGE_DATA_DIRECTORY DataDirectory3;
        public IMAGE_DATA_DIRECTORY DataDirectory4;
        public IMAGE_DATA_DIRECTORY DataDirectory5;
        public IMAGE_DATA_DIRECTORY DataDirectory6;
        public IMAGE_DATA_DIRECTORY DataDirectory7;
        public IMAGE_DATA_DIRECTORY DataDirectory8;
        public IMAGE_DATA_DIRECTORY DataDirectory9;
        public IMAGE_DATA_DIRECTORY DataDirectory10;
        public IMAGE_DATA_DIRECTORY DataDirectory11;
        public IMAGE_DATA_DIRECTORY DataDirectory12;
        public IMAGE_DATA_DIRECTORY DataDirectory13;
        public IMAGE_DATA_DIRECTORY DataDirectory14;
        public IMAGE_DATA_DIRECTORY DataDirectory15;
        public IMAGE_DATA_DIRECTORY DataDirectory16;
    }
    #endregion

    #region IMAGE_NT_HEADERS
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct IMAGE_NT_HEADERS
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER OptionalHeader;

        #region Constructors
        public IMAGE_NT_HEADERS(Stream stream)
        {
            this = new IMAGE_NT_HEADERS();
            Read(stream);
        }
        #endregion

        #region Methods
        public void Read(Stream stream)
        {
            var br = new BinaryReader(stream);

            Signature = br.ReadUInt32();

            Read<IMAGE_FILE_HEADER>(ref FileHeader, stream);

            Read<IMAGE_OPTIONAL_HEADER>(ref OptionalHeader, stream);
        }
        #endregion
    }
    #endregion

    #region IMAGE_OS2_HEADER
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct IMAGE_OS2_HEADER // OS/2 .EXE header
    {
        public ushort ne_magic;           // Magic number
        public sbyte ne_ver;             // Version number
        public sbyte ne_rev;             // Revision number
        public ushort ne_enttab;          // Offset of Entry Table
        public ushort ne_cbenttab;        // Number of bytes in Entry Table
        public uint ne_crc;             // Checksum of whole file
        public ushort ne_flags;           // Flag word
        public ushort ne_autodata;        // Automatic data segment number
        public ushort ne_heap;            // Initial heap allocation
        public ushort ne_stack;           // Initial stack allocation
        public uint ne_csip;            // Initial CS:IP setting
        public uint ne_sssp;            // Initial SS:SP setting
        public ushort ne_cseg;            // Count of file segments
        public ushort ne_cmod;            // Entries in Module Reference Table
        public ushort ne_cbnrestab;       // Size of non-resident name table
        public ushort ne_segtab;          // Offset of Segment Table
        public ushort ne_rsrctab;         // Offset of Resource Table
        public ushort ne_restab;          // Offset of resident name table
        public ushort ne_modtab;          // Offset of Module Reference Table
        public ushort ne_imptab;          // Offset of Imported Names Table
        public uint ne_nrestab;         // Offset of Non-resident Names Table
        public ushort ne_cmovent;         // Count of movable entries
        public ushort ne_align;           // Segment alignment shift count
        public ushort ne_cres;            // Count of resource segments
        public byte ne_exetyp;          // Target Operating system
        public byte ne_flagsothers;     // Other .EXE flags
        public ushort ne_pretthunks;      // offset to return thunks
        public ushort ne_psegrefbytes;    // offset to segment ref. bytes
        public ushort ne_swaparea;        // Minimum code swap area size
        public ushort ne_expver;          // Expected Windows version number

        #region Constructors
        public IMAGE_OS2_HEADER(Stream stream)
        {
            this = new IMAGE_OS2_HEADER();
            Read(ref this, stream);
        }
        #endregion
    }
    #endregion

    #region RESOURCE_TABLE
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal struct RESOURCE_TABLE
    {
        public ushort rscAlignShift;
        public TYPEINFO[] rscTypes;
        public ushort rscEndTypes;
        public byte[] rscResourceNames;
        public byte rscEndNames;

        #region Constructors
        public RESOURCE_TABLE(Stream stream)
        {
            this = new RESOURCE_TABLE();
            Read(stream);
        }
        #endregion

        #region Properties
        public string[] ResourceNames
        {
            get
            {
                var names = new ArrayBuilder<string>();
                int pos = 0;
                byte nameLen;
                while (pos < rscResourceNames.Length)
                {
                    nameLen = rscResourceNames[pos++];
                    byte[] name = new byte[nameLen];
#if !NETFRAMEWORK
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
                    _ = names.AddLast(Encoding.GetEncoding(1251).GetString(rscResourceNames, pos, nameLen));
                    pos += nameLen;
                }
                return names.ToArray();
            }
        }
        #endregion

        #region Methods
        public void Read(Stream stream)
        {
            var br = new BinaryReader(stream);

            rscAlignShift = br.ReadUInt16();
            var lTypeInfo = new ArrayBuilder<TYPEINFO>();
            for (var typeInfo = new TYPEINFO(stream); typeInfo.rtTypeID != 0; typeInfo = new TYPEINFO(stream))
                _ = lTypeInfo.AddLast(typeInfo);
            rscTypes = lTypeInfo.ToArray();
            rscEndTypes = 0;
            rscResourceNames = Array.Empty<byte>();
            for (byte nameLen = br.ReadByte(); nameLen != 0; nameLen = br.ReadByte())
            {
                byte[] newArray = new byte[rscResourceNames.Length + nameLen + 1];
                rscResourceNames.CopyTo(newArray, 0);
                newArray[rscResourceNames.Length] = nameLen;
                _ = stream.Read(newArray, rscResourceNames.Length + 1, nameLen);
                rscResourceNames = newArray;
            }
            rscEndNames = 0;
        }

        public unsafe void Write(Stream stream)
        {
            var br = new BinaryWriter(stream);

            br.Write(rscAlignShift);

            foreach (TYPEINFO typeInfo in rscTypes)
                typeInfo.Write(stream);

            br.Write(rscEndTypes);

            br.Write(rscResourceNames);
            br.Write(rscEndNames);
        }

        public List<GRPICONDIR> GetGroupIcons(Stream stream)
        {
            var groupIconDir = new List<GRPICONDIR>();
            for (int i = 0; i < rscTypes.Length; i++)
            {
                if (rscTypes[i].ResourceType != ResourceType.RT_GROUP_ICON)
                    continue;
                //StreamWriter sw = new StreamWriter("c:\\borrar\\icons\\test2\\dump_load_groups.txt", false);
                //sw.Write("index\tID\toffset\tlength\r\n");

                for (int j = 0; j < rscTypes[i].rtNameInfo.Length; j++)
                {
                    _ = stream.Seek((1 << rscAlignShift) * rscTypes[i].rtNameInfo[j].rnOffset, SeekOrigin.Begin);
                    var grpIconDir = new GRPICONDIR(stream);
                    //sw.Write(j.ToString("000") + "\t" + rscTypes[i].rtNameInfo[j].ID + "\t" + rscTypes[i].rtNameInfo[j].rnOffset + "\t" + rscTypes[i].rtNameInfo[j].rnLength + "\t" + grpIconDir.idEntries.Length + "\r\n");

                    //foreach(GRPICONDIRENTRY gentry in grpIconDir.idEntries)
                    //sw.Write("    " + gentry.nID + "\r\n");

                    groupIconDir.Add(grpIconDir);
                }
                //sw.Close();
                break;
            }
            return groupIconDir;
        }

        public void SetGroupIcons(Stream stream, IList<GRPICONDIR> grpIconDir)
        {
            int i;
            for (i = 0; i < rscTypes.Length; i++)
            {
                if (rscTypes[i].ResourceType == ResourceType.RT_GROUP_ICON)
                    break;
            }

            //StreamWriter sw = new StreamWriter("c:\\borrar\\icons\\test2\\dump_save_groups.txt", false);
            //sw.Write("index\tID\toffset\tlength\r\n");

            for (int j = 0; j < rscTypes[i].rtNameInfo.Length; j++)
            {
                _ = stream.Seek((1 << rscAlignShift) * rscTypes[i].rtNameInfo[j].rnOffset, SeekOrigin.Begin);
                //sw.Write(j.ToString("000") + "\t" + rscTypes[i].rtNameInfo[j].ID + "\t" + rscTypes[i].rtNameInfo[j].rnOffset + "\t" + rscTypes[i].rtNameInfo[j].rnLength + "\t" + grpIconDir[j].idEntries.Length + "\r\n");

                //foreach(GRPICONDIRENTRY gentry in grpIconDir[j].idEntries)
                //sw.Write("    " + gentry.nID + "\r\n");

                grpIconDir[j].Write(stream);
            }
            //sw.Close();
        }

        public Dictionary<ushort, IconImage> GetIcons(Stream stream)
        {
            var icons = new Dictionary<ushort, IconImage>();
            int i;
            for (i = 0; i < rscTypes.Length; i++)
            {
                if (rscTypes[i].ResourceType == ResourceType.RT_ICON)
                    break;
            }

            //StreamWriter sw = new StreamWriter("c:\\borrar\\icons\\test2\\dump_load_icons.txt", false);
            //sw.Write("index\tID\toffset\tlength\r\n");
            //string[] names = ResourceNames;
            for (int j = 0; j < rscTypes[i].rtNameInfo.Length; j++)
            {
                _ = stream.Seek((1 << rscAlignShift) * rscTypes[i].rtNameInfo[j].rnOffset, SeekOrigin.Begin);
                //sw.Write(j.ToString("000") + "\t" + rscTypes[i].rtNameInfo[j].ID + "\t" + rscTypes[i].rtNameInfo[j].rnOffset + "\t" + rscTypes[i].rtNameInfo[j].rnLength + "\r\n");
                icons.Add(rscTypes[i].rtNameInfo[j].ID, new IconImage(stream, (1 << rscAlignShift) * rscTypes[i].rtNameInfo[j].rnLength));
            }
            //sw.Close();

            return icons;
        }

        public void SetIcons(Stream stream, Dictionary<ushort, IconImage> icons)
        {
            int i;
            for (i = 0; i < rscTypes.Length; i++)
            {
                if (rscTypes[i].ResourceType == ResourceType.RT_ICON)
                    break;
            }

            //string[] names = ResourceNames;
            //StreamWriter sw = new StreamWriter("c:\\borrar\\icons\\test2\\dump_save_icons.txt", false);
            //sw.Write("index\tID\toffset\tlength\r\n");

            for (int j = 0; j < rscTypes[i].rtNameInfo.Length; j++)
            {
                _ = stream.Seek((1 << rscAlignShift) * rscTypes[i].rtNameInfo[j].rnOffset, SeekOrigin.Begin);
                //sw.Write(j.ToString("000") + "\t" + rscTypes[i].rtNameInfo[j].ID + "\t" + rscTypes[i].rtNameInfo[j].rnOffset + "\t" + rscTypes[i].rtNameInfo[j].rnLength + "\r\n");
                icons[rscTypes[i].rtNameInfo[j].ID].Write(stream);
            }
            //sw.Close();
        }

        public List<ushort> GetGroupIDs(/*Stream stream*/)
        {
            var groupIconIDs = new ArrayBuilder<ushort>();
            int i;
            for (i = 0; i < rscTypes.Length; i++)
            {
                if (rscTypes[i].ResourceType == ResourceType.RT_GROUP_ICON)
                    break;
            }

            for (int j = 0; j < rscTypes[i].rtNameInfo.Length; j++)
                _ = groupIconIDs.AddLast(rscTypes[i].rtNameInfo[j].ID);
            return groupIconIDs.ToList();
        }
        #endregion
    }
    #endregion

    #region TYPEINFO
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal struct TYPEINFO
    {
        public ushort rtTypeID;
        public ushort rtResourceCount;
        public uint rtReserved;
        public TNAMEINFO[] rtNameInfo;

        #region Constructors
        public TYPEINFO(Stream stream)
        {
            this = new TYPEINFO();
            Read(stream);
        }
        #endregion

        #region Properties
        public ResourceType ResourceType => (ResourceType)(rtTypeID & 0xFF);
        #endregion

        #region Methods
        public void Read(Stream stream)
        {
            var br = new BinaryReader(stream);

            rtTypeID = br.ReadUInt16();

            if (rtTypeID == 0)
                return;

            rtResourceCount = br.ReadUInt16();
            rtReserved = br.ReadUInt32();

            rtNameInfo = new TNAMEINFO[rtResourceCount];
            for (int i = 0; i < rtNameInfo.Length; i++)
                Read<TNAMEINFO>(ref rtNameInfo[i], stream);
        }

        public void Write(Stream stream)
        {
            var bw = new BinaryWriter(stream);

            bw.Write(rtTypeID);
            bw.Write(rtResourceCount);
            bw.Write(rtReserved);

            Write<TNAMEINFO>(rtNameInfo, stream);
        }
        #endregion
    }
    #endregion

    #region TNAMEINFO
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct TNAMEINFO
    {
        public ushort rnOffset;
        public ushort rnLength;
        public ushort rnFlags;
        public ushort rnID;
        public ushort rnHandle;
        public ushort rnUsage;

        #region Constructors
        public TNAMEINFO(Stream stream)
        {
            this = new TNAMEINFO();
            Read(ref this, stream);
        }
        #endregion

        #region Properties
        public ushort ID => rnID > 0x8000 ? (ushort)(rnID & ~0x8000) : rnID;

        public ResourceMemoryType ResourceMemoryType => (ResourceMemoryType)rnFlags;
        #endregion
    }
    #endregion

    #region ICONDIR
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct ICONDIR
    {
        public ushort idReserved;
        public ushort idType;
        public ushort idCount;

        #region Constructors
        public ICONDIR(ushort reserved, ushort type, ushort count)
        {
            idReserved = reserved;
            idType = type;
            idCount = count;
        }

        public ICONDIR(Stream stream)
        {
            this = new ICONDIR();
            Read(ref this, stream);
        }
        #endregion

        #region Properties
        public static ICONDIR Initalizated => new ICONDIR(0, 1, 0);
        #endregion
    }
    #endregion

    #region MEMICONDIRENTRY
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct MEMICONDIRENTRY
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        public byte bReserved;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public ushort wId;
    }
    #endregion

    #region MEMICONDIR
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal struct MEMICONDIR
    {
        public ushort wReserved;
        public ushort wType;
        public ushort wCount;
        public MEMICONDIRENTRY arEntries; // inline array
    }
    #endregion

    #region GRPICONDIR
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct GRPICONDIR
    {
        public ushort idReserved;
        public ushort idType;
        public ushort idCount;
        public GRPICONDIRENTRY[] idEntries;

        #region Constructors
        public GRPICONDIR(ushort reserved, ushort type, ushort count)
        {
            idReserved = reserved;
            idType = type;
            idCount = count;
            idEntries = Array.Empty<GRPICONDIRENTRY>();
        }

        public GRPICONDIR(Stream stream)
        {
            this = new GRPICONDIR();
            Read(stream);
        }
        #endregion

        #region Properties
        public static GRPICONDIR Initalizated => new GRPICONDIR(0, 1, 0);

        public int GroupDirSize => 6 + (idEntries.Length * sizeof(GRPICONDIRENTRY));
        #endregion

        #region Methods
        public void Read(Stream stream)
        {
            var br = new BinaryReader(stream);

            idReserved = br.ReadUInt16();
            idType = br.ReadUInt16();
            idCount = br.ReadUInt16();

            idEntries = new GRPICONDIRENTRY[idCount];
            for (int i = 0; i < idCount; i++)
                idEntries[i] = new GRPICONDIRENTRY(stream);
        }

        public void Write(Stream stream)
        {
            var bw = new BinaryWriter(stream);

            bw.Write(idReserved);
            bw.Write(idType);
            bw.Write(idCount);

            for (int i = 0; i < idCount; i++)
                Write<GRPICONDIRENTRY>(idEntries[i], stream);
        }
        #endregion
    }
    #endregion

    #region ICONDIRENTRY
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct ICONDIRENTRY
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        public byte bReserved;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public uint dwImageOffset;

        #region Constructors
        public ICONDIRENTRY(Stream stream)
        {
            this = new ICONDIRENTRY();
            Read(stream);
        }
        #endregion

        #region Methods
        public void Read(Stream stream)
        {
            var br = new BinaryReader(stream);
            byte[] array = new byte[sizeof(ICONDIRENTRY)];
            _ = br.Read(array, 0, sizeof(ICONDIRENTRY));
            fixed (byte* pData = array)
                this = *(ICONDIRENTRY*)pData;
        }

        public GRPICONDIRENTRY ToGrpIconEntry()
        {
            var grpIconEntry = new GRPICONDIRENTRY
            {
                bColorCount = bColorCount,
                bHeight = bHeight,
                bReserved = bReserved,
                bWidth = bWidth,
                dwBytesInRes = dwBytesInRes,
                wBitCount = wBitCount,
                wPlanes = wPlanes
            };
            return grpIconEntry;
        }
        #endregion
    }
    #endregion

    #region GRPICONDIRENTRY
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [Author("Franco, Gustavo")]
    internal unsafe struct GRPICONDIRENTRY
    {
        public byte bWidth;               // Width, in pixels, of the image
        public byte bHeight;              // Height, in pixels, of the image
        public byte bColorCount;          // Number of colors in image (0 if >=8bpp)
        public byte bReserved;            // Reserved
        public ushort wPlanes;              // Color Planes
        public ushort wBitCount;            // Bits per pixel
        public uint dwBytesInRes;         // how many bytes in this resource?
        public ushort nID;                  // the ID

        #region Constructors
        public GRPICONDIRENTRY(Stream stream)
        {
            this = new GRPICONDIRENTRY();
            Read(ref this, stream);
        }
        #endregion

        #region Methods

        public ICONDIRENTRY ToIconDirEntry()
        {
            var iconDirEntry = new ICONDIRENTRY
            {
                bColorCount = bColorCount,
                bHeight = bHeight,
                bReserved = bReserved,
                bWidth = bWidth,
                dwBytesInRes = dwBytesInRes,
                wBitCount = wBitCount,
                wPlanes = wPlanes
            };
            return iconDirEntry;
        }
        #endregion
    }
    #endregion
}
