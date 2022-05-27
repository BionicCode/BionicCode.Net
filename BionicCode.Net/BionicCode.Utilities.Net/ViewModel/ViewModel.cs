namespace BionicCode.Utilities.Net
{
  using System;
  using System.Windows;
  using System.Collections;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using JetBrains.Annotations;
  using BionicCode.Utilities.Net.Common;

  /// <summary>
  /// Base class recommended to use for view models across the application. Encapsulates implementations of <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>.
  /// </summary>
  public abstract partial class ViewModel : ViewModelCommon, IViewModel
  {
  }
}