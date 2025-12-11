namespace BionicCode.Utilities.Net
{
    using System;
    using System.ComponentModel;

    public abstract partial class ViewModelCommon
    {
        /// <summary>
        /// Configuration objectect to configure the behavior of the property set methods.
        /// </summary>
        public class SetValueOptions
        {
            private static readonly Lazy<SetValueOptions> DefaultOptionsFactory = new Lazy<SetValueOptions>(() => new SetValueOptions());

            /// <summary>
            ///  Gets the default behavior for the property set methods. <br/>By default the configuration sets <see cref="IsRejectEqualValuesEnabled"/> to <see langword="true"/>, <see cref="IsThrowExceptionOnValidationErrorEnabled"/> to <see langword="false"/> and <see cref="IsRejectInvalidValueEnabled"/> to <see langword="false"/>.
            /// </summary>
            /// <value>An instance configured with the default values.</value>
            public static SetValueOptions Default => SetValueOptions.DefaultOptionsFactory.Value;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="isRejectInvalidValueEnabled">Controls how the invalid property value is stored. Use this to ensure that the view model in a valid state. See <see cref="IsRejectInvalidValueEnabled"/>.</param>
            /// <param name="isThrowExceptionOnValidationErrorEnable">Controls if throwing an <exception cref="ArgumentException"></exception> on a failed validation is enabled. See <see cref="IsThrowExceptionOnValidationErrorEnabled"/>.</param>
            /// <param name="isRejectEqualValuesEnabled">Controls if the equality check before setting the value is enabled. See <see cref="IsRejectEqualValuesEnabled"/>.</param>
            public SetValueOptions(bool isRejectInvalidValueEnabled, bool isThrowExceptionOnValidationErrorEnable, bool isRejectEqualValuesEnabled)
            {
                this.IsRejectInvalidValueEnabled = isRejectInvalidValueEnabled;
                this.IsThrowExceptionOnValidationErrorEnabled = isThrowExceptionOnValidationErrorEnable;
                this.IsRejectEqualValuesEnabled = isRejectEqualValuesEnabled;
            }

            private SetValueOptions() : this(false, false, true)
            { }

            /// <summary>
            /// Gets how the invalid property value is stored. Use this to ensure that the view model in a valid state.
            /// </summary>
            /// <value>If <see langword="true"/> the invalid value is not stored to the backing field.<br/> The default is <see langword="false"/>.</value>
            public bool IsRejectInvalidValueEnabled { get; }
            /// <summary>
            /// Gets if throwing an <exception cref="ArgumentException"></exception> on a failed validation is enabled. 
            /// <br/>Use this when <c>ValidatesOnExceptions</c> on a <c>Binding</c> is set to <see langword="true"/>
            /// </summary>
            /// <value><see langword="true"/> if throwing an <exception cref="ArgumentException"></exception> on a failed validation is enabled. Otherwise <see langword="false"/>. 
            /// <br/>The default is <see langword="false"/>.</value>
            public bool IsThrowExceptionOnValidationErrorEnabled { get; }

            /// <summary>
            /// Gets if the equality check before setting the value is enabled.
            /// </summary>
            /// <value>If <see langword="true"/> the equality check before setting the value to avoid raising the <see cref="INotifyPropertyChanged.PropertyChanged"/> event on equality is enabled. If <see langword="false"/> equality check is deisabled. This will always raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
            /// <br/>the default is <see langword="true"/>.</value>
            public bool IsRejectEqualValuesEnabled { get; }
        }
    }
}
