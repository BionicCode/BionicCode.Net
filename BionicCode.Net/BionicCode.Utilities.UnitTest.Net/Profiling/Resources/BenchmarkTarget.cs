namespace BionicCode.Utilities.Net.UnitTest.Resources
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.ComponentModel.DataAnnotations;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;

  public class BenchmarkTarget<TParam>
  {
    [Profile(Runtime.Net6_0)]
    [ProfilerArgument(100)]
    [ProfilerArgument(200)]
    //[ProfilerArgument(300)]
    public int NumericValue
    {
      //[Profile]
      [ProfilerArgument(500)]
      get;

      //[Profile]
      [ProfilerArgument(500)]
      [ProfilerArgument(600)]
      [ProfilerArgument(700)]
      private set;
    }

    public int Count => this.KeyValuePairs.Count;

    private Dictionary<string, int> KeyValuePairs { get; }
    private Dictionary<int, string> KeyValuePairsReverse { get; }

    //[Profile]
    [ProfilerArgument(12, Index = "A")]
    [ProfilerArgument(20, Index = "T")]
    public int this[string key]
    {
      get => this.KeyValuePairs[key];
      set => this.KeyValuePairs[key] = value;
    }

    //[Profile]
    [ProfilerArgument("Twenty", Index = 20)]
    [ProfilerArgument("Twelve", Index = 12)]
    public string this[int key]
    {
      get => this.KeyValuePairsReverse[key];
      set => this.KeyValuePairsReverse[key] = value;
    }

    public BenchmarkTarget()
    {
      this.KeyValuePairs = new Dictionary<string, int>();
      this.KeyValuePairsReverse = new Dictionary<int, string>();
      int numericValue = 0;
      for (int itemCount = 65; itemCount < 65 + 26; itemCount++, numericValue++)
      {
        int smallLetterOffset = 32;
        this.KeyValuePairs.Add(new string(new[] { (char)itemCount }), numericValue);
        this.KeyValuePairsReverse.Add(numericValue, new string(new[] { (char)itemCount }));

        this.KeyValuePairs.Add(new string(new[] { (char)(itemCount + smallLetterOffset) }), numericValue + 26);
        this.KeyValuePairsReverse.Add(numericValue + 26, new string(new[] { (char)(itemCount + smallLetterOffset) }));
      }
    }

    //[ProfilerFactoryAttribute]
    private static BenchmarkTarget<TParam> factory = new BenchmarkTarget<TParam>(300);

    //[ProfilerFactoryAttribute]
    //private static BenchmarkTarget<TParam> CreateInstance() => new BenchmarkTarget<TParam>(300);

    [ProfilerFactoryAttribute]
    private static BenchmarkTarget<TParam> Factory { get; } = new BenchmarkTarget<TParam>(300);

    //[Profile]
    [ProfilerArgument(500)]
    public BenchmarkTarget(int numericValue) : this() => this.NumericValue = numericValue;

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
    [ProfilerArgument(100, "Test Value 1", new[] { "1", "2", "3" }, 1)]
    [DisplayName("RTM")]
    //[ProfilerArgument(10, "2", 2)]
    //[ProfilerArgument(200, "3", 3)]
    public async Task TimeConsumingMethod<TMethodParam1, TMethodParam2>(object delayInMilliseconds, TParam someValue, TMethodParam1 someValue1, TMethodParam2 someValue2) where TMethodParam1 : IEnumerable where TMethodParam2 : struct => await Task.Delay(TimeSpan.FromMilliseconds((int)delayInMilliseconds));
  }

  public class BenchmarkTargetAlternate<TParam>
  {
    [Profile]
    [ProfilerArgument("A")]
    [ProfilerArgument("B")]
    //[ProfilerArgument("C")]
    public string TextValue
    {
      //[Profile]
      [ProfilerArgument("D")]
      get;

      //[Profile]
      [ProfilerArgument("E")]
      [ProfilerArgument("F")]
      [ProfilerArgument("G")]
      private set;
    }

    public int Count => this.KeyValuePairs.Count;

    private Dictionary<string, int> KeyValuePairs { get; }
    private Dictionary<int, string> KeyValuePairsReverse { get; }

    [Profile]
    [ProfilerArgument(12, Index = "A")]
    [ProfilerArgument(20, Index = "T")]
    public int this[string key]
    {
      get => this.KeyValuePairs[key];
      set => this.KeyValuePairs[key] = value;
    }

    //[Profile]
    [ProfilerArgument("Twenty", Index = 20)]
    [ProfilerArgument("Twelve", Index = 12)]
    public string this[int key]
    {
      get => this.KeyValuePairsReverse[key];
      set => this.KeyValuePairsReverse[key] = value;
    }

    public BenchmarkTargetAlternate()
    {
      this.KeyValuePairs = new Dictionary<string, int>();
      this.KeyValuePairsReverse = new Dictionary<int, string>();
      int numericValue = 0;
      for (int itemCount = 65; itemCount < 65 + 26; itemCount++, numericValue++)
      {
        int smallLetterOffset = 32;
        this.KeyValuePairs.Add(new string(new[] { (char)itemCount }), numericValue);
        this.KeyValuePairsReverse.Add(numericValue, new string(new[] { (char)itemCount }));

        this.KeyValuePairs.Add(new string(new[] { (char)(itemCount + smallLetterOffset) }), numericValue + 26);
        this.KeyValuePairsReverse.Add(numericValue + 26, new string(new[] { (char)(itemCount + smallLetterOffset) }));
      }
    }

    //[ProfilerFactoryAttribute]
    private static BenchmarkTargetAlternate<TParam> factory = new BenchmarkTargetAlternate<TParam>("Created by factory field");

    //[ProfilerFactoryAttribute]
    //private static BenchmarkTarget<TParam> CreateInstance() => new BenchmarkTarget<TParam>(300);

    [ProfilerFactoryAttribute]
    private static BenchmarkTargetAlternate<TParam> Factory { get; } = new BenchmarkTargetAlternate<TParam>("Created by factory property");

    //[Profile]
    [ProfilerArgument("Malcolm X")]
    public BenchmarkTargetAlternate(string textValue) : this() => this.TextValue = textValue;

    //[Profile]
    [ProfilerArgument(500, "1")]
    [ProfilerArgument(1000, "2")]
    [ProfilerArgument(200, "3")]
    public static async Task TimeConsumingOperation(int delayInMilliseconds, TParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    //[Profile]
    [ProfilerArgument(500, "1")]
    [ProfilerArgument(1000, "2")]
    [ProfilerArgument(200, "3")]
    public static async Task TimeConsumingOperation<TMethodParam>(int delayInMilliseconds, TMethodParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    [Profile]
    [ProfilerArgument(100, new[] { "1", "2", "3" }, 1)]
    //[ProfilerArgument(10, "2", 2)]
    //[ProfilerArgument(200, "3", 3)]
    public async Task TimeConsumingOperation<TMethodParam1, TMethodParam2>(int delayInMilliseconds, TMethodParam1 someValue1, TMethodParam2 someValue2) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));
  }
}
