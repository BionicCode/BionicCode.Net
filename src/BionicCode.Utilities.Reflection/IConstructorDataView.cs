namespace BionicCode.Utilities.Net.Reflection;

public interface IConstructorDataView : IParameterizedMemberDataView, IMemberDataView, ISymbolInfoDataView
{
    object Invoke(params object?[] arguments);
    TResult Invoke<TResult>(params object?[] arguments);
}