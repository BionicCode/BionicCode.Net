namespace BionicCode.Utilities.Net
{
    #region Info
    // //  
    // BionicUtilities.Net.Standard
    #endregion

    internal class EventInfoTableEntry
    {
        public EventInfoTableEntry(EventData eventData) => this.EventData = eventData;

        public EventData EventData { get; }
    }
}
