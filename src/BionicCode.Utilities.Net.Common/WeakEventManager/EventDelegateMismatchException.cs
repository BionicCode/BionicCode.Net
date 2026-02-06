namespace BionicCode.Utilities.Net
{
    using System;

    [Serializable]
    public class EventDelegateMismatchException : Exception
    {
        public EventDelegateMismatchException()
        {
        }

        public EventDelegateMismatchException(string message) : base(message)
        {
        }

        public EventDelegateMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
