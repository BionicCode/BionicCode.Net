namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Linq;
  using System.Reflection;
  using System.Text;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    /// <summary>
    /// Converts a collection of objects to a <see cref="DataTable"/>, where public property names are translated to column names.
    /// </summary>
    /// <typeparam name="TData">The type of the objects stored in the <paramref name="source"/> collection.</typeparam>
    /// <param name="source">The source collection to convert to a <see cref="DataTable"/>.</param>
    /// <returns>A <see cref="DataTable"/> created from <c>public</c> properties of the objects contained in the <paramref name="source"/>.</returns>
    /// <remarks>The extension method creates a <see cref="DataTable"/> from the source object's <c>public</c> properties.
    /// <br/>Each public property is translated to a column where the property name is the column name.
    /// <br/>To control column naming, you can decorate the property with the <see cref="System.ComponentModel.DisplayNameAttribute"/> attribute to provide an alternative column name for the decorated property.
    /// <para>To exclude <c>public</c> properties from the conversion, decorate them with the <see cref="IgnoreAttribute"/> attribute.</para></remarks>
    public static DataTable ToDataTable<TData>(this IEnumerable<TData> source)
    {
      Type dataType = typeof(TData);
      IEnumerable<PropertyInfo> publicPropertyInfos = dataType.GetProperties()
        .Where(propertyInfo => propertyInfo.GetCustomAttribute<IgnoreAttribute>() is null);
      var result = new DataTable();
      var columnNameMapping = new Dictionary<string, string>();
      foreach (PropertyInfo publicPropertyInfo in publicPropertyInfos)
      {
        DataColumn newColumn = result.Columns.Add(publicPropertyInfo.Name, publicPropertyInfo.PropertyType);

        System.ComponentModel.DisplayNameAttribute displayNameAttribute = publicPropertyInfo.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
        if (displayNameAttribute != null)
        {
          newColumn.ColumnName = displayNameAttribute.DisplayName;
        }

        columnNameMapping.Add(publicPropertyInfo.Name, newColumn.ColumnName);
      }

      foreach (TData rowData in source)
      {
        DataRow newRow = result.NewRow();
        result.Rows.Add(newRow);
        foreach (PropertyInfo publicPropertyInfo in publicPropertyInfos)
        {
          object columnValue = publicPropertyInfo.GetValue(rowData);
          string columnName = columnNameMapping[publicPropertyInfo.Name];
          newRow[columnName] = columnValue;
        }
      }

      return result;
    }
  }
}
