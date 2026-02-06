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
            ViewModelImpl = new ViewModelImpl(PropertyValidationDelegateSingleErrorAsync);
            SenderType = ViewModelImpl.GetType();
            ViewModelImpl.PropertyValueChanged += OnPropertyValueChanged;
            ViewModelImpl.PropertyChanged += OnPropertyChanged;

            InvalidTextValue = "invalid test text";
            ValidTextValue = "VALIDTESTTEXT";
            PropertyChangedEventInvocationCount = 0;
            PropertyValueChangedEventInvocationCount = 0;

            UppercaseValidationErrorMessage = "ExecuteDelegate must be all uppercase, no spaces allowed.";
            StartsWithValidationErrorMessage = $"ExecuteDelegate must start with {ValidTextValue.First()}.";
        }

        public void Dispose()
        {
            ViewModelImpl.PropertyValueChanged -= OnPropertyValueChanged;
            ViewModelImpl.PropertyChanged -= OnPropertyChanged;
        }

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20

        [Fact]
        public void SilentSetValidatingPropertyWithNoPropertyChangedNotification()
        {
            //using IMonitor<ViewModelImpl> eventMonitor = ViewModelImpl.Monitor();
            //ViewModelImpl.SilentValidatingPropertyAsync = ValidTextValue;
            //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentValidatingPropertyAsync, "property was set silently.");
        }

        [Fact]
        public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResetToPreviousValue()
        {
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
              = ValidTextValue;

            // Should not trigger PropertyChanged
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
              = InvalidTextValue;

            ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
              = ValidTextValue;

            _ = PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
            _ = ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync.Should().Be(ValidTextValue);
        }

        [Fact]
        public void SetPropertySuccessfulValidation()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = ValidTextValue;
            _ = ViewModelImpl.ValidatingPropertyAsync.Should().Be(ValidTextValue);
        }

        [Fact]
        public void SetPropertySilentlySuccessfulValidation()
        {
            ViewModelImpl.SilentValidatingPropertyAsync
              = ValidTextValue;

            _ = ViewModelImpl.SilentValidatingPropertyAsync.Should().Be(ValidTextValue);
        }

        [Fact]
        public void SetPropertySuccessfulValidationUsingNullAsPropertyName()
        {
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync
              = ValidTextValue;

            _ = ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNullAsync
              .Should().Be(ValidTextValue);
        }

        [Fact]
        public void SetPropertyFailsValidationAndValueIsAccepted()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;

            _ = ViewModelImpl.ValidatingPropertyAsync.Should().Be(InvalidTextValue);
        }

        [Fact]
        public void SetPropertyFailsValidationAndValueIsRejected()
        {
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
              = InvalidTextValue;

            _ = ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync.Should().NotBe(InvalidTextValue);
        }

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20
        [Fact]
        public void SetPropertyAsyncFailsValidationAndRejectedValueDoesNotRaisePropertyChangedEvent()
        {
            //using IMonitor<ViewModelImpl> eventMonitor = ViewModelImpl.Monitor();
            //ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync
            //  = InvalidTextValue;
            //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.ValidatingPropertyRejectInvalidValueAsync, "beacuse property was set silently");
        }

        [Fact]
        public void SetPropertyAsyncFailsValidationAndValidationExceptionIsNotThrownBecauseCallIsNotAwaited() => _ = ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionOnInvalidValueButNotPropagatedByNonAwaitedAsnycExecutionAsync = InvalidTextValue)
            .Should().NotThrow<ArgumentException>();

        [Fact]
        public void SetPropertyAsyncFailsValidationAndValidationExceptionIsThrownBecauseCallIsExecutedSynchronously() => _ = ViewModelImpl
            .Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionOnInvalidValuePropagatedBySynchronousExecutionAsync = InvalidTextValue)
            .Should().Throw<ArgumentException>();

        [Fact]
        public void SetPropertyAsyncFailsValidationAndValidationExceptionIsThrownBecauseCallExecutedAndAwaitedExplicitly() => _ = ViewModelImpl.Awaiting(viewModel
                                                                                                                                     => viewModel.SetPropertyThrowExceptionOnInvalidValueUsingTrySetValueAsyncExplicitly(InvalidTextValue, nameof(viewModel.ValidatingPropertyThrowExceptionOnInvalidValue)))
            .Should().ThrowAsync<ArgumentException>();

        [Fact]
        public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejected()
        {
            _ = ViewModelImpl
              .Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync = InvalidTextValue)
              .Should().ThrowExactly<ArgumentException>();
            _ = ViewModelImpl.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValueAsync
              .Should().NotBe(InvalidTextValue);
        }

        [Fact]
        public void SetPropertyFailsValidationAndViewModelHasError()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;

            _ = ViewModelImpl.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void SetPropertyAfterPreviousValidationClearsViewModelHasError()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;
            ViewModelImpl.ValidatingPropertyAsync
              = ValidTextValue;

            _ = ViewModelImpl.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void SetPropertyAfterPreviousValidationClearsErrorMessages()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;
            ViewModelImpl.ValidatingPropertyAsync
              = ValidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors();

            _ = errors.Should().BeEmpty();
        }

        [Fact]
        public void SetPropertyFailsValidationAndViewModelPropertyHasError()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;

            _ = ViewModelImpl.PropertyHasError(nameof(ViewModelImpl.ValidatingPropertyAsync)).Should().BeTrue();
        }

        [Fact]
        public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessage()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingPropertyAsync));

            _ = errors.Should().HaveCount(1);
        }

        [Fact]
        public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessage()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;

            IEnumerable errors = ViewModelImpl.GetErrors(nameof(ViewModelImpl.ValidatingPropertyAsync));

            _ = errors.Cast<string>().Should().HaveCount(1);
        }

        [Fact]
        public void TwoPropertyValidationFailsAndGetPropertyErrorsForAllPropertiesReturnsTwoErrors()
        {
            ViewModelImpl.ValidatingPropertyAsync = InvalidTextValue;
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors();

            _ = errors.Should().HaveCount(2);
        }

        [Fact]
        public void PropertyValidationFailsAndGetPropertyErrorsMethodForPropertyReturnsTwoErrors()
        {
            ViewModelImpl.PropertyValidationDelegateAsync = PropertyValidationDelegateTwoErrorsAsync;
            ViewModelImpl.ValidatingPropertyAsync = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingPropertyAsync));

            _ = errors.Should().HaveCount(2);
        }

        [Fact]
        public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrors()
        {
            ViewModelImpl.ValidatingPropertyAsync = InvalidTextValue;
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAsync = InvalidTextValue;

            IEnumerable errors = ViewModelImpl.GetErrors();

            _ = errors.Cast<string>().Should().HaveCount(2);
        }

        [Fact]
        public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicate()
        {
            ViewModelImpl.ValidatingPropertyAsync
              = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingPropertyAsync));

            string firtsErrorMessage = errors.First();
            _ = firtsErrorMessage.Should().Be(UppercaseValidationErrorMessage);
        }

        private void OnPropertyValueChanged(object sender, PropertyValueChangedArgs<object> e)
        {
            _ = sender.Should().BeOfType(SenderType);

            PropertyValueChangedEventInvocationCount++;
            CurrentPropertyValueChangedArgs = e;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _ = sender.Should().BeOfType(SenderType);

            PropertyChangedEventInvocationCount++;
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
            : (false, new[] { UppercaseValidationErrorMessage }));

        private Func<string, Task<(bool IsValid, IEnumerable<object> ErrorMessages)>> PropertyValidationDelegateTwoErrorsAsync
          => text =>
          {
              var errorMessages = new List<object>();
              if (!text.All(char.IsUpper))
              {
                  errorMessages.Add(UppercaseValidationErrorMessage);
              }

              //if (!text.StartsWith(ValidTextValue.First()))
              if (text.First() != ValidTextValue.First())
              {
                  errorMessages.Add(StartsWithValidationErrorMessage);
              }

              (bool, IEnumerable<object> errorMessages) result = (errorMessages.IsEmpty(), errorMessages);
              return Task.FromResult(result);
          };
    }
}
