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

        public static Delegate Generate<TEventSource>(string eventName, object target, string targetDelegateMethodName, ParameterList targetDelegateMethodParameterList)
        {
            ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
            ArgumentNullExceptionEx.ThrowIfNull(target, nameof(target));
            ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(targetDelegateMethodName, nameof(targetDelegateMethodName));

            Type targetType = target.GetType();
            SymbolInfoDataCacheKey symbolCacheKey = SymbolInfoDataCacheKey.CreateForAnonymousMethodOrProperty(targetType.TypeHandle, targetDelegateMethodName, targetDelegateMethodParameterList, targetDelegateMethodParameterList.GenericTypeParameterCount, SymbolKind.MemberMethod);
            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(symbolCacheKey, out MethodData proxyDelegateMethodData);

            symbolCacheKey = SymbolInfoDataCacheKey.CreateForAnonymousFieldOrEvent(typeof(TEventSource).TypeHandle, eventName, SymbolKind.MemberEvent);
            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(symbolCacheKey, out EventData eventData);

            TypeData eventHandlerTypeData = eventData.EventHandlerTypeData;
            MethodData invocatorData = eventData.InvocatorMethodData;
            ParameterList eventHandlerParameters = invocatorData.Parameters;
            Delegate eventHandler = GenerateProxy(eventHandlerParameters, eventHandlerTypeData, target, proxyDelegateMethodData);
            LogDebug("Dynamically generated proxy event handler.");

            return eventHandler;
        }

        private static Delegate GenerateProxy(ParameterList eventHandlerParameters, TypeData eventDelegateTypeData, object delegateMethodTarget, MethodData delegateMethodData)
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
