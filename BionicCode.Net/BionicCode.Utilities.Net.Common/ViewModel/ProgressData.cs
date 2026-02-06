namespace BionicCode.Utilities.Net
{
    using System;

    public readonly struct ProgressData : IEquatable<ProgressData>
    {
        /// <summary>
        /// Data model to report progress to a implementation of <see cref="IProgressReporterCommon"/>. When using the <see cref="IProgress{T}"/> returned from the <see cref="IProgressReporterCommon.CreateProgressReporterFromCurrentThread"/> method, the <see cref="ProgressData"/> serves as the argument.
        /// </summary>
        /// <param name="message">A progress message.</param>
        /// <param name="progress">The progress value.</param>
        public ProgressData(string message, double progress)
        {
            Message = message;
            Progress = progress;
        }

        /// <summary>
        /// The progress message text.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// The progress value.
        /// </summary>
        public double Progress { get; }

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public static bool operator ==(ProgressData left, ProgressData right) => left.Equals(right);

        public static bool operator !=(ProgressData left, ProgressData right) => !(left == right);

        public bool Equals(ProgressData other) => throw new NotImplementedException();
    }
}
