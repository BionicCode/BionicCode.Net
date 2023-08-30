namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using MathNet = System.Math;

#if !NETSTANDARD
  using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

  /// <summary>
  /// Collection of extension methods e.g. visual tree traversal
  /// </summary>
  public static partial class HelperExtensions
  {
    /// <summary>
    /// Traverses the visual tree towards the root until an element with a matching element name is found.
    /// </summary>
    /// <typeparam name="TParent">The type the visual parent must match.</typeparam>
    /// <param name="child"></param>
    /// <param name="resultElement"></param>
    /// <returns></returns>
#if NET
    public static bool TryFindVisualParentElement<TParent>(this DependencyObject child, out TParent? resultElement)
      where TParent : DependencyObject
#else
    public static bool TryFindVisualParentElement<TParent>(this DependencyObject child, out TParent resultElement)
      where TParent : DependencyObject
#endif
    {
      resultElement = null;

      DependencyObject parentElement = VisualTreeHelper.GetParent(child);

      if (parentElement is TParent parent)
      {
        resultElement = parent;
        return true;
      }

      return parentElement?.TryFindVisualParentElement(out resultElement) ?? false;
    }

    /// <summary>
    /// Traverses the visual tree towards the root until an element with a matching element type is found.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="elementName">The element name the visual parent must match.</param>
    /// <param name="resultElement"></param>
    /// <returns></returns>
#if NET
    public static bool TryFindVisualParentElementByName<TChild>(
      this DependencyObject child,
      string elementName,
      out TChild? resultElement) where TChild : FrameworkElement
#else
    public static bool TryFindVisualParentElementByName<TChild>(
      this DependencyObject child,
      string elementName,
      out TChild resultElement) where TChild : FrameworkElement
#endif
    {
      resultElement = null;

      DependencyObject parentElement = VisualTreeHelper.GetParent(child);

      if (parentElement is FrameworkElement frameworkElement &&
          frameworkElement.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase))
      {
        resultElement = frameworkElement as TChild;
        return true;
      }

      return parentElement?.TryFindVisualParentElementByName(elementName, out resultElement) ?? false;
    }

    /// <summary>
    /// Traverses the visual tree towards the leafs until an element with a matching element type is found.
    /// </summary>
    /// <typeparam name="TChild">The type the visual child must match.</typeparam>
    /// <param name="parent"></param>
    /// <param name="resultElement"></param>
    /// <returns></returns>
#if NET
    public static bool TryFindVisualChildElement<TChild>(this DependencyObject parent, out TChild? resultElement)
      where TChild : DependencyObject
#else
    public static bool TryFindVisualChildElement<TChild>(this DependencyObject parent, out TChild resultElement)
      where TChild : DependencyObject
#endif
    {
      resultElement = null;

      if (parent is System.Windows.Controls.Primitives.Popup popup)
      {
        parent = popup.Child;
        if (parent == null)
        {
          return false;
        }
      }

      for (var childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(parent); childIndex++)
      {
        DependencyObject childElement = VisualTreeHelper.GetChild(parent, childIndex);

        if (childElement is TChild child)
        {
          resultElement = child;
          return true;
        }

        if (childElement.TryFindVisualChildElement(out resultElement))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Traverses the visual tree towards the leafs until an element with a matching element name is found.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="childElementName">The name the visual child's name must match.</param>
    /// <param name="resultElement">The found element or <c>null</c> if no matching element was found.</param>
    /// <returns><c>true</c> when an element with the specified <paramref name="childElementName"/> was found, otherwise <c>false</c>.</returns>
#if NET
    public static bool TryFindVisualChildElementByName<TChild>(
      this DependencyObject parent,
      string childElementName,
      out TChild? resultElement) where TChild : FrameworkElement
#else
    public static bool TryFindVisualChildElementByName<TChild>(
      this DependencyObject parent,
      string childElementName,
      out TChild resultElement) where TChild : FrameworkElement
#endif
    {
      resultElement = null;
      
      if (parent is System.Windows.Controls.Primitives.Popup popup)
      {
        parent = popup.Child;
        if (parent == null)
        {
          return false;
        }
      }

      for (var childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(parent); childIndex++)
      {
        DependencyObject childElement = VisualTreeHelper.GetChild(parent, childIndex);

        if (childElement is FrameworkElement uiElement && uiElement.Name.Equals(
          childElementName,
          StringComparison.OrdinalIgnoreCase))
        {
          resultElement = uiElement as TChild;
          return true;
        }

        if (childElement.TryFindVisualChildElementByName(childElementName, out resultElement))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Traverses the visual tree towards the leafs until all elements with a matching element type is found.
    /// Returns an <see cref="IEnumerable{T}"/> to enable deferred traversal.
    /// </summary>
    /// <typeparam name="TChildren">The type the visual children must match.</typeparam>
    /// <param name="parent">The current extended <see cref="DependencyObject"/>.</param>
    /// <returns>An enumerable collection of matching visual child elements.</returns>
    public static IEnumerable<TChildren> EnumerateVisualChildElements<TChildren>(this DependencyObject parent)
      where TChildren : DependencyObject
    {
      for (var childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(parent); childIndex++)
      {
        DependencyObject childElement = VisualTreeHelper.GetChild(parent, childIndex);

        if (childElement is System.Windows.Controls.Primitives.Popup popup)
        {
          childElement = popup.Child;
        }

        if (childElement is TChildren element)
        {
          yield return element;
        }

        foreach (TChildren visualChildElement in childElement.EnumerateVisualChildElements<TChildren>())
        {
          yield return visualChildElement;
        }
      }
    }

    /// <summary>
    /// Deep clones a <see cref="UIElement"/>.
    /// </summary>
    /// <param name="elementToClone"></param>
    /// <returns>A clone of the <paramref name="elementToClone"/> instance.</returns>
    /// <remarks>This member uses serialization to produce a clone of the <see cref="UIElement"/> instance.</remarks>
    public static UIElement CloneElement(this UIElement elementToClone)
    {
      string serializedElement = XamlWriter.Save(elementToClone);
      var cloneElement = XamlReader.Parse(serializedElement) as UIElement;
      return cloneElement;
    }

    /// <summary>
    /// Tries to add a value to the visual tree of an unknown <see cref="FrameworkElement"/>. For example if the element is a <see cref="TextBlock"/> the value is assigned to the <see cref="TextBlock.Text"/> property.
    /// </summary>
    /// <param name="frameworkElement"></param>
    /// <param name="value"></param>
    /// <returns><c>true</c> if the assignment was succesful. Otherwise <c>false</c>.</returns>
    public static bool TryAssignValue(this FrameworkElement frameworkElement, object value)      
    {
      switch (frameworkElement)
      {
        case Border border: 
          if (value is UIElement)
          {
            border.Child = value as UIElement;
          }
          else
          {
            return false;
          }
          break;
        case Panel panel: 
          if (value is UIElement)
          {
            panel.Children.Add(value as UIElement);
          }
          else
          {
            return false;
          }
          break;
        case TextBlock textBlock:
          textBlock.Text = value?.ToString();
          break;
        case TextBox textBox:
          textBox.Text = value?.ToString();
          break;
        case ContentControl contentControl:
        {
          Type type = contentControl.GetType();
          string propertyName = type.GetCustomAttribute<ContentPropertyAttribute>()?.Name;
          if (string.IsNullOrWhiteSpace(propertyName) 
              || propertyName.Equals(nameof(contentControl.Content), StringComparison.Ordinal))
          {
            contentControl.Content = value;
          }
          else
          {
            if (!HelperExtensions.TrySetValueToPropertyOfType(value, type, propertyName, contentControl))
            {
              return false;
            }
            }
          break;
        }
        case ContentPresenter contentPresenter:
          contentPresenter.Content = value;
          break;
        case Control control:
        {
          Type type = control.GetType();
          string propertyName = type.GetCustomAttribute<ContentPropertyAttribute>()?.Name;
          if (string.IsNullOrWhiteSpace(propertyName))
          {
            control.DataContext = value;
          }
          else
          {
            if (!HelperExtensions.TrySetValueToPropertyOfType(value, type, propertyName, control))
            {
              return false;
            }
          }

          break;
        }
        default:
          frameworkElement.DataContext = value;
          break;
      }
      return true;
    }

    private static bool TrySetValueToPropertyOfType(object value, Type type, string contentPropertyName, Control control)
    {
      PropertyInfo propertyInfo = type.GetProperty(contentPropertyName);
      if (propertyInfo == null
          || !propertyInfo.PropertyType.IsInstanceOfType(value))
      {
        return false;
      }

      propertyInfo.SetValue(control, value);
      return true;
    }

    public static System.Windows.Point ToScreenPoint(this CartesianPoint cartesianPoint, double yAxisPositiveLimit) => new System.Windows.Point(cartesianPoint.X, yAxisPositiveLimit - cartesianPoint.Y);

    public static System.Windows.Point ToCartesianPoint(this System.Windows.Point cartesianPoint, double height) => new System.Windows.Point(cartesianPoint.X, height - cartesianPoint.Y);

    /// <summary>
    /// Converts a <see cref="BionicCode.Utilities.Net.CartesianPoint"/> to a <see cref="System.Windows.Point"/>.
    /// </summary>
    /// <param name="librarypoint">The curent instance of the <see cref="BionicCode.Utilities.Net.CartesianPoint"/>.</param>
    /// <returns>A <see cref="System.Windows.Point"/> with equivalent dimension values.</returns>
    public static System.Windows.Point ToWpfPoint(this BionicCode.Utilities.Net.CartesianPoint librarypoint) => new System.Windows.Point(librarypoint.X, librarypoint.Y);

    public static double ToRadians(this double angleDegrees) => angleDegrees * (MathNet.PI / 180);
    public static double ToDegrees(this double angleRadians) => angleRadians * (180 / MathNet.PI);

    public static (CartesianPoint MaxX, CartesianPoint MinX, CartesianPoint MaxY, CartesianPoint MinY) GetExtremePoints(this IEnumerable<CartesianPoint> points)
    {
      CartesianPoint maxX = default;
      CartesianPoint minX = default;
      CartesianPoint maxY = default;
      CartesianPoint minY = default;
      foreach (CartesianPoint point in points)
      {
        if (point.X > maxX.X)
        {
          maxX = point;
        }
        if (point.X < minX.X)
        {
          minX = point;
        }
        if (point.Y > maxY.Y)
        {
          maxY = point;
        }
        if (point.Y < minY.Y)
        {
          minY = point;
        }
      }
      return (maxX, minX, maxY, minY);
    }
  }
#endif
}