namespace BionicCode.Utilities.Net
{
  using System;

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (net48)'
Before:
  using System.Windows;
  using System.Collections;
After:
  using System.Collections;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (netstandard2.0)'
Before:
  using System.Windows;
  using System.Collections;
After:
  using System.Collections;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (net6.0-windows)'
Before:
  using System.Windows;
  using System.Collections;
After:
  using System.Collections;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (net472)'
Before:
  using System.Windows;
  using System.Collections;
After:
  using System.Collections;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (netstandard2.1)'
Before:
  using System.Windows;
  using System.Collections;
After:
  using System.Collections;
*/
  using System.ComponentModel;
  using System.
/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (net48)'
Before:
  using JetBrains.Annotations;
After:
  using System.Windows;
  using JetBrains.Annotations;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (netstandard2.0)'
Before:
  using JetBrains.Annotations;
After:
  using System.Windows;
  using JetBrains.Annotations;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (net6.0-windows)'
Before:
  using JetBrains.Annotations;
After:
  using System.Windows;
  using JetBrains.Annotations;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (net472)'
Before:
  using JetBrains.Annotations;
After:
  using System.Windows;
  using JetBrains.Annotations;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Wpf (netstandard2.1)'
Before:
  using JetBrains.Annotations;
After:
  using System.Windows;
  using JetBrains.Annotations;
*/
Windows;

  /// <summary>
  /// Base class recommended to use for view models across the application. Encapsulates implementations of <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>.
  /// </summary>
  public abstract class ViewModel : ViewModelCommon, IViewModel
  {
#if !NETSTANDARD
    /// <summary>
    /// Creates a <see cref="IProgress{T}"/> instance that is associated with the caller's thread.
    /// The registered progress callback is the virtual <see cref="ViewModelCommon.OnProgress(ProgressData)"/> member.
    /// </summary>
    /// <remarks>The returned <see cref="IProgress{T}"/> instance is associated with the application's primary dispatcher thread. Progress is always reported to the UI thread that is associated with the <c>Dispatcher</c> returned by <c>Application.Current.Dispatcher</c>.</remarks>
    /// <returns>A <see cref="IProgress{ProgressData}"/> instance that always posts progress to the UI thread.</returns>
    public IProgress<ProgressData> CreateProgressReporterFromUiThread() => Application.Current.Dispatcher.Invoke(() => new Progress<ProgressData>(OnProgress));
#endif
  }
}