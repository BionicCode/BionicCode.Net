namespace BionicCode.Utilities.Net.Common
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
  using BionicCode.Utilities.Net;
  using JetBrains.Annotations;
  using ProgressChangedEventArgs = ProgressChangedEventArgs;

  /// <summary>
  /// Base class recommended to use for view models across the application. Encapsulates implementations of <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>.
  /// </summary>
  public abstract class ViewModelCommon : IViewModelCommon
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    protected ViewModelCommon()
    {
      this.Errors = new Dictionary<string, IList<object>>();
      this.ValidatedAttributedProperties = new HashSet<string>();
    }

    /// <summary>
    /// Generic property setter. Sets the value of any property of the extending view model by passing in a the corresponding property backing field. Automatically raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>If equality checking is enabled by setting the <paramref name="isRejectEqualValuesEnabled"/> parameter to <c>true</c> and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. If equality checking is enabled and no <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter, the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.</remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="isRejectEqualValuesEnabled">Default: <c>true</c>. Default: <c>true</c>. Enable equality check before setting the value to avoid raising the <see cref="INotifyPropertyChanged.PropertyChanged"/> event on equality. Set to <c>false</c> to always raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when equality checking is enabled and the new property equals the old property value.</returns>    
    protected virtual bool TrySetValue<TValue>(TValue value, ref TValue targetBackingField, bool isRejectEqualValuesEnabled = true, IEqualityComparer<TValue> equalityComparer = null, [CallerMemberName] string propertyName = null)
    {
      if (isRejectEqualValuesEnabled && (equalityComparer?.Equals(value, targetBackingField) ?? ReferenceEquals(value, targetBackingField)))
      {
        return false;
      }

      TValue oldValue = targetBackingField;
      targetBackingField = value;
      OnPropertyChanged(oldValue, value, propertyName);
      return true;
    }

    /// <summary>
    /// Generic property setter. Silently sets the value of any property of the extending view model by passing in a the corresponding property backing field. Suppresses a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>If equality checking is enabled by setting the <paramref name="isRejectEqualValuesEnabled"/> parameter to <c>true</c> and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. If equality checking is enabled and no <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter, the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.</remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="isRejectEqualValuesEnabled">Default: <c>true</c>. Default: <c>true</c>. Enable equality check before setting the value to avoid raising the <see cref="INotifyPropertyChanged.PropertyChanged"/> event on equality. Set to <c>false</c> to always raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, ref TValue targetBackingField, bool isRejectEqualValuesEnabled = true, IEqualityComparer<TValue> equalityComparer = null, [CallerMemberName] string propertyName = null)
    {
      this.IsSilent = true;
      bool isSuccessful = TrySetValue(value, ref targetBackingField, isRejectEqualValuesEnabled, equalityComparer, propertyName);
      this.IsSilent = false;
      return isSuccessful;
    }

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.</param>
    /// <param name="isRejectInvalidValueEnabled">When <c>true</c> the invalid value is not stored to the backing field.<br/> Use this to ensure that the view model in a valid state.</param>
    /// <param name="isThrowExceptionOnValidationErrorEnabled">Enable throwing an <exception cref="ArgumentException"></exception> if validation failed. Use this when <c>ValidatesOnExceptions</c> on a <c>Binding</c> is set to <c>true</c></param>
    /// <param name="isRejectEqualValuesEnabled">Default: <c>true</c>. Enable equality check before setting the value to avoid raising the <see cref="INotifyPropertyChanged.PropertyChanged"/> event on equality. Set to <c>false</c> to always raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="isThrowExceptionOnValidationErrorEnabled"/> is set to <c>true</c> and validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.<br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios, <br/> it is recommended to set <paramref name="isThrowExceptionOnValidationErrorEnabled"/> to <c>true</c> and set the validation mode of the binding to <c>Binding.ValidatesOnExceptions</c>.<br/>If not doing so, the binding target will clear the new value and show the last valid value instead. <br/>If equality checking is enabled by setting the <paramref name="isRejectEqualValuesEnabled"/> parameter to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. If equality checking is enabled and no <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter, the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.</remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, bool isRejectInvalidValueEnabled = false, bool isThrowExceptionOnValidationErrorEnabled = false, bool isRejectEqualValuesEnabled = true, IEqualityComparer<TValue> equalityComparer = null, [CallerMemberName] string propertyName = null)
    {
      bool previousValidationHasFailed = propertyName != null && PropertyHasError(propertyName);
      bool isValueValid = IsValueValid(value, validationDelegate, propertyName);

      if (!isValueValid && isRejectInvalidValueEnabled)
      {
        if (isThrowExceptionOnValidationErrorEnabled)
        {
          throw new ArgumentException(string.Empty);
        }

        return false;
      }

      if (isRejectEqualValuesEnabled && (equalityComparer?.Equals(value, targetBackingField) ?? ReferenceEquals(value, targetBackingField)))
      {
        return false;
      }

      TValue oldValue = targetBackingField;
      targetBackingField = value;
      OnPropertyChanged(oldValue, value, propertyName);
      if (!isValueValid && isThrowExceptionOnValidationErrorEnabled)
      {
        throw new ArgumentException(string.Empty);
      }
      return isValueValid;
    }

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.</param>
    /// <param name="isRejectEqualValuesEnabled">Default: <c>true</c>. Default: <c>true</c>. Enable equality check before setting the value to avoid raising the <see cref="INotifyPropertyChanged.PropertyChanged"/> event on equality. Set to <c>false</c> to always raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <param name="isRejectInvalidValueEnabled">When <c>true</c> the invalid value is not stored to the backing field.<br/> Use this to ensure that the view model in a valid state.</param>
    /// <param name="isThrowExceptionOnValidationErrorEnabled">Enable throwing an <exception cref="ArgumentException"></exception> if validation failed. Use this when <c>ValidatesOnExceptions</c> on a <c>Binding</c> is set to <c>true</c></param>
    /// <exception cref="ArgumentException">Thrown on validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.<br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios, <br/> it is recommended to set <paramref name="isThrowExceptionOnValidationErrorEnabled"/> to <c>true</c> and set the validation mode of the binding to <c>Binding.ValidatesOnExceptions</c>.<br/>If not doing so, the binding target will clear the new value and show the last valid value instead. <br/>If equality checking is enabled by setting the <paramref name="isRejectEqualValuesEnabled"/> parameter to <c>true</c> and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. If equality checking is enabled and no <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter, the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.</remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, bool isRejectInvalidValueEnabled = false, bool isThrowExceptionOnValidationErrorEnabled = false, bool isRejectEqualValuesEnabled = true, IEqualityComparer<TValue> equalityComparer = null, [CallerMemberName] string propertyName = null)
    {
      this.IsSilent = true;
      bool isSuccessful = TrySetValue(
        value,
        validationDelegate,
        ref targetBackingField,
        isRejectInvalidValueEnabled,
        isThrowExceptionOnValidationErrorEnabled,
        isRejectEqualValuesEnabled,
        equalityComparer,
        propertyName);
      this.IsSilent = false;
      return isSuccessful;
    }

    /// <summary>
    /// Can be used to check whether a value is valid.
    /// </summary>
    /// <typeparam name="TValue">Generic type parameter of the value to check.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="validationDelegate">The validation delegate <see cref="Func{TVAlue,TResult}"/>which is invoked on the value.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.</param>
    /// <returns><c>true</c> when the value is valid, otherwise <c>false</c>.</returns>
    protected virtual bool IsValueValid<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, [CallerMemberName] string propertyName = null)
    {
      if (propertyName == null)
      {
        return validationDelegate(value).IsValid;
      }

      ClearErrors(propertyName);
      (bool IsValid, IEnumerable<object> ErrorMessages) = validationDelegate?.Invoke(value) ?? (true, Enumerable.Empty<object>());
      if (!IsValid)
      {
        AddErrorRange(propertyName, ErrorMessages);
      }

      bool allAttributesValid = IsPropertyAttributeValid(value, propertyName);

      return IsValid && allAttributesValid;
    }

    protected virtual bool IsPropertyAttributeValid<TValue>(
    TValue value,
    string propertyName)
    {
      this.ValidatedAttributedProperties.Add(propertyName);

      // The result flag
      bool isValueValid = true;

      // Check if property is decorated with validation attributes
      // using reflection
      IEnumerable<Attribute> validationAttributes = GetType()
        .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        ?.GetCustomAttributes(typeof(ValidationAttribute)) ?? new List<Attribute>();

      // Validate using attributes if present
      if (validationAttributes.Any())
      {
        var validationContext = new ValidationContext(this, null, null) { MemberName = propertyName };
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateProperty(value, validationContext, validationResults))
        {
          isValueValid = false;
          AddErrorRange(propertyName, validationResults.Select(result => result.ErrorMessage));
        }
      }

      return isValueValid;
    }

      private void AddErrorRange(string propertyName, IEnumerable<object> newErrors, bool isWarning = false)
      {
        if (!newErrors.Any())
        {
          return;
        }

        if (!this.Errors.TryGetValue(propertyName, out IList<object> propertyErrors))
        {
          propertyErrors = new List<object>();
          this.Errors.Add(propertyName, propertyErrors);
        }

        if (isWarning)
        {
          foreach (object error in newErrors)
          {
            propertyErrors.Add(error);
          }
        }
        else
        {
          foreach (object error in newErrors)
          {
            propertyErrors.Insert(0, error);
          }
        }

        OnErrorsChanged(propertyName);
      }

    private void AddError(string propertyName, object newError, bool isWarning = false)
    {
      if (!this.Errors.TryGetValue(propertyName, out IList<object> propertyErrors))
      {
        propertyErrors = new List<object>();
        this.Errors.Add(propertyName, propertyErrors);
      }

      if (isWarning)
      {
        propertyErrors.Add(item: newError);
      }
      else
      {
        propertyErrors.Insert(0, newError);
      }

      OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Removes all error objects related to a property.
    /// </summary>
    /// <param name="propertyName">The property to clear error objects  for.</param>
    /// <returns><c>true</c> if an item was removed or <c>false</c> if no item was removed or the property was not found.</returns>
    protected virtual bool ClearErrors(string propertyName)
    {
      this.ValidatedAttributedProperties.Remove(propertyName);
      bool hasRemovedItem = this.Errors.Remove(propertyName);      
      if (hasRemovedItem)
      {
        OnErrorsChanged(propertyName);
      }

      return hasRemovedItem;
    }

    /// <inheritdoc />
    public virtual bool PropertyHasError([CallerMemberName] string propertyName = null) =>
      this.Errors.ContainsKey(propertyName ?? throw new ArgumentNullException(nameof(propertyName)));

    /// <inheritdoc />
    public IEnumerable<string> GetPropertyErrors(string propertyName = null) => GetErrors(propertyName).Cast<string>();

    /// <summary>
    /// Event fired whenever a child property changes its value.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Method called to fire a <see cref="PropertyChanged"/> event.
    /// Also raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event to support binding.
    /// </summary>
    /// <param name="propertyName"> The property name. </param>
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => OnPropertyChanged(default, default, propertyName);

    /// <summary>
    /// Method called to fire a <see cref="PropertyChanged"/> event.
    /// Also raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event to support binding.
    /// </summary>
    /// <param name="propertyName"> The property name. </param>
    /// <param name="oldValue">The value before the property change.</param>
    /// <param name="newValue">The value after the property change.</param>
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(object oldValue, object newValue, [CallerMemberName] string propertyName = null)
    {

      if (!this.ValidatedAttributedProperties.Contains(propertyName))
      {
        _ = IsPropertyAttributeValid(newValue, propertyName);
      }

      if (this.IsSilent)
      {
        return;
      }

      // Invoke INotifyPropertyChanged.PropertyChanged
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

      this.PropertyValueChanged?.Invoke(this, new PropertyValueChangedArgs<object>(propertyName, oldValue, newValue));
    }

    #region Implementation of INotifyDataErrorInfo

    /// <summary>
    /// Gets all error messages of the specified property. If the <paramref name="propertyName"/> is <c>null</c> all error messages will be returned.
    /// </summary>
    /// <param name="propertyName">The of the property of which the error messages should be returned.</param>
    /// <returns>An <see cref="IEnumerable"/> containing all error messages of the specified property.</returns>
    /// <remarks>If the <paramref name="propertyName"/> is <c>null</c> all current error messages will be returned.</remarks>
    public IEnumerable GetErrors(string propertyName = null) =>
        string.IsNullOrWhiteSpace(propertyName)
          ? this.Errors.SelectMany(entry => entry.Value)
          : this.Errors.TryGetValue(propertyName, out IList<object> errors)
            ? (IEnumerable)errors
            : new List<object>();

    /// <inheritdoc />
    public bool HasErrors => this.Errors.Any();

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    #endregion

    #region Implementation of IProgressReporter

    /// <summary>
    /// When overridden, handles the <see cref="IProgress{ProgressData}.Report(ProgressData)"/> that is invoked by the <see cref="IProgress{ProgressData}"/> instance returned from <see cref="CreateProgressReporterFromCurrentThread"/>. Can be used as progress delegate for any <see cref="IProgress{ProgressData}"/>.
    /// </summary>
    /// <param name="progress">The progress argument.</param>
    /// <remarks>The default implementation provides the followwing logic: a value of <see cref="double.NegativeInfinity"/> or <see cref="ViewModelCommon.DisableIndterminateMode"/> will automatically set the <see cref="ViewModelCommon.IsIndeterminate"/> property to <c>false</c>. A value of <see cref="double.PositiveInfinity"/> or <see cref="ViewModelCommon.EnableIndterminateMode"/> will automatically set the <see cref="ViewModelCommon.IsIndeterminate"/> property to <c>true</c>.
    /// </remarks>
    protected virtual void OnProgress(ProgressData progress)
    {
      this.ProgressText = progress.Message;
      this.IsIndeterminate = progress.Progress == ViewModelCommon.EnableIndterminateMode
        ? true
        : progress.Progress == ViewModelCommon.DisableIndterminateMode
          ? false
          : this.IsIndeterminate;
      this.ProgressValue = progress.Progress;
    }

    /// <summary>
    /// Constant representing value of <see cref="double.PositiveInfinity"/>. When assigned to <see cref="ProgressData.Progress"/> and when calling the default implementation of <see cref="OnProgress(ProgressData)"/> the value will automatically set <see cref="ViewModelCommon.IsIndeterminate"/> to <c>true</c>.
    /// </summary>
    public const double EnableIndterminateMode = double.PositiveInfinity;

    /// <summary>
    /// Constant representing value of <see cref="double.NegativeInfinity"/>. When assigned to <see cref="ProgressData.Progress"/> and when calling the default implementation of <see cref="OnProgress(ProgressData)"/> the value will automatically set <see cref="ViewModelCommon.IsIndeterminate"/> to <c>false</c>.
    /// </summary>
    public const double DisableIndterminateMode = double.NegativeInfinity;

    /// <summary>
    /// Creates a <see cref="IProgress{T}"/> instance that is associated with the caller's thread.
    /// The registered progress callback is the virtual <c><see cref="ViewModelCommon"/>.OnProgress(ProgressData)</c> member.
    /// </summary>
    /// <remarks>To create a <see cref="IProgress{T}"/> instance that is associated with the application's primary dispatcher thread, for example to update proerties that bind to a <c>DispatcherObject</c>, call <see cref="CreateProgressReporterFromUiThread"/>.</remarks>
    /// <returns>A <see cref="IProgress{ProgressData}"/> instance that posts progress to the thread <see cref="CreateProgressReporterFromCurrentThread"/> was called from.</returns>
    public IProgress<ProgressData> CreateProgressReporterFromCurrentThread() => new Progress<ProgressData>(OnProgress);

    /// <summary>
    /// 
    /// Raises the <see cref="IProgressReporterCommon.ProgressChanged"/> event.
    /// </summary>
    /// <param name="oldValue">The old progress value.</param>
    /// <param name="newValue">The new progress value.</param>
    protected virtual void OnProgressChanged(double oldValue, double newValue) => OnProgressChanged(oldValue, newValue, string.Empty);

    /// <summary>
    /// Raises the <see cref="IProgressReporterCommon.ProgressChanged"/> event.
    /// </summary>
    /// <param name="oldValue">The old progress value.</param>
    /// <param name="newValue">The new progress value.</param>
    /// <param name="progressText">The progress message.</param>
    protected virtual void OnProgressChanged(double oldValue, double newValue, string progressText)
    {
      this.IsReportingProgress = this.IsIndeterminate || (this.ProgressValue > 0 && this.ProgressValue < 100);
      this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(newValue, oldValue, progressText));
    }

    private bool isReportingProgress;
    /// <inheritdoc/>
    public bool IsReportingProgress
    {
      get => this.isReportingProgress;
      set => TrySetValue(value, ref this.isReportingProgress);
    }

    private bool isIndeterminate;
    /// <inheritdoc/>
    public bool IsIndeterminate
    {
      get => this.isIndeterminate;
      set
      {
        double oldValue = this.ProgressValue;
        TrySetValue(value, ref this.isIndeterminate);
        OnProgressChanged(oldValue, value ? -1 : this.ProgressValue);
      }
    }

    private string progressText;
    /// <inheritdoc/>
    public string ProgressText
    {
      get => this.progressText;
      set
      {
        if (TrySetValue(value, ref this.progressText))
        {
          OnProgressChanged(this.ProgressValue, this.ProgressValue, this.ProgressText);
        }
      }
    }

    private double progressValue;
    /// <inheritdoc/>
    public double ProgressValue
    {
      get => this.progressValue;
      set
      {
        double oldValue = this.ProgressValue;
        if (TrySetValue(value, ref this.progressValue))
        {
          OnProgressChanged(oldValue, this.ProgressValue, this.ProgressText);
        }
      }
    }

    /// <inheritdoc/>
    public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

    #endregion

    /// <inheritdoc />
    public event PropertyValueChangedEventHandler<object> PropertyValueChanged;

    private Dictionary<string, IList<object>> Errors { get; }
    private HashSet<string> ValidatedAttributedProperties { get; }
    private bool IsSilent { get; set; }

    /// <summary>
    /// Raised when the validation state of the view model has changed (e.g. error added or removed).
    /// </summary>
    /// <param name="propertyName"></param>
    protected virtual void OnErrorsChanged(string propertyName)
    {
      this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
  }
}