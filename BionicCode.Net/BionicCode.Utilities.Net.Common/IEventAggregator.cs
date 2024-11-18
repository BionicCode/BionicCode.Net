namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading;

  /// <summary>
  /// Allows listening to events without introducing direct coupling between observer and observable. The observer can handle events without introducing a dependency to the event source.
  /// </summary>
  [Obsolete("Please use the WeakEventAggregator and the related interfaces IWeakEventAggregator, IWeakEventAggregatorListener and IWeakEventAggregatorPublisher instead (same namespace)! This type offers a cleaned-up API and several relevant performance and feature improvements and is completely weak event based to allow listeners to be garbage collected without unsubscribing from the observed events.")]
  public interface IEventAggregator : IEventAggregatorListener, IEventAggregatorPublisher
  {
  }
}