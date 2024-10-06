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
    //[Profile(Runtime.Net6_0)]
    [ProfilerPropertyArgument(100)]
    [ProfilerPropertyArgument(200)]
    [ProfilerPropertyArgument(500, Accessor = PropertyAccessor.Set)]
    [ProfilerPropertyArgument(600, Accessor = PropertyAccessor.Set)]
    [ProfilerPropertyArgument(700, Accessor = PropertyAccessor.Set)]
    [ProfilerPropertyArgument(Accessor = PropertyAccessor.Get)]
    //[ProfilerArgument(300)]
    public int NumericValue
    {
      //[Profile]
      [ProfilerMethodArgument(2500)]
      get;

      //[Profile]
      [ProfilerMethodArgument(2500)]
      [ProfilerMethodArgument(2600)]
      [ProfilerMethodArgument(2700)]
      private set;
    }

    public int Count => this.KeyValuePairs.Count;

    private Dictionary<string, int> KeyValuePairs { get; }
    private Dictionary<int, string> KeyValuePairsReverse { get; }

    //[Profile]
    [ProfilerPropertyArgument(12, Index = new[] { "A" })]
    [ProfilerPropertyArgument(20, Index = new[] { "T" })]
    public int this[string key]
    {
      get => this.KeyValuePairs[key];
      set => this.KeyValuePairs[key] = value;
    }

    [Profile]
    [ProfilerPropertyArgument("Twenty", Index = new object[] { 20 })]
    [ProfilerPropertyArgument("Twelve", Index = new object[] { 12 })]
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
    [ProfilerMethodArgument(500)]
    public BenchmarkTarget(int numericValue) : this() => this.NumericValue = numericValue;

    //[Profile]
    [ProfilerMethodArgument(500, "1")]
    [ProfilerMethodArgument(1000, "2")]
    [ProfilerMethodArgument(200, "3")]
    public static async Task TimeConsumingMethod(int delayInMilliseconds, TParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    //[Profile]
    [ProfilerMethodArgument(500, "1")]
    [ProfilerMethodArgument(1000, "2")]
    [ProfilerMethodArgument(200, "3")]
    public static async Task TimeConsumingMethod<TMethodParam>(int delayInMilliseconds, TMethodParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    [Profile]
    [ProfilerMethodArgument(100, "Test Value 1", new[] { "1", "2", "3" }, 1)]
    [DisplayName("RTM")]
    //[ProfilerArgument(10, "2", 2)]
    //[ProfilerArgument(200, "3", 3)]
    public async Task TimeConsumingMethod<TMethodParam1, TMethodParam2>(object delayInMilliseconds, TParam someValue, TMethodParam1 someValue1, TMethodParam2 someValue2) where TMethodParam1 : IEnumerable where TMethodParam2 : struct => await Task.Delay(TimeSpan.FromMilliseconds((int)delayInMilliseconds));
  }

  public class BenchmarkTargetAlternate<TParam>
  {
    [Profile]
    [ProfilerPropertyArgument("A")]
    [ProfilerPropertyArgument("B")]
    [ProfilerPropertyArgument("E", Accessor = PropertyAccessor.Set)]
    [ProfilerPropertyArgument("F", Accessor = PropertyAccessor.Set)]
    [ProfilerPropertyArgument("G", Accessor = PropertyAccessor.Set)]
    [ProfilerPropertyArgument("D", Accessor = PropertyAccessor.Get)]
    //[ProfilerArgument("C")]
    public string TextValue
    {
      //[Profile]
      [ProfilerMethodArgument("D")]
      get;

      //[Profile]
      [ProfilerMethodArgument("E")]
      [ProfilerMethodArgument("F")]
      [ProfilerMethodArgument("G")]
      private set;
    }

    public int Count => this.KeyValuePairs.Count;

    private Dictionary<string, int> KeyValuePairs { get; }
    private Dictionary<int, string> KeyValuePairsReverse { get; }

    [Profile]
    [ProfilerPropertyArgument(12, Index = new[] { "A" })]
    [ProfilerPropertyArgument(20, Index = new[] { "T" })]
    public int this[string key]
    {
      get => this.KeyValuePairs[key];
      set => this.KeyValuePairs[key] = value;
    }

    //[Profile]
    [ProfilerPropertyArgument("Twenty", Index = new object[] { 20 })]
    [ProfilerPropertyArgument("Twelve", Index = new object[] { 12 })]
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
    [ProfilerMethodArgument("Malcolm X")]
    public BenchmarkTargetAlternate(string textValue) : this() => this.TextValue = textValue;

    //[Profile]
    [ProfilerMethodArgument(500, "1")]
    [ProfilerMethodArgument(1000, "2")]
    [ProfilerMethodArgument(200, "3")]
    public static async Task TimeConsumingOperation(int delayInMilliseconds, TParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    //[Profile]
    [ProfilerMethodArgument(500, "1")]
    [ProfilerMethodArgument(1000, "2")]
    [ProfilerMethodArgument(200, "3")]
    public static async Task TimeConsumingOperation<TMethodParam>(int delayInMilliseconds, TMethodParam someValue) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));

    [Profile]
    [ProfilerMethodArgument(100, new[] { "1", "2", "3" }, 1)]
    //[ProfilerArgument(10, "2", 2)]
    //[ProfilerArgument(200, "3", 3)]
    public async Task TimeConsumingOperation<TMethodParam1, TMethodParam2>(int delayInMilliseconds, TMethodParam1 someValue1, TMethodParam2 someValue2) => await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));
  }
}
