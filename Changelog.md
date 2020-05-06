IconLib
=======

Change log
==========

WinCopies.IconLib 0.75.0-rc (04/28/2020)
----------------------------------------

- Supports:
    - .Net Standard 2.0.
    - .Net framework 4.7.2
- Depends on:
    - WinCopies.Util
    - WinCopies.WindowsAPICodePack.Win32Native
    - Depends on SourceLink

- Depends on (.Net Standard only):
    - System.Drawing.Common
    - System.Encoding.Text.Pages

IconLib 0.73 (01/31/2008)
-------------------------

- Fixed a small problem with indexed 8bpp images.
- Properly processing when adding PNG24 images.
- Automatic Icon creation from a PNG or BMP32 for Vista, XP, W95 and Win31.
- Added a new namespace "ColorProcessing" which supports
  - Color Reduction
  - Dithering
  - Palette Optimization 
- Allow to save an IconImage as PNG or BMP32 with transparency.
- SingleIcon.Add() methods now returns a reference the IconImage just been created.
- Some code and method signatures changes but backward compatible.
- Demo application allows to export XOR, AND and Transparent Image, also now IconImages can be exported as PNG24 or BMP32. 

IconLib 0.72 (11/02/2006)
-------------------------

- Change default shift factor from 9 to 10 for ICL libraries. (now it supports 64MB Max ICL file size).
- Re-coded function to make vertical flip over Black&White images using pointers and memcopy (increased performance).
- Different Namespaces for Bitmap Encoders and Library Formats.
- Removed Static classes for Library Formats and replaced for a Interface from which the different formats implement.
- Included IconLib license type.

IconLib 0.71 (Initial Release)
------------------------------
