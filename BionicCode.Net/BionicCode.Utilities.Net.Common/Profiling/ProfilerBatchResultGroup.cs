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
    /// <value>The value can differ from the value of the indivudual items. 
    /// <br/>For example, this collection will have the value <see cref="ProfiledTargetType.Property"/> while the actual <see cref="ProfilerBatchResult.Context"/> has <see cref="ProfilerContext.TargetType"/> return <see cref="ProfiledTargetType.PropertyGet"/> and <see cref="ProfiledTargetType.PropertySet"/>.</value>
    internal ProfiledTargetType ProfiledTargetType { get; set;}

    /// <summary>
    /// The signature name of the profiled type member.
    /// </summary>
    internal string ProfiledTargetSignatureMemberName { get; set; }

    /// <summary>
    /// The signature name of the profiled type member.
    /// </summary>
    internal string ProfiledTargetMemberShortName { get; set; }

    internal TimeUnit CommonBaseUnit => this.Min(result => result.BaseUnit);

    public ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType)
    {
      this.ProfiledTargetType = profiledTargetType;
    }

    internal ProfilerBatchResultGroup()
    {
      this.ProfiledTargetType = ProfiledTargetType.None;
      this.ProfiledTargetSignatureMemberName = string.Empty;
    }

    public ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType, IEnumerable<ProfilerBatchResult> collection) : base(collection)
    {
      this.ProfiledTargetType = profiledTargetType;
    }

    public ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType, int capacity) : base(capacity)
    {
      this.ProfiledTargetType = profiledTargetType;
    }
  }
}