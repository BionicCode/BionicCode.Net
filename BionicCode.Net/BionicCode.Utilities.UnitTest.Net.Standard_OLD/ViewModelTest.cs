using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using BionicCode.Utilities.Net.Standard.ViewModel;
using BionicCode.Utilities.UnitTest.Net.Standard.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BionicCode.Utilities.UnitTest.Net.Standard
{
  [TestClass]
  public class ViewModelTest
  {
    [TestInitialize]
    public void Initialize()
    {
      this.InvalidTextValue = "testText";
      this.ValidTextValue = "TESTTEXT";
      this.EventInvocationCount = 0;
      this.ViewModelImpl = new ViewModelImpl();
      this.SenderType = this.ViewModelImpl.GetType();
    }

    [TestMethod]
    public void ReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;
      Assert.AreEqual(1, this.EventInvocationCount);
    }

    [TestMethod]
    public void SetPropertyToNullAndReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;
      this.ViewModelImpl.NonValidatingTextProperty = null;
      Assert.AreEqual(1, this.EventInvocationCount);
    }

    [TestMethod]
    public void SilentSetValidatingPropertyWithNoPropertyChangedNotification()
    {
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;
      this.ViewModelImpl.SilentValidatingTextPropertyExpectingUpperCaseValue = this.ValidTextValue;
      Assert.AreEqual(0, this.EventInvocationCount);
    }

    [TestMethod]
    public void SilentSetNonValidatingPropertyWithNoPropertyChangedNotification()
    {
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;
      this.ViewModelImpl.SilentNonValidatingTextProperty = this.ValidTextValue;
      Assert.AreEqual(0, this.EventInvocationCount);
    }

    [TestMethod]
    public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.PropertyValueChanged += OnPropertyValueChanged;
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;
      Assert.AreEqual(1, this.EventInvocationCount);
    }

    [TestMethod]
    public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsNullAndNewValueIsValidText()
    {
      this.ViewModelImpl.PropertyValueChanged += OnPropertyValueChanged;
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;
      Assert.AreEqual(1, this.EventInvocationCount);
    }

    [TestMethod]
    public void ReceiveNoPropertyChangedAfterFirstSetPropertySucceedsAndSecondValueIsEqualToPreviousValue()
    {
      this.ViewModelImpl.NonValidatingTextProperty
        = this.ValidTextValue;
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;
      this.ViewModelImpl.NonValidatingTextProperty
        = this.ValidTextValue;

      Assert.AreEqual(0, this.EventInvocationCount);
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.NonValidatingTextProperty);
    }

    [TestMethod]
    public void ReceiveOnePropertyChangedAfterFirstSetPropertyFailsValidationAndValueIsRejectedAndPropertyResettedToPreviousValue()
    {
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.ValidTextValue;
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.ValidTextValue;
      Assert.AreEqual(1, this.EventInvocationCount);
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue);
    }

    [TestMethod]
    public void ReceiveTwoPropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsInvalidTextAndNewValueIsValidText()
    {
      this.ViewModelImpl.PropertyValueChanged += OnPropertyValueChanged;
      this.ViewModelImpl.NonValidatingTextProperty = this.InvalidTextValue;
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;
      Assert.AreEqual(2, this.EventInvocationCount);
      Assert.AreEqual(this.InvalidTextValue, this.CurrentPropertyValueChangedArgs.OldValue, "Old value is wrong.");
      Assert.AreEqual(this.ValidTextValue, this.CurrentPropertyValueChangedArgs.NewValue, "New value is wrong");
    }

    [TestMethod]
    public void SetPropertyWithoutValidation()
    {
      this.ViewModelImpl.NonValidatingTextProperty
        = this.ValidTextValue;
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.NonValidatingTextProperty);
    }

    [TestMethod]
    public void SetPropertyWithoutValidationToNull()
    {
      this.ViewModelImpl.NonValidatingTextProperty
        = null;
      Assert.IsNull(this.ViewModelImpl.NonValidatingTextProperty);
    }

    [TestMethod]
    public void SetPropertySuccessfulValidation()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.ValidTextValue;
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue);
    }

    [TestMethod]
    public void SetPropertySilentlySuccessfulValidation()
    {
      this.ViewModelImpl.SilentValidatingTextPropertyExpectingUpperCaseValue
        = this.ValidTextValue;
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.SilentValidatingTextPropertyExpectingUpperCaseValue);
    }

    [TestMethod]
    public void SetPropertySilentlyNoValidation()
    {
      this.ViewModelImpl.SilentNonValidatingTextProperty
        = this.ValidTextValue;
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.SilentNonValidatingTextProperty);
    }

    [TestMethod]
    public void SetPropertySuccessfulValidationUsingNullAsPropertyName()
    {
      this.ViewModelImpl.ValidatingTextPropertyChangedNullArgAndRejectInvalidValue
        = this.ValidTextValue;
      Assert.AreEqual(this.ValidTextValue, this.ViewModelImpl.ValidatingTextPropertyChangedNullArgAndRejectInvalidValue);
    }

    [TestMethod]
    public void SetPropertyFailsValidationAndValueIsAccepted()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      Assert.AreEqual(this.InvalidTextValue, this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue);
    }

    [TestMethod]
    public void SetPropertyFailsValidationAndValueIsRejected()
    {
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.InvalidTextValue;
      Assert.AreNotEqual(this.InvalidTextValue, this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrown()
    {
      try
      {
        this.ViewModelImpl.ValidatingTextPropertyThrowingExceptionOnNonUpperCaseValue
          = this.InvalidTextValue;
      }
      finally
      {
        Assert.AreEqual(this.InvalidTextValue, this.ViewModelImpl.ValidatingTextPropertyThrowingExceptionOnNonUpperCaseValue);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejected()
    {
      try
      {
        this.ViewModelImpl.ValidatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue
          = this.InvalidTextValue;
      }
      finally
      {
        Assert.AreNotEqual(this.InvalidTextValue, this.ViewModelImpl.ValidatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue);
      }
    }

    [TestMethod]
    public void SetPropertyFailsValidationAndViewModelHasError()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      Assert.IsTrue(this.ViewModelImpl.HasErrors);
    }

    [TestMethod]
    public void SetPropertyAfterPreviousValidationClearsViewModelHasError()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = ValidTextValue;
      Assert.IsFalse(this.ViewModelImpl.HasErrors);
    }

    [TestMethod]
    public void SetPropertyAfterPreviousValidationClearsErrorMessages()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = ValidTextValue;
      Assert.AreEqual(0, this.ViewModelImpl.GetPropertyErrors().Count());
    }

    [TestMethod]
    public void SetPropertyFailsValidationAndViewModelPropertyHasError()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      Assert.IsTrue(this.ViewModelImpl.PropertyHasError(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue)));
    }

    [TestMethod]
    public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      System.Collections.Generic.IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue));

      Assert.AreEqual(1, errors.Count());
    }

    [TestMethod]
    public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      IEnumerable errors = this.ViewModelImpl.GetErrors(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue));

      Assert.AreEqual(1, errors.Cast<string>().Count());
    }

    [TestMethod]
    public void TwoPropertyValidationFailsAndGetPropertyErrorsReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue = this.InvalidTextValue;
      System.Collections.Generic.IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      Assert.AreEqual(2, errors.Count());
    }

    [TestMethod]
    public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue = this.InvalidTextValue;
      System.Collections.IEnumerable errors = this.ViewModelImpl.GetErrors();

      Assert.AreEqual(2, errors.Cast<string>().Count());
    }

    [TestMethod]
    public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicate()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      System.Collections.Generic.IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue));

      Assert.AreEqual(this.ViewModelImpl.ValidationErrorMessage, errors.First());
    }

    private void OnPropertyValueChanged(object sender, PropertyValueChangedArgs<object> e)
    {
      Assert.IsInstanceOfType(sender, this.SenderType);
      this.EventInvocationCount++;
      this.CurrentPropertyValueChangedArgs = e;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      Assert.IsInstanceOfType(sender, this.SenderType);
      this.EventInvocationCount++;
    }

    public string InvalidTextValue { get; set; }
    public string ValidTextValue { get; set; }
    public int EventInvocationCount { get; set; }
    public PropertyValueChangedArgs<object> CurrentPropertyValueChangedArgs { get; set; }
    public ViewModelImpl ViewModelImpl { get; set; }
    public TestEventSource2 EventSource2 { get; set; }
    public Type SenderType { get; set; }
  }
}
