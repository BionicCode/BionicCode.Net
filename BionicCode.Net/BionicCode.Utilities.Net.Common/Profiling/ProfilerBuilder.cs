namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading.Tasks;

  /// <summary>
  /// Class to configure the <see cref="Profiler"/> when used with annotated profiling. Call <see cref="Profiler.LogType{T}"/> to obtain an instance.
  /// </summary>
  public class ProfilerBuilder : IAttributeProfilerConfiguration
  {
    private Type Type { get; }
    private bool IsWarmUpEnabled { get; set; }
    private bool IsDefaultLogOutputEnabled { get; set; }
    private int Iterations { get; set; }
    private int WarmUpIterations { get; set; }
    private ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; set; }
    private ProfilerLoggerDelegate ProfilerLogger { get; set; }

    Type IAttributeProfilerConfiguration.Type => this.Type;
    bool IAttributeProfilerConfiguration.IsWarmupEnabled => this.IsWarmUpEnabled;
    bool IAttributeProfilerConfiguration.IsDefaultLogOutputEnabled => this.IsDefaultLogOutputEnabled;
    int IAttributeProfilerConfiguration.Iterations => this.Iterations;
    int IAttributeProfilerConfiguration.WarmUpIterations => this.WarmUpIterations;
    ProfilerLoggerAsyncDelegate IAttributeProfilerConfiguration.AsyncProfilerLogger => this.AsyncProfilerLogger;
    ProfilerLoggerDelegate IAttributeProfilerConfiguration.ProfilerLogger => this.ProfilerLogger;

    internal ProfilerBuilder(Type targetType)
    {
      this.Type = targetType;
      this.IsWarmUpEnabled = true;
      this.IsDefaultLogOutputEnabled = true;
      this.WarmUpIterations = Profiler.WarmUpCount;
      this.Iterations = 1;
    }

    /// <summary>
    /// Set a log delegate that allows to output the result to a sink, e.g. a file or application logger.
    /// </summary>
    /// <param name="iterations">The number of iterations to perform when executing the target code. The default is <c>1</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetIterations(int iterations)
    {
      this.Iterations = iterations;
      return this;
    }

    /// <summary>
    /// Set a log delegate that allows to output the result to a sink, e.g. a file or application logger.
    /// </summary>
    /// <param name="profilerLogger">A delegate that is invoked by profiler to pass in the result.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetLogger(ProfilerLoggerDelegate profilerLogger)
    {
      this.ProfilerLogger = profilerLogger;
      return this;
    }

    /// <summary>
    /// Set a log delegate that allows to asynchronously output the result to a sink, e.g. a file or application logger.
    /// </summary>
    /// <param name="profilerLogger">An asynchronous delegate that is invoked by profiler to pass in the result.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetAsyncLogger(ProfilerLoggerAsyncDelegate profilerLogger)
    {
      this.AsyncProfilerLogger = profilerLogger;
      return this;
    }

    /// <summary>
    /// Enable warm up iterations to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <param name="warmUpIterations">The number of iterations to perform before starting the profiling. The default is <c>4</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder EnableWarmUp(int warmUpIterations = 4)
    {
      this.WarmUpIterations = warmUpIterations;
      this.IsWarmUpEnabled = true;
      return this;
    }

    /// <summary>
    /// Disable warm up iterations. Warm up iterations can be scheduled to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <returns>The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.</returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder DisableWarmUp()
    {
      this.IsWarmUpEnabled = false;
      return this;
    }

    /// <summary>
    /// Enable default log output. The default is enabled.
    /// </summary>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>Enables to print the profiler results to a HTML document that will be automatically displayed in the default browser. 
    /// <br/>A second output sink is the default Output console window of Visual Studio, but only when in debug mode.
    /// </remarks>
    public ProfilerBuilder EnableDefaultLogOutput()
    {
      this.IsDefaultLogOutputEnabled = true;
      return this;
    }

    /// <summary>
    /// Disable default log output.
    /// </summary>
    /// <returns>The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.</returns>
    public ProfilerBuilder DisableDefaultLogOutput()
    {
      this.IsDefaultLogOutputEnabled = false;
      return this;
    }

    /// <summary>
    /// Enable warm up iterations to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <param name="warmUpIterations">The number of iterations to perform before starting the profiling. The default is <c>4</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder SetWarmUpIterations(int warmUpIterations)
    {
      this.WarmUpIterations = warmUpIterations;
      return this;
    }

    /// <summary>
    /// Execute the profiler using the current configuration.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ProfilerBatchResult"/> items where each <see cref="ProfilerBatchResult"/> holds the result of a particular profiled target to accumulate the individual <see cref="ProfilerResult"/> items for each iteration.
    /// </returns>
    public async Task<IEnumerable<ProfilerBatchResult>> RunAsync()
    {
      var attributeProfiler = new AttributeProfiler(this);
      IEnumerable<ProfilerBatchResult> result = await attributeProfiler.StartAsync();
      return result;
    }
  }

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
      var results = new List<ProfilerBatchResult>();
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
        ? throw new MissingMemberException(@$"Unable to create an instance of the type {this.Configuration.Type.FullName} that declares the profiled member.
{System.Environment.NewLine} Please provide a parameterless constructor. Alternatively, provide a factory member that returns a properly initialized instance of the declaring owner. That member must be static and decorated with the '{nameof(ProfilerFactoryAttribute)}'. Additionally that member must be either a parameterless method, a parameterless Func delegate, a property or field. The access modifier (e.g. private, internal, public) is not constrained.")
        : await ProfileMembersAsync(targetMembers, targetInstance);
    }

    private async Task<IEnumerable<ProfilerBatchResult>> ProfileMembersAsync(IEnumerable<ProfiledMemberInfo> memberInfos, object profiledInstance)
    {
      var profilerResults = new List<ProfilerBatchResult>();
      foreach (ProfiledMemberInfo memberInfo in memberInfos)
      {
        bool memberIsMethod = memberInfo is ProfiledMethodInfo;
        bool memberIsConstructor = memberInfo is ProfiledConstructorInfo;
        bool memberIsProperty = memberInfo is ProfiledPropertyInfo;

        ProfiledMethodInfo method = memberInfo as ProfiledMethodInfo;
        ProfiledConstructorInfo constructor = memberInfo as ProfiledConstructorInfo;
        ProfiledPropertyInfo property = memberInfo as ProfiledPropertyInfo;

        var memberResults = new List<ProfilerBatchResult>();
        foreach (IEnumerable<object> argumentList in memberInfo.ArgumentLists)
        {
          string fullSignatureName = string.Empty;
          //TODO::Check for generic method and construct generic based on parameter position and parameter type
          //var parameterForGenericParameterPosition = method.GetGenericMethodDefinition().GetParameters().First(parameterInfo => parameterInfo.ParameterType.Name == method.GetGenericMethodDefinition().GetGenericArguments()[0].Name);

          CreateTargetMemberDelegate(profiledInstance, memberInfo, argumentList);
          if (memberIsMethod)
          {
            fullSignatureName = CreateMemberSignatureName(method.MethodInfo);
            ProfilerBatchResult result = null;
            if (method.IsAsync)
            {
              result = await Profiler.LogTimeAsync(method.AsyncMethodDelegate.Invoke, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, method.SourceFilePath, method.LineNumber);
            }
            else
            {
              result = Profiler.LogTime(method.MethodDelegate.Invoke, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, method.SourceFilePath, method.LineNumber);
            }

            var context = new ProfilerContext(method.AssemblyName, fullSignatureName, ProfiledTargetType.Method, method.SourceFilePath, method.LineNumber);
            result.Context = context;

            if (memberResults.IsEmpty())
            {
              memberResults.Add(result);
            }
            else
            {
              memberResults.First().Combine(result);
            }
          }
          else if (memberIsConstructor)
          {
            ProfilerBatchResult result = null;
            fullSignatureName = CreateMemberSignatureName(constructor.ConstructorInfo);
            result = Profiler.LogTime(constructor.ConstructorDelegate, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, constructor.SourceFilePath, lineNumber: constructor.LineNumber);
            var context = new ProfilerContext(constructor.AssemblyName, fullSignatureName, ProfiledTargetType.Constructor, constructor.SourceFilePath, constructor.LineNumber);
            result.Context = context;

            if (memberResults.IsEmpty())
            {
              memberResults.Add(result);
            }
            else
            {
              memberResults.First().Combine(result);
            }
          }
          else if (memberIsProperty)
          {
            fullSignatureName = CreateMemberSignatureName(property.PropertyInfo);

            ProfilerBatchResult propertyGetResult = Profiler.LogTime(property.GetDelegate, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, property.SourceFilePath, lineNumber: property.LineNumber);
            var context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertyGet, property.SourceFilePath, property.LineNumber);
            propertyGetResult.Context = context;
            propertyGetResult.Index = 1;

            if (memberResults.IsEmpty())
            {
              memberResults.Add(propertyGetResult);
            }
            else
            {
              memberResults[0].Combine(propertyGetResult);
            }

            ProfilerBatchResult propertySetResult = Profiler.LogTime(property.SetDelegate, this.Configuration.WarmUpIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, property.SourceFilePath, lineNumber: property.LineNumber); 
            context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertySet, property.SourceFilePath, property.LineNumber);
            propertySetResult.Context = context;
            propertySetResult.Index = 2;

            if (memberResults.Count == 1)
            {
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
            await logger.LogAsync(memberResults, string.Empty);
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
          MethodInfo genericMethodInfo = profiledMethodInfo.MethodInfo.MakeGenericMethod(genericTypeArguments);
          if (profiledMethodInfo.IsAsync)
          {
            if (profiledMethodInfo.IsAsyncTask)
            {
              asyncMethod = () => (Task)genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
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

        IEnumerable<IEnumerable<object>> argumentLists = methodInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
          .Select(attribute => ((ProfilerArgumentAttribute)attribute).Arguments);
        var assemblyOfTargetType = Assembly.GetAssembly(this.Configuration.Type);
        string asseblyName = assemblyOfTargetType.GetName().Name;
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

        IEnumerable<IEnumerable<object>> argumentLists = constructorInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
          .Select(attribute => ((ProfilerArgumentAttribute)attribute).Arguments);
        var assemblyOfTargetType = Assembly.GetAssembly(this.Configuration.Type);
        string asseblyName = assemblyOfTargetType.GetName().Name;
        var profiledMethodInfo = new ProfiledConstructorInfo(argumentLists, constructorInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isConstructorStatic);
        targetMembers.Add(profiledMethodInfo);
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

        var profileAttribute = (ProfileAttribute) propertyInfo.GetCustomAttribute(typeof(ProfileAttribute));
        if (profileAttribute == null)
        {
          continue;
        }

        hasProfiledInstanceProperties |= !isPropertyStatic;

        bool isPropertyIndexer = propertyInfo.GetIndexParameters().Length > 0;
        IEnumerable<IEnumerable<object>> argumentLists = isPropertyIndexer 
          ? propertyInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
            .Cast<ProfilerArgumentAttribute>()
            .Select(argumentAttribute => new List<object> { new KeyValuePair<object, object>(argumentAttribute.Index, argumentAttribute.Arguments.FirstOrDefault()) }) 
          : propertyInfo.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
            .Cast<ProfilerArgumentAttribute>()
            .Select(attribute => attribute.Arguments);
        var assemblyOfTargetType = Assembly.GetAssembly(this.Configuration.Type);
        string asseblyName = assemblyOfTargetType.GetName().Name;
        var profiledMethodInfo = new ProfiledPropertyInfo(argumentLists, propertyInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, asseblyName, isPropertyIndexer, isPropertyStatic);
        targetMembers.Add(profiledMethodInfo);
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