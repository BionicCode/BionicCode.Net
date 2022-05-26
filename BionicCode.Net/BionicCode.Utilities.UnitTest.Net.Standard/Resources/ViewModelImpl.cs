using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BionicCode.Utilities.Net.Wpf.ViewModel;

namespace BionicCode.Utilities.UnitTest.Net.Standard.Resources
{
  public class ViewModelImpl : ViewModel
  {
    public ViewModelImpl(Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> propertyValidationDelegate, string validationErrorMessage)
    {
      this.ValidationErrorMessage = validationErrorMessage;
      this.PropertyValidationDelegate = propertyValidationDelegate;
    }

    private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegate { get; }
    private string ValidationErrorMessage { get; }

    private string nonValidatingTextProperty;
    public string NonValidatingTextProperty
    {
      get => this.nonValidatingTextProperty;
      set => TrySetValue(value, ref this.nonValidatingTextProperty);
    }

    private string silentNonValidatingTextProperty;
    public string SilentNonValidatingTextProperty
    {
      get => this.silentNonValidatingTextProperty;
      set => TrySetValueSilent(value, ref this.silentNonValidatingTextProperty);
    }

    private string validatingTextPropertyChangedNullArgAndRejectInvalidValue;
    public string ValidatingTextPropertyChangedNullArgAndRejectInvalidValue
    {
      get => this.validatingTextPropertyChangedNullArgAndRejectInvalidValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingTextPropertyChangedNullArgAndRejectInvalidValue,  propertyName:null, isRejectInvalidValueEnabled:true);
    }

    private string validatingTextPropertyExpectingUpperCaseValue;
    public string ValidatingTextPropertyExpectingUpperCaseValue
    {
      get => this.validatingTextPropertyExpectingUpperCaseValue;
      set => base.TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingTextPropertyExpectingUpperCaseValue);
    }

    private string silentValidatingTextPropertyExpectingUpperCaseValue;
    public string SilentValidatingTextPropertyExpectingUpperCaseValue
    {
      get => this.silentValidatingTextPropertyExpectingUpperCaseValue;
      set => base.TrySetValueSilent(value, this.PropertyValidationDelegate, ref this.silentValidatingTextPropertyExpectingUpperCaseValue);
    }

    private string validatingTextPropertyRejectingInvalidValue;
    public string ValidatingTextPropertyRejectingInvalidValue
    {
      get => this.validatingTextPropertyRejectingInvalidValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingTextPropertyRejectingInvalidValue, isRejectInvalidValueEnabled: true, isRejectEqualValuesEnabled: true);
    }

    private string validatingTextPropertyThrowingExceptionOnNonUpperCaseValue;
    public string ValidatingTextPropertyThrowingExceptionOnNonUpperCaseValue
    {
      get => this.validatingTextPropertyThrowingExceptionOnNonUpperCaseValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingTextPropertyThrowingExceptionOnNonUpperCaseValue, isThrowExceptionOnValidationErrorEnabled:true);
    }

    private string validatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue;
    public string ValidatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue
    {
      get => this.validatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue;
      set => TrySetValue(value, this.PropertyValidationDelegate, ref this.validatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue, isThrowExceptionOnValidationErrorEnabled:true, isRejectInvalidValueEnabled:true);
    }
  }
}
