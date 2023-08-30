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

  /// <summary>
  /// Base class recommended to use for view models across the application. Encapsulates implementations of <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>.
  /// </summary>
  public abstract partial class ViewModelCommon : IViewModelCommon
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
    /// <remarks>Equality checking is enabled by default and must be explicitly disabled if needed by passing a <see cref="SetValueOptions"/> to the apropriate overload. To determine equality <see cref="object.ReferenceEquals(object, object)"/> is called. <br/>This means when the new property value <paramref name="value"/> equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>To change the equality definition use an overload that accepts an <see cref="IEqualityComparer{T}"/> (for example <see cref="TrySetValue{TValue}(TValue, ref TValue, IEqualityComparer{TValue}, string)"/>). 
    /// <para>To validate the <paramref name="value"/>, use the <see cref="TrySetValue{TValue}(TValue, Func{TValue, ValueTuple{bool, IEnumerable{object}}}, ref TValue, SetValueOptions, IEqualityComparer{TValue}, string)"/>  method.
    /// <br/>For asynchronous validation use the <see cref="TrySetValueAsync{TValue}(TValue, TValue, Func{TValue, Task{ValueTuple{bool, IEnumerable{object}}}}, SetBackingFieldDelegate{TValue}, SetValueOptions, IEqualityComparer{TValue}, string)"/> method.</para>
    /// </remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when equality checking is enabled and the new property equals the old property value.</returns>    
    protected virtual bool TrySetValue<TValue>(TValue value, ref TValue targetBackingField, [CallerMemberName] string propertyName = null)
      => TrySetValueInternal(value, null, ref targetBackingField, new SetValueOptions(false, false, true), null, false, propertyName);

    /// <summary>
    /// Generic property setter. Sets the value of any property of the extending view model by passing in a the corresponding property backing field. Automatically raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and must be explicitly disabled if needed by passing a <see cref="SetValueOptions"/> to the apropriate overload. To determine equality <see cref="object.ReferenceEquals(object, object)"/> is called. <br/>This means when the new property value <paramref name="value"/> equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>To change the equality definition use an overload that accepts an <see cref="IEqualityComparer{T}"/> (for example <see cref="TrySetValue{TValue}(TValue, ref TValue, IEqualityComparer{TValue}, string)"/>). 
    /// <para>To validate the <paramref name="value"/>, use the <see cref="TrySetValue{TValue}(TValue, Func{TValue, ValueTuple{bool, IEnumerable{object}}}, ref TValue, SetValueOptions, IEqualityComparer{TValue}, string)"/>  method.
    /// <br/>For asynchronous validation use the <see cref="TrySetValueAsync{TValue}(TValue, TValue, Func{TValue, Task{ValueTuple{bool, IEnumerable{object}}}}, SetBackingFieldDelegate{TValue}, SetValueOptions, IEqualityComparer{TValue}, string)"/> method.</para>
    /// </remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>    
    /// <param name="methodConfiguration">Use to configure the method's behavior.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when equality checking is enabled and the new property equals the old property value.</returns>    
    protected virtual bool TrySetValue<TValue>(TValue value, ref TValue targetBackingField, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = null)
      => TrySetValueInternal(value, null, ref targetBackingField, methodConfiguration, null, false, propertyName);

    /// <summary>
    /// Generic property setter. Sets the value of any property of the extending view model by passing in a the corresponding property backing field. Automatically raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and must be explicitly disabled if needed by passing a <see cref="SetValueOptions"/> to the apropriate overload. To determine equality <see cref="object.ReferenceEquals(object, object)"/> is called. <br/>This means when the new property value <paramref name="value"/> equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. If equality checking is enabled and no <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter or the value is <c>NULL</c>, 
    /// the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>To validate the <paramref name="value"/>, use the <see cref="TrySetValue{TValue}(TValue, Func{TValue, ValueTuple{bool, IEnumerable{object}}}, ref TValue, SetValueOptions, IEqualityComparer{TValue}, string)"/>  method.
    /// <br/>For asynchronous validation use the <see cref="TrySetValueAsync{TValue}(TValue, TValue, Func{TValue, Task{ValueTuple{bool, IEnumerable{object}}}}, SetBackingFieldDelegate{TValue}, SetValueOptions, IEqualityComparer{TValue}, string)"/> method.</para>
    /// </remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. 
    /// <br/>Passing in an <see cref="IEqualityComparer"/> automatically enables value rejection in case the equality test succeeds (new value and old value are equal). If <paramref name="equalityComparer"/> is <c>>NULL</c> <see cref="object.ReferenceEquals"/> will be used to test for equality.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when equality checking is enabled and the new property equals the old property value.</returns>    
    protected virtual bool TrySetValue<TValue>(TValue value, ref TValue targetBackingField, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueInternal(value, null, ref targetBackingField, new SetValueOptions(false, false, true), equalityComparer, false, propertyName);

    /// <summary>
    /// Generic property setter. Sets the value of any property of the extending view model by passing in a the corresponding property backing field. Automatically raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and must be explicitly disabled if needed by passing a <see cref="SetValueOptions"/> to the apropriate overload. To determine equality <see cref="object.ReferenceEquals(object, object)"/> is called. <br/>This means when the new property value <paramref name="value"/> equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. If equality checking is enabled and no <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter or the value is <c>NULL</c>, 
    /// the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>To validate the <paramref name="value"/>, use the <see cref="TrySetValue{TValue}(TValue, Func{TValue, ValueTuple{bool, IEnumerable{object}}}, ref TValue, SetValueOptions, IEqualityComparer{TValue}, string)"/>  method.
    /// <br/>For asynchronous validation use the <see cref="TrySetValueAsync{TValue}(TValue, TValue, Func{TValue, Task{ValueTuple{bool, IEnumerable{object}}}}, SetBackingFieldDelegate{TValue}, SetValueOptions, IEqualityComparer{TValue}, string)"/> method.</para>
    /// </remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. 
    /// <br/>Passing in an <see cref="IEqualityComparer"/> automatically enables value rejection in case the equality test succeeds (new value and old value are equal). If <paramref name="equalityComparer"/> is <c>>NULL</c> <see cref="object.ReferenceEquals"/> will be used to test for equality.</param>
    /// <param name="methodConfiguration">Use to configure the method's behavior.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when equality checking is enabled and the new property equals the old property value.</returns>    
    protected virtual bool TrySetValue<TValue>(TValue value, ref TValue targetBackingField, IEqualityComparer<TValue> equalityComparer, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = null)
      => TrySetValueInternal(value, null, ref targetBackingField, methodConfiguration, equalityComparer, false, propertyName);

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>If the value is <c>NULL</c> the validation delegate <paramref name="validationDelegate"/> is executed and the related method result is returned but a validation error is not recorded.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>NULL</c>, the <paramref name="value"/> is validated without generating an error. This is because validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, [CallerMemberName] string propertyName = default)
      => TrySetValueInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, new SetValueOptions(false, false, true), null, true, propertyName);

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>If the value is <c>NULL</c> the validation delegate <paramref name="validationDelegate"/> is executed and the related method result is returned but a validation error is not recorded.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>NULL</c>, the <paramref name="value"/> is validated without generating an error. This is because validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, [CallerMemberName] string propertyName = default)
      => TrySetValueInternal(value, validationDelegate, ref targetBackingField, new SetValueOptions(false, false, true), null, true, propertyName);

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>If the value is <c>NULL</c> the validation delegate <paramref name="validationDelegate"/> is executed and the related method result is returned but a validation error is not recorded.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c> and the feature is always enabled by default) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>NULL</c>, the <paramref name="value"/> is validated without generating an error. This is because validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = default)
      => TrySetValueInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, methodConfiguration, null, true, propertyName);

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>If the value is <c>NULL</c> the validation delegate <paramref name="validationDelegate"/> is executed and the related method result is returned but a validation error is not recorded.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c> and the feature is always enabled by default) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>NULL</c>, the <paramref name="value"/> is validated without generating an error. This is because validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = default)
      => TrySetValueInternal(value, validationDelegate, ref targetBackingField, methodConfiguration, null, true, propertyName);

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>If the value is <c>NULL</c> the validation delegate <paramref name="validationDelegate"/> is executed and the related method result is returned but a validation error is not recorded.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c> and the feature is always enabled by default) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>If equality checking is enabled and no valid <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter, the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>NULL</c>, the <paramref name="value"/> is validated without generating an error. This is because validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = default)
      => TrySetValueInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, methodConfiguration, equalityComparer, true, propertyName);

    /// <summary>
    ///  Sets the value of the referenced property and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>If the value is <c>NULL</c> the validation delegate <paramref name="validationDelegate"/> is executed and the related method result is returned but a validation error is not recorded.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c> and the feature is always enabled by default) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>If equality checking is enabled and no valid <see cref="IEqualityComparer{T}"/> was provided by setting the <paramref name="equalityComparer"/> parameter, the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>NULL</c>, the <paramref name="value"/> is validated without generating an error. This is because validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValue<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = default)
      => TrySetValueInternal(value, validationDelegate, ref targetBackingField, methodConfiguration, equalityComparer, true, propertyName);

    private bool TrySetValueInternal<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, bool isValidationEnabled, [CallerMemberName] string propertyName = null)
    {
      if (isValidationEnabled && validationDelegate is null)
      {
        throw new ArgumentNullException(nameof(validationDelegate));
      }

      if (methodConfiguration == null)
      {
        methodConfiguration = SetValueOptions.Default;
      }

      bool isValueValid = !isValidationEnabled || IsValueValid(value, validationDelegate, propertyName);

      if (!isValueValid && methodConfiguration.IsRejectInvalidValueEnabled)
      {
        return methodConfiguration.IsThrowExceptionOnValidationErrorEnabled
          ? throw new ArgumentException(ExceptionMessages.GetArgumentExceptionMessage_ValidationFailed())
          : false;
      }

      if (methodConfiguration.IsRejectEqualValuesEnabled
        && (equalityComparer?.Equals(value, targetBackingField) ?? ReferenceEquals(value, targetBackingField)))
      {
        return false;
      }

      TValue oldValue = targetBackingField;
      targetBackingField = value;
      OnPropertyChanged(oldValue, value, propertyName);
      return !isValueValid && methodConfiguration.IsThrowExceptionOnValidationErrorEnabled
        ? throw new ArgumentException(ExceptionMessages.GetArgumentExceptionMessage_ValidationFailed())
        : isValueValid;
    }

    /// <summary>
    ///  Support for asynchronous validation. Sets the value of the referenced property and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate">A <see cref="SetBackingFieldDelegate{TValue}"/>. This delegate is required to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/>To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> TrySetValueAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, [CallerMemberName] string propertyName = "")
      => await TrySetValueInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate?.Invoke(propertyValue))), backingFieldSetterDelegate, new SetValueOptions(false, false, true), null, propertyName);

    /// <summary>
    ///  Support for asynchronous validation. Sets the value of the referenced property and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate">A <see cref="SetBackingFieldDelegate{TValue}"/>. This delegate is required to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> TrySetValueAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = "")
      => await TrySetValueInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate?.Invoke(propertyValue))), backingFieldSetterDelegate, methodConfiguration, null, propertyName);

    /// <summary>
    ///  Support for asynchronous validation. Sets the value of the referenced property and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate">A <see cref="SetBackingFieldDelegate{TValue}"/>. This delegate is required to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/><br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> TrySetValueAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
      => await TrySetValueInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate?.Invoke(propertyValue))), backingFieldSetterDelegate, methodConfiguration, equalityComparer, propertyName);

    /// <summary>
    ///  Support for asynchronous validation. Sets the value of the referenced property and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate">A <see cref="SetBackingFieldDelegate{TValue}"/>. This delegate is required to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/>To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> TrySetValueAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, [CallerMemberName] string propertyName = "")
      => await TrySetValueInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, new SetValueOptions(false, false, true), null, propertyName);

    /// <summary>
    ///  Support for asynchronous validation. Sets the value of the referenced property and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate">A <see cref="SetBackingFieldDelegate{TValue}"/>. This delegate is required to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> TrySetValueAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = "")
      => await TrySetValueInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, methodConfiguration, null, propertyName);

    /// <summary>
    ///  Support for asynchronous validation. Sets the value of the referenced property and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate">A <see cref="SetBackingFieldDelegate{TValue}"/>. This delegate is required to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> TrySetValueAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
      => await TrySetValueInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, methodConfiguration, equalityComparer, propertyName);

    private async Task<bool> TrySetValueInternalAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration = default, IEqualityComparer<TValue> equalityComparer = null, [CallerMemberName] string propertyName = "")
    {
      if (asyncValidationDelegate is null)
      {
        throw new ArgumentNullException(nameof(asyncValidationDelegate));
      }

      if (backingFieldSetterDelegate == null)
      {
        throw new ArgumentNullException(nameof(backingFieldSetterDelegate));
      }

      if (methodConfiguration == null)
      {
        methodConfiguration = SetValueOptions.Default;
      }

      bool isValueValid = await IsValueValidAsync(newValue, asyncValidationDelegate, propertyName);

      if (!isValueValid && methodConfiguration.IsRejectInvalidValueEnabled)
      {
        return methodConfiguration.IsThrowExceptionOnValidationErrorEnabled
          ? throw new ArgumentException(ExceptionMessages.GetArgumentExceptionMessage_ValidationFailed())
          : false;
      }

      if (methodConfiguration.IsRejectEqualValuesEnabled && (equalityComparer?.Equals(newValue, oldValue) ?? ReferenceEquals(newValue, oldValue)))
      {
        return false;
      }

      backingFieldSetterDelegate.Invoke(newValue);
      OnPropertyChanged(oldValue, newValue, propertyName);
      return !isValueValid && methodConfiguration.IsThrowExceptionOnValidationErrorEnabled
        ? throw new ArgumentException(ExceptionMessages.GetArgumentExceptionMessage_ValidationFailed())
        : isValueValid;
    }

    /// <summary>
    /// Generic property setter. Silently sets the value of any property of the extending view model by passing in a the corresponding property backing field. Suppresses a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.
    /// </param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, null, ref targetBackingField, methodConfiguration, equalityComparer, false, propertyName);

    /// <summary>
    /// Generic property setter. Silently sets the value of any property of the extending view model by passing in a the corresponding property backing field. Suppresses a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.
    /// .</param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, ref TValue targetBackingField, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, null, ref targetBackingField, methodConfiguration, null, false, propertyName);

    /// <summary>
    /// Generic property setter. Silently sets the value of any property of the extending view model by passing in a the corresponding property backing field. Suppresses a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.
    /// </param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, ref TValue targetBackingField, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, null, ref targetBackingField, new SetValueOptions(false, false, true), equalityComparer, false, propertyName);

    /// <summary>
    /// Generic property setter. Silently sets the value of any property of the extending view model by passing in a the corresponding property backing field. Suppresses a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for this property.
    /// </summary>
    /// <remarks>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    /// <typeparam name="TValue">The generic type parameter of the new property value.</typeparam>
    /// <param name="value">The new property value.</param>
    /// <param name="targetBackingField">The backing field of the target property for the new value. Passed in by reference using <c>ref</c> keyword.</param>
    /// <param name="propertyName">The name of the property that changes. By default the property name is automatically set to the property that called this setter method.
    /// </param>
    /// <returns><c>true</c> when the property has changed or <c>false</c> when the property value didn't change (e.g. on equality of old and new value).</returns>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, ref TValue targetBackingField, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, null, ref targetBackingField, new SetValueOptions(false, false, true), null, false, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, methodConfiguration, equalityComparer, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, methodConfiguration, null, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, new SetValueOptions(false, false, true), equalityComparer, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, Func<TValue, (bool IsValid, IEnumerable<object> ErrorMessages)> validationDelegate, ref TValue targetBackingField, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate is null ? null : new PropertyValidationDelegate<TValue>(propertyValue => new PropertyValidationResult(validationDelegate.Invoke(propertyValue))), ref targetBackingField, new SetValueOptions(false, false, true), null, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate, ref targetBackingField, methodConfiguration, equalityComparer, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="methodConfiguration">A <see cref="SetValueOptions"/> object to configure the behavior of the method.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate, ref targetBackingField, methodConfiguration, null, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate, ref targetBackingField, new SetValueOptions(false, false, true), equalityComparer, true, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes a validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="value">The new value which is to be set to the property.</param>
    /// <param name="validationDelegate">The callback that is used to validate the new value.</param>
    /// <param name="targetBackingField">The reference to the backing field.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="validationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool TrySetValueSilent<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, [CallerMemberName] string propertyName = null)
      => TrySetValueSilentInternal(value, validationDelegate, ref targetBackingField, new SetValueOptions(false, false, true), null, true, propertyName);

    private  bool TrySetValueSilentInternal<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, ref TValue targetBackingField, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, bool isValidationEnabled, [CallerMemberName] string propertyName = null)
    {
      if (isValidationEnabled && validationDelegate == null)
      {
        throw new ArgumentNullException(nameof(validationDelegate));
      }

      this.IsSilent = true;
      bool isSuccessful = TrySetValueInternal(
        value,
        validationDelegate,
        ref targetBackingField,
        methodConfiguration,
        equalityComparer,
        isValidationEnabled,
        propertyName);
      this.IsSilent = false;
      return isSuccessful;
    }

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="methodConfiguration"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate.Invoke(propertyValue))), backingFieldSetterDelegate, methodConfiguration, equalityComparer, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="methodConfiguration"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate.Invoke(propertyValue))), backingFieldSetterDelegate, methodConfiguration, null, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate.Invoke(propertyValue))), backingFieldSetterDelegate, new SetValueOptions(false, false, true), equalityComparer, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, Func<TValue, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate is null ? null : new PropertyValidationDelegateAsync<TValue>(async propertyValue => new PropertyValidationResult(await asyncValidationDelegate.Invoke(propertyValue))), backingFieldSetterDelegate, new SetValueOptions(false, false, true), null, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="methodConfiguration"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, methodConfiguration, equalityComparer, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="methodConfiguration"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="methodConfiguration"/> is configured with <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> set to <c>true</c> and validation failed</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/> it is recommended to set <paramref name="methodConfiguration"/> property <see cref="SetValueOptions.IsThrowExceptionOnValidationErrorEnabled"/> to <c>true</c>.
    /// <br/>If not doing so, the binding target will clear the new value and show the last valid value instead.
    /// <br/>If equality checking is enabled by setting the <paramref name="methodConfiguration"/> parameter's property <see cref="SetValueOptions.IsRejectEqualValuesEnabled"/> to <c>true</c> (the parameter defaults to <c>trur</c>) and the new value equals the old value, then the <see cref="INotifyPropertyChanged.PropertyChanged"/> event won't be raised. 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, methodConfiguration, null, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <param name="equalityComparer">A <see cref="IEqualityComparer{T}"/> to check for value equality. If this optional parameter is not provided <see cref="object.ReferenceEquals"/> will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, new SetValueOptions(false, false, true), equalityComparer, propertyName);

    /// <summary>
    ///  Silently sets the value of the referenced property without raising <see cref="INotifyPropertyChanged.PropertyChanged"/> and executes an asynchronous validation delegate.
    /// </summary>
    /// <typeparam name="TValue">The generic value type parameter</typeparam>
    /// <param name="newValue">The new value which is to be set to the property.</param>
    /// <param name="oldValue">The current value.</param>
    /// <param name="asyncValidationDelegate">The asynchronous callback that is used to validate the new value.</param>
    /// <param name="backingFieldSetterDelegate"></param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncValidationDelegate"/> is <c>NULL</c>.</exception>
    /// <returns>Returns <c>true</c> if the new value doesn't equal the old value and the new value is valid. Returns <c>false</c> if the new value equals the old value or the validation has failed.</returns>
    /// <remarks>This property setter supports invalid value rejection, which means values are only assigned to the backing field if they are valid which is when the <paramref name="asyncValidationDelegate"/> return <c>true</c>.
    /// <br/> To support visual validation error feed back and proper behavior in <c>TwoWay</c> binding scenarios where the validation mode <c>Binding.ValidatesOnExceptions</c> of the <c>System.Windows.Binding</c> is set to <c>true</c>, 
    /// <br/>Equality checking is enabled by default and the method will check for reference equality using <see cref="object.ReferenceEquals(object, object)"/>.
    /// <para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="newValue"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected async virtual Task<bool> TrySetValueSilentAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, [CallerMemberName] string propertyName = "")
      => await TrySetValueSilentInternalAsync(newValue, oldValue, asyncValidationDelegate, backingFieldSetterDelegate, new SetValueOptions(false, false, true), null, propertyName);

    private async Task<bool> TrySetValueSilentInternalAsync<TValue>(TValue newValue, TValue oldValue, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, SetBackingFieldDelegate<TValue> backingFieldSetterDelegate, SetValueOptions methodConfiguration, IEqualityComparer<TValue> equalityComparer, [CallerMemberName] string propertyName = "")
    {
      if (asyncValidationDelegate == null)
      {
        throw new ArgumentNullException(nameof(asyncValidationDelegate));
      }

      if (backingFieldSetterDelegate == null)
      {
        throw new ArgumentNullException(nameof(backingFieldSetterDelegate));
      }

      this.IsSilent = true;
      bool isSuccessful = await TrySetValueInternalAsync(
        newValue,
        oldValue,
        asyncValidationDelegate,
        backingFieldSetterDelegate,
        methodConfiguration,
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
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <returns><c>true</c> when the value is valid, otherwise <c>false</c>.</returns>
    /// <remarks><para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual bool IsValueValid<TValue>(TValue value, PropertyValidationDelegate<TValue> validationDelegate, [CallerMemberName] string propertyName = null)
    {
      if (validationDelegate is null)
      {
        throw new ArgumentNullException(nameof(validationDelegate));
      }

      if (propertyName == null)
      {
        return validationDelegate.Invoke(value).IsValid;
      }

      _ = ClearErrors(propertyName);
      (bool IsValid, IEnumerable<object> ErrorMessages) = validationDelegate.Invoke(value);
      if (!IsValid)
      {
        AddErrorRange(propertyName, ErrorMessages);
      }

      bool allAttributesValid = IsPropertyAttributeValid(value, propertyName);

      return IsValid && allAttributesValid;
    }

    /// <summary>
    /// Can be used to check whether a value is valid.
    /// </summary>
    /// <typeparam name="TValue">Generic type parameter of the value to check.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="asyncValidationDelegate">The validation delegate <see cref="Func{TVAlue,TResult}"/>which is invoked on the value.</param>
    /// <param name="propertyName">The name of the property to set. Default name is the property that called this method.
    /// <br/>Use <c>null</c> to validate the value without generating an error.</param>
    /// <returns><c>true</c> when the value is valid, otherwise <c>false</c>.</returns>
    /// <remarks><para>When the <paramref name="propertyName"/> value is <c>null</c>, the <paramref name="value"/> is validated without generating an error. Validation errors are always related to a particular property.</para></remarks>
    protected virtual async Task<bool> IsValueValidAsync<TValue>(TValue value, PropertyValidationDelegateAsync<TValue> asyncValidationDelegate, [CallerMemberName] string propertyName = null)
    {
      if (asyncValidationDelegate is null)
      {
        throw new ArgumentNullException(nameof(asyncValidationDelegate));
      }

      if (propertyName == null)
      {
        return (await asyncValidationDelegate(value)).IsValid;
      }

      _ = ClearErrors(propertyName);
      (bool IsValid, IEnumerable<object> ErrorMessages) = await asyncValidationDelegate.Invoke(value);
      if (!IsValid)
      {
        AddErrorRange(propertyName, ErrorMessages);
      }

      bool allAttributesValid = IsPropertyAttributeValid(value, propertyName);

      return IsValid && allAttributesValid;
    }

    /// <summary>
    /// Validates the value of a particular property against decorating validation attributes.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns><c>true</c> if vcalidation passed or the property is not decorated with any validation attributes. Otherwise <c>false</c>.</returns>
    protected virtual bool IsPropertyAttributeValid<TValue>(
    TValue value,
    string propertyName)
    {
      if (propertyName == null)
      {
        return true;
      }

      _ = this.ValidatedAttributedProperties.Add(propertyName);

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

    /// <summary>
    /// Removes all error objects related to a property.
    /// </summary>
    /// <param name="propertyName">The property to clear error objects  for.</param>
    /// <returns><c>true</c> if an item was removed or <c>false</c> if no item was removed or the property was not found.</returns>
    protected virtual bool ClearErrors(string propertyName)
    {
      _ = this.ValidatedAttributedProperties.Remove(propertyName);
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
    /// <remarks>To create a <see cref="IProgress{T}"/> instance that is associated with the application's primary dispatcher thread of a Windows targeting application, for example to update proerties that bind to a <c>DispatcherObject</c>, call <c>CreateProgressReporterFromUiThread</c>.</remarks>
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
        _ = TrySetValue(value, ref this.isIndeterminate);
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
    public event ProgressChangedEventHandler ProgressChanged;

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
      => this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
  }
}