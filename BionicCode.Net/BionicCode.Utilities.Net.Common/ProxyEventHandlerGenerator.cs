namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;

  internal static class ProxyEventHandlerGenerator
  {
    public static readonly object ConflictingMethodInfoExceptionDataKey = new object();

    public static Delegate Generate<TEventSource>(string eventName, object target, string targetDelegateMethodName, MemberParameterInfo[] targetDelegateMethodParameterList)
    {
      ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(target, nameof(target));
      ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(targetDelegateMethodName, nameof(targetDelegateMethodName));

      Type targetType = target.GetType();
      IMemberDataCacheKey symbolCacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(targetType.TypeHandle, targetDelegateMethodName, targetDelegateMethodParameterList);
      if (!SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(symbolCacheKey, out MethodData proxyDelegateMethodData))
      {
        throw new ArgumentException($"The provided method {targetDelegateMethodName} could not be found on the targetType {targetType.FullName}. Please verify the parameter list, the method name and the declaring targetType.");
      }

      symbolCacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(typeof(TEventSource).TypeHandle, eventName);
      if (!SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(symbolCacheKey, out EventData eventData))
      {
        throw new ArgumentException($"Unable to find event '{eventName}' on targetType {typeof(TEventSource).FullName}. The provided event name must specify an event that must be public, protected (including inherited members) or private and defined on the current TEventSource {typeof(TEventSource).FullName}.", nameof(eventName));
      }

      TypeData eventHandlerTypeData = eventData.EventHandlerTypeData;
      MethodData invocatorData = eventData.InvocatorMethodData;
      ParameterData[] eventHandlerParameters = invocatorData.Parameters;
      Delegate eventHandler = GenerateProxy(eventHandlerParameters, eventHandlerTypeData, target, proxyDelegateMethodData);
      LogDebug("Dynamically generated proxy event handler.");

      return eventHandler;
    }

    private static Delegate GenerateProxy(ParameterData[] eventHandlerParameters, TypeData eventDelegateTypeData, object delegateMethodTarget, MethodData delegateMethodData)
    {
      var expressionParameters = new List<ParameterExpression>();
      foreach (ParameterData parameter in eventHandlerParameters)
      {
        ParameterExpression expressionParameter = Expression.Parameter(parameter.ParameterTypeData.GetType(), parameter.Name);
        expressionParameters.Add(expressionParameter);
      }

      List<Expression> delegateParameters = expressionParameters.Cast<Expression>().ToList();
      if (delegateMethodData.Parameters.Any())
      {
        ParameterData lastParameter = delegateMethodData.Parameters.Last();
        if (lastParameter.IsParams)
        {
          IEnumerable<Expression> paramsParameterArguments = expressionParameters.Skip(lastParameter.GetParameterInfo().Position)
                        .Select(parameter => Expression.TypeAs(parameter, lastParameter.ParameterTypeData.GetType().GetElementType()))
                        .Cast<Expression>();
          NewArrayExpression argsArray = Expression.NewArrayInit(typeof(object), paramsParameterArguments);
          delegateParameters.RemoveRange(lastParameter.Position, expressionParameters.Count - lastParameter.Position);
          delegateParameters.Add(argsArray);
        }
      }

      ConstantExpression target = Expression.Constant(delegateMethodTarget);
      MethodInfo proxyDelegateMethod = delegateMethodData.GetMethodInfo();
      MethodCallExpression method;
      try
      {
        method = Expression.Call(target, proxyDelegateMethod, delegateParameters);
      }
      catch (ArgumentException e)
      {
        e.Data.Add(ProxyEventHandlerGenerator.ConflictingMethodInfoExceptionDataKey, proxyDelegateMethod);
        throw;
      }

      Type eventDelegateType = eventDelegateTypeData.GetType();
      Delegate proxyEventSourceHandler = Expression.Lambda(eventDelegateType, method, expressionParameters).Compile();

      return proxyEventSourceHandler;
    }

    private static void LogDebug(string message)
    {
#if DEBUG
      Debug.WriteLine($"{message}");
#endif
    }
  }
}