namespace BionicCode.Utilities.Net.UnitTest.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BionicCode.Utilities.Net;

    public class ViewModelImpl : ViewModel
    {
        public ViewModelImpl(Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> propertyValidationDelegate, Func<string, (bool, IEnumerable<string> ErrorMessages)> propertyValidationDelegate_Old)
        {
            PropertyValidationDelegate = propertyValidationDelegate;
            PropertyValidationDelegate_Old = propertyValidationDelegate_Old;
            PropertyValidationDelegateAsync = null;
        }

        public ViewModelImpl(Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> propertyValidationDelegateAsync)
        {
            PropertyValidationDelegate = null;
            PropertyValidationDelegate_Old = null;
            PropertyValidationDelegateAsync = propertyValidationDelegateAsync;
        }

        public Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> PropertyValidationDelegate { get; set; }
        public Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> PropertyValidationDelegateAsync { get; set; }
        public Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegate_Old { get; set; }

        private string nonValidatingProperty;
        public string NonValidatingProperty
        {
            get => nonValidatingProperty;
            set => TrySetValue(value, ref nonValidatingProperty);
        }

        private string silentNonValidatingProperty;
        public string SilentNonValidatingProperty
        {
            get => silentNonValidatingProperty;
            set => TrySetValueSilent(value, ref silentNonValidatingProperty);
        }

        private string validatingPropertyRejectInvalidValueAndPropertyNameIsNull;
        public string ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull
        {
            get => validatingPropertyRejectInvalidValueAndPropertyNameIsNull;
            set => TrySetValue(value, PropertyValidationDelegate, ref validatingPropertyRejectInvalidValueAndPropertyNameIsNull, propertyName: null, methodConfiguration: new SetValueOptions(true, false, true));
        }

        private string validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync;
        public string ValidatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync
        {
            get => validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync;
            set => TrySetValueAsync(value, validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync = valueToValidate, propertyName: null, methodConfiguration: new SetValueOptions(true, false, true));
        }

        //private string validatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old;
        //public string ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old
        //{
        //  get => validatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old;
        //  set => TrySetValue(value, PropertyValidationDelegate_Old, ref validatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old, propertyName: null, methodConfiguration: new SetValueOptions(true, false,true));
        //}

        private string validatingProperty;
        public string ValidatingProperty
        {
            get => validatingProperty;
            set => base.TrySetValue(value, PropertyValidationDelegate, ref validatingProperty);
        }

        private string validatingPropertyAsync;
        public string ValidatingPropertyAsync
        {
            get => validatingPropertyAsync;
            set => base.TrySetValueAsync(value, validatingPropertyAsync, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyAsync = valueToValidate);
        }

        //private string validatingProperty_Old;
        //public string ValidatingProperty_Old
        //{
        //  get => validatingProperty_Old;
        //  set => base.TrySetValue(value, PropertyValidationDelegate_Old, ref validatingProperty_Old);
        //}

        private string silentValidatingProperty;
        public string SilentValidatingProperty
        {
            get => silentValidatingProperty;
            set => base.TrySetValueSilent(value, PropertyValidationDelegate, ref silentValidatingProperty);
        }

        private string silentValidatingPropertyAsync;
        public string SilentValidatingPropertyAsync
        {
            get => silentValidatingPropertyAsync;
            set => base.TrySetValueSilentAsync(value, silentValidatingPropertyAsync, PropertyValidationDelegateAsync, valueToValidate => silentValidatingPropertyAsync = valueToValidate);
        }

        //private string silentValidatingProperty_Old;
        //public string SilentValidatingProperty_Old
        //{
        //  get => silentValidatingProperty_Old;
        //  set => TrySetValueSilent(value, PropertyValidationDelegate_Old, ref silentValidatingProperty_Old);
        //}

        private string validatingPropertyRejectInvalidValue;
        public string ValidatingPropertyRejectInvalidValue
        {
            get => validatingPropertyRejectInvalidValue;
            set => TrySetValue(value, PropertyValidationDelegate, ref validatingPropertyRejectInvalidValue, methodConfiguration: new SetValueOptions(true, false, true));
        }

        private string validatingPropertyRejectInvalidValueAsync;
        public string ValidatingPropertyRejectInvalidValueAsync
        {
            get => validatingPropertyRejectInvalidValueAsync;
            set => TrySetValueAsync(value, validatingPropertyRejectInvalidValueAsync, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyRejectInvalidValueAsync = valueToValidate, new SetValueOptions(true, false, true));
        }

        //private string validatingPropertyRejectInvalidValue_Old;
        //public string ValidatingPropertyRejectInvalidValue_Old
        //{
        //  get => validatingPropertyRejectInvalidValue_Old;
        //  set => TrySetValue(value, PropertyValidationDelegate_Old, ref validatingPropertyRejectInvalidValue_Old, isRejectInvalidValueEnabled: true, isRejectEqualValuesEnabled: true);
        //}

        private string validatingPropertyThrowExceptionOnInvalidValue;
        public string ValidatingPropertyThrowExceptionOnInvalidValue
        {
            get => validatingPropertyThrowExceptionOnInvalidValue;
            set => TrySetValue(value, PropertyValidationDelegate, ref validatingPropertyThrowExceptionOnInvalidValue, methodConfiguration: new SetValueOptions(false, true, false));
        }

        public Task<bool> SetPropertyThrowExceptionOnInvalidValueUsingTrySetValueAsyncExplicitly(string value, string propertyName)
          => TrySetValueAsync(value, validatingPropertyThrowExceptionOnInvalidValue, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyThrowExceptionOnInvalidValue = valueToValidate, methodConfiguration: new SetValueOptions(false, true, false), propertyName: propertyName);

        private string validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync;
        public string ValidatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync
        {
            get => validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync;
            set => _ = TrySetValueAsync(value, validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync = valueToValidate, new SetValueOptions(false, true, true));
        }

        private string validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync;
        public string ValidatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync
        {
            get => validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync;
            set => _ = TrySetValueAsync(value, validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync = valueToValidate, new SetValueOptions(false, true, true))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        //private string validatingPropertyThrowExceptionOnInvalidValue_Old;
        //public string ValidatingPropertyThrowingExceptionOnInvalidValue_Old
        //{
        //  get => validatingPropertyThrowExceptionOnInvalidValue_Old;
        //  set => TrySetValue(value, PropertyValidationDelegate_Old, ref validatingPropertyThrowExceptionOnInvalidValue_Old, isThrowExceptionOnValidationErrorEnabled: true);
        //}

        private string validatingPropertyThrowExceptionAndRejectValueOnInvalidValue;
        public string ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue
        {
            get => validatingPropertyThrowExceptionAndRejectValueOnInvalidValue;
            set => TrySetValue(value, PropertyValidationDelegate, ref validatingPropertyThrowExceptionAndRejectValueOnInvalidValue, new SetValueOptions(true, true, true));
        }

        private string validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync;
        public string ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync
        {
            get => validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync;
            set => _ = TrySetValueAsync(value, validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync, PropertyValidationDelegateAsync, valueToValidate => validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync = valueToValidate, new SetValueOptions(true, true, true)).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        //private string validatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old;
        //public string ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old
        //{
        //  get => validatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old;
        //  set => TrySetValue(value, PropertyValidationDelegate_Old, ref validatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old, isThrowExceptionOnValidationErrorEnabled: true, isRejectInvalidValueEnabled: true);
        //}
    }
}
