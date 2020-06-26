using System;

namespace BionicCode.Utilities.Net.Standard.IO
{
  [Flags]
  public enum FileExtensions : long
  {
    NotDefined = 0,
    Any = 1,
    Log = 2,
    Txt = 4,
    Ini = 8,
    Csv = 16,
    Bat = 32,
    Bak = 64,
    Config = 128,
    Sys = 256,
    Reg = 512,
    Info = 1024,
    Inf = 2048,
    Help = 4096,
    Hlp = 8192,
    Dll = 16384,
    Bin = 32768,
    Old = 65536,
    Iii = 131072,
    Xml = 262144,
    Jpg = 524288,
    Jpeg = 1048576,
    Bmp = 2097152,
    Exe = 4194304,
    Com = 8388608,
    Cgc = 16777216,
    Cgt = 33554432,
    Cfg = 67108864,
    Png = 134217728,
    Zip = 268435456,
    Bz2 = 536870912, // BZip/ BZip2
    Gz = 1073741824, // GZip
    SevenZip = 2147483648, // 7Zip
    Xz = 4294967296, // XZ
    Lz = 8589934592, // LZip
    Rar = 17179869184, // Rar
    Tar = 34359738368, // Tarball
    Xaml = 68719476736,
    Cpp = 137438953472,
    C = 274877906944,
    Cs = 549755813888,
    Js = 1099511627776,
    Archive = 2199023255552,
    NonArchive = 4398046511104
  }
}