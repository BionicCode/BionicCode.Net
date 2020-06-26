using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Framework.Converter
{
  /// <summary>
  ///   Converter to truncate file paths exceeding a specific length by replacing a number of characters with an ellipsis.
  /// </summary>
  public class FilePathTruncateConverter : DependencyObject, IValueConverter
  {
    public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
      "MaxLength",
      typeof(int),
      typeof(FilePathTruncateConverter),
      new PropertyMetadata(20));

    public int MaxLength
    {
      get => (int) GetValue(FilePathTruncateConverter.MaxLengthProperty);
      set => SetValue(FilePathTruncateConverter.MaxLengthProperty, value);
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return string.Empty;
      }

      string path = value.ToString();

      if (path.Contains("\\") == false)
      {
        return path;
      }

      int maxLength = parameter == null
        ? this.MaxLength
        : System.Convert.ToInt32(parameter);


      return maxLength >= path.Length ? path : InsertCharacterEllipsis(path, maxLength);
    }

    private string InsertCharacterEllipsis(string path, int maxLength)
    {
      string directory = Path.GetDirectoryName(path) ?? string.Empty;
      string fileName = Path.GetFileName(path) ?? string.Empty;
      string[] pathSegments = directory?.Split(new[] {@"\"}, StringSplitOptions.RemoveEmptyEntries);
      var pathBuilder = new StringBuilder();
      var pathSegmentIndex = 0;

      if (fileName.Length >= maxLength)
      {
        string fileExtension = Path.GetExtension(fileName);
        pathBuilder.Append(pathSegments[0]);
        pathBuilder.Append(@"\...\");
        pathBuilder.Append(fileName.Substring(0, (int) Math.Floor(maxLength / 2.0)));
        pathBuilder.Append(" ...");
        pathBuilder.Append(
          fileName.Substring(fileName.Length - fileExtension.Length - (int) Math.Ceiling(maxLength / 2.0)));
        return pathBuilder.ToString();
      }

      while (pathSegmentIndex < pathSegments.Length &&
             pathBuilder.Length + pathSegments[pathSegmentIndex].Length + 1 + fileName.Length < maxLength)
      {
        pathBuilder.Append(pathSegments[pathSegmentIndex++] + @"\");
      }

      if (pathSegmentIndex < pathSegments.Length)
      {
        pathBuilder.Append(@"...\");
      }

      pathBuilder.Append(fileName);
      return pathBuilder.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
  }
}