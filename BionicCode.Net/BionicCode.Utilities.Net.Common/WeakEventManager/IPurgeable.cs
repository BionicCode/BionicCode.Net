namespace BionicCode.Utilities.Net
{
  internal interface IPurgeable
  {
    bool TryPurge(bool isForced);
  }
}