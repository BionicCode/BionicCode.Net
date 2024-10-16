namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using Xunit;

  public class ViewModelTestAsync : IDisposable
  {
    public ViewModelTestAsync()
    {
      this.ViewModelImpl = new ViewModelImpl(this.PropertyValidationDelegateSingleErrorAsync);
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

    // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20

    [Fact]
    public void SilentSetValidatingPropertyWithNoPropertyChangedNotification()
    {
      //using IMonitor<ViewModelImpl> eventMonitor = this.ViewModelImpl.Monitor();
      //this.ViewModelImpl.SilentValidatingPropertyAsync = this.ValidTextValue;
      //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentValidatingPropertyAsync, "property was set silently.");
    }

    [Fact]
    public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResetToPreviousValue()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
        = this.ValidTextValue;

      // Should not trigger PropertyChanged
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
        = this.InvalidTextValue;

      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
        = this.ValidTextValue;

      _ = this.PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
      _ = this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySuccessfulValidation()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.ValidTextValue;
      _ = this.ViewModelImpl.ValidatingPropertyAsync.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySilentlySuccessfulValidation()
    {
      this.ViewModelImpl.SilentValidatingPropertyAsync
        = this.ValidTextValue;

      _ = this.ViewModelImpl.SilentValidatingPropertyAsync.Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertySuccessfulValidationUsingNullAsPropertyName()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync
        = this.ValidTextValue;

      _ = this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync
        .Should().Be(this.ValidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsAccepted()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;

      _ = this.ViewModelImpl.ValidatingPropertyAsync.Should().Be(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndValueIsRejected()
    {
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
        = this.InvalidTextValue;

      _ = this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync.Should().NotBe(this.InvalidTextValue);
    }

    // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20
    [Fact]
    public void SetPropertyAsyncFailsValidationAndRejectedValueDoesNotRaisePropertyChangedEvent()
    {
      //using IMonitor<ViewModelImpl> eventMonitor = this.ViewModelImpl.Monitor();
      //this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
      //  = this.InvalidTextValue;
      //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.ValidatingPropertyRejectInvalidValueAsync, "beacuse property was set silently");
    }

    [Fact]
    public void SetPropertyAsyncFailsValidationAndValidationExceptionIsNotThrownBecauseCallIsNotAwaited() => _ = this.ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync = this.InvalidTextValue)
        .Should().NotThrow<ArgumentException>();

    [Fact]
    public void SetPropertyAsyncFailsValidationAndValidationExceptionIsThrownBecauseCallIsExecutedSynchronously() => _ = this.ViewModelImpl
        .Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync = this.InvalidTextValue)
        .Should().Throw<ArgumentException>();

    [Fact]
    public void SetPropertyAsyncFailsValidationAndValidationExceptionIsThrownBecauseCallExecutedAndAwaitedExplicitly() => _ = this.ViewModelImpl.Awaiting(viewModel
                                                                                                                                 => viewModel.SetPropertyThrowExceptionOnInvalidValueUsingTrySetValueAsyncExplicitly(this.InvalidTextValue, nameof(viewModel.ValidatingPropertyThrowExceptionOnInvalidValue)))
        .Should().ThrowAsync<ArgumentException>();

    [Fact]
    public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejected()
    {
      _ = this.ViewModelImpl
        .Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync = this.InvalidTextValue)
        .Should().ThrowExactly<ArgumentException>();
      _ = this.ViewModelImpl.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync
        .Should().NotBe(this.InvalidTextValue);
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelHasError()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;

      _ = this.ViewModelImpl.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsViewModelHasError()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.ValidTextValue;

      _ = this.ViewModelImpl.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void SetPropertyAfterPreviousValidationClearsErrorMessages()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.ValidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      _ = errors.Should().BeEmpty();
    }

    [Fact]
    public void SetPropertyFailsValidationAndViewModelPropertyHasError()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;

      _ = this.ViewModelImpl.PropertyHasError(nameof(this.ViewModelImpl.ValidatingPropertyAsync)).Should().BeTrue();
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingPropertyAsync));

      _ = errors.Should().HaveCount(1);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessage()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;

      IEnumerable errors = this.ViewModelImpl.GetErrors(nameof(this.ViewModelImpl.ValidatingPropertyAsync));

      _ = errors.Cast<string>().Should().HaveCount(1);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetPropertyErrorsForAllPropertiesReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingPropertyAsync = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors();

      _ = errors.Should().HaveCount(2);
    }

    [Fact]
    public void PropertyValidationFailsAndGetPropertyErrorsMethodForPropertyReturnsTwoErrors()
    {
      this.ViewModelImpl.PropertyValidationDelegateAsync = this.PropertyValidationDelegateTwoErrorsAsync;
      this.ViewModelImpl.ValidatingPropertyAsync = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingPropertyAsync));

      _ = errors.Should().HaveCount(2);
    }

    [Fact]
    public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrors()
    {
      this.ViewModelImpl.ValidatingPropertyAsync = this.InvalidTextValue;
      this.ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync = this.InvalidTextValue;

      IEnumerable errors = this.ViewModelImpl.GetErrors();

      _ = errors.Cast<string>().Should().HaveCount(2);
    }

    [Fact]
    public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicate()
    {
      this.ViewModelImpl.ValidatingPropertyAsync
        = this.InvalidTextValue;

      IEnumerable<string> errors = this.ViewModelImpl.GetPropertyErrors(nameof(this.ViewModelImpl.ValidatingPropertyAsync));

      string firtsErrorMessage = errors.First();
      _ = firtsErrorMessage.Should().Be(this.UppercaseValidationErrorMessage);
    }

    private void OnPropertyValueChanged(object sender, PropertyValueChangedArgs<object> e)
    {
      _ = sender.Should().BeOfType(this.SenderType);

      this.PropertyValueChangedEventInvocationCount++;
      this.CurrentPropertyValueChangedArgs = e;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      _ = sender.Should().BeOfType(this.SenderType);

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

    private Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> PropertyValidationDelegateSingleErrorAsync 
      => text => Task.FromResult(text.All(char.IsUpper)
        ? (true, Enumerable.Empty<object>())
        : (false, new[] { this.UppercaseValidationErrorMessage }));

    private Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> PropertyValidationDelegateTwoErrorsAsync 
      => text =>
      {
        var errorMessages = new List<object>();
        if (!text.All(char.IsUpper))
        {
          errorMessages.Add(this.UppercaseValidationErrorMessage);
        }

        //if (!text.StartsWith(this.ValidTextValue.First()))
        if (text.First() != this.ValidTextValue.First())
        {
          errorMessages.Add(this.StartsWithValidationErrorMessage);
        }

        (bool, IEnumerable<object> errorMessages) result = (errorMessages.IsEmpty(), errorMessages);
        return Task.FromResult(result);
      };
  }
}
