namespace BionicCode.Utilities.Net.Reflection;

internal interface ISymbolViewProvider<TSymbolView>
    where TSymbolView : ISymbolInfoDataView
{
    TSymbolView View { get; }
}