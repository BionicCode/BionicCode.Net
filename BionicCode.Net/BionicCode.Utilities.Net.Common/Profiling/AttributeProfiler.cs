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
      if (this.Configuration.IsAutoDiscoverEnabled)
      {
        DiscoverTargetTypes(this.Configuration.AutoDiscoverSourceAssemblies, this.Configuration.TypeData);
      }

      foreach (TypeData typeDataToProfile in this.Configuration.TypeData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        ProfilerBatchResultGroupCollection typeMemberResults = await ProfileType(typeDataToProfile, cancellationToken);
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

    private void DiscoverTargetTypes(Assembly[] targetAssemblies, HashSet<TypeData> discoveredTypes)
    {
      if (targetAssemblies.Length == 0)
      {
        targetAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      }

      foreach (Assembly assembly in targetAssemblies)
      {
        Type[] typesInAssembly = assembly.GetTypes();
        for (int index = 0; index < typesInAssembly.Length; index++)
        {
          Type type = typesInAssembly[index];
          ProfilerAutoDiscoverAttribute attribute = type.GetCustomAttribute<ProfilerAutoDiscoverAttribute>();
          if (attribute != null)
          {
            if ((type.IsGenericTypeDefinition || type.ContainsGenericParameters))
            {
              if (attribute.GenericTypeParameters.Length == 0)
              {
                throw new ProfilerConfigurationException(ExceptionMessages.GetMissingGenericTypeArgumentsForAutoDiscoveredGenericTypeExceptionMessage(type));
              }

              type = type.MakeGenericType(attribute.GenericTypeParameters);
            }

            TypeData targetTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            _ = discoveredTypes.Add(targetTypeData);
          }
        }
      }
    }

    private async Task<ProfilerBatchResultGroupCollection> ProfileType(TypeData typeDataToProfile, CancellationToken cancellationToken)
    {
      bool hasProfiledInstanceMembers = false;
      bool isInstanceProviderMemberInfoRequired = !typeDataToProfile.IsAbstract;
      var targetMembers = new List<ProfiledMemberInfo>();
      bool hasHighPriorityInstanceProvider = false;
      InstanceProviderInfo instanceProviderInfo = null;
      GetTargetMethods(typeDataToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceMethods, ref hasHighPriorityInstanceProvider, ref instanceProviderInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceMethods;
      if (isInstanceProviderMemberInfoRequired && hasHighPriorityInstanceProvider)
      {
        isInstanceProviderMemberInfoRequired = false;
      }

      GetTargetProperties(typeDataToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceProperties, ref hasHighPriorityInstanceProvider, ref instanceProviderInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceProperties;
      if (isInstanceProviderMemberInfoRequired && hasHighPriorityInstanceProvider)
      {
        isInstanceProviderMemberInfoRequired = false;
      }

      GetTargetConstructors(typeDataToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceConstructors, ref hasHighPriorityInstanceProvider, ref instanceProviderInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceConstructors;

      if (isInstanceProviderMemberInfoRequired && hasProfiledInstanceMembers)
      {
        GetInstanceProviderField(typeDataToProfile, cancellationToken, ref hasHighPriorityInstanceProvider, ref instanceProviderInfo);
      }

      // We don't need an instance if we have an abstract (abstract, static) type
      // or the profiled members are only constructors.
      bool isTargetInstanceRequired = !typeDataToProfile.IsAbstract
        && hasProfiledInstanceMembers;

      object targetInstance = null;
      if (isTargetInstanceRequired)
      {
        if (instanceProviderInfo is null)
        {
          throw new MissingMemberException(ExceptionMessages.GetMissingCreationMemberOfProfiledTypeExceptionMessage(typeDataToProfile));
        }
        else
        {
          targetInstance = instanceProviderInfo.IsAwaitable ? await instanceProviderInfo.CreateTargetInstanceAsync(null) : instanceProviderInfo.CreateTargetInstance(null);
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      return await ProfileMembersAsync(targetMembers, targetInstance, typeDataToProfile, cancellationToken);
    }

    protected async Task<ProfilerBatchResultGroupCollection> ProfileMembersAsync(IEnumerable<ProfiledMemberInfo> memberInfos, object profiledInstance, TypeData typeDataToProfile, CancellationToken cancellationToken)
    {
      var resultGroups = new ProfilerBatchResultGroupCollection(typeDataToProfile);
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

          var profilerConfiguration = new RuntimeContextProfilerConfiguration(memberInfo.TargetFramework, new[] { typeDataToProfile }, this.Configuration);
          ProfiledTypeResultCollection resultCollection = await executionContext.ProfileTargetTypesInRuntimeContext(profilerConfiguration, cancellationToken);
          IEnumerable<ProfilerBatchResultGroup> externalContextResultGroups = resultCollection.SelectMany(groupCollection => groupCollection);
          foreach (ProfilerBatchResultGroup resultGroup in externalContextResultGroups)
          {
            resultGroups.Add(resultGroup);
          }
        }

        var memberResultGroup = new ProfilerBatchResultGroup
        {
          TargetSignature = memberInfo.Signature,
          TargetShortSignature = memberInfo.ShortSignature,
          TargetShortCompactSignature = memberInfo.ShortCompactSignature,
          TargetName = memberInfo.DisplayName,
          TargetShortName = memberInfo.ShortDisplayName
        };

        if (memberInfo is ProfiledMethodInfo method)
        {
          for (int argumentListIndex = 0; argumentListIndex < method.ArgumentInfo.Count; argumentListIndex++)
          {
            cancellationToken.ThrowIfCancellationRequested();

            MethodArgumentInfo arguments = method.ArgumentInfo[argumentListIndex];
            ProfilerTargetInvokeInfo invocationInfo;
            if (method.IsAwaitable)
            {
              if (method.IsAwaitableTask)
              {
                invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, arguments, method.Signature, method.DisplayName, method.ShortSignature, method.ShortDisplayName, method.MethodData.SymbolComponentInfo, method.Namespace, method.AssemblyName, asynchronousTaskMethodInvocator: method.MethodData.GetAwaitableTaskInvocator(), ProfiledTargetType.Method);
              }
              else if (method.IsAwaitableValueTask)
              {
                invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, arguments, method.Signature, method.DisplayName, method.ShortSignature, method.ShortDisplayName, method.MethodData.SymbolComponentInfo, method.Namespace, method.AssemblyName, asynchronousValueTaskMethodInvocator: method.MethodData.GetAwaitableValueTaskInvocator(), ProfiledTargetType.Method);
              }
              else
              {
                invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, method.Signature, method.DisplayName, method.ShortSignature, method.ShortDisplayName, method.MethodData.SymbolComponentInfo, method.Namespace, method.AssemblyName, arguments, asynchronousGenericValueTaskMethodInvocator: method.MethodData.GetAwaitableGenericValueTaskInvocator(), ProfiledTargetType.Method);
              }
            }
            else
            {
              invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, arguments, method.Signature, method.DisplayName, method.ShortSignature, method.ShortDisplayName, method.MethodData.SymbolComponentInfo, method.Namespace, method.AssemblyName, synchronousMethodInvocator: method.MethodData.GetInvocator(), ProfiledTargetType.Method);
            }

            var context = new ProfilerContext(invocationInfo, method.SourceFilePath, method.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, method.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
            ProfilerBatchResult result = await Profiler.LogTimeInternalAsync(context);

            if (memberResultGroup.IsEmpty())
            {
              result.ArgumentListCount = method.ArgumentInfo.Count;
              memberResultGroup.TargetType = invocationInfo.ProfiledTargetType;
              memberResultGroup.Add(result);
            }
            else
            {
              memberResultGroup.First().Combine(result);
            }
          }
        }
        else if (memberInfo is ProfiledConstructorInfo constructor)
        {
          for (int argumentListIndex = 0; argumentListIndex < constructor.ArgumentInfo.Count; argumentListIndex++)
          {
            cancellationToken.ThrowIfCancellationRequested();

            MethodArgumentInfo arguments = constructor.ArgumentInfo[argumentListIndex];
            var invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, arguments, constructor.Signature, constructor.DisplayName, constructor.ShortSignature, constructor.ShortDisplayName, constructor.ConstructorData.SymbolComponentInfo, constructor.Namespace, constructor.AssemblyName, constructorInvocator: constructor.ConstructorData.GetInvocator(), ProfiledTargetType.Constructor);
            var context = new ProfilerContext(invocationInfo, constructor.SourceFilePath, constructor.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, constructor.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
            ProfilerBatchResult result = await Profiler.LogTimeInternalAsync(context);

            if (memberResultGroup.IsEmpty())
            {
              result.ArgumentListCount = constructor.ArgumentInfo.Count;
              memberResultGroup.TargetType = invocationInfo.ProfiledTargetType;
              memberResultGroup.Add(result);
            }
            else
            {
              memberResultGroup.First().Combine(result);
            }
          }
        }
        else if (memberInfo is ProfiledPropertyInfo property)
        {
          for (int argumentIndex = 0; argumentIndex < property.Arguments.Count; argumentIndex++)
          {
            cancellationToken.ThrowIfCancellationRequested();

            PropertyArgumentInfo argument = property.Arguments[argumentIndex];
            ProfilerTargetInvokeInfo invocationInfo;
            if (property.PropertyData.CanRead && (argument.Accessor & PropertyAccessor.Get) != 0)
            {
              ProfiledTargetType targetType = property.IsIndexer 
                ? ProfiledTargetType.IndexerGet 
                : ProfiledTargetType.PropertyGet;

              invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argument, property.Signature, property.DisplayName, property.ShortSignature, property.ShortDisplayName, property.PropertyData.SymbolComponentInfo, property.Namespace, property.AssemblyName, propertyGetInvocator: property.PropertyData.GetGetInvocator(), targetType);
              var context = new ProfilerContext(invocationInfo, property.SourceFilePath, property.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, property.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
              ProfilerBatchResult propertyGetResult = await Profiler.LogTimeInternalAsync(context);
              propertyGetResult.Index = 0;

              if (memberResultGroup.IsEmpty())
              {
                propertyGetResult.ArgumentListCount = property.Arguments.Count;
                memberResultGroup.TargetType = invocationInfo.ProfiledTargetType;
                memberResultGroup.Add(propertyGetResult);
              }
              else
              {
                memberResultGroup[0].Combine(propertyGetResult);
              }
            }

            if (property.PropertyData.CanWrite && (argument.Accessor & PropertyAccessor.Set) != 0)
            {
              ProfiledTargetType targetType = property.IsIndexer
                ? ProfiledTargetType.IndexerSet
                : ProfiledTargetType.PropertySet;

              invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argument, property.Signature, property.DisplayName, property.ShortSignature, property.ShortDisplayName, property.PropertyData.SymbolComponentInfo, property.Namespace, property.AssemblyName, propertySetInvocator: property.PropertyData.GetSetInvocator(), targetType);
              var context = new ProfilerContext(invocationInfo, property.SourceFilePath, property.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, property.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
              ProfilerBatchResult propertySetResult = await Profiler.LogTimeInternalAsync(context);
              propertySetResult.Index = 1;

              if (memberResultGroup.Count == 1)
              {
                propertySetResult.ArgumentListCount = property.Arguments.Count;
                memberResultGroup.Add(propertySetResult);
              }
              else
              {
                memberResultGroup[1].Combine(propertySetResult);
              }
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

    private void GetTargetMethods(TypeData typeDataToProfile, bool isFindInstanceProviderEnabled, in IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out bool hasProfiledInstanceMethods, ref bool hasHighPriorityInstanceProvider, ref InstanceProviderInfo instanceProviderInfo)
    {
      hasProfiledInstanceMethods = false;

      foreach (MethodData methodData in typeDataToProfile.MethodsData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        MethodInfo methodInfo = methodData.GetMethodInfo();
        bool isMethodStatic = methodData.IsStatic;
        bool isFactoryMethod = isMethodStatic && typeDataToProfile.GetType().IsAssignableFrom(methodData.ReturnTypeData.GetType());
        if (isFindInstanceProviderEnabled
          && !hasHighPriorityInstanceProvider
          && isFactoryMethod)
        {
          ProfilerFactoryAttribute profilerFactoryAttribute = methodInfo.GetCustomAttribute<ProfilerFactoryAttribute>(false);
          hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
          object[] argumentList = hasHighPriorityInstanceProvider
            ? profilerFactoryAttribute.ArgumentList
            : Array.Empty<object>();

          if (hasHighPriorityInstanceProvider || instanceProviderInfo is null)
          {
            instanceProviderInfo = new InstanceProviderInfo(methodData, argumentList);
          }
        }

        IEnumerable<ProfileAttribute> profileAttributes = methodInfo.GetCustomAttributes<ProfileAttribute>(false);
        foreach (ProfileAttribute profileAttribute in profileAttributes)
        {
          cancellationToken.ThrowIfCancellationRequested();

          if (profileAttribute == null
            || (profileAttribute.TargetFramework != Runtime.Current && profileAttribute.TargetFramework != Runtime.Default && profileAttribute.TargetFramework != this.Configuration.Runtime))
          {
            continue;
          }

          hasProfiledInstanceMethods |= !isMethodStatic;

          var argumentLists = new List<MethodArgumentInfo>();
          int methodParameterCount = methodData.Parameters.Length;
          bool isMethodParameterless = methodParameterCount == 0;
          if (!isMethodParameterless)
          {
            IEnumerable<ProfilerMethodArgumentAttribute> argumentAttributes = methodInfo.GetCustomAttributes<ProfilerMethodArgumentAttribute>(false);
            int argumentListIndex = 0;
            foreach (ProfilerMethodArgumentAttribute attribute in argumentAttributes)
            {
              var argumentInfo = new MethodArgumentInfo(attribute.Arguments.ToList(), argumentListIndex++);
              argumentLists.Add(argumentInfo);
            }

            if (argumentLists.IsEmpty())
            {
              string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Method(methodParameterCount);
              throw new ProfilerConfigurationException(exceptionMessage);
            }
          }

          var profiledMethodInfo = new ProfiledMethodInfo(argumentLists, methodData, profileAttribute.SourceFilePath, profileAttribute.LineNumber, typeDataToProfile.AssemblyName, profileAttribute.TargetFramework, isMethodStatic);
          targetMembers.Add(profiledMethodInfo);
        }
      }
    }

    private void GetTargetConstructors(TypeData typeDataToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out bool hasProfiledInstanceConstructors, ref bool hasHighPriorityInstanceProvider, ref InstanceProviderInfo instanceProviderInfo)
    {
      hasProfiledInstanceConstructors = false;

      foreach (ConstructorData constructorData in typeDataToProfile.ConstructorsData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        ConstructorInfo constructorInfo = constructorData.GetConstructorInfo();
        bool isConstructorStatic = constructorData.IsStatic;
        if (isFindInstanceProviderEnabled
          && !hasHighPriorityInstanceProvider
          && !isConstructorStatic)
        {
          ProfilerFactoryAttribute profilerFactoryAttribute = constructorInfo.GetCustomAttribute<ProfilerFactoryAttribute>(false);
          hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
          object[] argumentList = hasHighPriorityInstanceProvider
            ? profilerFactoryAttribute.ArgumentList
            : Array.Empty<object>();

          if (hasHighPriorityInstanceProvider || (instanceProviderInfo is null && constructorData.Parameters.Length == 0))
          {
            instanceProviderInfo = new InstanceProviderInfo(constructorData, argumentList);
          }
        }

        IEnumerable<ProfileAttribute> profileAttributes = constructorInfo.GetCustomAttributes<ProfileAttribute>(false);
        foreach (ProfileAttribute profileAttribute in profileAttributes)
        {
          cancellationToken.ThrowIfCancellationRequested();

          if (profileAttribute == null
            || (profileAttribute.TargetFramework != Runtime.Current && profileAttribute.TargetFramework != Runtime.Default && profileAttribute.TargetFramework != this.Configuration.Runtime))
          {
            continue;
          }

          hasProfiledInstanceConstructors |= !constructorData.IsStatic;

          var argumentLists = new List<MethodArgumentInfo>();
          int constructorParameterCount = constructorData.Parameters.Length;
          bool isConstructorParameterless = constructorParameterCount == 0;
          if (!isConstructorParameterless)
          {
            IEnumerable<ProfilerMethodArgumentAttribute> argumentAttributes = constructorInfo.GetCustomAttributes<ProfilerMethodArgumentAttribute>(false);
            int argumentListIndex = 0;
            foreach (ProfilerMethodArgumentAttribute attribute in argumentAttributes)
            {
              var argumentInfo = new MethodArgumentInfo(attribute.Arguments.ToList(), argumentListIndex++);
              argumentLists.Add(argumentInfo);
            }

            if (argumentLists.IsEmpty())
            {
              string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Constructor(constructorParameterCount);
              throw new ProfilerConfigurationException(exceptionMessage);
            }
          }

          ProfiledConstructorInfo profiledConstructorInfo = new ProfiledConstructorInfo(argumentLists, constructorData, profileAttribute.SourceFilePath, profileAttribute.LineNumber, typeDataToProfile.AssemblyName, profileAttribute.TargetFramework, isConstructorStatic);
          targetMembers.Add(profiledConstructorInfo);
        }
      }
    }

    private void GetTargetProperties(TypeData typeDataToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out bool hasProfiledInstanceProperties, ref bool hasHighPriorityInstanceProvider, ref InstanceProviderInfo instanceProviderInfo)
    {
      hasProfiledInstanceProperties = false;
      foreach (PropertyData propertyData in typeDataToProfile.PropertiesData)
      {
        cancellationToken.ThrowIfCancellationRequested();
        PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
        bool isPropertyStatic = propertyData.IsStatic;
        bool isPropertyIndexer = propertyData.IsIndexer;
        if (isFindInstanceProviderEnabled
          && !hasHighPriorityInstanceProvider
          && isPropertyStatic
          && propertyData.CanRead
          && typeDataToProfile.GetType().IsAssignableFrom(propertyData.PropertyTypeData.GetType()))
        {
            ProfilerFactoryAttribute profilerFactoryAttribute = propertyInfo.GetCustomAttribute<ProfilerFactoryAttribute>(false);
            hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
            object[] argumentList = hasHighPriorityInstanceProvider && isPropertyIndexer
              ? profilerFactoryAttribute.ArgumentList
              : Array.Empty<object>();
           
          if (hasHighPriorityInstanceProvider || instanceProviderInfo is null)
          { 
            instanceProviderInfo = new InstanceProviderInfo(propertyData, argumentList);
          }
        }

        IEnumerable<ProfileAttribute> profileAttributes = propertyInfo.GetCustomAttributes<ProfileAttribute>(false);
        foreach (ProfileAttribute profileAttribute in profileAttributes)
        {
          cancellationToken.ThrowIfCancellationRequested();

          if (profileAttribute == null
            || (profileAttribute.TargetFramework != Runtime.Current && profileAttribute.TargetFramework != Runtime.Default && profileAttribute.TargetFramework != this.Configuration.Runtime))
          {
            continue;
          }

          hasProfiledInstanceProperties |= !isPropertyStatic;

          IList<PropertyArgumentInfo> argumentList = new List<PropertyArgumentInfo>();
          bool isRequiringArguments = isPropertyIndexer || propertyData.CanWrite;
          if (isRequiringArguments)
          {
            IEnumerable<ProfilerPropertyArgumentAttribute> argumentAttributes = propertyInfo.GetCustomAttributes<ProfilerPropertyArgumentAttribute>(false);
            int argumentListIndex = 0;
            foreach (ProfilerPropertyArgumentAttribute attribute in argumentAttributes)
            {
              var argumentInfo = new PropertyArgumentInfo(attribute.Value, attribute.Index, attribute.Accessor, argumentListIndex++);
              argumentList.Add(argumentInfo);
            }

            if (argumentList.IsEmpty())
            {
              string propertyExceptionMessage = isPropertyIndexer
                ? ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_IndexerProperty()
                : ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Property();

              throw new ProfilerConfigurationException(propertyExceptionMessage);
            }
          }

          var profiledPropertyInfo = new ProfiledPropertyInfo(argumentList, propertyData, profileAttribute.SourceFilePath, profileAttribute.LineNumber, typeDataToProfile.AssemblyName, profileAttribute.TargetFramework, isPropertyStatic);
          targetMembers.Add(profiledPropertyInfo);
        }
      }
    }

    private void GetInstanceProviderField(TypeData typeDataToProfile, CancellationToken cancellationToken, ref bool hasHighPriorityInstanceProvider, ref InstanceProviderInfo instanceProviderInfo)
    {
      foreach (FieldData fieldData in typeDataToProfile.FieldsData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        FieldInfo fieldInfo = fieldData.GetFieldInfo();
        if (fieldData.IsStatic
          && !hasHighPriorityInstanceProvider
          && typeDataToProfile.GetType().IsAssignableFrom(fieldData.FieldTypeData.GetType()))
        {
          hasHighPriorityInstanceProvider = fieldInfo.GetCustomAttribute<ProfilerFactoryAttribute>(false) != null;
          if (hasHighPriorityInstanceProvider || instanceProviderInfo is null)
          {
            instanceProviderInfo = new InstanceProviderInfo(fieldData, Array.Empty<object>());
            break;
          }

          if (hasHighPriorityInstanceProvider)
          {
            break;
          }
        }
      }
    }
  } 
}