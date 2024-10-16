namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    /// <summary>
    /// Coverts any type to a <see cref="Dictionary{TKey,TValue}"/>, where the <c>TKey</c> is the member name and <c>TValue</c> the member's value.
    /// </summary>
    /// <param name="instanceToConvert"></param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/>, where 
    /// <br/> 
    /// <list type="table">
    /// <listheader>
    /// <term><c>TKey</c> </term>
    /// <term><c>TValue</c> </term>
    /// </listheader>
    /// <item>
    /// <term>is the member's name of type <see cref="string"/> </term>
    /// <term>is the member's value of type <see cref="object"/>, when the value is a primitive type like <see langword="int"/> or a <see cref="string"/></term>
    /// </item>
    /// <item>
    /// <term> </term>
    /// <term>is the member's value of type <see cref="Dictionary{TKey, TValue}"/>, when the value is a complex type that itself was converted to a <c>Dictionary&lt;string, object&gt;</c></term>
    /// </item>
    /// </list>.
    /// <br/>This rules apply to the complete object graph.</returns>
    /// <remarks>This method recursively traverses the complete object graph and converts every object node i.e. property value (execpt primitive types and <see cref="string"/>) to a <see cref="Dictionary{TKey, TValue}"/>. 
    /// <br/>It creates entries for all <see langword="public"/> instance and class properties of this object. Each entry represents a property as key-value-pair of property name and property value.
    /// <para>To create a flat map of the object graph, use the <see cref="ToFlatDictionary(object)"/></para>
    /// <para>Use the <see cref="IgnoreInObjectGraphAttribute"/> attribute to decorate properties that should be excluded.</para></remarks>
    public static Dictionary<string, object> ToDictionary(this object instanceToConvert)
    {
      var resultDictionary = instanceToConvert.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Where(propertyInfo => !IsPropertyIndexer(propertyInfo) && propertyInfo.GetCustomAttribute(typeof(IgnoreInObjectGraphAttribute)) == null)
        .ToDictionary(
          propertyInfo => propertyInfo.Name,
          propertyInfo => HelperExtensionsCommon.ConvertPropertyToDictionary(propertyInfo, instanceToConvert, isRecursive: true));

      return resultDictionary;
    }

    /// <summary>
    /// Coverts any type to a flattened <see cref="Dictionary{TKey,TValue}"/>, where the <c>TKey</c> is the member's name and <c>TValue</c> the member's value.
    /// <br/>Flattened in this context means, only the object graph's root is converted.
    /// </summary>
    /// <param name="instanceToConvert"></param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/>, where 
    /// <br/> 
    /// <list type="table">
    /// <listheader>
    /// <term><c>TKey</c> </term>
    /// <term><c>TValue</c> </term>
    /// </listheader>
    /// <item>
    /// <term>is the member's name of type <see cref="string"/> </term>
    /// <term>is the member's value of type <see cref="object"/></term>
    /// </item>
    /// </list>
    /// <br/>Since <see cref="ToFlatDictionary(object)"/> returns a flat dictionary, the result graph has no depth.In other words, this rules apply to the root of the object graph.</returns>
    /// <remarks>This method only traverses the root of the object's graph and to convert it to a <see cref="Dictionary{TKey, TValue}"/>. <br/>It creates entries for all <see langword="public"/> instance and class properties of this object. Each entry represents a property as key-value-pair of property name (of type <see cref="string"/>) and property value (of type <see cref="object"/>).    
    /// <para>To traverse the complete object graph recursively (to convert every object node i.e. property value to a <see cref="Dictionary{TKey, TValue}"/>), use the <see cref="ToDictionary(object)"/> method.</para>
    /// <para>Use the <see cref="IgnoreInObjectGraphAttribute"/> attribute to decorate properties that should be excluded.</para></remarks>
    public static Dictionary<string, object> ToFlatDictionary(this object instanceToConvert)
    {

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard21)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net472)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net50)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net80)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard20)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net60)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net70)'
Before:
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
After:
      var resultDictionary = instanceToConvert.GetType()
*/
      var resultDictionary = instanceToConvert.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Where(propertyInfo => !IsPropertyIndexer(propertyInfo) && propertyInfo.GetCustomAttribute(typeof(IgnoreInObjectGraphAttribute)) == null)
        .ToDictionary(
          propertyInfo => propertyInfo.Name,
          propertyInfo => HelperExtensionsCommon.ConvertPropertyToDictionary(propertyInfo, instanceToConvert, isRecursive: false));

      return resultDictionary;
    }

    private static object ConvertPropertyToDictionary(PropertyInfo propertyInfo, object owner, bool isRecursive)
    {
      Type propertyType = propertyInfo.PropertyType;
      object propertyValue = propertyInfo.GetValue(owner);

      // If recursion is disabled stop traversal to return a flat dictionary (object graph root)
      if (!isRecursive)
      {
        return propertyValue;
      }

      if (propertyValue is Type)
      {
        return propertyValue;
      }

      if (propertyValue is Delegate func)
      {
        return func;
      }

      // If property is a collection don't traverse collection properties but the items instead
      if (!propertyType.Equals(typeof(string)) && propertyValue is IEnumerable enumerable)
      {
        var items = new Dictionary<string, object>();
        int index = 0;
        foreach (object item in enumerable)
        {
          // If property is a primitive type or a string,
          // then stop traversal.
          if (item.GetType().IsPrimitive || item is string)
          {
            items.Add(index.ToString(), item);
          }
          else if (item is IEnumerable enumerableItem)
          {
            items.Add(index.ToString(), HelperExtensionsCommon.ConvertIEnumerableToDictionary(enumerableItem));
          }
          else
          {

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard21)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net472)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net50)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net80)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard20)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net60)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net70)'
Before:
            Dictionary<string, object> dictionary = item.ToDictionary();
After:
            var dictionary = item.ToDictionary();
*/
            var dictionary = item.ToDictionary();
            items.Add(index.ToString(), dictionary);
          }

          index++;
        }

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
        var resultDictionary = properties.ToDictionary(
          subtypePropertyInfo => subtypePropertyInfo.Name,
          subtypePropertyInfo => propertyValue == null
            ? null
            : HelperExtensionsCommon.ConvertPropertyToDictionary(subtypePropertyInfo, propertyValue, isRecursive));
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
          var dictionary = item.ToDictionary();
          items.Add(index.ToString(), dictionary);
        }

        index++;
      }

      items.Add("IsCollection", true);
      items.Add("Count", index);
      return items;
    }

    private static bool IsPropertyIndexer(PropertyInfo propertyInfo) => propertyInfo.GetIndexParameters().Any();
  }
}
