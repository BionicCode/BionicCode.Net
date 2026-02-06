namespace BionicCode.Utilities.Net
{
    using System.Collections.Generic;

    /// <summary>
    /// The result that must be returned from a custom validation delegate tha matches the <see cref="PropertyValidationDelegate{TValue}"/> delegate.
    /// </summary>
    public class PropertyValidationResult
    {
        /// <summary>
        /// MemberConstructor.
        /// </summary>
        /// <param name="isValid"><see langword="true"/> when the validation has successfully passed. <see langword="false"/> when the validation has failed.</param>
        /// <param name="errorMessages">A collection of error messasge objects that can be displayed in the UI. It's expected that the client will generate one message for each validation error of the currently validated property.</param>
        public PropertyValidationResult(bool isValid, IEnumerable<object> errorMessages)
        {
            IsValid = isValid;
            ErrorMessages = errorMessages;
        }

        /// <summary>
        /// MemberConstructor.
        /// </summary>
        /// <param name="isValid"><see langword="true"/> when the validation has successfully passed. <see langword="false"/> when the validation has failed.</param>
        /// <param name="errorMessage">An error messasge object that can be displayed in the UI.</param>
        public PropertyValidationResult(bool isValid, object errorMessage)
        {
            IsValid = isValid;
            ErrorMessages = new[] { errorMessage };
        }

        internal PropertyValidationResult((bool IsValid, IEnumerable<object> ErrorMessages) validationResult)
        {
            IsValid = validationResult.IsValid;
            ErrorMessages = validationResult.ErrorMessages;
        }

        /// <summary>
        /// Deconstructor.
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="errorMessages"></param>
        /// <remarks>This deconstructor was mainly introduced to suppport backwards compatibility withlegacy versions where the delegate was returning a <c>ValueTuple</c>.</remarks>
        public void Deconstruct(out bool isValid, out IEnumerable<object> errorMessages)
        {
            isValid = IsValid;
            errorMessages = ErrorMessages;
        }

        /// <summary>
        /// Returns whether tha validation was successful or has failed.
        /// </summary>
        /// <value><see langword="true"/> when the validation has successfully passed. <see langword="false"/> when the validation has failed.</value>
        public bool IsValid { get; }

        /// <summary>
        /// A collection of error message objects. 
        /// </summary>
        /// <value>It usually contains one message for each property error.</value>
        public IEnumerable<object> ErrorMessages { get; }
    }
}
