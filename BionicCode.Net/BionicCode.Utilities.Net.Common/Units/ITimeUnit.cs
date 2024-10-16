namespace BionicCode.Utilities.Net
{
  using System;

  public interface ITimeUnit : IComparable<ITimeUnit>, IComparable, IConvertible, IEquatable<ITimeUnit>
  {
    TimeUnit Unit { get; }
    double Value { get; }

    ITimeUnit ToUnit(TimeUnit unit);
    Seconds ToSiUnit();
  }
}