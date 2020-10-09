using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using BionicCode.Utilities.Net.Standard;

namespace BionicCode.Utilities.Net.Framework.Wpf.Markup
{
  /// <summary>
  /// XAML extension to invert the <see cref="Value"/> property using the current <see cref="IValueInverter"/>. Supports every <see cref="MarkupExtension"/> e.g., <see cref="BindingBase"/>.
  /// </summary>
  /// <example>Provide the value locally or via data binding:
  /// <para><code>&lt;TextBox Text="{Invert True}" /&gt; // returns: False</code>
  /// <code>&lt;TextBox Text="{Invert 12}" /&gt; // returns: -12</code>
  /// <code>&lt;TextBox Text="{Invert {x:Static Visibility.Hidden}}" /&gt; // returns: Visibility.Visible</code>
  /// <code>&lt;TextBox Text="{Invert {Binding TextValue}}" /&gt;</code></para></example>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#invertExtension">See advanced example</seealso>
  public class InvertExtension : MarkupExtension
  {
    /// <summary>
    /// The Value to invert. Can everything that the provided <see cref="ValueInverter"/> can invert. The value can also be a <see cref="BindingBase"/> or any other <see cref="MarkupExtension"/> that can provide the invertible value.
    /// </summary>
    /// <value>The value to invert.</value>
    public object Value { get; set; }

    /// <summary>
    /// The inversion mode.
    /// </summary>
    /// <value>A value of <see cref="InversionMode"/>.</value>
    public InversionMode Mode { get; set; }

    /// <summary>
    /// The implementation of <see cref="IValueInverter"/>.
    /// </summary>
    /// <value>An implementation of <see cref="IValueInverter"/> to ise for the conversion.</value>
    public IValueInverter ValueInverter { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public InvertExtension() : this(DependencyProperty.UnsetValue)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">Any value that can be converted by the provided <see cref="ValueInverter"/>. Can be any <see cref="MarkupExtension"/> like <see cref="BindingBase"/> that can provide a valid value.</param>
    public InvertExtension(object value)
    {
      this.Value = value;
      this.ValueInverter = new DefaultValueInverter();
      this.Mode = InversionMode.Default;
    }

    #region Overrides of MarkupExtension

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      object valueToInvert;
      if (this.Value is MarkupExtension innerMarkupExtension)
      {
        valueToInvert = GetValueToInvertFromMarkupExtension(innerMarkupExtension, serviceProvider);
        if (valueToInvert is BindingBase bindingMarkupExtension)
        {
          return bindingMarkupExtension.ProvideValue(serviceProvider);
        }
      }
      else
      {
        valueToInvert = this.Value;
      }

      var provideValueTargetService = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
      Type targetPropertyType = (provideValueTargetService.TargetProperty as DependencyProperty).PropertyType;

      return valueToInvert == DependencyProperty.UnsetValue
        ? valueToInvert
        : this.ValueInverter.TryInvertValue(valueToInvert, out object invertedValue)
          ? targetPropertyType.Equals(typeof(string))
            ? invertedValue.ToString()
            : invertedValue
          : valueToInvert;
    }
    #endregion


    /// <summary>
    /// Extracts the value from a <see cref="MarkupExtension"/>.
    /// </summary>
    /// <param name="wrappedMarkupExtension">The <see cref="MarkupExtension"/> to extract the value form.</param>
    /// <param name="serviceProvider">The instance of the <see cref="IServiceProvider"/> which was passed to the <see cref="MarkupExtension.ProvideValue"/> override.</param>
    /// <returns>The value of <paramref name="wrappedMarkupExtension"/>.</returns>
    protected object GetValueToInvertFromMarkupExtension(MarkupExtension wrappedMarkupExtension,
      IServiceProvider serviceProvider)
    {
      if (wrappedMarkupExtension is Binding bindingExpression)
      {
        return GetValueFomBinding(serviceProvider, bindingExpression);
      }

      return wrappedMarkupExtension.ProvideValue(serviceProvider);
    }

    private object GetValueFomBinding(IServiceProvider serviceProvider, Binding bindingExpression)
    {
      var provideValueTargetService = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
      object targetObject = provideValueTargetService?.TargetObject;
      if (targetObject == null)
      {
        return this;
      }

      var bindingResolver = new BindingResolver(
        targetObject as FrameworkElement,
        provideValueTargetService.TargetProperty as DependencyProperty)
      {
        ResolvedTargetValueFilter = InvertBindingTarget,
        ResolvedSourceValueFilter = InvertBindingSource
      };

      return bindingResolver.ResolveBinding(bindingExpression);
    }

    private object InvertBindingTarget(object resolvedValue) =>
      this.Mode != InversionMode.OneWay && this.ValueInverter.TryInvertValue(resolvedValue, out object invertedValue)
        ? invertedValue
        : resolvedValue;

    private object InvertBindingSource(object resolvedValue) =>
      this.Mode != InversionMode.OneWayToSource && this.ValueInverter.TryInvertValue(resolvedValue, out object invertedValue)
        ? invertedValue
        : resolvedValue;
  }
}