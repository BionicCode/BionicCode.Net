namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// A delegate that can be passed to a <see cref="ViewModelCommon.TrySetValue{TValue}(TValue, Func{TValue, ValueTuple{bool, IEnumerable{object}}}, ref TValue, string)"/> and overload to provide a custom validation method.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to validate.</typeparam>
  /// <param name="propertyValue">The value to validate.</param>
  /// <returns>The validation result.</returns>
  public delegate PropertyValidationResult PropertyValidationDelegate<TValue>(TValue propertyValue);

  /// <summary>
  /// A delegate that can be passed to a <see cref="ViewModelCommon.TrySetValueAsync{TValue}(TValue, TValue, Func{TValue, Task{ValueTuple{bool, IEnumerable{object}}}}, SetBackingFieldDelegate{TValue}, ViewModelCommon.SetValueOptions, IEqualityComparer{TValue}, string)"/> and overload 
  /// <br/>to provide a custom asynchroous validation method.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to validate.</typeparam>
  /// <param name="propertyValue">The value to validate.</param>
  /// <returns>The validation result.</returns>
  public delegate Task<PropertyValidationResult> PropertyValidationDelegateAsync<TValue>(TValue propertyValue);
}
