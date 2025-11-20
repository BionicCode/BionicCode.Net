namespace BionicCode.SIUnits.Core
{
    using System.Collections.Frozen;

    /// <summary>
    /// Converts SI unit symbol string to its corresponding <see cref="SIBaseDimension"/>.
    /// </summary>
    public static class SymbolToSIBaseDimensionsConverter
    {
        private static readonly FrozenDictionary<string, SIBaseDimension> SymbolToSIBaseDimensionMap
            = new Dictionary<string, SIBaseDimension>
            {
                { "m", SIBaseDimension.Length },
                { "kg", SIBaseDimension.Mass },
                { "s", SIBaseDimension.Time },
                { "A", SIBaseDimension.ElectricCurrent },
                { "K", SIBaseDimension.ThermodynamicTemperature },
                { "mol", SIBaseDimension.AmountOfSubstance },
                { "cd", SIBaseDimension.LuminousIntensity }
            }.ToFrozenDictionary();

        /// <summary>
        /// Converts the specified SI unnit symbol to its corresponding <see cref="SIBaseDimension"/>.
        /// </summary>
        /// <param name="symbol">The SI unit symbol to convert.</param>
        /// <returns>A <see cref="SIBaseDimension"/> that corrsponds to the SI unnit symbol <paramref name="symbol"/>.</returns>
        /// <exception cref="ArgumentException">The symbol is not defined.</exception>
        public static SIBaseDimension ToSIBaseDimension(string symbol)
            => SymbolToSIBaseDimensionMap.TryGetValue(symbol, out SIBaseDimension sIBaseDimension) 
                ? sIBaseDimension  
                : throw new ArgumentException($"Undefined symbol. '{symbol}' is not a SI unit symbol.", nameof(symbol));
    }

}
