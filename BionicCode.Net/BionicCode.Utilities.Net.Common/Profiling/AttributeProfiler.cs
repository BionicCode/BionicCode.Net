﻿namespace BionicCode.Utilities.Net
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

    private async Task<ProfilerBatchResultGroupCollection> ProfileType(TypeData typeDataToProfile, CancellationToken cancellationToken)
    {
      object targetInstance = null;
      bool hasProfiledInstanceMembers = false;
      bool isInstanceProviderMemberInfoRequired = !typeDataToProfile.IsAbstract;
      var targetMembers = new List<ProfiledMemberInfo>();
      GetTargetMethods(typeDataToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceMethods, out InstanceProviderInfo instanceProviderMethodInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceMethods;
      if (isInstanceProviderMemberInfoRequired && instanceProviderMethodInfo != null)
      {
        isInstanceProviderMemberInfoRequired = false;
        targetInstance = ((MethodInfo)instanceProviderMethodInfo?.MemberInfo).Invoke(null, instanceProviderMethodInfo.ArgumentList);
      }

      GetTargetProperties(typeDataToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out bool hasProfiledInstanceProperties, out InstanceProviderInfo instanceProviderPropertyInfo);
      hasProfiledInstanceMembers |= hasProfiledInstanceProperties;
      if (isInstanceProviderMemberInfoRequired && instanceProviderPropertyInfo != null)
      {
        isInstanceProviderMemberInfoRequired = false;
        targetInstance = ((PropertyInfo)instanceProviderPropertyInfo.MemberInfo).GetValue(null, null);
      }

      if (isInstanceProviderMemberInfoRequired)
      {
        InstanceProviderInfo instanceProviderFieldInfo = GetInstanceProviderField(typeDataToProfile, cancellationToken);
        isInstanceProviderMemberInfoRequired = instanceProviderFieldInfo == null;
        targetInstance = ((FieldInfo)instanceProviderFieldInfo?.MemberInfo)?.GetValue(null);
      }

      GetTargetConstructors(typeDataToProfile, isInstanceProviderMemberInfoRequired, targetMembers, cancellationToken, out InstanceProviderInfo instanceProviderConstructorInfo);

      // We don't need an instance if we have an abstract (abstract, static) type
      // or the profiled members are only constructors.
      bool isTargetInstanceRequired = !typeDataToProfile.IsAbstract
        && hasProfiledInstanceMembers;

      if (isTargetInstanceRequired && targetInstance == null)
      {
        targetInstance = ((ConstructorInfo)instanceProviderConstructorInfo?.MemberInfo)?.Invoke(instanceProviderConstructorInfo.ArgumentList);
      }

      if (isTargetInstanceRequired && targetInstance == null)
      {
        throw new MissingMemberException(ExceptionMessages.GetMissingCreationMemberOfProfiledTypeExceptionMessage(typeDataToProfile));
      }

      cancellationToken.ThrowIfCancellationRequested();

      return await ProfileMembersAsync(targetMembers, targetInstance, typeDataToProfile, cancellationToken);
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

        //string fullSignatureName = memberInfo.MemberInfoData.Signature;

        var memberResultGroup = new ProfilerBatchResultGroup();
        for (int argumentListIndex = 0; argumentListIndex < memberInfo.ArgumentLists.Count(); argumentListIndex++)
        {
          cancellationToken.ThrowIfCancellationRequested();

          object[] argumentList = memberInfo.ArgumentLists[argumentListIndex].ToArray();
          if (memberInfo is ProfiledMethodInfo method)
          {
            ProfilerTargetInvokeInfo invocationInfo;
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
              : Profiler.LogTimeInternal(context);

            //ProfiledTargetType targetType = method.MethodName.StartsWith("set_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 1
            //  ? ProfiledTargetType.PropertySet
            //  : method.MethodName.StartsWith("get_", StringComparison.Ordinal) && method.MethodInfo.GetParameters().Length == 0
            //    ? ProfiledTargetType.PropertyGet
            //    : ProfiledTargetType.Method;

            if (memberResultGroup.IsEmpty())
            {
              memberResultGroup.ProfiledTargetType = invocationInfo.ProfiledTargetType;
              memberResultGroup.ProfiledTargetSignatureMemberName = memberInfo.Signature;
              memberResultGroup.ProfiledTargetMemberShortName = memberInfo.DisplayName;
              result.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResultGroup.Add(result);
            }
            else
            {
              memberResultGroup.First().Combine(result);
            }
          }
          else if (memberInfo is ProfiledConstructorInfo constructor)
          {
            var invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argumentList, argumentListIndex, constructor.Signature, constructor.DisplayName, constructor.Namespace, constructor.AssemblyName, constructor.ConstructorData.GetInvocator(), ProfiledTargetType.Constructor);
            var context = new ProfilerContext(invocationInfo, constructor.SourceFilePath, constructor.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, constructor.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
            ProfilerBatchResult result = Profiler.LogTimeInternal(context);

            if (memberResultGroup.IsEmpty())
            {
              memberResultGroup.ProfiledTargetType = invocationInfo.ProfiledTargetType;
              memberResultGroup.ProfiledTargetSignatureMemberName = constructor.Signature;
              memberResultGroup.ProfiledTargetMemberShortName = constructor.DisplayName;
              result.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
              memberResultGroup.Add(result);
            }
            else
            {
              memberResultGroup.First().Combine(result);
            }
          }
          else if (memberInfo is ProfiledPropertyInfo property)
          {
            ProfilerTargetInvokeInfo invocationInfo;
            if (property.PropertyData.CanRead)
            {
              invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argumentList, argumentListIndex, property.Signature, property.DisplayName, property.Namespace, property.AssemblyName, property.PropertyData.GetGetInvocator(), ProfiledTargetType.PropertyGet);
              var context = new ProfilerContext(invocationInfo, property.SourceFilePath, property.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, property.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
              ProfilerBatchResult propertyGetResult = Profiler.LogTimeInternal(context);
              propertyGetResult.Index = 0;

              if (memberResultGroup.IsEmpty())
              {
                memberResultGroup.ProfiledTargetType = invocationInfo.ProfiledTargetType;
                memberResultGroup.ProfiledTargetSignatureMemberName = property.Signature;
                memberResultGroup.ProfiledTargetMemberShortName = property.DisplayName;
                propertyGetResult.ArgumentListCount = argumentList is VoidArgumentList ? 0 : memberInfo.ArgumentLists.Count();
                memberResultGroup.Add(propertyGetResult);
              }
              else
              {
                memberResultGroup[0].Combine(propertyGetResult);
              }
            }

            if (property.PropertyData.CanWrite)
            {
              invocationInfo = new ProfilerTargetInvokeInfo(profiledInstance, argumentList, argumentListIndex, property.Signature, property.DisplayName, property.Namespace, property.AssemblyName, property.PropertyData.GetSetInvocator(), ProfiledTargetType.PropertySet);
              var context = new ProfilerContext(invocationInfo, property.SourceFilePath, property.LineNumber, this.Configuration.WarmupIterations, this.Configuration.Iterations, property.TargetFramework, this.Configuration.BaseUnit, this.Configuration.ProfilerLogger, this.Configuration.AsyncProfilerLogger);
              ProfilerBatchResult propertySetResult = Profiler.LogTimeInternal(context);
              propertySetResult.Index = 1;

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

    private void GetTargetMethods(TypeData typeDataToProfile, bool isFindInstanceProviderEnabled, in IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out bool hasProfiledInstanceMethods, out InstanceProviderInfo instanceProviderMethodInfo)
    {
      hasProfiledInstanceMethods = false;
      instanceProviderMethodInfo = null;
      bool hasHighPriorityInstanceProvider = false;

      foreach (MethodData methodData in typeDataToProfile.MethodsData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        MethodInfo methodInfo = methodData.GetMethodInfo();
        bool isMethodStatic = methodData.IsStatic;
        bool isFactoryMethod = isMethodStatic && typeDataToProfile.GetType().IsAssignableFrom(methodData.ReturnTypeData.GetType());
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

            instanceProviderMethodInfo = new InstanceProviderInfo(argumentList, methodData.get);
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

          IList<IEnumerable<object>> argumentLists = new List<IEnumerable<object>> { new VoidArgumentList() };
          int methodParameterCount = methodData.Parameters.Length;
          bool isMethodParameterless = methodParameterCount == 0;
          if (!isMethodParameterless)
          {
            argumentLists = methodInfo.GetCustomAttributes<ProfilerArgumentAttribute>(false)
              .Select(attribute => attribute.Arguments)
              .ToList();
            if (argumentLists.IsEmpty())
            {
              string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Method(methodParameterCount);
              throw new MissingProfilerArgumentAttributeException(exceptionMessage);
            }
          }

          string assemblyName = typeDataToProfile.AssemblyName;
          var profiledMethodInfo = new ProfiledMethodInfo(argumentLists, methodData, profileAttribute.SourceFilePath, profileAttribute.LineNumber, assemblyName, profileAttribute.TargetFramework, isMethodStatic);
          targetMembers.Add(profiledMethodInfo);
        }
      }
    }

    private void GetTargetConstructors(TypeData typeDataToProfile, bool isFindInstanceProviderEnabled, IList<ProfiledMemberInfo> targetMembers, CancellationToken cancellationToken, out InstanceProviderInfo instanceProviderConstructorInfo)
    {
      instanceProviderConstructorInfo = null;
      bool hasHighPriorityInstanceProvider = false;
      foreach (ConstructorData constructorData in typeDataToProfile.ConstructorsData)
      {
        cancellationToken.ThrowIfCancellationRequested();

        bool isConstructorStatic = constructorData.IsStatic;
        if (isFindInstanceProviderEnabled
          && !isConstructorStatic)
        {
          if ((instanceProviderConstructorInfo == null && constructorData.Parameters.Length == 0)
            || !hasHighPriorityInstanceProvider)
          {
            ProfilerFactoryAttribute profilerFactoryAttribute = constructorData.GetConstructorInfo().GetCustomAttribute<ProfilerFactoryAttribute>(false);
            hasHighPriorityInstanceProvider = profilerFactoryAttribute != null;
            object[] argumentList = hasHighPriorityInstanceProvider
              ? profilerFactoryAttribute.ArgumentList
              : Array.Empty<object>();

            instanceProviderConstructorInfo = new InstanceProviderInfo(argumentList, constructorData.GetInvocator());
          }
        }

        IEnumerable<ProfileAttribute> profileAttributes = constructorData.GetCustomAttributes(typeof(ProfileAttribute))
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
          int constructorParameterCount = constructorData.GetParameters().Length;
          bool isConstructorParameterless = constructorParameterCount == 0;
          if (!isConstructorParameterless)
          {
            argumentLists = constructorData.GetCustomAttributes(typeof(ProfilerArgumentAttribute), false)
              .Select(attribute => ((ProfilerArgumentAttribute)attribute).Arguments);
            if (argumentLists.IsEmpty())
            {
              string exceptionMessage = ExceptionMessages.GetMissingProfiledArgumentAttributeExceptionMessage_Constructor(constructorParameterCount);
              throw new MissingProfilerArgumentAttributeException(exceptionMessage);
            }
          }

          string assemblyName = this.Configuration.GetAssembly(typeDataToProfile).GetName().Name;
          ProfiledConstructorInfo profiledConstructorInfo = new ProfiledConstructorInfo(argumentLists, constructorData, profileAttribute.SourceFilePath, profileAttribute.LineNumber, assemblyName, profileAttribute.TargetFramework, isConstructorStatic);
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