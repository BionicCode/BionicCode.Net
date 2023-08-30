namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  /// <summary>
  /// Eventhandler for the <see cref="IProgressReporterCommon.ProgressChanged"/> event.
  /// </summary>
  /// <param name="sender">the event source.</param>
  /// <param name="e">The event data.</param>
  public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);

}
