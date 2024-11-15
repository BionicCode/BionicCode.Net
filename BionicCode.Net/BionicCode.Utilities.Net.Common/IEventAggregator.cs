namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading;

  /// <summary>
  /// Allows listening to events without introducing direct coupling between observer and observable. The observer can handle events without introducing a dependency to the event source.
  /// </summary>
  public interface IEventAggregator : IEventAggregatorPublisher, IEventAggregatorListener
  {
  }
}