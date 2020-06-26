#region Info

// //  
// BionicCode.BionicUtilities.Net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BionicCode.Utilities.Net.Standard;
using BionicCode.Utilities.Net.Standard.Collections.Generic;

namespace BionicCode.Utilities.Net.Framework.Markup
{
  internal class BindingResolver : FrameworkElement
  {
    #region ResolvedValue attached property

    public static readonly DependencyProperty ResolvedValueProperty = DependencyProperty.RegisterAttached(
      "ResolvedValue", typeof(object), typeof(BindingResolver), new PropertyMetadata(default(object), BindingResolver.OnResolvedValueChanged));

    public static void SetResolvedValue(DependencyObject attachingElement, object value) => attachingElement.SetValue(BindingResolver.ResolvedValueProperty, value);

    public static object GetResolvedValue(DependencyObject attachingElement) => (object)attachingElement.GetValue(BindingResolver.ResolvedValueProperty);

    #endregion ResolvedValue attached property

    public DependencyProperty TargetProperty { get; set; }
    public WeakReference Target { get; set; }
    public InversionMode InversionMode { get; set; }
    public IValueInverter ValueInverter { get; set; }
    private bool IsUpDating { get; set; }
    private static WeakReferenceKeyDictionary<WeakReference, BindingResolver> BindingTargetToBindingResolversMap { get; } = new WeakReferenceKeyDictionary<WeakReference, BindingResolver>();

    public BindingResolver(DependencyObject target, DependencyProperty targetProperty)
    {
      if (target == null)
      {
        throw new ArgumentNullException(nameof(target));
      }
      if (targetProperty == null)
      {
        throw new ArgumentNullException(nameof(targetProperty));
      }
      this.Target = new WeakReference(target);
      this.TargetProperty = targetProperty;
      this.ValueInverter = new DefaultValueInverter();
      this.InversionMode = InversionMode.Default;
    }

    private void AddBindingTargetToLookupTable()
    {
      BindingResolver.BindingTargetToBindingResolversMap.Add(this.Target, this);
    }

    public object ResolveBinding(Binding bindingExpression, InversionMode inversionMode)
    {
      var bindingTarget = this.Target.Target as DependencyObject;
      if (bindingTarget == null)
      {
        throw new InvalidOperationException("Unable to resolve sourceBinding. Binding target is 'null', because the reference has already been garbage collected.");
      }

      AddBindingTargetToLookupTable();

      Binding binding = bindingExpression;
      this.InversionMode = inversionMode == InversionMode.Default
        ? BindingResolver.GetInversionModeFromBindingMode(binding.Mode)
        : inversionMode;

      Binding sourceBinding = CloneBinding(binding);
      BindingOperations.SetBinding(
        bindingTarget,
        BindingResolver.ResolvedValueProperty,
        sourceBinding);

      Binding targetBinding = CloneBinding(binding, this);
      targetBinding.Path = new PropertyPath(BindingResolver.ResolvedValueProperty);

      return targetBinding;
    }

    private Binding CloneBinding(Binding binding)
    {
      Binding clonedBinding;
      if (!string.IsNullOrWhiteSpace(binding.ElementName))
      {
        clonedBinding = CloneBinding(binding, binding.ElementName);
      }
      else if (binding.Source != null)
      {
        clonedBinding = CloneBinding(binding, binding.Source);
      }
      else if (binding.RelativeSource != null)
      {
        clonedBinding = CloneBinding(binding, binding.RelativeSource);
      }
      else
      {
        clonedBinding = CloneBindingWithoutSource(binding);
      }

      return clonedBinding;
    }

    private Binding CloneBinding(Binding binding, object bindingSource)
    {
      Binding clonedBinding = CloneBindingWithoutSource(binding);
      clonedBinding.Source = bindingSource;
      return clonedBinding;
    }

    private Binding CloneBinding(Binding binding, RelativeSource relativeSource)
    {
      Binding clonedBinding = CloneBindingWithoutSource(binding);
      clonedBinding.RelativeSource = relativeSource;
      return clonedBinding;
    }

    private Binding CloneBinding(Binding binding, string elementName)
    {
      Binding clonedBinding = CloneBindingWithoutSource(binding);
      clonedBinding.ElementName = elementName;
      return clonedBinding;
    }

    private MultiBinding CloneBinding(MultiBinding binding, IEnumerable<BindingBase> bindings)
    {
      MultiBinding clonedBinding = CloneBindingWithoutSource(binding);
      bindings.ToList().ForEach(clonedBinding.Bindings.Add);
      return clonedBinding;
    }

    private PriorityBinding CloneBinding(PriorityBinding binding, IEnumerable<BindingBase> bindings)
    {
      PriorityBinding clonedBinding = CloneBindingWithoutSource(binding);
      bindings.ToList().ForEach(clonedBinding.Bindings.Add);
      return clonedBinding;
    }

    private TBinding CloneBindingWithoutSource<TBinding>(TBinding sourceBinding) where TBinding : BindingBase, new()
    {
      var clonedBinding = new TBinding();
      switch (sourceBinding)
      {
        case Binding binding:
        {
          var newBinding = clonedBinding as Binding;
          newBinding.AsyncState = binding.AsyncState;
          newBinding.BindingGroupName = binding.BindingGroupName;
          newBinding.BindsDirectlyToSource = binding.BindsDirectlyToSource;
          newBinding.Converter = binding.Converter;
          newBinding.ConverterCulture = binding.ConverterCulture;
          newBinding.ConverterParameter = binding.ConverterParameter;
          newBinding.FallbackValue = binding.FallbackValue;
          newBinding.IsAsync = binding.IsAsync;
          newBinding.Mode = binding.Mode;
          newBinding.NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated;
          newBinding.NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated;
          newBinding.NotifyOnValidationError = binding.NotifyOnValidationError;
          newBinding.Path = binding.Path;
          newBinding.StringFormat = binding.StringFormat;
          newBinding.TargetNullValue = binding.TargetNullValue;
          newBinding.UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter;
          newBinding.UpdateSourceTrigger = binding.UpdateSourceTrigger;
          newBinding.ValidatesOnDataErrors = binding.ValidatesOnDataErrors;
          newBinding.ValidatesOnExceptions = binding.ValidatesOnExceptions;
          newBinding.XPath = binding.XPath;
          newBinding.Delay = binding.Delay;
          newBinding.ValidatesOnNotifyDataErrors = binding.ValidatesOnNotifyDataErrors;
          binding.ValidationRules.ToList().ForEach(newBinding.ValidationRules.Add);
          break;
        }
        case PriorityBinding priorityBinding:
        {
          var newBinding = clonedBinding as PriorityBinding;
          newBinding.BindingGroupName = priorityBinding.BindingGroupName;
          newBinding.FallbackValue = priorityBinding.FallbackValue;
          newBinding.StringFormat = priorityBinding.StringFormat;
          newBinding.TargetNullValue = priorityBinding.TargetNullValue;
          newBinding.Delay = priorityBinding.Delay;
          break;
        }
        case MultiBinding multiBinding:
        {
          var newBinding = clonedBinding as MultiBinding;
          newBinding.BindingGroupName = multiBinding.BindingGroupName;
          newBinding.Converter = multiBinding.Converter;
          newBinding.ConverterCulture = multiBinding.ConverterCulture;
          newBinding.ConverterParameter = multiBinding.ConverterParameter;
          newBinding.FallbackValue = multiBinding.FallbackValue;
          newBinding.Mode = multiBinding.Mode;
          newBinding.NotifyOnSourceUpdated = multiBinding.NotifyOnSourceUpdated;
          newBinding.NotifyOnTargetUpdated = multiBinding.NotifyOnTargetUpdated;
          newBinding.NotifyOnValidationError = multiBinding.NotifyOnValidationError;
          newBinding.StringFormat = multiBinding.StringFormat;
          newBinding.TargetNullValue = multiBinding.TargetNullValue;
          newBinding.UpdateSourceExceptionFilter = multiBinding.UpdateSourceExceptionFilter;
          newBinding.UpdateSourceTrigger = multiBinding.UpdateSourceTrigger;
          newBinding.ValidatesOnDataErrors = multiBinding.ValidatesOnDataErrors;
          newBinding.ValidatesOnExceptions = multiBinding.ValidatesOnExceptions;
          newBinding.Delay = multiBinding.Delay;
          newBinding.ValidatesOnNotifyDataErrors = multiBinding.ValidatesOnNotifyDataErrors;
          multiBinding.ValidationRules.ToList().ForEach(newBinding.ValidationRules.Add);
          break;
        }
        default: return null;
      }

      return clonedBinding;
    }

    private static void OnResolvedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DependencyObject bindingTarget = null;

      if (d is BindingResolver bindingResolver)
      {
        if (bindingResolver.IsUpDating)
        {
          return;
        }

        bindingResolver.IsUpDating = true;
        bindingResolver.UpdateSource();
        bindingTarget = bindingResolver.Target.Target as DependencyObject;
        bindingResolver.IsUpDating = false;
      }
      else
      {
        if (BindingResolver.BindingTargetToBindingResolversMap.TryGetValue(d, out bindingResolver))
        {
          if (bindingResolver.IsUpDating)
          {
            return;
          }

          bindingResolver.IsUpDating = true;
          bindingResolver.UpdateTarget();
          bindingTarget = bindingResolver.Target.Target as DependencyObject;
          bindingResolver.IsUpDating = false;
        }
      }
    }

    private static bool TryClearBindings(DependencyObject bindingTarget, BindingResolver bindingResolver)
    {
      if (bindingTarget == null)
      {
        return false;
      }

      Binding binding = BindingOperations.GetBinding(bindingTarget, bindingResolver.TargetProperty);
      if (binding != null && binding.Mode == BindingMode.OneTime)
      {
        BindingOperations.ClearBinding(bindingTarget, BindingResolver.ResolvedValueProperty);
        BindingOperations.ClearBinding(bindingTarget, bindingResolver.TargetProperty);
      }

      return true;
    }

    private static InversionMode GetInversionModeFromBindingMode(BindingMode bindingMode)
    {
      switch (bindingMode)
      {
        case BindingMode.Default: return InversionMode.OneWay;
        case BindingMode.OneWay: return InversionMode.OneWay;
        case BindingMode.OneWayToSource: return InversionMode.OneWayToSource;
        case BindingMode.TwoWay: return InversionMode.TwoWay;
        case BindingMode.OneTime: return InversionMode.OneTime;
        default: return InversionMode.OneWay;
      }
    }

    private void UpdateTarget()
    {
      var target = this.Target.Target as DependencyObject;
      if (target == null)
      {
        return;
      }

      object resolvedValue = BindingResolver.GetResolvedValue(target);
      object value = this.InversionMode != InversionMode.OneWayToSource
        ? this.ValueInverter.InvertValue(resolvedValue)
        : resolvedValue;

      BindingResolver.SetResolvedValue(this, value);
    }

    private void UpdateSource()
    {
      var target = this.Target.Target as DependencyObject;
      if (target == null)
      {
        return;
      }

      object resolvedValue = BindingResolver.GetResolvedValue(this);
      object value = this.InversionMode != InversionMode.OneWay
        ? this.ValueInverter.InvertValue(resolvedValue)
        : resolvedValue;

      BindingResolver.SetResolvedValue(target, value);
    }
  }
}