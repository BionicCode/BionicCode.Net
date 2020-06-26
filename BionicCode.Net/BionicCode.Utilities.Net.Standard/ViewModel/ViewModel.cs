using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BionicCode.Utilities.Net.Standard.ViewModel
{
  /// <summary>
  /// Base class recommended to use for view models across the application. Encapsulates implementations of <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>.
  /// </summary>
  public abstract class ViewModel : IViewModel
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    protected ViewModel()
    {
      this.Errors = new Dictionary<string, IEnumerable<string>>();
    }

    /// <summary>
    /// Generic property setter. Sets the value of any property of the extending view model by passing in a the corresponding property backing field. Automatically raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>If new value equals the old value the value of the property won't change and the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. Uses the <c>Equals</c> implementation to check for equality.</remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValue<TValue>(TValue value, ref TValue targetBackingField, [CallerMemberName] string propertyName = null)
    {
      if (value?.Equals(targetBackingField) ?? false)
      {
        return false;
      }

      TValue oldValue = targetBackingField;
      targetBackingField = value;
      OnPropertyChanged(propertyName, oldValue, value);
      return true;
    }

    /// <summary>
    /// Generic property setter. Silently sets the value of any property of the extending view model by passing in a the corresponding property backing field. Suppresses a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>If new value equals the old value the value of the property won't change and the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. Uses the <c>Equals</c> implementation to check for equality.</remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, ref TValue targetBackingField, [CallerMemberName] string propertyName = null)
    {
      this.IsSilent = true;
      bool isSuccessful = TrySetValue(value, ref targetBackingField, propertyName);
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
    /// <exception cref="ArgumentException">Thrown on validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.<br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios, <br/> it is recommended to set <paramref name="isThrowExceptionOnValidationErrorEnabled"/> to <c>true</c> and set the validation mode of the binding to <c>Binding.ValidatesOnExceptions</c>.<br/>If not doing so, the binding target will clear the new value and show the last valid value instead.</remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<string> ErrorMessages)> validationDelegate, ref TValue targetBackingField, bool isRejectInvalidValueEnabled = false, bool isThrowExceptionOnValidationErrorEnabled = false, [CallerMemberName] string propertyName = null)
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

      if (value?.Equals(targetBackingField) ?? false)
      {
        if (isValueValid && previousValidationHasFailed)
        {
          OnPropertyChanged(propertyName, value, value);
        }
        return false;
      }

      TValue oldValue = targetBackingField;
      targetBackingField = value;
      OnPropertyChanged(propertyName, oldValue, value);
      if (!isValueValid && isThrowExceptionOnValidationErrorEnabled)
      {
        throw new ArgumentException(string.Empty);
      }
      return true;
    }

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.</param>
    /// <param name="isRejectInvalidValueEnabled">When <c>true</c> the invalid value is not stored to the backing field.<br/> Use this to ensure that the view model in a valid state.</param>
    /// <param name="isThrowExceptionOnValidationErrorEnabled">Enable throwing an <exception cref="ArgumentException"></exception> if validation failed. Use this when <c>ValidatesOnExceptions</c> on a <c>Binding</c> is set to <c>true</c></param>
    /// <exception cref="ArgumentException">Thrown on validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.<br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios, <br/> it is recommended to set <paramref name="isThrowExceptionOnValidationErrorEnabled"/> to <c>true</c> and set the validation mode of the binding to <c>Binding.ValidatesOnExceptions</c>.<br/>If not doing so, the binding target will clear the new value and show the last valid value instead.</remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<string> ErrorMessages)> validationDelegate, ref TValue targetBackingField, bool isRejectInvalidValueEnabled = false, bool isThrowExceptionOnValidationErrorEnabled = false, [CallerMemberName] string propertyName = null)
    {
      this.IsSilent = true;
      bool isSuccessful = TrySetValue(
        value,
        validationDelegate,
        ref targetBackingField,
        isRejectInvalidValueEnabled,
        isThrowExceptionOnValidationErrorEnabled,
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
    protected virtual bool IsValueValid<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<string> ErrorMessages)> validationDelegate, [CallerMemberName] string propertyName = null)
    {
      if (propertyName == null)
      {
        return validationDelegate(value).IsValid;
      }

      this.Errors.Remove(propertyName);
      (bool IsValid, IEnumerable<string> ErrorMessages) validationResult = validationDelegate(value);
      if (!validationResult.IsValid)
      {
        this.Errors.Add(propertyName, validationResult.ErrorMessages);
      }

      OnErrorsChanged(propertyName);
      return validationResult.IsValid;
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
    /// <param name="oldValue">The value before the property change.</param>
    /// <param name="newValue">The value after the property change.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, object oldValue = null, object newValue = null)
    {
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
          : this.Errors.TryGetValue(propertyName, out IEnumerable<string> errors) 
            ? errors 
            : new List<string>();

      /// <inheritdoc />
      public bool HasErrors => this.Errors.Any();

      /// <inheritdoc />
      public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

      #endregion

      #region Implementation of IBaseViewModel

      /// <inheritdoc />
      public event PropertyValueChangedEventHandler<object> PropertyValueChanged;

      #endregion

      private Dictionary<string, IEnumerable<string>> Errors { get; set; }
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