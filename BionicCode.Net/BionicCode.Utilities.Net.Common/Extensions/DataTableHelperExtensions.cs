namespace BionicCode.Utilities.Net.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Linq;
  using System.Text;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    /// <summary>
    /// Adds a new <see cref="DataColumn"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <typeparam name="TData">The data type of the column value.</typeparam>
    /// <param name="source">The <see cref="DataTable"/> to add the new <see cref="DataColumn"/> to.</param>
    /// <param name="columnName">The column's name.</param>
    /// <remarks>Added columns are initialized to the default value of <typeparamref name="TData"/> for all existing rows.
    /// <br/><see cref="AddColumn{TData}(DataTable, string, int)"/> should be called before an overload of <see cref="AddRow(DataTable, object[])"/></remarks>
    public static void AddColumn<TData>(this DataTable source, string columnName)
    {
      DataColumn newColumn = new DataColumn(columnName, typeof(TData));

      source.Columns.Add(newColumn);
      int newColumnIndex = source.Columns.IndexOf(newColumn);

      // Initialize existing rows with default value for the new column
      foreach (DataRow row in source.Rows)
      {
        row[newColumnIndex] = default(TData);
      }
    }

    /// <summary>
    /// Adds a new <see cref="DataColumn"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <typeparam name="TData">The data type of the column value.</typeparam>
    /// <param name="source">The <see cref="DataTable"/> to add the new <see cref="DataColumn"/> to.</param>
    /// <param name="columnName">The column's name.</param>
    /// <param name="columnIndex">The explicit index of the created <see cref="DataColumn"/>.</param>
    /// <remarks>If <paramref name="columnIndex"/> is less than 0 or greater than the existing number of columns - 1 (greater than the ordinal of the last column) then an Invalid <see cref="ArgumentException"/> is thrown.
    /// <para>Any columns between the previous and new ordinal will be renumbered, to adjust for a column's new ordinal.</para>
    /// Added columns are initialized to the default value of <typeparamref name="TData"/> for all existing rows.
    /// <br/><see cref="AddColumn{TData}(DataTable, string, int)"/> should be called before an overload of <see cref="AddRow(DataTable, object[])"/></remarks>
    public static void AddColumn<TData>(this DataTable source, string columnName, int columnIndex)
    {
      DataColumn newColumn = new DataColumn(columnName, typeof(TData));

      source.Columns.Add(newColumn);
      newColumn.SetOrdinal(columnIndex);
      int newColumnIndex = source.Columns.IndexOf(newColumn);

      // Initialize existing rows with default value for the new column
      foreach (DataRow row in source.Rows)
      {
        row[newColumnIndex] = default(TData);
      }
    }

    /// <summary>
    /// Adds a new <see cref="DataRow"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="source">The <see cref="DataTable"/> to add the new <see cref="DataRow"/> to.</param>
    /// <param name="columnValues">The row's cell values ordered by column index.</param>
    public static void AddRow(this DataTable source, params object[] columnValues)
    {
      DataRow rowModelWithCurrentColumns = source.NewRow();
      source.Rows.Add(rowModelWithCurrentColumns);

      for (int columnIndex = 0; columnIndex < source.Columns.Count; columnIndex++)
      {
        rowModelWithCurrentColumns[columnIndex] = columnValues[columnIndex];
      }
    }

    /// <summary>
    /// Adds a new <see cref="DataRow"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="source">The <see cref="DataTable"/> to add the new <see cref="DataRow"/> to.</param>
    /// <param name="columnValues">The ordered row's cell values, where the item index maps to the column index.</param>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="columnValues"/> contains less elements than <paramref name="source"/> has columns.</exception>
    public static void AddRow(this DataTable source, IEnumerable<object> columnValues)
    {
      DataRow rowModelWithCurrentColumns = source.NewRow();
      source.Rows.Add(rowModelWithCurrentColumns);

      for (int columnIndex = 0; columnIndex < source.Columns.Count; columnIndex++)
      {
        rowModelWithCurrentColumns[columnIndex] = columnValues.ElementAt(columnIndex);
      }
    }

    /// <summary>
    /// Adds a new <see cref="DataRow"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="source">The <see cref="DataTable"/> to add the new <see cref="DataRow"/> to.</param>
    /// <param name="columnValues">The row's cell values passed as tuples of <c>(int ColumnIndex, object ColumnValue)</c>.</param>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="columnValues"/> contains less elements than <paramref name="source"/> has columns or a contained tuple has an invalid index.</exception>
    public static void AddRow(this DataTable source, params (int ColumnIndex, object ColumnValue)[] columnValues)
    {
      DataRow rowModelWithCurrentColumns = source.NewRow();
      source.Rows.Add(rowModelWithCurrentColumns);
      foreach ((int ColumnIndex, object ColumnValue) in columnValues)
      {
        rowModelWithCurrentColumns[ColumnIndex] = ColumnValue;
      }
    }
  }
}
