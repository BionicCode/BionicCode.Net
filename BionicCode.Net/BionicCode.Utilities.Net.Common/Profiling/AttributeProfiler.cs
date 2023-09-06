namespace BionicCode.Utilities.Net
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

    internal async Task<IEnumerable<ProfilerBatchResult>> StartAsync()
    {
      object targetInstance = null;
      MemberInfo instanceProviderMemberInfo = null;
      bool hasProfiledInstanceMembers = false;
      bool findInstanceProviderMemberInfo = !this.Configuration.Type.IsAbstract;
      var targetMembers = new List<ProfiledMemberInfo>();
      GetTargetMethods(findInstanceProviderMemberInfo, targetMembers, out bool hasProfiledInstanceMethods, out MethodInfo instanceProviderMethodInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceMethods;
      if (findInstanceProviderMemberInfo && instanceProviderMemberInfo != null)
      {
        findInstanceProviderMemberInfo = false;
        targetInstance = instanceProviderMethodInfo.Invoke(null, null);
      }

      GetTargetProperties(findInstanceProviderMemberInfo, targetMembers, out bool hasProfiledInstanceProperties, out PropertyInfo instanceProviderPropertyInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceProperties;
      if (findInstanceProviderMemberInfo && instanceProviderPropertyInfo != null)
      {
        findInstanceProviderMemberInfo = false;
        targetInstance = instanceProviderPropertyInfo.GetValue(null, null);
      }

      if (findInstanceProviderMemberInfo)
      {
        FieldInfo instanceProviderFieldInfo = GetInstanceProviderField();
        findInstanceProviderMemberInfo = instanceProviderFieldInfo == null;
        targetInstance = instanceProviderFieldInfo?.GetValue(null);
      }

      GetTargetConstructors(findInstanceProviderMemberInfo, targetMembers, out ConstructorInfo instanceProviderConstructorInfo);

      // We don't need an instance if we have an abstract (abstract, static) type
      // or the profiled members are only constructors.
      bool isTargetInstanceRequired = !this.Configuration.Type.IsAbstract
        && hasProfiledInstanceMembers;

      if (isTargetInstanceRequired && targetInstance == null)
      {
        targetInstance = instanceProviderConstructorInfo?.Invoke(null);
      }

      return isTargetInstanceRequired && targetInstance == null
        ? throw new MissingMemberException(ExceptionMessages.GetMissingCreationMemberOfProfiledTypeExceptionMessage(this.Configuration.Type))
        : await ProfileMembersAsync(targetMembers, targetInstance);
    }

    private async Task<IEnumerable<ProfilerBatchResult>> ProfileMembersAsync(IEnumerable<ProfiledMemberInfo> memberInfos, object profiledInstance)
    {
      var profilerResults = new ProfilerBatchResultCollection();
      foreach (ProfiledMemberInfo memberInfo in memberInfos)
      {
        string fullSignatureName = CreateMemberSignatureName(memberInfo.MemberInfo);
        bool memberIsMethod = memberInfo is ProfiledMethodInfo;
        bool memberIsConstructor = memberInfo is ProfiledConstructorInfo;
        bool memberIsProperty = memberInfo is ProfiledPropertyInfo;

        ProfiledMethodInfo method = memberInfo as ProfiledMethodInfo;
        ProfiledConstructorInfo constructor = memberInfo as ProfiledConstructorInfo;
        ProfiledPropertyInfo property = memberInfo as ProfiledPropertyInfo;

        var memberResults = new ProfilerBatchResultCollection();
        foreach (IEnumerable<object> argumentList in memberInfo.ArgumentLists)
        {
          //TODO::Check for generic method and construct generic based on parameter position and parameter type
          //var parameterForGenericParameterPosition = method.GetGenericMethodDefinition().GetParameters().First(parameterInfo => parameterInfo.ParameterType.Name == method.GetGenericMethodDefinition().GetGenericArguments()[0].Name);

          CreateTargetMemberDelegate(profiledInstance, memberInfo, argumentList);
          if (memberIsMethod)
          {
            ProfilerBatchResult result = null;
            if (method.IsAsync)
            {
              result = await Profiler.LogTimeAsync(method.AsyncMethodDelegate.Invoke, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, method.SourceFilePath, method.LineNumber);
            }
            else
            {
              result = Profiler.LogTime(method.MethodDelegate.Invoke, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, method.SourceFilePath, method.LineNumber);
            }

            ProfiledTargetType targetType = method.MethodName.StartsWith("set_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 1
              ? ProfiledTargetType.PropertySet 
              : method.MethodName.StartsWith("get_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 0
                ? ProfiledTargetType.PropertyGet 
                : ProfiledTargetType.Method;

            var context = new ProfilerContext(method.AssemblyName, fullSignatureName, targetType, method.SourceFilePath, method.LineNumber, memberInfo.MemberInfo);
            result.Context = context;

            if (memberResults.IsEmpty())
            {
              memberResults.ProfiledTargetType = ProfiledTargetType.Method;
              memberResults.ProfiledTargetMemberName = fullSignatureName;
              result.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResults.Add(result);
            }
            else
            {
              memberResults.First().Combine(result);
            }
          }
          else if (memberIsConstructor)
          {
            ProfilerBatchResult result = Profiler.LogTime(constructor.ConstructorDelegate, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, constructor.SourceFilePath, lineNumber: constructor.LineNumber);
            var context = new ProfilerContext(constructor.AssemblyName, fullSignatureName, ProfiledTargetType.Constructor, constructor.SourceFilePath, constructor.LineNumber, memberInfo.MemberInfo);
            result.Context = context;

            if (memberResults.IsEmpty())
            {
              memberResults.ProfiledTargetType = ProfiledTargetType.Constructor;
              memberResults.ProfiledTargetMemberName = fullSignatureName;
              result.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResults.Add(result);
            }
            else
            {
              memberResults.First().Combine(result);
            }
          }
          else if (memberIsProperty)
          {
            ProfilerBatchResult propertyGetResult = Profiler.LogTime(property.GetDelegate, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, property.SourceFilePath, lineNumber: property.LineNumber);
            var context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertyGet, property.SourceFilePath, property.LineNumber, memberInfo.MemberInfo);
            propertyGetResult.Context = context;
            propertyGetResult.Index = 0;

            if (memberResults.IsEmpty())
            {
              memberResults.ProfiledTargetType = ProfiledTargetType.Property;
              memberResults.ProfiledTargetMemberName = fullSignatureName;
              propertyGetResult.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResults.Add(propertyGetResult);
            }
            else
            {
              memberResults[0].Combine(propertyGetResult);
            }

            ProfilerBatchResult propertySetResult = Profiler.LogTime(property.SetDelegate, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, property.SourceFilePath, lineNumber: property.LineNumber);
            context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertySet, property.SourceFilePath, property.LineNumber, memberInfo.MemberInfo);
            propertySetResult.Context = context;
            propertySetResult.Index = 1;

            if (memberResults.Count == 1)
            {
              propertySetResult.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResults.Add(propertySetResult);
            }
            else
            {
              memberResults[1].Combine(propertySetResult);
            }
          }
        }

        if (memberResults.Any() && this.Configuration.IsDefaultLogOutputEnabled)
        {
          foreach (IProfilerLogger logger in this.DefaultLoggers)
          {
            await logger.LogAsync(memberResults);
          }
        }

        memberResults.ForEach(profilerResults.Add);
      }

      return profilerResults;
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

    private void GetTargetMethods(bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, out bool hasProfiledInstanceMethods, out MethodInfo instanceProviderMethodInfo)
    {
      hasProfiledInstanceMethods = false;
      instanceProviderMethodInfo = null;
      foreach (MethodInfo methodInfo in this.Configuration.Type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        bool isMethodStatic = methodInfo.IsStatic;
        if (isFindInstanceProviderEnabled
          && instanceProviderMethodInfo == null
          && isMethodStatic
          && methodInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute)) != null
          && this.Configuration.Type.IsAssignableFrom(methodInfo.ReturnType))
        {
          instanceProviderMethodInfo = methodInfo;
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

        string asseblyName = this.Configuration.TypeAssembly.GetName().Name;
        var profiledMethodInfo = new ProfiledMethodInfo(argumentLists, methodInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isMethodStatic);
        targetMembers.Add(profiledMethodInfo);
      }
    }

    private void GetTargetConstructors(bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, out ConstructorInfo instanceProviderConstructorInfo)
    {
      instanceProviderConstructorInfo = null;
      foreach (ConstructorInfo constructorInfo in this.Configuration.Type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        bool isConstructorStatic = constructorInfo.IsStatic;
        if (isFindInstanceProviderEnabled
          && instanceProviderConstructorInfo == null
          && !isConstructorStatic
          && constructorInfo.GetParameters().Length == 0)
        {
          instanceProviderConstructorInfo = constructorInfo;
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

        string asseblyName = this.Configuration.TypeAssembly.GetName().Name;
        ProfiledConstructorInfo profiledConstructorInfo = new ProfiledConstructorInfo(argumentLists, constructorInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isConstructorStatic);
        targetMembers.Add(profiledConstructorInfo);
      }
    }

    private void GetTargetProperties(bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, out bool hasProfiledInstanceProperties, out PropertyInfo instanceProviderPropertyInfo)
    {
      hasProfiledInstanceProperties = false;
      instanceProviderPropertyInfo = null;
      foreach (PropertyInfo propertyInfo in this.Configuration.Type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        bool isPropertyStatic = propertyInfo.GetGetMethod(true)?.IsStatic ?? false;

        if (isFindInstanceProviderEnabled
          && instanceProviderPropertyInfo == null
          && isPropertyStatic
          && propertyInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute)) != null
          && this.Configuration.Type.IsAssignableFrom(propertyInfo.PropertyType))
        {
          instanceProviderPropertyInfo = propertyInfo;
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

        string asseblyName = this.Configuration.TypeAssembly.GetName().Name;
        var profiledPropertyInfo = new ProfiledPropertyInfo(argumentLists, propertyInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isPropertyIndexer, isPropertyStatic);
        targetMembers.Add(profiledPropertyInfo);
      }
    }

    private FieldInfo GetInstanceProviderField()
    {
      foreach (FieldInfo fieldInfo in this.Configuration.Type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (fieldInfo.IsStatic
          && fieldInfo.GetCustomAttribute(typeof(ProfilerFactoryAttribute)) != null
          && this.Configuration.Type.IsAssignableFrom(fieldInfo.FieldType))
        {
          return fieldInfo;
        }
      }

      return null;
    }
  }  
}