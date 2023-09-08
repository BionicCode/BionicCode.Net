namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;

  internal static class TimeValueConverter
  {
    internal static double ToMinutes(Seconds seconds) => seconds.Value / 60;
    internal static double ToMinutes(Milliseconds milliseconds) => milliseconds.Value * 1E-3 / 60;
    internal static double ToMinutes(Microseconds microseconds) => microseconds.Value * 1E-6 / 60;
    internal static double ToMinutes(Nanoseconds nanoseconds) => nanoseconds.Value * 1E-9 / 60;

    internal static double ToSeconds(Minutes minutes) => minutes.Value * 60;
    internal static double ToSeconds(Milliseconds milliseconds) => milliseconds.Value * 1E-3;
    internal static double ToSeconds(Microseconds microseconds) => microseconds.Value * 1E-6;
    internal static double ToSeconds(Nanoseconds nanoseconds) => nanoseconds.Value * 1E-9;

    internal static double ToMilliseconds(Minutes minutes) => minutes.Value * 60 * 1E3;
    internal static double ToMilliseconds(Seconds seconds) => seconds.Value * 1E3;
    internal static double ToMilliseconds(Microseconds microseconds) => microseconds.Value * 1E-3;
    internal static double ToMilliseconds(Nanoseconds nanoseconds) => nanoseconds.Value * 1E-6;

    internal static double ToMicroseconds(Minutes minutes) => minutes.Value * 60 * 1E6;
    internal static double ToMicroseconds(Seconds seconds) => seconds.Value * 1E6;
    internal static double ToMicroseconds(Milliseconds milliseconds) => milliseconds.Value * 1E3;
    internal static double ToMicroseconds(Nanoseconds nanoseconds) => nanoseconds.Value * 1E-3;

    internal static double ToNanoseconds(Minutes minutes) => minutes.Value * 60 * 1E9;
    internal static double ToNanoseconds(Seconds seconds) => seconds.Value * 1E9;
    internal static double ToNanoseconds(Milliseconds milliseconds) => milliseconds.Value * 1E6;
    internal static double ToNanoseconds(Microseconds microseconds) => microseconds.Value * 1E3;

    internal static double ConvertTo(TimeUnit timeUnit, Minutes minutes)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return minutes;
        case TimeUnit.Seconds:
          return ToSeconds(minutes);
        case TimeUnit.Milliseconds:
          return ToMilliseconds(minutes);
        case TimeUnit.Microseconds:
          return ToMicroseconds(minutes);
        case TimeUnit.Nanoseconds:
          return ToNanoseconds(minutes);
        case TimeUnit.Minutes:
          return minutes;
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Seconds seconds)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return seconds;
        case TimeUnit.Seconds:
          return seconds;
        case TimeUnit.Milliseconds:
          return ToMilliseconds(seconds);
        case TimeUnit.Microseconds:
          return ToMicroseconds(seconds);
        case TimeUnit.Nanoseconds:
          return ToNanoseconds(seconds);
        case TimeUnit.Minutes:
          return ToMinutes(seconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Milliseconds milliseconds)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return milliseconds;
        case TimeUnit.Seconds:
          return ToSeconds(milliseconds);
        case TimeUnit.Milliseconds:
          return milliseconds;
        case TimeUnit.Microseconds:
          return ToMicroseconds(milliseconds);
        case TimeUnit.Nanoseconds:
          return ToNanoseconds(milliseconds);
        case TimeUnit.Minutes:
          return ToMinutes(milliseconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Microseconds microseconds)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return microseconds;
        case TimeUnit.Seconds:
          return ToSeconds(microseconds);
        case TimeUnit.Milliseconds:
          return ToMilliseconds(microseconds);
        case TimeUnit.Microseconds:
          return microseconds;
        case TimeUnit.Nanoseconds:
          return ToNanoseconds(microseconds);
        case TimeUnit.Minutes:
          return ToMinutes(microseconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Nanoseconds nanoseconds)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return nanoseconds;
        case TimeUnit.Seconds:
          return ToSeconds(nanoseconds);
        case TimeUnit.Milliseconds:
          return ToMilliseconds(nanoseconds);
        case TimeUnit.Microseconds:
          return ToMicroseconds(nanoseconds);
        case TimeUnit.Nanoseconds:
          return nanoseconds;
        case TimeUnit.Minutes:
          return ToMinutes(nanoseconds);
        default:
          throw new NotSupportedException();
      }
    }
  }
}