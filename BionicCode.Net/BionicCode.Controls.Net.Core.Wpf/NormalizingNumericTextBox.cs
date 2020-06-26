using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace BionicCode.Controls.Net.Core.Wpf
{
  /// <summary>
  /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
  ///
  /// Step 1a) Using this custom control in a XAML file that exists in the current project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:WpfTestRange.Main"
  ///
  ///
  /// Step 1b) Using this custom control in a XAML file that exists in a different project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:WpfTestRange.Main;assembly=WpfTestRange.Main"
  ///
  /// You will also need to add a project reference from the project where the XAML file lives
  /// to this project and Rebuild to avoid compilation errors:
  ///
  ///     Right click on the target project in the Solution Explorer and
  ///     "Add Reference"->"Projects"->[Browse to and select this project]
  ///
  ///
  /// Step 2)
  /// Go ahead and use your control in the XAML file.
  ///
  ///     <MyNamespace:NormalizingUnitTextBox/>
  ///
  /// </summary>
[TemplatePart(Name = "PART_UnitsItemsHost", Type = typeof(ItemsControl))]
public class NormalizingNumericTextBox : TextBox
{
  public static readonly DependencyProperty UnitsProperty = DependencyProperty.Register(
    "Units",
    typeof(IEnumerable<Unit>),
    typeof(NormalizingNumericTextBox),
    new PropertyMetadata(default(IEnumerable<Unit>), NormalizingNumericTextBox.OnUnitsChanged));

  public IEnumerable<Unit> Units
  {
    get => (IEnumerable<Unit>) GetValue(NormalizingNumericTextBox.UnitsProperty);
    set => SetValue(NormalizingNumericTextBox.UnitsProperty, value);
  }

  public static readonly DependencyProperty SelectedUnitProperty = DependencyProperty.Register(
    "SelectedUnit",
    typeof(Unit),
    typeof(NormalizingNumericTextBox),
    new FrameworkPropertyMetadata(
      default(Unit),
      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
      NormalizingNumericTextBox.OnSelectedUnitChanged));

  public Unit SelectedUnit
  {
    get => (Unit) GetValue(NormalizingNumericTextBox.SelectedUnitProperty);
    set => SetValue(NormalizingNumericTextBox.SelectedUnitProperty, value);
  }

  public static readonly DependencyProperty NormalizedValueProperty = DependencyProperty.Register(
    "NormalizedValue",
    typeof(decimal),
    typeof(NormalizingNumericTextBox),
    new FrameworkPropertyMetadata(
      default(decimal),
      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
      NormalizingNumericTextBox.OnNormalizedValueChanged));

  public decimal NormalizedValue
  {
    get => (decimal) GetValue(NormalizingNumericTextBox.NormalizedValueProperty);
    set => SetValue(NormalizingNumericTextBox.NormalizedValueProperty, value);
  }

  private ItemsControl PART_UnitsItemsHost { get; set; }
  private bool IsNormalizing { get; set; }

  static NormalizingNumericTextBox()
  {
    FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
      typeof(NormalizingNumericTextBox),
      new FrameworkPropertyMetadata(typeof(NormalizingNumericTextBox)));
  }

  public NormalizingNumericTextBox()
  {
  }

  private static void OnNormalizedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var _this = d as NormalizingNumericTextBox;
    _this.ConvertNormalizedValueToNumericText();
  }

  private static void OnSelectedUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    (d as NormalizingNumericTextBox).NormalizeNumericText();
  }

  private static void OnUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var _this = d as NormalizingNumericTextBox;
    _this.SelectedUnit = _this.Units.FirstOrDefault();
  }

  /// <inheritdoc />
  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    this.PART_UnitsItemsHost = GetTemplateChild("PART_UnitsItemsHost") as ItemsControl;

    if (this.PART_UnitsItemsHost == null)
    {
      throw new InvalidOperationException($"{nameof(this.PART_UnitsItemsHost)} not found in ControlTemplate");
    }

    this.PART_UnitsItemsHost.SetBinding(
      Selector.SelectedItemProperty,
      new Binding(nameof(this.SelectedUnit)) {Source = this});
    this.PART_UnitsItemsHost.SetBinding(
      ItemsControl.ItemsSourceProperty,
      new Binding(nameof(this.Units)) {Source = this});
    this.SelectedUnit = this.Units?.FirstOrDefault();
  }

  #region Overrides of TextBoxBase

  /// <inheritdoc />
  protected override void OnTextChanged(TextChangedEventArgs e)
  {
    base.OnTextChanged(e);
    if (this.IsNormalizing)
    {
      return;
    }

    NormalizeNumericText();
  }

  /// <inheritdoc />
  protected override void OnTextInput(TextCompositionEventArgs e)
  {
    // Suppress non numeric characters
    if (!decimal.TryParse(e.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal _))
    {
      e.Handled = true;
      return;
    }

    base.OnTextInput(e);
  }

  #endregion Overrides of TextBoxBase

  private void NormalizeNumericText()
  {
    this.IsNormalizing = true;
    if (decimal.TryParse(this.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal numericValue))
    {
      this.NormalizedValue = numericValue * this.SelectedUnit.BaseFactor;
    }

    this.IsNormalizing = false;
  }

  private void ConvertNormalizedValueToNumericText()
  {
    this.IsNormalizing = true;
    decimal value = this.NormalizedValue / this.SelectedUnit.BaseFactor;
    this.Text = value.ToString(CultureInfo.CurrentCulture);
    this.IsNormalizing = false;
  }
}
}