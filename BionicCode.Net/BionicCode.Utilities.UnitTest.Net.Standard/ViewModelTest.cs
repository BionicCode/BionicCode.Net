namespace BionicCode.Utilities.Net.UnitTest
{
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using System;
  using System.Linq;
  using Xunit;
  using FluentAssertions;
  using System.ComponentModel;
  using BionicCode.Utilities.Net;
  using System.Collections.Generic;
  using System.Collections;
  using FluentAssertions.Events;

  public class ViewModelTest : IDisposable
  {

    public ViewModelTest()
    {
      this.ViewModelImpl = new ViewModelImpl(this.PropertyValidationDelegateSingleError, this.PropertyValidationDelegate_Old);
      this.SenderType = this.ViewModelImpl.GetType();
      this.ViewModelImpl.PropertyValueChanged += OnPropertyValueChanged;
      this.ViewModelImpl.PropertyChanged += OnPropertyChanged;

      this.InvalidTextValue = "invalid test text";
      this.ValidTextValue = "VALIDTESTTEXT";
      this.PropertyChangedEventInvocationCount = 0;
      this.PropertyValueChangedEventInvocationCount = 0;

      this.UppercaseValidationErrorMessage = "Value must be all uppercase, no spaces allowed.";
      this.StartsWithValidationErrorMessage = $"Value must start with {this.ValidTextValue.First()}.";
    }

    public void Dispose()
    {
      this.ViewModelImpl.PropertyValueChanged -= OnPropertyValueChanged;
      this.ViewModelImpl.PropertyChanged -= OnPropertyChanged;
    }

    [Fact]
    public void ReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.NonValidatingProperty = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void SetPropertyToNullAndReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      // Initialize test
      this.ViewModelImpl.NonValidatingProperty = string.Empty;
      this.PropertyChangedEventInvocationCount = 0;

      // Execute test
      this.ViewModelImpl.NonValidatingProperty = null;

      this.PropertyChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void SilentSetValidatingPropertyWithNoPropertyChangedNotification()
    {
      using IMonitor<ViewModelImpl> eventMonitor = this.ViewModelImpl.Monitor();
      this.ViewModelImpl.SilentValidatingProperty = this.ValidTextValue;

      eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentValidatingProperty);
    }

    [Fact]
    public void SilentSetValidatingPropertyWithNoPropertyChangedNotificationUsingStringMessageDelegate()
    {
      using IMonitor<ViewModelImpl> eventMonitor = this.ViewModelImpl.Monitor();
      this.ViewModelImpl.SilentValidatingProperty_Old = this.ValidTextValue;

      eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentValidatingProperty_Old);
    }

    [Fact]
    public void SetPropertyFailsValidationAndRejectedValueDoesNotRaisePropertyChangedEvent()
    {
      using IMonitor<ViewModelImpl> eventMonitor = this.ViewModelImpl.Monitor();
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue
        = this.InvalidTextValue;
      eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.ValidatingPropertyRejectInvalidValue, "beacuse property was set silently");
    }

    [Fact]
    public void SilentSetNonValidatingPropertyWithNoPropertyChangedNotification()
    {
      this.ViewModelImpl.SilentNonValidatingProperty = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextProperty()
    {
      this.ViewModelImpl.NonValidatingProperty = this.ValidTextValue;

      this.PropertyValueChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsNullAndNewValueIsValidText()
    {
      this.ViewModelImpl.NonValidatingProperty = this.ValidTextValue;

      this.PropertyValueChangedEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void ReceiveNoPropertyChangedAfterFirstSetPropertySucceedsAndSecondValueIsEqualToPreviousValue()
    {
      this.ViewModelImpl.NonValidatingProperty
        = this.ValidTextValue;
      this.ViewModelImpl.NonValidatingProperty
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1);
      this.ViewModelImpl.NonValidatingProperty.Should().Be(this.ValidTextValue, "new value equals old value.");
    }

    [Fact]
    public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResettedToPreviousValue()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue
        = this.ValidTextValue;

      // Should not trigger PropertyChanged
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResettedToPreviousValueUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        = this.ValidTextValue;

      // Should not trigger PropertyChanged
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        = this.ValidTextValue;

      this.PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void ReceiveTwoPropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsInvalidTextAndNewValueIsValidText()
    {
      this.ViewModelImpl.NonValidatingProperty = this.InvalidTextValue;
      this.ViewModelImpl.NonValidatingProperty = this.ValidTextValue;

      this.PropertyValueChangedEventInvocationCount.Should().Be(2);
      (this.CurrentPropertyValueChangedArgs.OldValue as string).Should().Be(this.InvalidTextValue, "it's the old value");
      (this.CurrentPropertyValueChangedArgs.NewValue as string).Should().Be(this.ValidTextValue, "it's the new value");
    }

    [Fact]
    public void SetPropertyWithoutValidation()
    {
      this.ViewModelImpl.NonValidatingProperty
        = this.ValidTextValue;
      this.ViewModelImpl.NonValidatingProperty.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertyWithoutValidationToNull()
    {
      this.ViewModelImpl.NonValidatingProperty
        = null;
      this.ViewModelImpl.NonValidatingProperty.Should().BeNull();
    }

    [Fact]
    public void SetPropertySuccessfulValidation()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.ValidTextValue;
      this.ViewModelImpl.ValidatingProperty.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySuccessfulValidationUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.ValidTextValue;
      this.ViewModelImpl.ValidatingProperty_Old.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySilentlySuccessfulValidation()
    {
      this.ViewModelImpl.SilentValidatingProperty
        = this.ValidTextValue;

      this.ViewModelImpl.SilentValidatingProperty.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySilentlySuccessfulValidationUsingStringMessageDelegate()
    {
      this.ViewModelImpl.SilentValidatingProperty_Old
        = this.ValidTextValue;

      this.ViewModelImpl.SilentValidatingProperty_Old.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySilentlyNoValidation()
    {
      using IMonitor<ViewModelImpl> eventMonitor = this.ViewModelImpl.Monitor();

      this.ViewModelImpl.SilentNonValidatingProperty
        = this.ValidTextValue;

      eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentNonValidatingProperty);
      this.ViewModelImpl.SilentNonValidatingProperty.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySuccessfulValidationUsingNullAsPropertyName()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull
        = this.ValidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull
        .Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySuccessfulValidationUsingNullAsPropertyNameUsingStringMessageDelegatei()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old
        = this.ValidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old
        .Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsAccepted()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingProperty.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsAcceptedUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingProperty_Old.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsRejected()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue.Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsRejectedUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old.Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrown()
    {
      this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionOnInvalidValue = this.InvalidTextValue).Should().ThrowExactly<ArgumentException>();
      this.ViewModelImpl.ValidatingPropertyThrowExceptionOnInvalidValue.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrownUsingStringMessageDelegate()
    {
      this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowingExceptionOnInvalidValue_Old = this.InvalidTextValue).Should().ThrowExactly<ArgumentException>();
      this.ViewModelImpl.ValidatingPropertyThrowingExceptionOnInvalidValue_Old.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejected()
    {
      this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue = this.InvalidTextValue).Should().ThrowExactly<ArgumentException>();
      this.ViewModelImpl.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue.Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejectedUsingStringMessageDelegate()
    {
      this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old = this.InvalidTextValue).Should().ThrowExactly<ArgumentException>();
      this.ViewModelImpl.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old.Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelHasError()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;

      this.ViewModelImpl.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelHasErrorUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;

      this.ViewModelImpl.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsViewModelHasError()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingProperty
        = this.ValidTextValue;

      this.ViewModelImpl.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsViewModelHasErrorUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingProperty_Old
        = this.ValidTextValue;

      this.ViewModelImpl.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsErrorMessages()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingProperty
        = this.ValidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      errors.Should().BeEmpty();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsErrorMessagesUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingProperty_Old
        = this.ValidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      errors.Should().BeEmpty();
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelPropertyHasError()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;

      this.ViewModelImpl.PropertyHasError(nameof(this.ViewModelImpl.ValidatingProperty)).Should().BeTrue();
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelPropertyHasErrorUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;

      this.ViewModelImpl.PropertyHasError(nameof(this.ViewModelImpl.ValidatingProperty_Old)).Should().BeTrue();
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;
      
      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingProperty));

      errors.Should().HaveCount(1);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessageUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingProperty_Old));

      errors.Should().HaveCount(1);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;

      IEnumerable errors = this.ViewModelImpl.GetErrors(nameof(this.ViewModelImpl.ValidatingProperty));

      errors.Cast<string>().Should().HaveCount(1);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessageUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;

      IEnumerable errors = this.ViewModelImpl.GetErrors(nameof(this.ViewModelImpl.ValidatingProperty_Old));

      errors.Cast<string>().Should().HaveCount(1);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetPropertyErrorsForAllPropertiesReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingProperty = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue = this.InvalidTextValue;
      
      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      errors.Should().HaveCount(2);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetPropertyErrorsForAllPropertiesReturnsTwoErrorsUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      errors.Should().HaveCount(2);
    }

    [Fact]
    public void PropertyValidationFailsAndGetPropertyErrorsMethodForPropertyReturnsTwoErrors()
    {
      this.ViewModelImpl.PropertyValidationDelegate = this.PropertyValidationDelegateTwoErrors;
      this.ViewModelImpl.ValidatingProperty = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingProperty));

      errors.Should().HaveCount(2);
    }

    [Fact]
    public void PropertyValidationFailsAndGetPropertyErrorsMethodForPropertyReturnsTwoErrorsUsingStringMessageDelegate()
    {
      this.ViewModelImpl.PropertyValidationDelegate_Old = this.PropertyValidationDelegateTwoErrors_Old;
      this.ViewModelImpl.ValidatingProperty_Old = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingProperty_Old));

      errors.Should().HaveCount(2);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingProperty = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue = this.InvalidTextValue;
      
      IEnumerable errors = this.ViewModelImpl.GetErrors();

      errors.Cast<string>().Should().HaveCount(2);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrorsUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old = this.InvalidTextValue;

      IEnumerable errors = this.ViewModelImpl.GetErrors();

      errors.Cast<string>().Should().HaveCount(2);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicate()
    {
      this.ViewModelImpl.ValidatingProperty
        = this.InvalidTextValue;
      
      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingProperty));

      string firtsErrorMessage = errors.First();
      firtsErrorMessage.Should().Be(this.UppercaseValidationErrorMessage);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicateUsingStringMessageDelegate()
    {
      this.ViewModelImpl.ValidatingProperty_Old
        = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingProperty_Old));

      string firtsErrorMessage = errors.First();
      firtsErrorMessage.Should().Be(this.UppercaseValidationErrorMessage);
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
    private string UppercaseValidationErrorMessage { get; }
    public string StartsWithValidationErrorMessage { get; }

    private Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> PropertyValidationDelegateSingleError =>
      text => text.All(char.IsUpper) 
      ? (true, Enumerable.Empty<object>()) 
      : (false, new[] { this.UppercaseValidationErrorMessage });

    private Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> PropertyValidationDelegateTwoErrors =>
      text =>
      {
        var errorMessages = new List<string>();
        if (!text.All(char.IsUpper))
        {
          errorMessages.Add(this.UppercaseValidationErrorMessage);
        }

        if (!text.StartsWith(this.ValidTextValue.First()))
        {
          errorMessages.Add(this.StartsWithValidationErrorMessage);
        }

        return (errorMessages.IsEmpty(), errorMessages);
      };

    private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegate_Old =>
      text => text.All(char.IsUpper) 
      ? (true, Enumerable.Empty<string>()) 
      : (false, new[] { this.UppercaseValidationErrorMessage });

    private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegateTwoErrors_Old =>
      text =>
      {
        var errorMessages = new List<string>();
        if (!text.All(char.IsUpper))
        {
          errorMessages.Add(this.UppercaseValidationErrorMessage);
        }

        if (!text.StartsWith(this.ValidTextValue.First()))
        {
          errorMessages.Add(this.StartsWithValidationErrorMessage);
        }

        return (errorMessages.IsEmpty(), errorMessages);
      };
  }
}
