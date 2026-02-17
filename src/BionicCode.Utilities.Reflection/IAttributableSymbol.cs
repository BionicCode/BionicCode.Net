namespace BionicCode.Utilities.Net.Reflection;

internal interface IAttributableSymbol
{
    bool HasCompilerAttribute(string attributeName, bool inherit = false);
    bool HasCompilerAttribute(Type? attributeType, string? attributeName = null, bool inherit = false);
    bool HasCompilerAttribute<TAttribute>(string? attributeName = null, bool inherit = false) where TAttribute : Attribute;
    bool IsDefined(Type attributeType, bool inherit = false);
    bool IsDefined<TAttribute>(bool inherit = false) where TAttribute : Attribute;
}