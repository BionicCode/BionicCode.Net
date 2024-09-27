namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  public class ProfilerBatchResultGroupCollection : List<ProfilerBatchResultGroup>
  {
    public ProfilerBatchResultGroupCollection()
    {
    }

    internal ProfilerBatchResultGroupCollection(TypeData profiledTypeData) => this.ProfiledTypeData = profiledTypeData;

    internal ProfilerBatchResultGroupCollection(IEnumerable<ProfilerBatchResultGroup> collection, TypeData profiledTypeData) : base(collection) => this.ProfiledTypeData = profiledTypeData;

    internal ProfilerBatchResultGroupCollection(int capacity, TypeData profiledTypeData) : base(capacity) => this.ProfiledTypeData = profiledTypeData;

    internal TypeData ProfiledTypeData { get; }
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