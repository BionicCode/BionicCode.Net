namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    internal class NormalDistributionData<TData>
    {
        public NormalDistributionData(int index, IEnumerable<TData> values, double mean, double standardDeviation, TimeUnit baseUnit, int originalProfilerResultCount)
        {
            Index = index;
            Values = values;
            Mean = mean;
            StandardDeviation = standardDeviation;
            BaseUnit = baseUnit;
            OriginalProfilerResultCount = originalProfilerResultCount;
        }

        public TData this[int index] => Values.ElementAt(index);

        [JsonIgnore]
        public int Index { get; }
        [JsonIgnore]
        public int Count => Values.Count();

        [JsonPropertyName("values")]
        public IEnumerable<TData> Values { get; }
        [JsonIgnore]
        public double Mean { get; }
        [JsonIgnore]
        public double StandardDeviation { get; }

        [JsonIgnore]
        public string Title { get; set; }

        [JsonIgnore]
        public TimeUnit BaseUnit { get; set; }

        [JsonIgnore]
        public int OriginalProfilerResultCount { get; }
    }
}
