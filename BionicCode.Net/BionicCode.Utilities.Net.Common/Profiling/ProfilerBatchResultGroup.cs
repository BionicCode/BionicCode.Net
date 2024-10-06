namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// A list to hold a collection of <see cref="ProfilerBatchResult"/> items.
  /// </summary>
  public class ProfilerBatchResultGroup : List<ProfilerBatchResult>
  {
    /// <summary>
    /// Describes the profiled member that the <see cref="ProfilerBatchResult"/> items represent.
    /// </summary>
    /// <value>The value can differ from the value of the individual items. 
    /// <br/>For example, this collection will have the value <see cref="ProfiledTargetType.Property"/> while the actual <see cref="ProfilerBatchResult.Context"/> has <see cref="ProfilerContext.TargetType"/> return <see cref="ProfiledTargetType.PropertyGet"/> and <see cref="ProfiledTargetType.PropertySet"/>.</value>
    internal ProfiledTargetType TargetType { get; set; }

    /// <summary>
    /// The signature of the profiled type including the declaring type (in case if the target is a member).
    /// </summary>
    internal string TargetSignature { get; set; }

    /// <summary>
    /// The signature of the profiled type without the declaring type (in case if the target is a member).
    /// </summary>
    internal string TargetShortSignature { get; set; }

    /// <summary>
    /// The signature name of the profiled type without the declaring type (in case if the target is a member), attributes, generic constraints and inheritance list.
    /// </summary>
    internal string TargetShortCompactSignature { get; set; }

    /// <summary>
    /// The name of the profiled type including the declaring type (in case if the target is a member)..
    /// </summary>
    internal string TargetName { get; set; }

    /// <summary>
    /// The name of the profiled type without the declaring type (in case if the target is a member).
    /// </summary>
    internal string TargetShortName { get; set; }

    internal TimeUnit CommonBaseUnit => this.Min(result => result.BaseUnit);

    internal ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType) => this.TargetType = profiledTargetType;

    internal ProfilerBatchResultGroup()
    {
      this.TargetType = ProfiledTargetType.None;
      this.TargetSignature = string.Empty;
    }

    internal ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType, IEnumerable<ProfilerBatchResult> collection) : base(collection) => this.TargetType = profiledTargetType;

    internal ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType, int capacity) : base(capacity) => this.TargetType = profiledTargetType;
  }
}