namespace BionicCode.Utilities.Net
{
  using System;

  /// <summary>
  /// Used to decorate a <see langword="static"/> member that returns an instance for the currently profiled type.
  /// </summary>
  /// <remarks>This attribute can be used in case the type has no default constructor, or profiling requires a specially initialized instance.
  /// <br/>The default constructor must be an instance constructor and can be defined with any access modifier (e.g. <see langword="public"/> or <see langword="private"/>).
  /// <br/>The factory member must be <see langword="static"/> and can be a method, property or field and must return an instance of the type that declares the profiled memeber.
  /// <para></para>
  /// <br/>The <see cref="Profiler"/> creates an instance of the profiled type using one of the following providers ranked by prcedence:
  /// <br/>
  /// <list type="number">
  /// <item>It first checks if the type has a <see langword="static"/> member that is decorated with the <see cref="ProfilerFactoryAttribute"/> and returns the same type as the type that declares the profiled member. 
  /// <br/>The first memeber that satisfies the constraints is used to obtain a pre-configured instance in order to invoked the profiled member.</item>
  /// <item>If no such attribute was found, it tries to find a parameterless instance constructor. This constrcutor can be <see langword="public"/>, <see langword="private"/>, or of any other access modifier.</item>
  /// <item>If no parameterles constructor was found and the profiled member is not a <see langword="static"/> member a <see cref="MissingMemberException"/> exception is thrown.</item>
  /// </list>
  /// </remarks>
  [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
  public sealed class ProfilerFactoryAttribute : Attribute
  {
    /// <summary>
    /// Constructor without any required parameters.
    /// </summary>
    public ProfilerFactoryAttribute() => this.ArgumentList = Array.Empty<object>();

    /// <summary>
    /// Constructor without any required parameters.
    /// </summary>
    public ProfilerFactoryAttribute(params object[] argumentList)
    {
      this.ArgumentList = argumentList;
    }

    public object[] ArgumentList { get; }
  }
}