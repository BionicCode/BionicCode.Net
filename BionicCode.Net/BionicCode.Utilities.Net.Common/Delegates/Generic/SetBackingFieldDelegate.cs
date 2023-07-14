namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// A delegate that defines a method to set the backing field of a property.
  /// </summary>
  /// <typeparam name="TValue">The type of the property.</typeparam>
  /// <param name="newValue">The value to be assigned to the backingfield of the property.</param>
  /// <remarks>This delegate is rewuired to set the backingfield of a property by an async method. Async methods can't define <c>ref</c> parameters.</remarks>
  public delegate void SetBackingFieldDelegate<TValue>(TValue newValue);
}
