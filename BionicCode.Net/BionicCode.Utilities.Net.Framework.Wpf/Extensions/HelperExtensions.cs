using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Popup = System.Windows.Controls.Primitives.Popup;

namespace BionicCode.Utilities.Net.Framework.Wpf.Extensions
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
      public static bool TryFindVisualParentElementByName<TChild>(
        this DependencyObject child,
        string elementName,
        out TChild resultElement) where TChild : FrameworkElement
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
      /// <param name="resultElement">The found element or <c>null</c> if no matching element was found.</param>
      /// <returns><c>true</c> when an element with the specified <paramref name="childElementName"/> was found, otherwise <c>false</c>.</returns>
      public static bool TryFindVisualChildElementByName<TChild>(
        this DependencyObject parent,
        string childElementName,
        out TChild resultElement) where TChild : FrameworkElement
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
      [Obsolete("Use 'EnumerateVisualChildElements()' instead")]
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

          if (childElement is Popup popup)
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
      /// Coverts any type to a <see cref="Dictionary{TKey,TValue}"/>, where the <c>TKey</c> is the member name and <c>TValue</c> the member's value.
      /// </summary>
      /// <param name="instanceToConvert"></param>
      /// <returns>A <see cref="Dictionary{TKey,TValue}"/>, where the <c>TKey</c> is the member name of type <see cref="string"/> and <c>TValue</c> the member's value of type <see cref="object"/>.</returns>
      public static Dictionary<string, object> ToDictionary(this object instanceToConvert)
      {
        Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
          .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
          .Where(propertyInfo => !propertyInfo.GetIndexParameters().Any())
          .ToDictionary(
            propertyInfo => propertyInfo.Name,
            propertyInfo => HelperExtensions.ConvertPropertyToDictionary(propertyInfo, instanceToConvert));
        resultDictionary.Add("IsCollection", false);
        return resultDictionary;
      }

      private static object ConvertPropertyToDictionary(PropertyInfo propertyInfo, object owner)
      {
        Type propertyType = propertyInfo.PropertyType;
        object propertyValue = propertyInfo.GetValue(owner);
        if (propertyValue is Type)
        {
          return propertyValue;
        }

        // If property is a collection don't traverse collection properties but the items instead
        if (!propertyType.Equals(typeof(string)) && typeof(IEnumerable).IsAssignableFrom(propertyType))
        {
          var items = new Dictionary<string, object>();
          var enumerable = propertyInfo.GetValue(owner) as IEnumerable;
          int index = 0;
          foreach (object item in enumerable)
          {
            // If property is a string stop traversal
            if (item.GetType().IsPrimitive || item is string)
            {
              items.Add(index.ToString(), item);
            }
            else if (item is IEnumerable enumerableItem)
            {
              items.Add(index.ToString(), HelperExtensions.ConvertIEnumerableToDictionary(enumerableItem));
            }
            else
            {
              Dictionary<string, object> dictionary = item.ToDictionary();
              items.Add(index.ToString(), dictionary);
            }

            index++;
          }

          items.Add("IsCollection", true);
          items.Add("Count", index);
          return items;
        }

        // If property is a string stop traversal
        if (propertyType.IsPrimitive || propertyType.Equals(typeof(string)))
        {
          return propertyValue;
        }

        PropertyInfo[] properties =
          propertyType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        if (properties.Any())
        {
          Dictionary<string, object> resultDictionary = properties.ToDictionary(
            subtypePropertyInfo => subtypePropertyInfo.Name,
            subtypePropertyInfo => propertyValue == null
              ? null
              : HelperExtensions.ConvertPropertyToDictionary(subtypePropertyInfo, propertyValue));
          resultDictionary.Add("IsCollection", false);
          return resultDictionary;
        }

        return propertyValue;
      }

      private static Dictionary<string, object> ConvertIEnumerableToDictionary(IEnumerable enumerable)
      {
        var items = new Dictionary<string, object>();
        int index = 0;
        foreach (object item in enumerable)
        {
          // If property is a string stop traversal
          if (item.GetType().IsPrimitive || item is string)
          {
            items.Add(index.ToString(), item);
          }
          else
          {
            Dictionary<string, object> dictionary = item.ToDictionary();
            items.Add(index.ToString(), dictionary);
          }

          index++;
        }

        items.Add("IsCollection", true);
        items.Add("Count", index);
        return items;
      }
    }
}