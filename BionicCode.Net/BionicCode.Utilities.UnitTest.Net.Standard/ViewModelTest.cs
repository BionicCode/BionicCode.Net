using BionicCode.Utilities.UnitTest.Net.Standard.Resources;
using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using System.ComponentModel;
using BionicCode.Utilities.Net.Standard.ViewModel;
using System.Collections.Generic;
using System.Collections;

namespace BionicCode.Utilities.UnitTest.Net.Standard
{
  
  public class ViewModelTest : IDisposable
  {

    public ViewModelTest()
    {
      this.ValidationErrorMessage = "Value must be all uppercase, no spaces allowed.";
      this.ViewModelImpl = new ViewModelImpl(this.PropertyValidationDelegate, this.ValidationErrorMessage);
      this.SenderType = this.ViewModelImpl.GetType();
      this.ViewModelImpl.PropertyValueChanged += OnPropertyValueChanged;
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;

      this.InvalidTextValue = "invalid test text";
      this.ValidTextValue = "VALIDTESTTEXT";
      this.PropertyChangedEventInvocationCount = 0;
      this.PropertyValueChangedEventInvocationCount = 0;
    }

    public void Dispose()
    {
      this.ViewModelImpl.PropertyValueChanged -= OnPropertyValueChanged;
      this.ViewModelImpl.PropertyChanged -= OnPropertyChanged;
    }

    private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegate =>
      text => text.All(char.IsUpper) ? (true, Array.Empty<string>()) : (false, new[] { this.ValidationErrorMessage });

    [Fact]
    public void ReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void SetPropertyToNullAndReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      // Initialize test
      this.ViewModelImpl.NonValidatingTextProperty = string.Empty;
      this.PropertyChangedEventInvocationCount = 0;

      // Execute test
      this.ViewModelImpl.NonValidatingTextProperty = null;

      this.PropertyChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void SilentSetValidatingPropertyWithNoPropertyChangedNotification()
    {
      this.ViewModelImpl.SilentValidatingTextPropertyExpectingUpperCaseValue = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void SilentSetNonValidatingPropertyWithNoPropertyChangedNotification()
    {
      this.ViewModelImpl.SilentNonValidatingTextProperty = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;

      this.PropertyValueChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsNullAndNewValueIsValidText()
    {
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;

      this.PropertyValueChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void ReceiveNoPropertyChangedAfterFirstSetPropertySucceedsAndSecondValueIsEqualToPreviousValue()
    {
      this.ViewModelImpl.NonValidatingTextProperty
        = this.ValidTextValue;
      this.ViewModelImpl.NonValidatingTextProperty
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1);
      this.ViewModelImpl.NonValidatingTextProperty.Should().Be(this.ValidTextValue, "new value equals old value.");
    }

    [Fact]
    public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResettedToPreviousValue()
    {
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.ValidTextValue;

      // Should not trigger PropertyChanged
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void ReceiveTwoPropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsInvalidTextAndNewValueIsValidText()
    {
      this.ViewModelImpl.NonValidatingTextProperty = this.InvalidTextValue;
      this.ViewModelImpl.NonValidatingTextProperty = this.ValidTextValue;

      this.PropertyValueChangedEventInvocationCount.Should().Be(2);
      (this.CurrentPropertyValueChangedArgs.OldValue as string).Should().Be(this.InvalidTextValue, "it's the old value");
      (this.CurrentPropertyValueChangedArgs.NewValue as string).Should().Be(this.ValidTextValue, "it's the new value");
    }

    [Fact]
    public void SetPropertyWithoutValidation()
    {
      this.ViewModelImpl.NonValidatingTextProperty
        = this.ValidTextValue;
      this.ViewModelImpl.NonValidatingTextProperty.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertyWithoutValidationToNull()
    {
      this.ViewModelImpl.NonValidatingTextProperty
        = null;
      this.ViewModelImpl.NonValidatingTextProperty.Should().BeNull();
    }

    [Fact]
    public void SetPropertySuccessfulValidation()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.ValidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySilentlySuccessfulValidation()
    {
      this.ViewModelImpl.SilentValidatingTextPropertyExpectingUpperCaseValue
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(0);
      this.ViewModelImpl.SilentValidatingTextPropertyExpectingUpperCaseValue.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySilentlyNoValidation()
    {
      this.ViewModelImpl.SilentNonValidatingTextProperty
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(0);
      this.ViewModelImpl.SilentNonValidatingTextProperty.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySuccessfulValidationUsingNullAsPropertyName()
    {
      this.ViewModelImpl.ValidatingTextPropertyChangedNullArgAndRejectInvalidValue
        = this.ValidTextValue;

      this.ViewModelImpl.ValidatingTextPropertyChangedNullArgAndRejectInvalidValue.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsAccepted()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsRejected()
    {
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue.Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrown()
    {
      this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingTextPropertyThrowingExceptionOnNonUpperCaseValue = this.InvalidTextValue).Should().ThrowExactly<ArgumentException>();
      this.ViewModelImpl.ValidatingTextPropertyThrowingExceptionOnNonUpperCaseValue.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejected()
    {
      this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue = this.InvalidTextValue).Should().ThrowExactly<ArgumentException>();
      this.ViewModelImpl.ValidatingTextPropertyThrowingExceptionAndRejectValueOnNonUpperCaseValue.Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelHasError()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;

      this.ViewModelImpl.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsViewModelHasError()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = ValidTextValue;

      this.ViewModelImpl.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsErrorMessages()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = ValidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      errors.Should().BeEmpty();
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelPropertyHasError()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;

      this.ViewModelImpl.PropertyHasError(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue)).Should().BeTrue();
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      
      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue));

      errors.Should().HaveCount(1);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;

      IEnumerable errors = this.ViewModelImpl.GetErrors(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue));

      errors.Cast<string>().Should().HaveCount(1);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetPropertyErrorsReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue = this.InvalidTextValue;
      
      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      errors.Should().HaveCount(2);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingTextPropertyRejectingInvalidValue = this.InvalidTextValue;
      
      IEnumerable errors = this.ViewModelImpl.GetErrors();

      errors.Cast<string>().Should().HaveCount(2);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicate()
    {
      this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue
        = this.InvalidTextValue;
      
      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingTextPropertyExpectingUpperCaseValue));

      string firtsErrorMessage = errors.First();
      firtsErrorMessage.Should().Be(this.ValidationErrorMessage);
    }

    private void OnPropertyValueChanged(object sender, PropertyValueChangedArgs<object> e)
    {
      sender.Should().BeOfType(this.SenderType);

      this.PropertyValueChangedEventInvocationCount++;
      this.CurrentPropertyValueChangedArgs = e;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      sender.Should().BeOfType(this.SenderType);

      this.PropertyChangedEventInvocationCount++;
    }

    private string InvalidTextValue { get; }
    private string ValidTextValue { get; }
    private int PropertyChangedEventInvocationCount { get; set; }
    private int PropertyValueChangedEventInvocationCount { get; set; }
    private PropertyValueChangedArgs<object> CurrentPropertyValueChangedArgs { get; set; }
    private ViewModelImpl ViewModelImpl { get; }
    private TestEventSource2 EventSource2 { get; }
    private Type SenderType { get; set; }
    private string ValidationErrorMessage { get; }
  }
}
