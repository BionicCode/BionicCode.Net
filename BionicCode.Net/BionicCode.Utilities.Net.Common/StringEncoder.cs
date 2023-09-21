namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Text.RegularExpressions;

  internal static class StringEncoder
  {
    private static string StringFormatEscapeRegexPattern { get; } = @"(?<openingBrace>\{(?!\d+\}))|(?<closingBrace>\}(?<!\{\d+\}))";
    private static Regex StringFormatEncoderRegex { get; } = new Regex(StringFormatEscapeRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    private static Dictionary<string, string> ReplacementTable { get; } = new Dictionary<string, string>()
    {
      { "{", "{{" },
      {"}", "}}" },
    };

    // Escape special chracters like '{' to make string compatible for 'string.Format'
    public static string EncodeFormatString(string stringToEncode) => StringFormatEncoderRegex.Replace(stringToEncode, GetSubstitution);

    public static string GetSubstitution(Match match)
    {
      string substitution = ReplacementTable.TryGetValue(match.Value, out string replacement)
      ? replacement
      : match.Value;

      return substitution;
    }

    //.Replace()
    //.Replace("}", "}}")
    //.Replace("$!", "{")
    //.Replace("!$", "}");

  }
}