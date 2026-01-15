namespace BionicCode.Utilities.Net.Profiling
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ProfilerTargetInvokeInfo
    {
        /// <summary>
        /// Use for property set()
        /// </summary>
        /// <param name="target"></param>
        /// <param name="methodArguments"></param>
        /// <param name="argumentListIndex"></param>
        /// <param name="targetSignature"></param>
        /// <param name="targetDisplayName"></param>
        /// <param name="targetNamespace"></param>
        /// <param name="targetAssemblyName"></param>
        /// <param name="propertySetInvocator"></param>
        /// <param name="profiledTargetType"></param>
        protected ProfilerTargetInvokeInfo(object? target,
          string targetSignature,
          string targetDisplayName,
          string targetShortSignature,
          string targetShortDisplayName,
          SymbolComponentInfo symbolComponentInfo,
          string targetNamespace,
          string targetAssemblyName,
          ProfiledTargetType profiledTargetType)
        {
            this.Target = target;
            this.ProfiledTargetType = profiledTargetType;
            this.Signature = targetSignature;
            this.AssemblyName = targetAssemblyName;
            this.DisplayName = targetDisplayName;
            this.Namespace = targetNamespace;
            this.ShortDisplayName = targetShortDisplayName;
            this.ShortSignature = targetShortSignature;
            this.SymbolComponentInfo = symbolComponentInfo;
        }

        public string Signature { get; }
        public string DisplayName { get; }
        public string ShortSignature { get; }
        public string ShortDisplayName { get; }
        public SymbolComponentInfo? SymbolComponentInfo { get; }
        public string Namespace { get; }
        public string AssemblyName { get; }
        public object? Target { get; }
        public ProfiledTargetType ProfiledTargetType { get; }
    }

    internal class ProfilerMethodInvokeInfo : ProfilerTargetInvokeInfo
    {
        #region Constructors

        private ProfilerMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, object?> synchronousMethodInvocator,
            ProfiledTargetType profiledTargetType) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                profiledTargetType)
        {
            ArgumentNullException.ThrowIfNull(synchronousMethodInvocator);
            this.SynchronousMethodInvoker = synchronousMethodInvocator;
            this.MethodArgument = methodArgument;
        }

        private ProfilerMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, Task> asynchronousTaskMethodInvocator,
            ProfiledTargetType profiledTargetType) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                profiledTargetType)
        {
            ArgumentNullException.ThrowIfNull(asynchronousTaskMethodInvocator);
            this.AsynchronousTaskMethodInvoker = asynchronousTaskMethodInvocator;
            this.MethodArgument = methodArgument;
        }

        private ProfilerMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, Task<object?>> asynchronousGenericTaskMethodInvocator,
            ProfiledTargetType profiledTargetType) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                profiledTargetType)
        {
            ArgumentNullException.ThrowIfNull(asynchronousGenericTaskMethodInvocator);
            this.AsynchronousGenericTaskMethodInvoker = asynchronousGenericTaskMethodInvocator;
            this.MethodArgument = methodArgument;
        }

        private ProfilerMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, ValueTask> asynchronousValueTaskMethodInvocator,
            ProfiledTargetType profiledTargetType) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                profiledTargetType)
        {
            ArgumentNullException.ThrowIfNull(asynchronousValueTaskMethodInvocator);
            this.AsynchronousValueTaskMethodInvoker = asynchronousValueTaskMethodInvocator;
            this.MethodArgument = methodArgument;
        }

        private ProfilerMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, ValueTask<object?>> asynchronousGenericValueTaskMethodInvocator,
            ProfiledTargetType profiledTargetType) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                profiledTargetType)
        {
            ArgumentNullException.ThrowIfNull(asynchronousGenericValueTaskMethodInvocator);
            this.AsynchronousGenericValueTaskMethodInvoker = asynchronousGenericValueTaskMethodInvocator;
            this.MethodArgument = methodArgument;
        }

        #endregion Constructors

        public static ProfilerMethodInvokeInfo CreateSynchronousMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, object?> synchronousMethodInvocator,
            ProfiledTargetType profiledTargetType)
        {
            return new ProfilerMethodInvokeInfo(target,
                methodArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                synchronousMethodInvocator,
                profiledTargetType);
        }

        public static ProfilerMethodInvokeInfo CreateAsynchronousTaskMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, Task> asynchronousTaskMethodInvocator,
            ProfiledTargetType profiledTargetType)
        {
            return new ProfilerMethodInvokeInfo(target,
                methodArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                asynchronousTaskMethodInvocator,
                profiledTargetType);
        }

        public static ProfilerMethodInvokeInfo CreateAsynchronousGenericTaskMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, Task<object?>> asynchronousGenericTaskMethodInvocator,
            ProfiledTargetType profiledTargetType)
        {
            return new ProfilerMethodInvokeInfo(target,
                methodArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                asynchronousGenericTaskMethodInvocator,
                profiledTargetType);
        }

        public static ProfilerMethodInvokeInfo CreateAsynchronousValueTaskMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, ValueTask> asynchronousValueTaskMethodInvocator,
            ProfiledTargetType profiledTargetType)
        {
            return new ProfilerMethodInvokeInfo(target,
                methodArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                asynchronousValueTaskMethodInvocator,
                profiledTargetType);
        }

        public static ProfilerMethodInvokeInfo CreateAsynchronousGenericValueTaskMethodInvokeInfo(object? target,
            MethodArgumentInfo methodArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, ValueTask<object?>> asynchronousGenericValueTaskMethodInvocator,
            ProfiledTargetType profiledTargetType)
        {
            return new ProfilerMethodInvokeInfo(target,
                methodArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                asynchronousGenericValueTaskMethodInvocator,
                profiledTargetType);
        }

        public Func<object?, object?[]?, object?>? SynchronousMethodInvoker { get; }
        public Func<object?, object?[]?, Task>? AsynchronousTaskMethodInvoker { get; }
        public Func<object?, object?[]?, Task<object?>>? AsynchronousGenericTaskMethodInvoker { get; }
        public Func<object?, object?[]?, ValueTask>? AsynchronousValueTaskMethodInvoker { get; }
        public Func<object?, object?[]?, ValueTask<object?>>? AsynchronousGenericValueTaskMethodInvoker { get; }
        public MethodArgumentInfo MethodArgument { get; }
    }

    internal class ProfilerConstructorInvokeInfo : ProfilerTargetInvokeInfo
    {
        /// <summary>
        /// Use for property set()
        /// </summary>
        /// <param name="target"></param>
        /// <param name="methodArguments"></param>
        /// <param name="argumentListIndex"></param>
        /// <param name="targetSignature"></param>
        /// <param name="targetDisplayName"></param>
        /// <param name="targetNamespace"></param>
        /// <param name="targetAssemblyName"></param>
        /// <param name="propertySetInvocator"></param>
        /// <param name="profiledTargetType"></param>
        private ProfilerConstructorInvokeInfo(object? target,
          MethodArgumentInfo methodArgument,
          string targetSignature,
          string targetDisplayName,
          string targetShortSignature,
          string targetShortDisplayName,
          SymbolComponentInfo symbolComponentInfo,
          string targetNamespace,
          string targetAssemblyName,
          Func<object?[]?, object> constructorInvocator,
          ProfiledTargetType profiledTargetType) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                profiledTargetType)
        {
            ArgumentNullException.ThrowIfNull(constructorInvocator);
            this.ConstructorInvoker = constructorInvocator;
            this.MethodArgument = methodArgument;
        }

        public static ProfilerConstructorInvokeInfo CreateConstructorInvokeInfo(object? target,
          MethodArgumentInfo methodArgument,
          string targetSignature,
          string targetDisplayName,
          string targetShortSignature,
          string targetShortDisplayName,
          SymbolComponentInfo symbolComponentInfo,
          string targetNamespace,
          string targetAssemblyName,
          Func<object?[]?, object> constructorInvocator,
          ProfiledTargetType profiledTargetType)
        {
            return new ProfilerConstructorInvokeInfo(target,
              methodArgument,
              targetSignature,
              targetDisplayName,
              targetShortSignature,
              targetShortDisplayName,
              symbolComponentInfo,
              targetNamespace,
              targetAssemblyName,
              constructorInvocator,
              profiledTargetType);
        }

        public Func<object?[]?, object>? ConstructorInvoker { get; }
        public MethodArgumentInfo MethodArgument { get; }
    }

    internal class ProfilerPropertyInvokeInfo : ProfilerTargetInvokeInfo
    {
        private ProfilerPropertyInvokeInfo(object? target,
            PropertyArgumentInfo propertyArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName) : base(target,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                ProfiledTargetType.PropertyGet)
        {
            this.PropertyArgument = propertyArgument;
        }

        public static ProfilerPropertyGetInvokeInfo CreatePropertyGetInvokeInfo(object? target,
            PropertyArgumentInfo propertyArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, object?> propertyGetInvocator)
        {
            return new ProfilerPropertyGetInvokeInfo(target,
                propertyArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                propertyGetInvocator);
        }

        public static ProfilerPropertySetInvokeInfo CreatePropertySetInvokeInfo(object target,
            PropertyArgumentInfo propertyArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Action<object?, object?, object?[]?> propertySetInvocator)
        {
            return new ProfilerPropertySetInvokeInfo(target,
                propertyArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName,
                propertySetInvocator);
        }

        public PropertyArgumentInfo PropertyArgument { get; }

        public class ProfilerPropertyGetInvokeInfo : ProfilerPropertyInvokeInfo
        {
            public ProfilerPropertyGetInvokeInfo(object? target,
                PropertyArgumentInfo propertyArgument,
                string targetSignature,
                string targetDisplayName,
                string targetShortSignature,
                string targetShortDisplayName,
                SymbolComponentInfo symbolComponentInfo,
                string targetNamespace,
                string targetAssemblyName,
                Func<object?, object?[]?, object?> propertyGetInvocator) : base(target,
                    propertyArgument,
                    targetSignature,
                    targetDisplayName,
                    targetShortSignature,
                    targetShortDisplayName,
                    symbolComponentInfo,
                    targetNamespace,
                    targetAssemblyName)
            {
                ArgumentNullException.ThrowIfNull(propertyGetInvocator);
                this.PropertyGetInvoker = propertyGetInvocator;
            }

            public Func<object, object?[]?, object?> PropertyGetInvoker { get; }
        }

        public class ProfilerPropertySetInvokeInfo : ProfilerPropertyInvokeInfo
        {
            public ProfilerPropertySetInvokeInfo(object target,
                PropertyArgumentInfo propertyArgument,
                string targetSignature,
                string targetDisplayName,
                string targetShortSignature,
                string targetShortDisplayName,
                SymbolComponentInfo symbolComponentInfo,
                string targetNamespace,
                string targetAssemblyName,
                Action<object?, object?, object?[]?> propertySetInvocator) : base(target,
                    propertyArgument,
                    targetSignature,
                    targetDisplayName,
                    targetShortSignature,
                    targetShortDisplayName,
                    symbolComponentInfo,
                    targetNamespace,
                    targetAssemblyName)
            {
                ArgumentNullException.ThrowIfNull(propertySetInvocator);
                this.PropertySetInvoker = propertySetInvocator;
            }

            public Action<object?, object?, object?[]?> PropertySetInvoker { get; }
        }
    }
}
