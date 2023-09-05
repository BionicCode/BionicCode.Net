namespace BionicCode.Utilities.Net.UnitTest.Resources
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class BenchmarkTarget<TParam>
  {
    //[Profile]
    [ProfilerArgument(100)]
    //[ProfilerArgument(200)]
    //[ProfilerArgument(300)]
    public int NumericValue
    {
      //[Profile]
      //[ProfilerArgument(500)]
      get;

      //[Profile]
      [ProfilerArgument(500)]
      [ProfilerArgument(600)]
      [ProfilerArgument(700)]
      private set;
    }

    private Dictionary<string, int> KeyValuePairs { get; }
    private Dictionary<int, string> KeyValuePairsReverse { get; }

    //[Profile]
    [ProfilerArgument(12, Index = "Twelve")]
    public int this[string key]
    {
      get => this.KeyValuePairs[key];
      set => this.KeyValuePairs[key] = value;
    }

    //[Profile]
    [ProfilerArgument("Twenty", Index = 20)]
    public string this[int key]
    {
      get => this.KeyValuePairsReverse[key];
      set => this.KeyValuePairsReverse[key] = value;
    }

    public BenchmarkTarget()
    {
      this.KeyValuePairs = new Dictionary<string, int>()
      { { "Twelve", 12 } };
      this.KeyValuePairsReverse = new Dictionary<int, string>()
      { { 20, "Twenty" } };
      //System.Threading.Thread.Sleep(500);
    }

    //[ProfilerFactoryAttribute]
    //private static BenchmarkTarget<TParam> factory  = new BenchmarkTarget<TParam>(300);

    //[Profile]
    [ProfilerArgument(500)]
    public BenchmarkTarget(int numericValue)
    {
      //this.NumericValue = numericValue;
    }

    //[Profile]
    [ProfilerArgument(500, "1")]
    [ProfilerArgument(1000, "2")]
    [ProfilerArgument(200, "3")]
    public static async Task TimeConsumingMethod(int delayInMilliseconds, TParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    //[Profile]
    [ProfilerArgument(500, "1")]
    [ProfilerArgument(1000, "2")]
    [ProfilerArgument(200, "3")]
    public static async Task TimeConsumingMethod<TMethodParam>(int delayInMilliseconds, TMethodParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    [Profile]
    [ProfilerArgument(500, "1", 1)]
    [ProfilerArgument(1000, "2", 2)]
    [ProfilerArgument(200, "3", 3)]
    public static async Task TimeConsumingMethod<TMethodParam1, TMethodParam2>(int delayInMilliseconds, TMethodParam1 someValue1, TMethodParam2 someValue2) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));
  }
}
