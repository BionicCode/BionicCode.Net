namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Management;
  using System.Reflection;

  internal class ProfiledPropertyInfo : ProfiledMemberInfo
  {
    public ProfiledPropertyInfo(IList<PropertyArgumentInfo> argumentList, PropertyData propertyData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
      this.PropertyData = propertyData;
      this.Arguments = argumentList;
    }

    public PropertyData PropertyData { get; }
    public bool IsIndexer => this.PropertyData.IsIndexer;

    public string MethodReturnTypeDisplayName => this.PropertyData.PropertyTypeData.DisplayName;
    public override MemberInfoData MemberInfoData => this.PropertyData;
    public IList<PropertyArgumentInfo> Arguments { get; }
  }
}