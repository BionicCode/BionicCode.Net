using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BionicCode.Utilities.Net.Framework.Extensions
{
  /// <summary>
  /// Collection of extension methods e.g. visual tree traversal
  /// </summary>
  public static class HelperExtensions
  {
    /// <summary>
    /// Traverses the visual tree towards the root until an element with a matching element name is found.
    /// </summary>
    /// <typeparam name="TParent">The type the visual parent must match.</typeparam>
    /// <param name="child"></param>
    /// <param name="resultElement"></param>
    /// <returns></returns>
    public static bool TryFindVisualParentElement<TParent>(this DependencyObject child, out TParent resultElement)
      where TParent : DependencyObject
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
    public static bool TryFindVisualParentElementByName(
      this DependencyObject child,
      string elementName,
      out FrameworkElement resultElement)
    {
      resultElement = null;

      var parentElement = VisualTreeHelper.GetParent(child);

      if (parentElement is FrameworkElement frameworkElement &&
          frameworkElement.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase))
      {
        resultElement = frameworkElement;
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
      public static bool TryFindVisualChildElement<TChild>(this DependencyObject parent, out TChild resultElement)
        where TChild : DependencyObject
      {
        resultElement = null;

        if (parent is Popup popup)
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
    /// <param name="resultElement"></param>
    /// <returns></returns>
    public static bool TryFindVisualChildElementByName(
      this DependencyObject parent,
      string childElementName,
      out FrameworkElement resultElement)
    {
      resultElement = null;


      if (parent is Popup popup)
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
          resultElement = uiElement;
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
    /// Returns an <see cref="IEnumerable&lt;TChildren&gt;"/> to enable deferred traversal.
    /// </summary>
    /// <typeparam name="TChildren">The type the visual children must match.</typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static IEnumerable<TChildren> FindVisualChildElements<TChildren>(this DependencyObject parent)
      where TChildren : DependencyObject
    {
      for (var childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(parent); childIndex++)
      {
        DependencyObject childElement = VisualTreeHelper.GetChild(parent, childIndex);

        if (childElement is Popup popup)
        {
          childElement = popup.Child;
        }

        if (childElement is TChildren element)
        {
          yield return element;
        }

        foreach (TChildren visualChildElement in childElement.FindVisualChildElements<TChildren>())
        {
          yield return visualChildElement;
        }
      }
    }
  }
}