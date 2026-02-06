namespace BionicCode.SIUnits.Core
{
    using System.Collections.Frozen;

    /// <summary>
    /// Converts <see cref="SIBaseDimension"/> to the string representatopn of the corresponding SI unit symbol.
    /// </summary>
    public static class SIBaseDimensionsToSymbolConverter
    {
        private static readonly FrozenDictionary<SIBaseDimension, string> SIBaseDimensionToSymbolMap
            = new Dictionary<SIBaseDimension, string>
            {
                { SIBaseDimension.Length, "m" },
                { SIBaseDimension.Mass, "kg" },
                { SIBaseDimension.Time, "s" },
                { SIBaseDimension.ElectricCurrent, "A" },
                { SIBaseDimension.ThermodynamicTemperature, "K" },
                { SIBaseDimension.AmountOfSubstance, "mol" },
                { SIBaseDimension.LuminousIntensity, "cd" }
            }.ToFrozenDictionary();

        /// <summary>
        /// Converts the specified <see cref="SIBaseDimension"/> to its corresponding SI unit symbol.
        /// </summary>
        /// <param name="siBaseDimension">The <see cref="SIBaseDimension"/> to convert.</param>
        /// <returns>A <see langword="string"/> that represents the SI unit symbol that corresponds to the <paramref name="siBaseDimension"/>.</returns>
        public static string ToSymbol(this SIBaseDimension siBaseDimension)
            => SIBaseDimensionToSymbolMap.TryGetValue(siBaseDimension, out string symbol)
                ? symbol
                : throw new ArgumentException("Enum is not defined.", nameof(siBaseDimension));
    }
}
