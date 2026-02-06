namespace BionicCode.Utilities.Net.Profiling;

using System.Collections.Generic;

internal class ProfiledPropertyInfo : ProfiledMemberInfo
{
    public ProfiledPropertyInfo(IList<PropertyArgumentInfo> argumentList, PropertyData propertyData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
        PropertyData = propertyData;
        Arguments = argumentList;
    }

    public PropertyData PropertyData { get; }
    public bool IsIndexer => PropertyData.IsIndexer;

    public string MethodReturnTypeDisplayName => PropertyData.PropertyTypeData.DisplayName;
    public override MemberData MemberInfoData => PropertyData;
    public IList<PropertyArgumentInfo> Arguments { get; }
}
