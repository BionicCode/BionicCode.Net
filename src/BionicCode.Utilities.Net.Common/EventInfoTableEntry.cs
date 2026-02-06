namespace BionicCode.Utilities.Net;

#region Info
// //  
// BionicUtilities.Net.Standard
#endregion

internal class EventInfoTableEntry
{
    public EventInfoTableEntry(EventData eventData) => EventData = eventData;

    public EventData EventData { get; }
}
