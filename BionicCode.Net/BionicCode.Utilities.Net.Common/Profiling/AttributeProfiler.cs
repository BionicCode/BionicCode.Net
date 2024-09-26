namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.Profiling;
  using BionicCode.Utilities.Net.Profiling.Ipc;

  internal class ExecutionContext : IDisposable
  {
    public bool IsRunning => (!this.serverConnection?.IsDisposed ?? false) || (this.serverConnection?.IsConnected ?? false) && this.isRunningInternal;
    public Process Process { get; private set; }
    public Runtime Runtime { get; }
    public event EventHandler ContextClosed;
    private FileInfo executableFileInfo;
    private PipeServerConnection serverConnection;
    private const string ExecutableFilePathTemplate = "../../../Profiling/ContextExecutables/{0}/runtime.exe";
    private IpcProcessArgument processArgument;
    private string processArgumentJsonText;
    private bool isRunningInternal;
    private bool disposedValue;

    internal ExecutionContext(Runtime runtime)
    {
      this.Runtime = runtime;
    }

    public async Task<bool> TryStartAsync(CancellationToken cancellationToken)
    {
      if (this.IsRunning)
      {
        return false;
      }

      if (!this.IsRunning)
      {
        await InitializeConnectionContextAsync();
      }

      var startInfo = new ProcessStartInfo()
      {
        CreateNoWindow = true,
        FileName = this.executableFileInfo.FullName,
        Arguments = processArgumentJsonText,
        UseShellExecute = false,
        //RedirectStandardOutput = true,
        RedirectStandardError = true,
      };

      this.Process = new Process
      {
        StartInfo = startInfo,
        EnableRaisingEvents = true
      };
      this.Process.Exited += OnProcessExited;
      this.Process.ErrorDataReceived += OnProcessErrorReceived;
      
      bool isProcessRunning =  this.Process.Start();
      if (isProcessRunning)
      {
        this.isRunningInternal = await this.serverConnection.TryConnectAsync(cancellationToken);
      }

      return this.IsRunning;
    }

    public void Close()
    {
      // Disconnects and disposes the server connection
      this.serverConnection.Close();
      this.isRunningInternal = false;
    }

    public async Task<ProfiledTypeResultCollection> ProfileTargetTypesInRuntimeContext(IAttributeProfilerConfiguration configuration, CancellationToken cancellationToken)
    {
      if (!this.IsRunning && !await TryStartAsync(cancellationToken))
      {
        throw new InvalidOperationException("Pipe server is not connected.");
      }

      var message = new ProfilerContextMessage(configuration);
      IPipeMessage<ProfilerContextMessage> ipcMessage = this.serverConnection.CreateNewConversation(MessageType.ReceiveRequest, message);
      await this.serverConnection.WriteToPipeAsync(ipcMessage).ConfigureAwait(false);
      IPipeMessage<ProfilerResultMessage> response = await this.serverConnection.ReadFromPipeAsync<ProfilerResultMessage>().ConfigureAwait(false);
      
      return response.IsValid && response.HasData && response.Id == ipcMessage.Id
        ? response.Data.Results
        : new ProfiledTypeResultCollection();
    }

    private async Task InitializeConnectionContextAsync()
    {
      var serverClientLinkId = Guid.NewGuid();
      this.serverConnection = new PipeServerConnection(serverClientLinkId);
      this.processArgument = new IpcProcessArgument(this.serverConnection.PipeId, this.serverConnection.ServerClientLinkId);
      
      string processArgumentJson = await this.processArgument.ToJsonAsync().ConfigureAwait(false);
      this.processArgumentJsonText = processArgumentJson.Replace(@"""", @"\""");

      this.executableFileInfo = CreateExecutableFilePath();
    }

    private FileInfo CreateExecutableFilePath()
    {
      if (this.executableFileInfo is null)
      {
        string directoryName = this.Runtime.ToString().Replace('_', '.');
        string filePath = string.Format(ExecutionContext.ExecutableFilePathTemplate, directoryName);
        this.executableFileInfo = new FileInfo(filePath);
      }

      return this.executableFileInfo;
    }

    private void OnProcessErrorReceived(object sender, DataReceivedEventArgs e) 
      => throw new ProfilerExecutionRuntimeContextExeceptionException($"Process exited with the following error message: {e.Data}.");

    private void OnProcessExited(object sender, EventArgs e) 
      => OnContextClosed();

    private void OnContextClosed()
      => this.ContextClosed?.Invoke(this, EventArgs.Empty);

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null

        this.Process.Dispose();

        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ExecutionContext()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }

  internal class AttributeProfiler
  {
    private IAttributeProfilerConfiguration Configuration { get; }
    private IEnumerable<IProfilerLogger> DefaultLoggers { get; }
    private Dictionary<Runtime, ExecutionContext> ExecutionContexts { get; }

    public AttributeProfiler(IAttributeProfilerConfiguration configuration)
    {
      this.Configuration = configuration;
      this.DefaultLoggers = new List<IProfilerLogger>() { new HtmlLogger() };
      this.ExecutionContexts = new Dictionary<Runtime, ExecutionContext>();
    }

    internal async Task<ProfiledTypeResultCollection> StartAsync(CancellationToken cancellationToken)
    {
      var typeResults = new ProfiledTypeResultCollection();
      foreach (Type typeToProfile in this.Configuration.Types)
      {
        cancellationToken.ThrowIfCancellationRequested();

        ProfilerBatchResultGroupCollection typeMemberResults = await ProfileType(typeToProfile, cancellationToken);
        typeResults.Add(typeMemberResults);
      }

      if (typeResults.Any() && this.Configuration.IsDefaultLogOutputEnabled)
      {
        foreach (IProfilerLogger logger in this.DefaultLoggers)
        {
          await logger.LogAsync(typeResults, cancellationToken);
        }
      }

      foreach (KeyValuePair<Runtime, ExecutionContext> entry in this.ExecutionContexts)
      {
        ExecutionContext executionContext = entry.Value;
        executionContext.Close();
      }

      return typeResults;
    }

    private async Task<ProfilerBatchResultGroupCollection> ProfileType(Type typeToProfile, CancellationToken cancellationToken)
    {
      object targetInstance = null;
      bool hasProfiledInstanceMembers = false;
      bool isInstanceProviderMemberInfoRequired = !typeToProfile.IsAbstract;
      var targetMembers = new List<ProfiledMemberInfo>();
      GetTargetMethods(typeToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceMethods, out InstanceProviderInfo instanceProviderMethodInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceMethods;
      if (isInstanceProviderMemberInfoRequired && instanceProviderMethodInfo != null)
      {
        isInstanceProviderMemberInfoRequired = false;
        targetInstance = ((MethodInfo)instanceProviderMethodInfo?.MemberInfo).Invoke(null, instanceProviderMethodInfo.ArgumentList);
      }

      GetTargetProperties(typeToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceProperties, out InstanceProviderInfo instanceProviderPropertyInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceProperties;
      if (isInstanceProviderMemberInfoRequired && instanceProviderPropertyInfo != null)
      {
        isInstanceProviderMemberInfoRequired = false;
        targetInstance = ((PropertyInfo)instanceProviderPropertyInfo.MemberInfo).GetValue(null, null);
      }

      if (isInstanceProviderMemberInfoRequired)
      {
        InstanceProviderInfo instanceProviderFieldInfo = GetInstanceProviderField(typeToProfile, cancellationToken);
        isInstanceProviderMemberInfoRequired = instanceProviderFieldInfo == null;
        targetInstance = ((FieldInfo)instanceProviderFieldInfo?.MemberInfo)?.GetValue(null);
      }

      GetTargetConstructors(typeToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out InstanceProviderInfo instanceProviderConstructorInfo);

      // We don't need an instance if we have an abstract (abstract, static) type
      // or the profiled members are only constructors.
      bool isTargetInstanceRequired = !typeToProfile.IsAbstract
        && hasProfiledInstanceMembers;

      if (isTargetInstanceRequired && targetInstance == null)
      {
        targetInstance = ((ConstructorInfo)instanceProviderConstructorInfo?.MemberInfo)?.Invoke(instanceProviderConstructorInfo.ArgumentList);
      }

      if (isTargetInstanceRequired && targetInstance == null)
      {
        throw new MissingMemberException(ExceptionMessages.GetMissingCreationMemberOfProfiledTypeExceptionMessage(typeToProfile));
      }

      cancellationToken.ThrowIfCancellationRequested();

      return await ProfileMembersAsync(targetMembers, targetInstance, typeToProfile, cancellationToken);
    }

    protected async Task<ProfilerBatchResultGroupCollection> ProfileMembersAsync(IEnumerable<ProfiledMemberInfo> memberInfos, object profiledInstance, Type typeToProfile, CancellationToken cancellationToken)
    {
      var resultGroups = new ProfilerBatchResultGroupCollection(typeToProfile);
      foreach (ProfiledMemberInfo memberInfo in memberInfos)
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (memberInfo.TargetFramework != this.Configuration.Runtime)
        {
          if (!this.ExecutionContexts.TryGetValue(memberInfo.TargetFramework, out ExecutionContext executionContext))
          {
            executionContext = new ExecutionContext(memberInfo.TargetFramework);
            this.ExecutionContexts.Add(memberInfo.TargetFramework, executionContext);
          }

          if (!executionContext.IsRunning)
          {
            if (!await executionContext.TryStartAsync(cancellationToken))
            {
              throw new InvalidOperationException($"Unable to start profiler context for {memberInfo.TargetFramework} runtime.");
            }
          }

          var profilerConfiguration = new RuntimeContextProfilerConfiguration(memberInfo.TargetFramework, new[] { typeToProfile }, this.Configuration);
          ProfiledTypeResultCollection resultCollection = await executionContext.ProfileTargetTypesInRuntimeContext(profilerConfiguration, cancellationToken);
          IEnumerable<ProfilerBatchResultGroup> externalContextResultGroups = resultCollection.SelectMany(groupCollection => groupCollection);
          foreach (ProfilerBatchResultGroup resultGroup in externalContextResultGroups)
          {
            resultGroups.Add(resultGroup);
          }
        }

        string fullSignatureName = memberInfo.MemberInfoData.Signature;
        bool memberIsConstructor = memberInfo is ProfiledConstructorInfo constructor;
        bool memberIsProperty = memberInfo is ProfiledPropertyInfo property;

        var memberResultGroup = new ProfilerBatchResultGroup();
        for (int argumentListIndex = 0; argumentListIndex < memberInfo.ArgumentLists.Count(); argumentListIndex++)
        {
          cancellationToken.ThrowIfCancellationRequested();

          object[] argumentList = memberInfo.ArgumentLists[argumentListIndex].ToArray();
          ProfilerTargetInvokeInfo invocationInfo;
          if (memberInfo is ProfiledMethodInfo method)
          {
            if (method.IsAwaitable)
            {
              if (method.IsAwaitableTask)
              {
                invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argumentList, argumentListIndex, method.Signature, method.DisplayName, method.Namespace, method.AssemblyName, method.MethodData.GetAwaitableTaskInvocator(), ProfiledTargetType.Method);
              }
              else if (method.IsAwaitableValueTask)
              {
                invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argumentList, argumentListIndex, method.Signature, method.DisplayName, method.Namespace, method.AssemblyName, method.MethodData.GetAwaitableValueTaskInvocator(), ProfiledTargetType.Method);
              }
              else
              {
                invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, method.Signature, method.DisplayName, method.Namespace, method.AssemblyName, argumentList, argumentListIndex, method.MethodData.GetAwaitableGenericValueTaskInvocator(), ProfiledTargetType.Method);
              }
            }
            else
            {
              invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argumentList, argumentListIndex, method.Signature, method.DisplayName, method.Namespace, method.AssemblyName, method.MethodData.GetInvocator(), ProfiledTargetType.Method);

            }

            var context = new ProfilerContext(invocationInfo, method.SourceFilePath, method.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, method.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
            ProfilerBatchResult result = method.IsAwaitable
              ? await Profiler.LogTimeAsyncInternal(context)
              : Profiler.LogTimeInternal(invocationInfo, this.Configuration.WarmupIterations, this.Configuration.Iterations, this.Configuration.ProfilerLogger, method.SourceFilePath, method.LineNumber);
            ProfiledTargetType targetType = method.MethodName.StartsWith("set_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 1
              ? ProfiledTargetType.PropertySet
              : method.MethodName.StartsWith("get_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 0
                ? ProfiledTargetType.PropertyGet
                : ProfiledTargetType.Method;

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
            var context = new ProfilerContext(constructor.AssemblyName, fullSignatureName, ProfiledTargetType.Constructor, constructor.SourceFilePath, constructor.LineNumber, memberInfo.MemberInfoData, this.Configuration.WarmupIterations, memberInfo.TargetFramework);
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
            var context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertyGet, property.SourceFilePath, property.LineNumber, memberInfo.MemberInfoData, this.Configuration.WarmupIterations, memberInfo.TargetFramework);
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
            context = new ProfilerContext(property.AssemblyName, fullSignatureName, ProfiledTargetType.PropertySet, property.SourceFilePath, property.LineNumber, memberInfo.MemberInfoData, this.Configuration.WarmupIterations, memberInfo.TargetFramework);
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

    //private void CreateTargetMemberDelegate(object targetInstance, ProfiledMemberInfo profiledMemberInfo, IEnumerable<object> argumentList)
    //{
    //  if (profiledMemberInfo.IsStatic)
    //  {
    //    targetInstance = null;
    //  }

    //  if (profiledMemberInfo is ProfiledConstructorInfo profiledConstructorInfo)
    //  {
    //    Action constructor = () => profiledConstructorInfo.ConstructorInfo.Invoke(argumentList.ToArray());
    //    profiledConstructorInfo.ConstructorDelegate = constructor;

    //    return;
    //  }

    //  if (profiledMemberInfo is ProfiledMethodInfo profiledMethodInfo)
    //  {
    //    Action method = null;
    //    Func<Task> asyncTaskMethod = null;
    //    Func<dynamic> asyncValueTaskMethod = null;
    //    //if (profiledMethodInfo.IsGeneric)
    //    //{
    //    //  Type[] genericTypeArguments = profiledMethodInfo.MethodInfo.GetGenericArguments();
    //    //  ParameterInfo[] methodParameters = profiledMethodInfo.MethodInfo.GetParameters();
    //    //  var genericParameterTypes = new Type[genericTypeArguments.Length];
    //    //  int genericParameterTypesIndex = 0;
    //    //  foreach (Type genericTypeArgument in genericTypeArguments)
    //    //  {
    //    //    for (int genericParameterIndex = 0; genericParameterIndex < methodParameters.Length; genericParameterIndex++)
    //    //    {
    //    //      ParameterInfo parameterAtCurrentPosition = methodParameters[genericParameterIndex];
    //    //      if (parameterAtCurrentPosition.ParameterType.Name.Equals(genericTypeArgument.Name, StringComparison.Ordinal))
    //    //      {
    //    //        object argumentForGenericParameterPosition = argumentList.ElementAt(genericParameterIndex);
    //    //        genericParameterTypes[genericParameterTypesIndex++] = argumentForGenericParameterPosition.GetType();
    //    //      }
    //    //    }
    //    //  }

    //    //  MethodInfo genericMethodInfo = profiledMethodInfo.MethodInfo.MakeGenericMethod(genericParameterTypes);
    //    //  if (profiledMethodInfo.IsAwaitable)
    //    //  {
    //    //    if (profiledMethodInfo.IsAwaitableTask)
    //    //    {
    //    //      asyncMethod = async () =>
    //    //      {
    //    //        try
    //    //        {
    //    //          await (Task)genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
    //    //        }
    //    //        catch (ArgumentException e)
    //    //        {

    //    //          throw new ProfilerArgumentException(ExceptionMessages.GetArgumentListMismatchExceptionMessage(), e);
    //    //        }
    //    //      };
    //    //    }
    //    //    else
    //    //    {
    //    //      asyncMethod = async () => await (ValueTask)genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
    //    //    }
    //    //  }
    //    //  else
    //    //  {
    //    //    method = () => genericMethodInfo.Invoke(targetInstance, argumentList.ToArray());
    //    //  }
    //    //}
    //    //else
    //    if (profiledMethodInfo.IsAwaitable)
    //    {
    //      if (profiledMethodInfo.IsAwaitableTask)
    //      {
    //        asyncTaskMethod = () => (Task)profiledMethodInfo.MethodData.Invoke(targetInstance, argumentList.ToArray());
    //      }
    //      else
    //      {
    //        asyncTaskMethod = async () => await (ValueTask)profiledMethodInfo.MethodInfo.Invoke(targetInstance, argumentList.ToArray());
    //      }
    //    }
    //    else
    //    {
    //      method = () => profiledMethodInfo.MethodInfo.Invoke(targetInstance, argumentList.ToArray());
    //    }

    //    profiledMethodInfo.MethodDelegate = method;
    //    profiledMethodInfo.AsyncMethodDelegate = asyncTaskMethod;

    //    return;
    //  }

    //  if (profiledMemberInfo is ProfiledPropertyInfo profiledPropertyInfo)
    //  {
    //    MethodInfo getMethodInfo = profiledPropertyInfo.PropertyInfo.GetGetMethod(true);
    //    MethodInfo setMethodInfo = profiledPropertyInfo.PropertyInfo.GetSetMethod(true);
    //    Action setMethod = null;
    //    Action getMethod = null;

    //    if (profiledPropertyInfo.IsIndexer)
    //    {
    //      var argumentInfo = (KeyValuePair<object, object>)argumentList.FirstOrDefault();
    //      setMethod = () => profiledPropertyInfo.PropertyInfo.SetValue(targetInstance, argumentInfo.Value, new[] { argumentInfo.Key });
    //      getMethod = () => profiledPropertyInfo.PropertyInfo.GetValue(targetInstance, new[] { argumentInfo.Key });
    //    }
    //    else
    //    {
    //      setMethod = () => profiledPropertyInfo.PropertyInfo.SetValue(targetInstance, argumentList.ElementAtOrDefault(0));
    //      getMethod = () => profiledPropertyInfo.PropertyInfo.GetValue(targetInstance);
    //    }

    //    profiledPropertyInfo.SetDelegate = setMethod;
    //    profiledPropertyInfo.GetDelegate = getMethod;
    //  }
    //}

    private void GetTargetMethods(Type typeToProfile, bool isFindInstanceProviderEnabled, in IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out bool hasProfiledInstanceMethods, out InstanceProviderInfo instanceProviderMethodInfo)
    {
      hasProfiledInstanceMethods = false;
      instanceProviderMethodInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      TypeData typeToProfileData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeToProfile);
      foreach (MethodData methodData in typeToProfileData.MethodsData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        MethodInfo methodInfo = methodData.GetMethodInfo();
        bool isMethodStatic = methodData.IsStatic;
        bool isFactoryMethod = isMethodStatic && typeToProfile.IsAssignableFrom(methodData.ReturnTypeData.GetType());
        if (isFindInstanceProviderEnabled
          && isFactoryMethod)
        {
          if (instanceProviderMethodInfo == null || !hasHighPriorityInstanceProvider)
          {
            ProfilerFactoryAttribute profilerFactoryAttribute = methodInfo.GetCustomAttribute<ProfilerFactoryAttribute>();
            hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
            object[] argumentList = hasHighPriorityInstanceProvider
              ? profilerFactoryAttribute.ArgumentList
              : Array.Empty<object>();

            instanceProviderMethodInfo = new InstanceProviderInfo(argumentList, methodData);
          }
        }

        IEnumerable<ProfileAttribute> profileAttributes = methodInfo.GetCustomAttributes<ProfileAttribute>();
        foreach (ProfileAttribute profileAttribute in profileAttributes)
        {
          cancellationToken.ThrowIfCancellationRequested();

          if (profileAttribute == null
          || (profileAttribute.TargetFramework != Runtime.Current && profileAttribute.TargetFramework != Runtime.Default && profileAttribute.TargetFramework != this.Configuration.Runtime))
          {
            continue;
          }

          hasProfiledInstanceMethods |= !isMethodStatic;

          IEnumerable<IEnumerable<object>> argumentLists = new List<IEnumerable<object>> { new VoidArgumentList() };
          int methodParameterCount = methodData.Parameters.Length;
          bool isMethodParameterless = methodParameterCount == 0;
          if (!isMethodParameterless)
          {
            argumentLists = methodInfo.GetCustomAttributes<ProfilerArgumentAttribute>(false)
              .Select(attribute => attribute.Arguments);
            if (argumentLists.IsEmpty())
            {
              string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Method(methodParameterCount);
              throw new MissingProfilerArgumentAttributeException(exceptionMessage);
            }
          }

          string assemblyName = this.Configuration.GetAssembly(typeToProfile).GetName().Name;
          var profiledMethodInfo = new ProfiledMethodInfo(argumentLists, methodData, profileAttribute.SourceFilePath, profileAttribute.LineNumber, assemblyName, profileAttribute.TargetFramework, isMethodStatic);
          targetMembers.Add(profiledMethodInfo);
        }
      }
    }

    private void GetTargetConstructors(Type typeToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out InstanceProviderInfo instanceProviderConstructorInfo)
    {
      instanceProviderConstructorInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      foreach (ConstructorInfo constructorInfo in typeToProfile.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        cancellationToken.ThrowIfCancellationRequested();

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

        IEnumerable<ProfileAttribute> profileAttributes = constructorInfo.GetCustomAttributes(typeof(ProfileAttribute))
          .Cast<ProfileAttribute>();
        foreach (ProfileAttribute profileAttribute in profileAttributes)
        {
          cancellationToken.ThrowIfCancellationRequested();

          if (profileAttribute == null
            || (profileAttribute.TargetFramework != Runtime.Current && profileAttribute.TargetFramework != Runtime.Default && profileAttribute.TargetFramework != this.Configuration.Runtime))
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

          string assemblyName = this.Configuration.GetAssembly(typeToProfile).GetName().Name;
          ProfiledConstructorInfo profiledConstructorInfo = new ProfiledConstructorInfo(argumentLists, constructorInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, assemblyName, profileAttribute.TargetFramework, isConstructorStatic);
          targetMembers.Add(profiledConstructorInfo);
        }
      }
    }

    private void GetTargetProperties(Type typeToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out bool hasProfiledInstanceProperties, out InstanceProviderInfo instanceProviderPropertyInfo)
    {
      hasProfiledInstanceProperties = false;
      instanceProviderPropertyInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      foreach (PropertyInfo propertyInfo in typeToProfile.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        cancellationToken.ThrowIfCancellationRequested();

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

        IEnumerable<ProfileAttribute> profileAttributes = propertyInfo.GetCustomAttributes(typeof(ProfileAttribute))
          .Cast<ProfileAttribute>();
        foreach (ProfileAttribute profileAttribute in profileAttributes)
        {
          cancellationToken.ThrowIfCancellationRequested();

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

          string assemblyName = this.Configuration.GetAssembly(typeToProfile).GetName().Name;
          var profiledPropertyInfo = new ProfiledPropertyInfo(argumentLists, propertyInfo, profileAttribute.SourceFilePath, profileAttribute.LineNumber, assemblyName, profileAttribute.TargetFramework, isPropertyIndexer, isPropertyStatic);
          targetMembers.Add(profiledPropertyInfo);
        }
      }
    }

    private InstanceProviderInfo GetInstanceProviderField(Type typeToProfile, CancellationToken cancellationToken)
    {
      bool hasHighPriorityInstanceProvider = false;
      FieldInfo instanceProviderFieldInfo = null;
      foreach (FieldInfo fieldInfo in typeToProfile.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        cancellationToken.ThrowIfCancellationRequested();

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