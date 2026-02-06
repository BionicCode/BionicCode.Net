namespace BionicCode.Utilities.Net.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class ViewModelTest : IDisposable
    {

        public ViewModelTest()
        {
            ViewModelImpl = new ViewModelImpl(PropertyValidationDelegateSingleError, PropertyValidationDelegate_Old);
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

        [Fact]
        public void ReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
        {
            ViewModelImpl.NonValidatingProperty = ValidTextValue;

            _ = PropertyChangedEventInvocationCount.Should().Be(1);
        }

        [Fact]
        public void SetPropertyToNullAndReceiveOneDefaultPropertyChangedNotificationWithPropertyNameNonValidatingTextProperty()
        {
            // Initialize test
            ViewModelImpl.NonValidatingProperty = string.Empty;
            PropertyChangedEventInvocationCount = 0;

            // Execute test
            ViewModelImpl.NonValidatingProperty = null;

            _ = PropertyChangedEventInvocationCount.Should().Be(1);
        }

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20
        [Fact]
        public void SilentSetValidatingPropertyWithNoPropertyChangedNotification()
        {
            //using IMonitor<ViewModelImpl> eventMonitor = ViewModelImpl.Monitor();
            //ViewModelImpl.SilentValidatingProperty = ValidTextValue;

            //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentValidatingProperty);
        }

        //[Fact]
        //public void SilentSetValidatingPropertyWithNoPropertyChangedNotificationUsingStringMessageDelegate()
        //{
        //  using IMonitor<ViewModelImpl> eventMonitor = ViewModelImpl.Monitor();
        //  ViewModelImpl.SilentValidatingProperty_Old = ValidTextValue;

        //  eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentValidatingProperty_Old);
        //}

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20
        [Fact]
        public void SetPropertyFailsValidationAndRejectedValueDoesNotRaisePropertyChangedEvent()
        {
            //using IMonitor<ViewModelImpl> eventMonitor = ViewModelImpl.Monitor();
            //ViewModelImpl.ValidatingPropertyRejectInvalidValue
            //  = InvalidTextValue;
            //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.ValidatingPropertyRejectInvalidValue, "beacuse property was set silently");
        }

        [Fact]
        public void SilentSetNonValidatingPropertyWithNoPropertyChangedNotification()
        {
            ViewModelImpl.SilentNonValidatingProperty = ValidTextValue;

            _ = PropertyChangedEventInvocationCount.Should().Be(0);
        }

        [Fact]
        public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextProperty()
        {
            ViewModelImpl.NonValidatingProperty = ValidTextValue;

            _ = PropertyValueChangedEventInvocationCount.Should().Be(1);
        }

        [Fact]
        public void ReceiveOnePropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsNullAndNewValueIsValidText()
        {
            ViewModelImpl.NonValidatingProperty = ValidTextValue;

            _ = PropertyValueChangedEventInvocationCount.Should().Be(1);
        }

        [Fact]
        public void ReceiveNoPropertyChangedAfterFirstSetPropertySucceedsAndSecondValueIsEqualToPreviousValue()
        {
            ViewModelImpl.NonValidatingProperty
              = ValidTextValue;
            ViewModelImpl.NonValidatingProperty
              = ValidTextValue;

            _ = PropertyChangedEventInvocationCount.Should().Be(1);
            _ = ViewModelImpl.NonValidatingProperty.Should().Be(ValidTextValue, "new value equals old value.");
        }

        [Fact]
        public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResettedToPreviousValue()
        {
            ViewModelImpl.ValidatingPropertyRejectInvalidValue
              = ValidTextValue;

            // Should not trigger PropertyChanged
            ViewModelImpl.ValidatingPropertyRejectInvalidValue
              = InvalidTextValue;

            ViewModelImpl.ValidatingPropertyRejectInvalidValue
              = ValidTextValue;

            _ = PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
            _ = ViewModelImpl.ValidatingPropertyRejectInvalidValue.Should().Be(ValidTextValue);
        }

        //[Fact]
        //public void ReceiveOnePropertyChangedAfterSecondSetPropertyFailsValidationAndValueIsRejectedAndPropertyResettedToPreviousValueUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        //    = ValidTextValue;

        //  // Should not trigger PropertyChanged
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        //    = InvalidTextValue;

        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        //    = ValidTextValue;

        //  PropertyChangedEventInvocationCount.Should().Be(1, "second assignment was rejected due to failing validation and third assignment has new value equals old value");
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old.Should().Be(ValidTextValue);
        //}

        [Fact]
        public void ReceiveTwoPropertyValueChangedNotificationWithPropertyNameNonValidatingTextPropertyWhereOldValueIsInvalidTextAndNewValueIsValidText()
        {
            ViewModelImpl.NonValidatingProperty = InvalidTextValue;
            ViewModelImpl.NonValidatingProperty = ValidTextValue;

            _ = PropertyValueChangedEventInvocationCount.Should().Be(2);
            _ = (CurrentPropertyValueChangedArgs.OldValue as string).Should().Be(InvalidTextValue, "it's the old value");
            _ = (CurrentPropertyValueChangedArgs.NewValue as string).Should().Be(ValidTextValue, "it's the new value");
        }

        [Fact]
        public void SetPropertyWithoutValidation()
        {
            ViewModelImpl.NonValidatingProperty
              = ValidTextValue;
            _ = ViewModelImpl.NonValidatingProperty.Should().Be(ValidTextValue);
        }

        [Fact]
        public void SetPropertyWithoutValidationToNull()
        {
            ViewModelImpl.NonValidatingProperty
              = null;
            _ = ViewModelImpl.NonValidatingProperty.Should().BeNull();
        }

        [Fact]
        public void SetPropertySuccessfulValidation()
        {
            ViewModelImpl.ValidatingProperty
              = ValidTextValue;
            _ = ViewModelImpl.ValidatingProperty.Should().Be(ValidTextValue);
        }

        //[Fact]
        //public void SetPropertySuccessfulValidationUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = ValidTextValue;
        //  ViewModelImpl.ValidatingProperty_Old.Should().Be(ValidTextValue);
        //}

        [Fact]
        public void SetPropertySilentlySuccessfulValidation()
        {
            ViewModelImpl.SilentValidatingProperty
              = ValidTextValue;

            _ = ViewModelImpl.SilentValidatingProperty.Should().Be(ValidTextValue);
        }

        //[Fact]
        //public void SetPropertySilentlySuccessfulValidationUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.SilentValidatingProperty_Old
        //    = ValidTextValue;

        //  ViewModelImpl.SilentValidatingProperty_Old.Should().Be(ValidTextValue);
        //}

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20
        [Fact]
        public void SetPropertySilentlyNoValidation()
        {
            //using IMonitor<ViewModelImpl> eventMonitor = ViewModelImpl.Monitor();

            //ViewModelImpl.SilentNonValidatingProperty
            //  = ValidTextValue;

            //eventMonitor.Should().NotRaisePropertyChangeFor(viewModel => viewModel.SilentNonValidatingProperty);
            //_ = ViewModelImpl.SilentNonValidatingProperty.Should().Be(ValidTextValue);
        }

        [Fact]
        public void SetPropertySuccessfulValidationUsingNullAsPropertyName()
        {
            ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull
              = ValidTextValue;

            _ = ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull
              .Should().Be(ValidTextValue);
        }

        //[Fact]
        //public void SetPropertySuccessfulValidationUsingNullAsPropertyNameUsingStringMessageDelegatei()
        //{
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old
        //    = ValidTextValue;

        //  ViewModelImpl.ValidatingPropertyRejectInvalidValueAndPropertyNameIsNull_Old
        //    .Should().Be(ValidTextValue);
        //}

        [Fact]
        public void SetPropertyFailsValidationAndValueIsAccepted()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;

            _ = ViewModelImpl.ValidatingProperty.Should().Be(InvalidTextValue);
        }

        //[Fact]
        //public void SetPropertyFailsValidationAndValueIsAcceptedUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;

        //  ViewModelImpl.ValidatingProperty_Old.Should().Be(InvalidTextValue);
        //}

        [Fact]
        public void SetPropertyFailsValidationAndValueIsRejected()
        {
            ViewModelImpl.ValidatingPropertyRejectInvalidValue
              = InvalidTextValue;

            _ = ViewModelImpl.ValidatingPropertyRejectInvalidValue.Should().NotBe(InvalidTextValue);
        }

        //[Fact]
        //public void SetPropertyFailsValidationAndValueIsRejectedUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old
        //    = InvalidTextValue;

        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old.Should().NotBe(InvalidTextValue);
        //}

        [Fact]
        public void SetPropertyFailsValidationAndValidationExceptionIsThrown()
        {
            _ = ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionOnInvalidValue = InvalidTextValue).Should().ThrowExactly<ArgumentException>();
            _ = ViewModelImpl.ValidatingPropertyThrowExceptionOnInvalidValue.Should().Be(InvalidTextValue);
        }

        //[Fact]
        //public void SetPropertyFailsValidationAndValidationExceptionIsThrownUsingStringMessageDelegate()
        //{
        //  _ = ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowingExceptionOnInvalidValue_Old = InvalidTextValue).Should().ThrowExactly<ArgumentException>();
        //  _ = ViewModelImpl.ValidatingPropertyThrowingExceptionOnInvalidValue_Old.Should().Be(InvalidTextValue);
        //}

        [Fact]
        public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejected()
        {
            _ = ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue = InvalidTextValue).Should().ThrowExactly<ArgumentException>();
            _ = ViewModelImpl.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue.Should().NotBe(InvalidTextValue);
        }

        //[Fact]
        //public void SetPropertyFailsValidationAndValidationExceptionIsThrownAndValueRejectedUsingStringMessageDelegate()
        //{
        //  _ = ViewModelImpl.Invoking(viewModel => viewModel.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old = InvalidTextValue).Should().ThrowExactly<ArgumentException>();
        //  _ = ViewModelImpl.ValidatingPropertyThrowExceptionAndRejectValueOnInvalidValue_Old.Should().NotBe(InvalidTextValue);
        //}

        [Fact]
        public void SetPropertyFailsValidationAndViewModelHasError()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;

            _ = ViewModelImpl.HasErrors.Should().BeTrue();
        }

        //[Fact]
        //public void SetPropertyFailsValidationAndViewModelHasErrorUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;

        //  ViewModelImpl.HasErrors.Should().BeTrue();
        //}

        [Fact]
        public void SetPropertyAfterPreviousValidationClearsViewModelHasError()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;
            ViewModelImpl.ValidatingProperty
              = ValidTextValue;

            _ = ViewModelImpl.HasErrors.Should().BeFalse();
        }

        //[Fact]
        //public void SetPropertyAfterPreviousValidationClearsViewModelHasErrorUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;
        //  ViewModelImpl.ValidatingProperty_Old
        //    = ValidTextValue;

        //  ViewModelImpl.HasErrors.Should().BeFalse();
        //}

        [Fact]
        public void SetPropertyAfterPreviousValidationClearsErrorMessages()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;
            ViewModelImpl.ValidatingProperty
              = ValidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors();

            _ = errors.Should().BeEmpty();
        }

        //[Fact]
        //public void SetPropertyAfterPreviousValidationClearsErrorMessagesUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;
        //  ViewModelImpl.ValidatingProperty_Old
        //    = ValidTextValue;

        //  IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors();

        //  errors.Should().BeEmpty();
        //}

        [Fact]
        public void SetPropertyFailsValidationAndViewModelPropertyHasError()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;

            _ = ViewModelImpl.PropertyHasError(nameof(ViewModelImpl.ValidatingProperty)).Should().BeTrue();
        }

        //[Fact]
        //public void SetPropertyFailsValidationAndViewModelPropertyHasErrorUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;

        //  ViewModelImpl.PropertyHasError(nameof(ViewModelImpl.ValidatingProperty_Old)).Should().BeTrue();
        //}

        [Fact]
        public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessage()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingProperty));

            _ = errors.Should().HaveCount(1);
        }

        //[Fact]
        //public void SinglePropertyValidationFailsAndGetPropertyErrorsByNameReturnsSingleErrorMessageUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;

        //  IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingProperty_Old));

        //  errors.Should().HaveCount(1);
        //}

        [Fact]
        public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessage()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;

            IEnumerable errors = ViewModelImpl.GetErrors(nameof(ViewModelImpl.ValidatingProperty));

            _ = errors.Cast<string>().Should().HaveCount(1);
        }

        //[Fact]
        //public void SinglePropertyValidationFailsAndGetErrorsByNameReturnsSingleErrorMessageUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;

        //  IEnumerable errors = ViewModelImpl.GetErrors(nameof(ViewModelImpl.ValidatingProperty_Old));

        //  errors.Cast<string>().Should().HaveCount(1);
        //}

        [Fact]
        public void TwoPropertyValidationFailsAndGetPropertyErrorsForAllPropertiesReturnsTwoErrors()
        {
            ViewModelImpl.ValidatingProperty = InvalidTextValue;
            ViewModelImpl.ValidatingPropertyRejectInvalidValue = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors();

            _ = errors.Should().HaveCount(2);
        }

        //[Fact]
        //public void TwoPropertyValidationFailsAndGetPropertyErrorsForAllPropertiesReturnsTwoErrorsUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old = InvalidTextValue;
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old = InvalidTextValue;

        //  IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors();

        //  errors.Should().HaveCount(2);
        //}

        [Fact]
        public void PropertyValidationFailsAndGetPropertyErrorsMethodForPropertyReturnsTwoErrors()
        {
            ViewModelImpl.PropertyValidationDelegate = PropertyValidationDelegateTwoErrors;
            ViewModelImpl.ValidatingProperty = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingProperty));

            _ = errors.Should().HaveCount(2);
        }

        //[Fact]
        //public void PropertyValidationFailsAndGetPropertyErrorsMethodForPropertyReturnsTwoErrorsUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.PropertyValidationDelegate_Old = PropertyValidationDelegateTwoErrors_Old;
        //  ViewModelImpl.ValidatingProperty_Old = InvalidTextValue;

        //  IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingProperty_Old));

        //  errors.Should().HaveCount(2);
        //}

        [Fact]
        public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrors()
        {
            ViewModelImpl.ValidatingProperty = InvalidTextValue;
            ViewModelImpl.ValidatingPropertyRejectInvalidValue = InvalidTextValue;

            IEnumerable errors = ViewModelImpl.GetErrors();

            _ = errors.Cast<string>().Should().HaveCount(2);
        }

        //[Fact]
        //public void TwoPropertyValidationFailsAndGetErrorsReturnsTwoErrorsUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old = InvalidTextValue;
        //  ViewModelImpl.ValidatingPropertyRejectInvalidValue_Old = InvalidTextValue;

        //  IEnumerable errors = ViewModelImpl.GetErrors();

        //  errors.Cast<string>().Should().HaveCount(2);
        //}

        [Fact]
        public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicate()
        {
            ViewModelImpl.ValidatingProperty
              = InvalidTextValue;

            IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingProperty));

            string firtsErrorMessage = errors.First();
            _ = firtsErrorMessage.Should().Be(UppercaseValidationErrorMessage);
        }

        //[Fact]
        //public void SinglePropertyValidationFailsAndGetSingleErrorMessageThatMatchesPredicateUsingStringMessageDelegate()
        //{
        //  ViewModelImpl.ValidatingProperty_Old
        //    = InvalidTextValue;

        //  IEnumerable<string> errors = ViewModelImpl.GetPropertyErrors(nameof(ViewModelImpl.ValidatingProperty_Old));

        //  string firtsErrorMessage = errors.First();
        //  firtsErrorMessage.Should().Be(UppercaseValidationErrorMessage);
        //}

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

        private Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> PropertyValidationDelegateSingleError
          => text => text.All(char.IsUpper)
          ? (true, Enumerable.Empty<object>())
          : (false, new[] { UppercaseValidationErrorMessage });

        private Func<string, (bool IsValid, IEnumerable<object> ErrorMessages)> PropertyValidationDelegateTwoErrors
          => text =>
          {
              var errorMessages = new List<string>();
              if (!text.All(char.IsUpper))
              {
                  errorMessages.Add(UppercaseValidationErrorMessage);
              }

              if (text.First() != ValidTextValue.First())
              {
                  errorMessages.Add(StartsWithValidationErrorMessage);
              }

              return (errorMessages.IsEmpty(), errorMessages);
          };

        private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegate_Old
          => text => text.All(char.IsUpper)
          ? (true, Enumerable.Empty<string>())
          : (false, new[] { UppercaseValidationErrorMessage });

        private Func<string, (bool IsValid, IEnumerable<string> ErrorMessages)> PropertyValidationDelegateTwoErrors_Old
          => text =>
          {
              var errorMessages = new List<string>();
              if (!text.All(char.IsUpper))
              {
                  errorMessages.Add(UppercaseValidationErrorMessage);
              }

              if (text.First() != ValidTextValue.First())
              {
                  errorMessages.Add(StartsWithValidationErrorMessage);
              }

              return (errorMessages.IsEmpty(), errorMessages);
          };
    }
}
