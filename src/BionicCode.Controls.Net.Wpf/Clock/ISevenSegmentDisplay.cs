namespace BionicCode.Controls.Net.Wpf
{
    #region Info

    // 2021/02/04  12:21
    // BionicCode.Controls.Net.Wpf

    #endregion

    using System.Collections.Generic;

    public interface ISevenSegmentDisplay
    {
        SortedSet<ISevenSegmentDigit> Digits { get; }
    }
}
