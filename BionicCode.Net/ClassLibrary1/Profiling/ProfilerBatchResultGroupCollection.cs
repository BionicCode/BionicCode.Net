namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  public class ProfilerBatchResultGroupCollection : List<ProfilerBatchResultGroup>
  {
    public ProfilerBatchResultGroupCollection()
    {
    }

    public ProfilerBatchResultGroupCollection(Type profiledType) => this.ProfiledType = profiledType;

    public ProfilerBatchResultGroupCollection(IEnumerable<ProfilerBatchResultGroup> collection, Type profiledType) : base(collection) => this.ProfiledType = profiledType;

    public ProfilerBatchResultGroupCollection(int capacity, Type profiledType) : base(capacity) => this.ProfiledType = profiledType;

    public Type ProfiledType { get; }
  }

  public class ProfiledTypeResultCollection : List<ProfilerBatchResultGroupCollection>
  {
    public ProfiledTypeResultCollection()
    {
    }

    public ProfiledTypeResultCollection(IEnumerable<ProfilerBatchResultGroupCollection> collection) : base(collection)
    {
    }

    public ProfiledTypeResultCollection(int capacity) : base(capacity)
    {
    }
  }
}