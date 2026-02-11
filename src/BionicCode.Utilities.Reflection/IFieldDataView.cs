namespace BionicCode.Utilities.Net.Reflection;

public interface IFieldDataView : IMemberDataView, ISymbolInfoDataView
{
    ITypeDataView FieldTypeData { get; }
    RuntimeFieldHandle Handle { get; }
    bool IsConst { get; }
    bool IsInitOnly { get; }
    bool IsReadonly { get; }
    bool IsRef { get; }

    object? GetValue(object? target);
    void SetStructValue<TTarget, TValue>(ref TTarget target, TValue? value) where TTarget : struct;
    void SetValue(object? target, object? value);
}