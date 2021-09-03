using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BionicCode.Utilities.Net.Standard.ViewModel;

namespace BionicCode.Utilities.UnitTest.Net.Standard.Resources
{
  public class ViewModelImpl : ViewModel
  {
    public ViewModelImpl()
    {
      this.ValidationErrorMessage = "Value must be all uppercase";
    }

    private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> ValidateProperty()
    {
      return text => text.All(char.IsUpper) ? (true, Array.Empty<string>()) : (false, new[] { this.ValidationErrorMessage });
    }

    public string ValidationErrorMessage { get; }

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
      set => TrySetValue(value, ValidateProperty(), ref this.validatingTextPropertyChangedNullArgAndRejectInvalidValue,  propertyName:null, isRejectInvalidValueEnabled:true);
    }

    private string validatingTextPropertyExpectingUpperCaseValue;
    public string ValidatingTextPropertyExpectingUpperCaseValue
    {
      get => this.validatingTextPropertyExpectingUpperCaseValue;
      set => base.TrySetValue(value, ValidateProperty(), ref this.validatingTextPropertyExpectingUpperCaseValue);
    }

    private string silentValidatingTextPropertyExpectingUpperCaseValue;
    public string SilentValidatingTextPropertyExpectingUpperCaseValue
    {
      get => this.silentValidatingTextPropertyExpectingUpperCaseValue;
      set => base.TrySetValueSilent(value, ValidateProperty(), ref this.silentValidatingTextPropertyExpectingUpperCaseValue);
    }

    private string validatingTextPropertyRejectingInvalidValue;
    public string ValidatingTextPropertyRejectingInvalidValue
    {
      get => this.validatingTextPropertyRejectingInvalidValue;
      set => TrySetValue(value, ValidateProperty(), ref this.validatingTextPropertyRejectingInvalidValue, true);
    }

    private string validatingTextPropertyThrowingExceptionOnNonUpperCaseValue;
    public string ValidatingTextPropertyThrowingExceptionOnNonUpperCaseValue
    {
      get => this.validatingTextPropertyThrowingExceptionOnNonUpperCaseValue;
      set => TrySetValue(value, ValidateProperty(), ref this.validatingTextPropertyThrowingExceptionOnNonUpperCaseValue, isThrowExceptionOnValidationErrorEnabled:true);
    }

    private string validatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue;
    public string ValidatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue
    {
      get => this.validatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue;
      set => TrySetValue(value, ValidateProperty(), ref this.validatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue, isThrowExceptionOnValidationErrorEnabled:true, isRejectInvalidValueEnabled:true);
    }
  }
}
