namespace BionicCode.Utilities.Net
{
  using System;

  internal enum ProfiledTargetType
  {
    None = 0,
    Method = 1,
    Constructor = 2 | Method,
    Delegate = 4 | Method,
    Event = 6,
    Scope = 7,
    Property = 8,
    PropertyGet = 16 | Property,
    PropertySet = 32 | Property,
    Indexer = 64 | Property,
    IndexerGet = 128 | Indexer | PropertyGet,
    IndexerSet = 256 | Indexer | PropertySet,
  }
}