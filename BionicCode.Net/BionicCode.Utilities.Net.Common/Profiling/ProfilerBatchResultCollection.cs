namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;

  /// <summary>
  /// A list to hold a collection of <see cref="ProfilerBatchResult"/> items.
  /// </summary>
  public class ProfilerBatchResultCollection : List<ProfilerBatchResult>
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
    internal string ProfiledTargetMemberName { get; set; }

    public ProfilerBatchResultCollection(ProfiledTargetType profiledTargetType)
    {
      this.ProfiledTargetType = profiledTargetType;
    }

    internal ProfilerBatchResultCollection()
    {
      this.ProfiledTargetType = ProfiledTargetType.None;
      this.ProfiledTargetMemberName = string.Empty;
    }

    public ProfilerBatchResultCollection(ProfiledTargetType profiledTargetType, IEnumerable<ProfilerBatchResult> collection) : base(collection)
    {
      this.ProfiledTargetType = profiledTargetType;
    }

    public ProfilerBatchResultCollection(ProfiledTargetType profiledTargetType, int capacity) : base(capacity)
    {
      this.ProfiledTargetType = profiledTargetType;
    }
  }
}