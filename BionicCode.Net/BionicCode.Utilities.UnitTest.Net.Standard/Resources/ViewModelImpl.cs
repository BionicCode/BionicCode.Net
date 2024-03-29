﻿namespace BionicCode.Utilities.UnitTest.Net.Resources
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;

  public class ViewModelImpl : ViewModel
  {
    public ViewModelImpl(Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> propertyValidationDelegate, Func<string, (bool, IEnumerable<string> ErrorMessages)> propertyValidationDelegate_Old)
    {
      this.PropertyValidationDelegate = propertyValidationDelegate;
      this.PropertyValidationDelegate_Old = propertyValidationDelegate_Old;
      this.PropertyValidationDelegateAsync = null;
    }

    public ViewModelImpl(Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> propertyValidationDelegateAsync)
    {
      this.PropertyValidationDelegate = null;
      this.PropertyValidationDelegate_Old = null;
      this.PropertyValidationDelegateAsync = propertyValidationDelegateAsync;
    }

    public Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> PropertyValidationDelegate { get; set; }
    public Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> PropertyValidationDelegateAsync { get; set; }
    public Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegate_Old { get; set; }
    
    private string nonValidatingProperty;
    public string NonValidatingProperty
    {
      get => this.nonValidatingProperty;
      set => TrySetValue(value, ref this.nonValidatingProperty);
    }

    private string silentNonValidatingProperty;
    public string SilentNonValidatingProperty
    {
      get => this.silentNonValidatingProperty;
      set => TrySetValueSilent(value, ref this.silentNonValidatingProperty);
    }

    private string validatingPropertyRejectInvalidValueAndPropertyNameIsNull;
    public string ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull
    {
      get => this.validatingPropertyRejectInvalidValueAndPropertyNameIsNull;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingPropertyRejectInvalidValueAndPropertyNameIsNull,  propertyName:null, isRejectInvalidValueEnabled:true);
    }

    private string validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync;
    public string ValidatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync
    {
      get => this.validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync;
      set => TrySetValueAsync(value, this.validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync, this.PropertyValidationDelegateAsync, value => this.validatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync = value, propertyName: null, isRejectInvalidValueEnabled: true);
    }

    private string validatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old;
    public string ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old
    {
      get => this.validatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old;
      set => TrySetValue(value, this.PropertyValidationDelegate_Old, ref this.validatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old, propertyName: null, isRejectInvalidValueEnabled: true);
    }

    private string validatingProperty;
    public string ValidatingProperty
    {
      get => this.validatingProperty;
      set => base.TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingProperty);
    }

    private string validatingPropertyAsync;
    public string ValidatingPropertyAsync
    {
      get => this.validatingPropertyAsync;
      set => base.TrySetValueAsync(value, this.validatingPropertyAsync, this.PropertyValidationDelegateAsync, value => this.validatingPropertyAsync = value);
    }

    private string validatingProperty_Old;
    public string ValidatingProperty_Old
    {
      get => this.validatingProperty_Old;
      set => base.TrySetValue(value, this.PropertyValidationDelegate_Old, ref this.validatingProperty_Old);
    }

    private string silentValidatingProperty;
    public string SilentValidatingProperty
    {
      get => this.silentValidatingProperty;
      set => base.TrySetValueSilent(value, this.PropertyValidationDelegate, ref this.silentValidatingProperty);
    }

    private string silentValidatingPropertyAsync;
    public string SilentValidatingPropertyAsync
    {
      get => this.silentValidatingPropertyAsync;
      set => base.TrySetValueSilentAsync(value, this.silentValidatingPropertyAsync, this.PropertyValidationDelegateAsync, value => this.silentValidatingPropertyAsync = value);
    }
    private string silentValidatingProperty_Old;
    public string SilentValidatingProperty_Old
    {
      get => this.silentValidatingProperty_Old;
      set => TrySetValueSilent(value, this.PropertyValidationDelegate_Old, ref this.silentValidatingProperty_Old);
    }

    private string validatingPropertyRejectInvalidValue;
    public string ValidatingPropertyRejectInvalidValue
    {
      get => this.validatingPropertyRejectInvalidValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingPropertyRejectInvalidValue, isRejectInvalidValueEnabled: true, isRejectEqualValuesEnabled: true);
    }

    private string validatingPropertyRejectInvalidValueAsync;
    public string ValidatingPropertyRejectInvalidValueAsync
    {
      get => this.validatingPropertyRejectInvalidValueAsync;
      set => TrySetValueAsync(value, this.validatingPropertyRejectInvalidValueAsync, this.PropertyValidationDelegateAsync, value => this.validatingPropertyRejectInvalidValueAsync = value, isRejectInvalidValueEnabled: true, isRejectEqualValuesEnabled: true);
    }

    private string validatingPropertyRejectInvalidValue_Old;
    public string ValidatingPropertyRejectInvalidValue_Old
    {
      get => this.validatingPropertyRejectInvalidValue_Old;
      set => TrySetValue(value, this.PropertyValidationDelegate_Old, ref this.validatingPropertyRejectInvalidValue_Old, isRejectInvalidValueEnabled: true, isRejectEqualValuesEnabled: true);
    }

    private string validatingPropertyThrowExceptionOnInvalidValue;
    public string ValidatingPropertyThrowExceptionOnInvalidValue
    {
      get => this.validatingPropertyThrowExceptionOnInvalidValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingPropertyThrowExceptionOnInvalidValue, isThrowExceptionOnValidationErrorEnabled:true);
    }

    public Task<bool> SetPropertyThrowExceptionOnInvalidValueUsingTrySetValueAsyncExplicitly(string value, string propertyName)
    {
       return TrySetValueAsync(value, this.validatingPropertyThrowExceptionOnInvalidValue, this.PropertyValidationDelegateAsync, value => this.validatingPropertyThrowExceptionOnInvalidValue = value, isThrowExceptionOnValidationErrorEnabled: true, propertyName: propertyName);           
    }

    private string validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync;
    public string ValidatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync
    {
      get => this.validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync;
      set
      {
        _ = TrySetValueAsync(value, this.validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync, this.PropertyValidationDelegateAsync, value => this.validatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync = value, isThrowExceptionOnValidationErrorEnabled: true);
      }
    }

    private string validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync;
    public string ValidatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync
    {
      get => this.validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync;
      set
      {
        _ =  TrySetValueAsync(value, this.validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync, this.PropertyValidationDelegateAsync, value => this.validatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync = value, isThrowExceptionOnValidationErrorEnabled: true)
          .ConfigureAwait(false)
          .GetAwaiter()
          .GetResult();
      }
    }

    private string validatingPropertyThrowExceptionOnInvalidValue_Old;
    public string ValidatingPropertyThrowingExceptionOnInvalidValue_Old
    {
      get => this.validatingPropertyThrowExceptionOnInvalidValue_Old;
      set => TrySetValue(value, this.PropertyValidationDelegate_Old, ref this.validatingPropertyThrowExceptionOnInvalidValue_Old, isThrowExceptionOnValidationErrorEnabled: true);
    }

    private string validatingPropertyThrowExceptionAndRejectValueOnInvalidValue;
    public string ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue
    {
      get => this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValue, isThrowExceptionOnValidationErrorEnabled:true, isRejectInvalidValueEnabled:true);
    }

    private string validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync;
    public string ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync
    {
      get => this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync;
      set => _ = TrySetValueAsync(value, this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync, this.PropertyValidationDelegateAsync, value => this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync = value, isThrowExceptionOnValidationErrorEnabled: true, isRejectInvalidValueEnabled: true).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private string validatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old;
    public string ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old
    {
      get => this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old;
      set => TrySetValue(value, this.PropertyValidationDelegate_Old, ref this.validatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old, isThrowExceptionOnValidationErrorEnabled: true, isRejectInvalidValueEnabled: true);
    }
  }
}
