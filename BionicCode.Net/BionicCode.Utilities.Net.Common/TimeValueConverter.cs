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

    internal static double ConvertTo(TimeUnit timeUnit, Minutes minutes, bool isMaxTimerResolutionRoundingEnabled)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
        case TimeUnit.Minutes:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Minutes, minutes) : minutes.Value;
        case TimeUnit.Seconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Seconds, ToSeconds(minutes)) : ToSeconds(minutes);
        case TimeUnit.Milliseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Milliseconds, ToMilliseconds(minutes)) : ToMilliseconds(minutes);
        case TimeUnit.Microseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Microseconds, ToMicroseconds(minutes)) : ToMicroseconds(minutes);
        case TimeUnit.Nanoseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Nanoseconds, ToNanoseconds(minutes)) : minutes.Value;
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Seconds seconds, bool isMaxTimerResolutionRoundingEnabled)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
        case TimeUnit.Seconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Seconds, seconds) : seconds.Value;
        case TimeUnit.Milliseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Milliseconds, ToMilliseconds(seconds)) : ToMilliseconds(seconds);
        case TimeUnit.Microseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Microseconds, ToMicroseconds(seconds)) : ToMicroseconds(seconds);
        case TimeUnit.Nanoseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Nanoseconds, ToNanoseconds(seconds)) : ToNanoseconds(seconds);
        case TimeUnit.Minutes:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Minutes, ToMinutes(seconds)) : ToMinutes(seconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Milliseconds milliseconds, bool isMaxTimerResolutionRoundingEnabled)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
        case TimeUnit.Milliseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Milliseconds, milliseconds) : milliseconds.Value;
        case TimeUnit.Seconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Seconds, ToSeconds(milliseconds)) : ToSeconds(milliseconds);
        case TimeUnit.Microseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Microseconds, ToMicroseconds(milliseconds)) : ToMicroseconds(milliseconds);
        case TimeUnit.Nanoseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Nanoseconds, ToNanoseconds(milliseconds)) : ToNanoseconds(milliseconds);
        case TimeUnit.Minutes:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Minutes, ToMinutes(milliseconds)) : ToMinutes(milliseconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Microseconds microseconds, bool isMaxTimerResolutionRoundingEnabled)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
        case TimeUnit.Microseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Microseconds, microseconds) : microseconds.Value;
        case TimeUnit.Seconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Seconds, ToSeconds(microseconds)) : ToSeconds(microseconds);
        case TimeUnit.Milliseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Milliseconds, ToMilliseconds(microseconds)) : ToMilliseconds(microseconds);
        case TimeUnit.Nanoseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Nanoseconds, ToNanoseconds(microseconds)) : ToNanoseconds(microseconds);
        case TimeUnit.Minutes:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Minutes, ToMinutes(microseconds)) : ToMinutes(microseconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double ConvertTo(TimeUnit timeUnit, Nanoseconds nanoseconds, bool isMaxTimerResolutionRoundingEnabled)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
        case TimeUnit.Nanoseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Nanoseconds, nanoseconds) : nanoseconds.Value;
        case TimeUnit.Seconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Seconds, ToSeconds(nanoseconds)) : ToSeconds(nanoseconds);
        case TimeUnit.Milliseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Milliseconds, ToMilliseconds(nanoseconds)) : ToMilliseconds(nanoseconds);
        case TimeUnit.Microseconds:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Microseconds, ToMicroseconds(nanoseconds)) : ToMicroseconds(nanoseconds);
        case TimeUnit.Minutes:
          return isMaxTimerResolutionRoundingEnabled ? TrimDecimalsTo(TimeUnit.Minutes, ToMinutes(nanoseconds)) : ToMinutes(nanoseconds);
        default:
          throw new NotSupportedException();
      }
    }

    internal static double TrimDecimalsTo(TimeUnit timeUnit, double timeValue)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return timeValue == -0 ? 0 : timeValue;
        case TimeUnit.Nanoseconds:
          double nanoseconds = System.Math.Round(timeValue, 0);
          return nanoseconds == -0 ? 0 : nanoseconds; ;
        case TimeUnit.Seconds:
          double seconds =  System.Math.Round(timeValue, 7);
          return seconds == -0 ? 0 : seconds;
        case TimeUnit.Milliseconds:
          double millieseconds = System.Math.Round(timeValue, 4);
          return millieseconds == -0 ? 0 : millieseconds;
        case TimeUnit.Microseconds:
          double microseconds = System.Math.Round(timeValue, 1);
          return microseconds == -0 ? 0 : microseconds;
        case TimeUnit.Minutes:
          double minutes = System.Math.Round(timeValue, 9);
          return minutes == -0 ? 0 : minutes;
        default:
          throw new NotSupportedException();
      }
    }
  }
}