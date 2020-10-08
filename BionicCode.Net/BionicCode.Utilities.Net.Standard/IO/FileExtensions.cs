using System;

namespace BionicCode.Utilities.Net.Standard.IO
{
  /// <summary>
  /// An enumeration of common file extensions.
  /// </summary>
  [Flags]
  public enum FileExtensions : long
  {
    /// <summary>
    /// Undefined value.
    /// </summary>
    NotDefined = 0,
    /// <summary>
    /// Represents wildcard '.*'
    /// </summary>
    Any = 1,
    /// <summary>
    /// .log
    /// </summary>
    Log = 2,
    /// <summary>
    /// .txt
    /// </summary>
    Txt = 4,
    /// <summary>
    /// .ini
    /// </summary>
    Ini = 8,
    /// <summary>
    /// .csv
    /// </summary>
    Csv = 16,
    /// <summary>
    /// .bat
    /// </summary>
    Bat = 32,
    /// <summary>
    /// .bak
    /// </summary>
    Bak = 64,
    /// <summary>
    /// .config
    /// </summary>
    Config = 128,
    /// <summary>
    /// .sys
    /// </summary>
    Sys = 256,
    /// <summary>
    /// .reg
    /// </summary>
    Reg = 512,
    /// <summary>
    /// .info
    /// </summary>
    Info = 1024,
    /// <summary>
    /// .inf
    /// </summary>
    Inf = 2048,
    /// <summary>
    /// .help
    /// </summary>
    Help = 4096,
    /// <summary>
    /// .hlp
    /// </summary>
    Hlp = 8192,
    /// <summary>
    /// .dll
    /// </summary>
    Dll = 16384,
    /// <summary>
    /// .bin
    /// </summary>
    Bin = 32768,
    /// <summary>
    /// .old
    /// </summary>
    Old = 65536,
    /// <summary>
    /// .iii
    /// </summary>
    Iii = 131072,
    /// <summary>
    /// .xml
    /// </summary>
    Xml = 262144,
    /// <summary>
    /// .jpg
    /// </summary>
    Jpg = 524288,
    /// <summary>
    /// .jpeg
    /// </summary>
    Jpeg = 1048576,
    /// <summary>
    /// .bmp
    /// </summary>
    Bmp = 2097152,
    /// <summary>
    /// .exe
    /// </summary>
    Exe = 4194304,
    /// <summary>
    /// .com
    /// </summary>
    Com = 8388608,
    /// <summary>
    /// .cgc
    /// </summary>
    Cgc = 16777216,
    /// <summary>
    /// .cgt
    /// </summary>
    Cgt = 33554432,
    /// <summary>
    /// .cfg
    /// </summary>
    Cfg = 67108864,
    /// <summary>
    /// .png
    /// </summary>
    Png = 134217728,
    /// <summary>
    /// .zip
    /// </summary>
    Zip = 268435456,
    /// <summary>
    /// .bz2
    /// </summary>
    Bz2 = 536870912, // BZip/ BZip2
    /// <summary>
    /// .gz
    /// </summary>
    Gz = 1073741824, // GZip
    /// <summary>
    /// .sevenzip
    /// </summary>
    SevenZip = 2147483648, // 7Zip
    /// <summary>
    /// .xz
    /// </summary>
    Xz = 4294967296, // XZ
    /// <summary>
    /// .lz
    /// </summary>
    Lz = 8589934592, // LZip
    /// <summary>
    /// .rar
    /// </summary>
    Rar = 17179869184, // Rar
    /// <summary>
    /// .tar
    /// </summary>
    Tar = 34359738368, // Tarball
    /// <summary>
    /// .xaml
    /// </summary>
    Xaml = 68719476736,
    /// <summary>
    /// .cpp
    /// </summary>
    Cpp = 137438953472,
    /// <summary>
    /// .c
    /// </summary>
    C = 274877906944,
    /// <summary>
    /// .cs
    /// </summary>
    Cs = 549755813888,
    /// <summary>
    /// .js
    /// </summary>
    Js = 1099511627776,
    /// <summary>
    /// .archive
    /// </summary>
    Archive = 2199023255552,
    /// <summary>
    /// .nonarchive
    /// </summary>
    NonArchive = 4398046511104
  }
}