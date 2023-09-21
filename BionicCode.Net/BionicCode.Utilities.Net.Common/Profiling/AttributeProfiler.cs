﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using System.Threading.Tasks;

  internal class AttributeProfiler
  {    
    private IAttributeProfilerConfiguration Configuration { get; }
    private IEnumerable<IProfilerLogger> DefaultLoggers { get; }

    public AttributeProfiler(IAttributeProfilerConfiguration configuration)
    {
      this.Configuration = configuration;
      this.DefaultLoggers = new List<IProfilerLogger>() { new HtmlLogger() };
    }

    internal async Task<ProfiledTypeResultCollection> StartAsync()
    {
      var typeResults = new ProfiledTypeResultCollection();
      foreach (Type typeToProfile in this.Configuration.Type)
      {
        ProfilerBatchResultGroupCollection typeMemberResults = await ProfileType(typeToProfile);
        typeResults.Add(typeMemberResults);
      }

      if (typeResults.Any() && this.Configuration.IsDefaultLogOutputEnabled)
      {
        foreach (IProfilerLogger logger in this.DefaultLoggers)
        {
          await logger.LogAsync(typeResults);
        }
      }

      return typeResults;
    }

    private async Task<ProfilerBatchResultGroupCollection> ProfileType(Type typeToProfile)
    {
      object targetInstance = null;
      bool hasProfiledInstanceMembers = false;
      bool isInstanceProviderMemberInfoRequired = !typeToProfile.IsAbstract;
      var targetMembers = new List<ProfiledMemberInfo>();
      GetTargetMethods(typeToProfile, isInstanceProviderMemberInfoRequired, targetMembers, out bool hasProfiledInstanceMethods, out InstanceProviderInfo instanceProviderMethodInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceMethods;
      if (isInstanceProviderMemberInfoRequired && instanceProviderMethodInfo != null)
      {
        isInstanceProviderMemberInfoRequired = false;
        targetInstance = ((MethodInfo)instanceProviderMethodInfo?.MemberInfo).Invoke(null, instanceProviderMethodInfo.ArgumentList);
      }

      GetTargetProperties(typeToProfile, isInstanceProviderMemberInfoRequired, targetMembers, out bool hasProfiledInstanceProperties, out InstanceProviderInfo instanceProviderPropertyInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceProperties;
      if (isInstanceProviderMemberInfoRequired && instanceProviderPropertyInfo != null)
      {
        isInstanceProviderMemberInfoRequired = false;
        targetInstance = ((PropertyInfo)instanceProviderPropertyInfo.MemberInfo).GetValue(null, null);
      }

      if (isInstanceProviderMemberInfoRequired)
      {
        InstanceProviderInfo instanceProviderFieldInfo = GetInstanceProviderField(typeToProfile);
        isInstanceProviderMemberInfoRequired = instanceProviderFieldInfo == null;
        targetInstance = ((FieldInfo)instanceProviderFieldInfo?.MemberInfo)?.GetValue(null);
      }

      GetTargetConstructors(typeToProfile, isInstanceProviderMemberInfoRequired, targetMembers, out InstanceProviderInfo instanceProviderConstructorInfo);

      // We don't need an instance if we have an abstract (abstract, static) type
      // or the profiled members are only constructors.
      bool isTargetInstanceRequired = !typeToProfile.IsAbstract
        && hasProfiledInstanceMembers;

      if (isTargetInstanceRequired && targetInstance == null)
      {
        targetInstance = ((ConstructorInfo)instanceProviderConstructorInfo?.MemberInfo)?.Invoke(instanceProviderConstructorInfo.ArgumentList);
      }

      return isTargetInstanceRequired && targetInstance == null
        ? throw new MissingMemberException(ExceptionMessages.GetMissingCreationMemberOfProfiledTypeExceptionMessage(typeToProfile))
        : await ProfileMembersAsync(targetMembers, targetInstance, typeToProfile);
    }

    private async Task<ProfilerBatchResultGroupCollection> ProfileMembersAsync(IEnumerable<ProfiledMemberInfo> memberInfos, object profiledInstance, Type typeToProfile)
    {
      var resultGroups = new ProfilerBatchResultGroupCollection(typeToProfile);
      foreach (ProfiledMemberInfo memberInfo in memberInfos)
      {
        string fullSignatureName = CreateMemberSignatureName(memberInfo.MemberInfo);
        bool memberIsMethod = memberInfo is ProfiledMethodInfo;
        bool memberIsConstructor = memberInfo is ProfiledConstructorInfo;
        bool memberIsProperty = memberInfo is ProfiledPropertyInfo;

        ProfiledMethodInfo method = memberInfo as ProfiledMethodInfo;
        ProfiledConstructorInfo constructor = memberInfo as ProfiledConstructorInfo;
        ProfiledPropertyInfo property = memberInfo as ProfiledPropertyInfo;

        var memberResultGroup = new ProfilerBatchResultGroup();
        for (int argumentListIndex = 0; argumentListIndex < memberInfo.ArgumentLists.Count(); argumentListIndex++)
        {
          IEnumerable<object> argumentList = memberInfo.ArgumentLists.ElementAt(argumentListIndex);
          CreateTargetMemberDelegate(profiledInstance, memberInfo, argumentList);
          if (memberIsMethod)
          {
            ProfilerBatchResult result = null;
            if (method.IsAsync)
            {
              result = await Profiler.LogTimeAsyncInternal(method.AsyncMethodDelegate.Invoke, null, null, this.Configuration.WarmupIterations, this.Configuration.Iterations, argumentListIndex, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger, this.Configuration.BaseUnit, method.SourceFilePath, method.LineNumber);
            }
            else
            {
              result = Profiler.LogTimeInternal(method.MethodDelegate.Invoke, this.Configuration.WarmupIterations, this.Configuration.Iterations, argumentListIndex, this.Configuration.ProfilerLogger, this.Configuration.BaseUnit, method.SourceFilePath, method.LineNumber);
            }

            ProfiledTargetType targetType = method.MethodName.StartsWith("set_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 1
              ? ProfiledTargetType.PropertySet 
              : method.MethodName.StartsWith("get_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 0
                ? ProfiledTargetType.PropertyGet 
                : ProfiledTargetType.Method;

            var context = new ProfilerContext(method.AssemblyName, fullSignatureName, targetType, method.SourceFilePath, method.LineNumber, memberInfo.MemberInfo, this.Configuration.WarmupIterations);
            result.Context = context;
            result.BaseUnit = this.Configuration.BaseUnit;

            if (memberResultGroup.IsEmpty())
            {
              memberResultGroup.ProfiledTargetType = ProfiledTargetType.Method;
              memberResultGroup.ProfiledTargetSignatureMemberName = fullSignatureName;
              memberResultGroup.ProfiledTargetMemberShortName = method.MethodInfo.ToDisplayName();
              result.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResultGroup.Add(result);
            }
            else
            {
              memberResultGroup.First().Combine(result);
            }
          }
          else if (memberIsConstructor)
          {
            ProfilerBatchResult result = Profiler.LogTimeInternal(constructor.ConstructorDelegate, this.Configuration.WarmupIterations, this.Configuration.Iterations, argumentListIndex, this.Configuration.ProfilerLogger, this.Configuration.BaseUnit, constructor.SourceFilePath, lineNumber: constructor.LineNumber);
            var context = new ProfilerContext(constructor.AssemblyName, fullSignatureName, ProfiledTargetType.Constructor, constructor.SourceFilePath, constructor.LineNumber, memberInfo.MemberInfo, this.Configuration.WarmupIterations);
            result.Context = context;
            result.BaseUnit = this.Configuration.BaseUnit;

            if (memberResultGroup.IsEmpty())
            {
              memberResultGroup.ProfiledTargetType = ProfiledTargetType.Constructor;
              memberResultGroup.ProfiledTargetSignatureMemberName = fullSignatureName;
              memberResultGroup.ProfiledTargetMemberShortName = constructor.ConstructorInfo.ToDisplayName();
              result.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResultGroup.Add(result);
            }
            else
            {
              memberResultGroup.First().Combine(result);
            }
          }
          else if (memberIsProperty)
          {
            ProfilerBatchResult propertyGetResult = Profiler.LogTimeInternal(property.GetDelegate, this.Configuration.WarmupIterations, this.Configuration.Iterations, argumentListIndex, this.Configuration.ProfilerLogger, this.Configuration.BaseUnit, property.SourceFilePath, lineNumber: property.LineNumber);
            var context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertyGet, property.SourceFilePath, property.LineNumber, memberInfo.MemberInfo, this.Configuration.WarmupIterations);
            propertyGetResult.Context = context;
            propertyGetResult.Index = 0;
            propertyGetResult.BaseUnit = this.Configuration.BaseUnit;

            if (memberResultGroup.IsEmpty())
            {
              memberResultGroup.ProfiledTargetType = ProfiledTargetType.Property;
              memberResultGroup.ProfiledTargetSignatureMemberName = fullSignatureName;
              memberResultGroup.ProfiledTargetMemberShortName = property.PropertyInfo.ToDisplayName();
              propertyGetResult.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResultGroup.Add(propertyGetResult);
            }
            else
            {
              memberResultGroup[0].Combine(propertyGetResult);
            }

            ProfilerBatchResult propertySetResult = Profiler.LogTimeInternal(property.SetDelegate, this.Configuration.WarmupIterations, this.Configuration.Iterations, argumentListIndex, this.Configuration.ProfilerLogger, this.Configuration.BaseUnit, property.SourceFilePath, lineNumber: property.LineNumber);
            context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertySet, property.SourceFilePath, property.LineNumber, memberInfo.MemberInfo, this.Configuration.WarmupIterations);
            propertySetResult.Context = context;
            propertySetResult.Index = 1;
            propertySetResult.BaseUnit = this.Configuration.BaseUnit;

            if (memberResultGroup.Count == 1)
            {
              propertySetResult.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResultGroup.Add(propertySetResult);
            }
            else
            {
              memberResultGroup[1].Combine(propertySetResult);
            }
          }
        }

        if (memberResultGroup.Any())
        {
          resultGroups.Add(memberResultGroup);
        }
      }

      return resultGroups;
    }

    private string CreateMemberSignatureName(MemberInfo memberInfo)
    {
      string fullMemberName = memberInfo.ToDisplaySignatureName();

      //if (methodInfo.MethodInfo.IsHideBySig)
      //{
      //  _ = fullMemberNameBuilder.Append("new")
      //    .Append(" ");
      //}

      return fullMemberName.ToString();
    }

    private void CreateTargetMemberDelegate(object targetInstance, ProfiledMemberInfo profiledMemberInfo, IEnumerable<object> argumentList)
    {
      if (profiledMemberInfo.IsStatic)
      {
        targetInstance = null;
      }

      if (profiledMemberInfo is ProfiledConstructorInfo profiledConstructorInfo)
      {
        Action constructor = () => profiledConstructorInfo.ConstructorInfo.Invoke(argumentList.ToArray());
        profiledConstructorInfo.ConstructorDelegate = constructor;

        return;
      }

      if (profiledMemberInfo is ProfiledMethodInfo profiledMethodInfo)
      {
        Action method = null;
        Func<Task> asyncMethod = null;
        if (profiledMethodInfo.IsGeneric)
        {
          Type[] genericTypeArguments = profiledMethodInfo.MethodInfo.GetGenericArguments();
          ParameterInfo[] methodParameters = profiledMethodInfo.MethodInfo.GetParameters();
          var genericParameterTypes = new Type[genericTypeArguments.Length];
          int genericParameterTypesIndex = 0;
          foreach (Type genericTypeArgument in genericTypeArguments)
          {
            for (int genericParameterIndex = 0; genericParameterIndex < methodParameters.Length; genericParameterIndex++)
            {
              ParameterInfo parameterAtCurrentPosition = methodParameters[genericParameterIndex];
              if (parameterAtCurrentPosition.ParameterType.Name.Equals(genericTypeArgument.Name, StringComparison.Ordinal))
              {
                object argumentForGenericParameterPosition = argumentList.ElementAt(genericParameterIndex);
                genericParameterTypes[genericParameterTypesIndex++] = argumentForGenericParameterPosition.GetType();
              }
            }
          }

          MethodInfo genericMethodInfo = profiledMethodInfo.MethodInfo.MakeGenericMethod(genericParameterTypes);
          if (profiledMethodInfo.IsAsync)
          {
            if (profiledMethodInfo.IsAsyncTask)
            {
              asyncMethod = async () =>
              {
                try
                {
                  await (Task)genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
                }
                catch (ArgumentException e)
                {

                  throw new ProfilerArgumentException(ExceptionMessages.GetArgumentListMismatchExceptionMessage(), e);
                }
              };
            }
            else
            {
              asyncMethod = async () => await (ValueTask)genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
            }
          }
          else
          {
            method = () => genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
          }
        }
        else if (profiledMethodInfo.IsAsync)
        {
          if (profiledMethodInfo.IsAsyncTask)
          {
            asyncMethod = () => (Task)profiledMethodInfo.MethodInfo.Invoke(targetInstance, argumentList.ToArray());
          }
          else
          {
            asyncMethod = async () => await (ValueTask)profiledMethodInfo.MethodInfo.Invoke(targetInstance, argumentList.ToArray());
          }
        }
        else
        {
          method = () => profiledMethodInfo.MethodInfo.Invoke(targetInstance, argumentList.ToArray());
        }

        profiledMethodInfo.MethodDelegate = method;
        profiledMethodInfo.AsyncMethodDelegate = asyncMethod;

        return;
      }

      if (profiledMemberInfo is ProfiledPropertyInfo profiledPropertyInfo)
      {
        MethodInfo getMethodInfo = profiledPropertyInfo.PropertyInfo.GetGetMethod(true);
        MethodInfo setMethodInfo = profiledPropertyInfo.PropertyInfo.GetSetMethod(true);
        Action setMethod = null;
        Action getMethod = null;

        if (profiledPropertyInfo.IsIndexer)
        {
          var argumentInfo = (KeyValuePair<object, object>)argumentList.FirstOrDefault();
          setMethod = () => profiledPropertyInfo.PropertyInfo.SetValue(targetInstance, argumentInfo.Value, new[] { argumentInfo.Key });
          getMethod = () => profiledPropertyInfo.PropertyInfo.GetValue(targetInstance, new[] { argumentInfo.Key });
        }
        else
        {
          setMethod = () => profiledPropertyInfo.PropertyInfo.SetValue(targetInstance, argumentList.ElementAtOrDefault(0));
          getMethod = () => profiledPropertyInfo.PropertyInfo.GetValue(targetInstance);
        }

        profiledPropertyInfo.SetDelegate = setMethod;
        profiledPropertyInfo.GetDelegate = getMethod;
      }
    }

    private void GetTargetMethods(Type typeToProfile,bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, out bool hasProfiledInstanceMethods, out InstanceProviderInfo instanceProviderMethodInfo)
    {
      hasProfiledInstanceMethods = false;
      instanceProviderMethodInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      foreach (MethodInfo methodInfo in typeToProfile.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        bool isMethodStatic = methodInfo.IsStatic;
        if (isFindInstanceProviderEnabled
          && isMethodStatic
          && typeToProfile.IsAssignableFrom(methodInfo.ReturnType))
        {
          if (instanceProviderMethodInfo == null || !hasHighPriorityInstanceProvider)
          {
            var profilerFactoryAttribute = (ProfilerFactoryAttribute)methodInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute));
            hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
            object[] argumentList = hasHighPriorityInstanceProvider
              ? profilerFactoryAttribute.ArgumentList
              : Array.Empty<object>();

            instanceProviderMethodInfo = new InstanceProviderInfo(argumentList, methodInfo);
          }
        }

        var profileAttribute = (ProfileAttribute)methodInfo.GetCustomAttribute(typeof(ProfileAttribute));
        if (profileAttribute == null)
        {
          continue;
        }

        hasProfiledInstanceMethods |= !isMethodStatic;

        IEnumerable<IEnumerable<object>> argumentLists = new List<IEnumerable<object>> { new VoidArgumentList() };
        int methodParameterCount = methodInfo.GetParameters().Length;
        bool isMethodParameterless = methodParameterCount == 0;
        if (!isMethodParameterless)
        {
          argumentLists = methodInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
            .Select(attribute => ((ProfilerArgumentAttribute)attribute).Arguments);
          if (argumentLists.IsEmpty())
          {
            string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Method(methodParameterCount);
            throw new MissingProfilerArgumentAttributeException(exceptionMessage);
          }
        }

        string asseblyName = this.Configuration.GetAssembly(typeToProfile).GetName().Name;
        var profiledMethodInfo = new ProfiledMethodInfo(argumentLists, methodInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isMethodStatic);
        targetMembers.Add(profiledMethodInfo);
      }
    }

    private void GetTargetConstructors(Type typeToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, out InstanceProviderInfo instanceProviderConstructorInfo)
    {
      instanceProviderConstructorInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      foreach (ConstructorInfo constructorInfo in typeToProfile.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        bool isConstructorStatic = constructorInfo.IsStatic;
        if (isFindInstanceProviderEnabled
          && !isConstructorStatic)
        {
          if ((instanceProviderConstructorInfo == null && constructorInfo.GetParameters().Length == 0) 
            || !hasHighPriorityInstanceProvider)
          {
            var profilerFactoryAttribute = (ProfilerFactoryAttribute)constructorInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute));
            hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
            object[] argumentList = hasHighPriorityInstanceProvider
              ? profilerFactoryAttribute.ArgumentList
              : Array.Empty<object>();

            instanceProviderConstructorInfo = new InstanceProviderInfo(argumentList, constructorInfo);
          }
        }

        var profileAttribute = (ProfileAttribute)constructorInfo.GetCustomAttribute(typeof(ProfileAttribute));
        if (profileAttribute == null)
        {
          continue;
        }

        IEnumerable<IEnumerable<object>> argumentLists = new List<IEnumerable<object>> { new VoidArgumentList() };
        int constructorParameterCount = constructorInfo.GetParameters().Length;
        bool isConstructorParameterless = constructorParameterCount == 0;
        if (!isConstructorParameterless)
        {
          argumentLists = constructorInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
            .Select(attribute => ((ProfilerArgumentAttribute)attribute).Arguments);
          if (argumentLists.IsEmpty())
          {
            string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Constructor(constructorParameterCount);
            throw new MissingProfilerArgumentAttributeException(exceptionMessage);
          }
        }

        string asseblyName = this.Configuration.GetAssembly(typeToProfile).GetName().Name;
        ProfiledConstructorInfo profiledConstructorInfo = new ProfiledConstructorInfo(argumentLists, constructorInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isConstructorStatic);
        targetMembers.Add(profiledConstructorInfo);
      }
    }

    private void GetTargetProperties(Type typeToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, out bool hasProfiledInstanceProperties, out InstanceProviderInfo instanceProviderPropertyInfo)
    {
      hasProfiledInstanceProperties = false;
      instanceProviderPropertyInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      foreach (PropertyInfo propertyInfo in typeToProfile.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        bool isPropertyStatic = propertyInfo.GetGetMethod(true)?.IsStatic ?? false;

        if (isFindInstanceProviderEnabled
          && isPropertyStatic
          && typeToProfile.IsAssignableFrom(propertyInfo.PropertyType))
        {
          if (instanceProviderPropertyInfo == null || !hasHighPriorityInstanceProvider)
          {
            hasHighPriorityInstanceProvider = ((ProfilerFactoryAttribute)propertyInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute))) != null;
            instanceProviderPropertyInfo = new InstanceProviderInfo(Array.Empty<object>(), propertyInfo);
          }
        }

        var profileAttribute = (ProfileAttribute)propertyInfo.GetCustomAttribute(typeof(ProfileAttribute));
        if (profileAttribute == null)
        {
          continue;
        }

        hasProfiledInstanceProperties |= !isPropertyStatic;
        
        IEnumerable<IEnumerable<object>> argumentLists = new List<IEnumerable<object>> { new VoidArgumentList() };
        bool isPropertyIndexer = propertyInfo.GetIndexParameters().Length > 0;
        bool isRequiringArguments = isPropertyIndexer || propertyInfo.GetSetMethod(true) != null;
        if (isRequiringArguments)
        {
          argumentLists = isPropertyIndexer
          ? propertyInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
            .Cast<ProfilerArgumentAttribute>()
            .Select(argumentAttribute => new List<object> { new KeyValuePair<object, object>(argumentAttribute.Index, argumentAttribute.Arguments.FirstOrDefault()) })
          : propertyInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
            .Cast<ProfilerArgumentAttribute>()
            .Select(attribute => attribute.Arguments);
          if (argumentLists.IsEmpty())
          {
            string propertyExceptionMesssage = isPropertyIndexer 
              ? ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_IndexerProperty()
              : ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Property();
            throw new MissingProfilerArgumentAttributeException(propertyExceptionMesssage);
          }
        }

        string asseblyName = this.Configuration.GetAssembly(typeToProfile).GetName().Name;
        var profiledPropertyInfo = new ProfiledPropertyInfo(argumentLists, propertyInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isPropertyIndexer, isPropertyStatic);
        targetMembers.Add(profiledPropertyInfo);
      }
    }

    private InstanceProviderInfo GetInstanceProviderField(Type typeToProfile)
    {
      bool hasHighPriorityInstanceProvider = false;
      FieldInfo instanceProviderFieldInfo = null;
      foreach (FieldInfo fieldInfo in typeToProfile.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (fieldInfo.IsStatic
          && typeToProfile.IsAssignableFrom(fieldInfo.FieldType))
        {
          if (instanceProviderFieldInfo == null || !hasHighPriorityInstanceProvider)
          {
            hasHighPriorityInstanceProvider = fieldInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute)) != null;
            instanceProviderFieldInfo = fieldInfo;
            if (hasHighPriorityInstanceProvider)
            {
              break;
            }
          }
        }
      }

      return new InstanceProviderInfo(Array.Empty<object>(), instanceProviderFieldInfo);
    }
  }  
}