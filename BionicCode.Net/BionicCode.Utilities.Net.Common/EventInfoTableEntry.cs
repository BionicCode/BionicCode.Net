namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;

  internal class EventInfoTableEntry
  {
    public EventInfoTableEntry(EventData eventData, Delegate eventHandler)
    {
      this.EventData = eventData;
      this.EventHandler = eventHandler;
    }

    public EventData EventData { get; }
    public Delegate EventHandler { get; }
  }
}