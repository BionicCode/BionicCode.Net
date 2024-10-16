namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// The enum values are ordered from least restrictive to most restrictive. Therefore, the hierarchy can be determined by comparing their integral numeric.
  /// <br/>For example, using the less than operator <c>&lt;</c> to compare two <see cref="AccessModifier"/> values like <c>Console.WriteLine(AccessModifier.Public &lt; AccessModifier.Private)</c> would print "True", expressing that <see langword="Public"/> is less restrictive than <see langword="private"/>.
  /// 
  /// ╔════════════════════════════════════════╦════════╦════════════════════╦══════════╦═══════════╦═══════════════════╦═════════╗
  /// ║ Caller's location                      ║ public ║ protected internal ║ internal ║ protected ║ private protected ║ private ║
  /// ╠════════════════════════════════════════╬════════╬════════════════════╬══════════╬═══════════╬═══════════════════╬═════════╣
  /// ║ Within the class                       ║   ✓    ║          ✓         ║     ✓    ║    ✓      ║         ✓         ║    ✓    ║
  /// ╠════════════════════════════════════════╬════════╬════════════════════╬══════════╬═══════════╬═══════════════════╬═════════╣
  /// ║ Derived class (same assembly)          ║   ✓    ║          ✓         ║     ✓    ║    ✓      ║         ✓         ║    ╳    ║
  /// ╠════════════════════════════════════════╬════════╬════════════════════╬══════════╬═══════════╬═══════════════════╬═════════╣
  /// ║ Non-derived class (same assembly)      ║   ✓    ║          ✓         ║     ✓    ║    ╳      ║         ╳         ║    ╳    ║
  /// ╠════════════════════════════════════════╬════════╬════════════════════╬══════════╬═══════════╬═══════════════════╬═════════╣
  /// ║ Derived class (different assembly)     ║   ✓    ║          ✓         ║     ╳    ║    ✓      ║         ╳         ║    ╳    ║
  /// ╠════════════════════════════════════════╬════════╬════════════════════╬══════════╬═══════════╬═══════════════════╬═════════╣
  /// ║ Non-derived class (different assembly) ║   ✓    ║          ╳         ║     ╳    ║    ╳      ║         ╳         ║    ╳    ║
  /// ╚════════════════════════════════════════╩════════╩════════════════════╩══════════╩═══════════╩═══════════════════╩═════════╝
  /// </remarks>

  public enum AccessModifier
  {
    Undefined = 0,
    /// <summary>
    /// Should be interpreted as <see langword="internal"/> per language default.
    /// </summary>
    Default,
    /// <summary>
    /// Represents the <see langword="public"/> modifier.
    /// </summary>
    Public,
    /// <summary>
    /// Represents the <see langword="protected internal"/> modifier.
    /// </summary>
    ProtectedInternal,
    /// <summary>
    /// Represents the <see langword="internal"/> modifier.
    /// </summary>
    Internal,
    /// <summary>
    /// Represents the <see langword="protected"/> modifier.
    /// </summary>
    Protected,
    /// <summary>
    /// Represents the <see langword="private protected"/> modifier.
    /// </summary>
    PrivateProtected,
    /// <summary>
    /// Represents the <see langword="private"/> modifier.
    /// </summary>
    Private,
  }

  public class AccessModifierComparer : Comparer<AccessModifier>
  {
    public override int Compare(AccessModifier x, AccessModifier y)
    {
      return x is AccessModifier.Internal && y is AccessModifier.Protected ? 0 : x.CompareTo(y);
    }
  }
}