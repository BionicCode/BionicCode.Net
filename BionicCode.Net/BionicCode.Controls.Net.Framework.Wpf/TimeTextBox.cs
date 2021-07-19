using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace BionicCode.Controls.Net.Framework.Wpf
{
  public class TimeTextBox : TextBox
  {
    #region Overrides of TextBoxBase

    /// <inheritdoc />
    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
      base.OnKeyUp(e);
      switch (e.Key)
      {
        case Key.Enter:
          GetBindingExpression(TextBox.TextProperty).UpdateSource();
          break;
      }
    }

    #endregion

    #region Overrides of UIElement

    /// <inheritdoc />
    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
      base.OnPreviewTextInput(e);
      if (!IsValidInput(e.Text))
      {
        e.Handled = true;
      }
    }

    private bool IsValidInput(string input)
    {
      string lookaheadInput = this.Text.Insert(this.CaretIndex, input);
      return (int.TryParse(input, out _) || input.Equals(":", StringComparison.OrdinalIgnoreCase))
             && IsValidTime(lookaheadInput);
    }

    #endregion


    private bool IsValidTime(string input) =>
      TimeSpan.TryParse(input, new DateTimeFormatInfo(), out _) 
        || (input.EndsWith(":", StringComparison.OrdinalIgnoreCase) 
      &&
      input.Count(character => character.Equals(':')) < 3);
  }
}
