namespace BionicCode.Utilities.Net
{
  using System.Runtime.CompilerServices;

  /// <summary>
  /// A set of helpers to simplify some reflection tasks.
  /// </summary>
  public static class ReflectionHelper
  {
    /// <summary>
    /// A convenient helper to return the member name of the current context without the need to decorate the member with the <see cref="CallerMemberNameAttribute"/> attribute.
    /// </summary>
    /// <param name="callerMemberName">The member name (optional and not required).</param>
    /// <returns>The member name of the current context.</returns>
    /// <remarks>Passing a <paramref name="callerMemberName"/> argument is not required as thie value is actually retrieved via refelection.</remarks>
    public static string GetCurrentMemberName([CallerMemberName] string callerMemberName = null) => callerMemberName;

    /// <summary>
    /// A convenient helper to return the file path of the source file that contains the current caller context without the need to decorate the member with the <see cref="CallerFilePathAttribute"/> attribute.
    /// </summary>
    /// <param name="callerFilePath">The file path (optional and not required).</param>
    /// <returns>The file path of the source file that contains the current context.</returns>
    /// <remarks>Passing a <paramref name="callerFilePath"/> argument is not required as thie value is actually retrieved via refelection.</remarks>
    public static string GetCurrentFilePath([CallerFilePath] string callerFilePath = null) => callerFilePath;

    /// <summary>
    /// A convenient helper to return the line number of the caller at which the current context was called without the need to decorate the member with the <see cref="CallerLineNumberAttribute"/> attribute.
    /// </summary>
    /// <param name="callerLineNumber">The line number (optional and not required).</param>
    /// <returns>The line number of the caller at which the current context was called.</returns>
    /// <remarks>Passing a <paramref name="callerLineNumber"/> argument is not required as thie value is actually retrieved via refelection.</remarks>
    public static int GetCurrentLineNumber([CallerLineNumber] int callerLineNumber = default) => callerLineNumber;
  }
}
