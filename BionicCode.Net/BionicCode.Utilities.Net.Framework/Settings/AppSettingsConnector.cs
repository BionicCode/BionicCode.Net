using System;
using System.Collections.Specialized;
using System.Configuration;

namespace BionicCode.Utilities.Net.Framework.Settings
{
  /// <summary>
  /// Exposes a basic static API to access the AppSettings file in order to manage application settings. 
  /// </summary>
  public static class AppSettingsConnector
  {
    /// <summary>
    /// If exists, this method will return the corresponding value of the specified key.
    /// </summary>
    /// <param name="key">The key that maps to a specific setting.</param>
    /// <param name="value">The actual settings <see cref="string"/> value of the specified key.</param>
    /// <returns><c>true</c> when an entry for the specified <paramref name="key"/> was found. Otherwise <c>false</c>.</returns>
    public static bool TryReadString(string key, out string value)
    {
      value = default;
      NameValueCollection appSettings = ConfigurationManager.AppSettings;
      if (appSettings[key] == null)
      {
        return false;
      }
      value = appSettings[key];
      return true;
    }

    /// <summary>
    /// If exists, this method will return the corresponding value of the specified key.
    /// </summary>
    /// <param name="key">The key that maps to a specific setting.</param>
    /// <param name="value">The actual settings <see cref="int"/> value of the specified key.</param>
    /// <returns><c>true</c> when an entry for the specified <paramref name="key"/> was found. Otherwise <c>false</c>.</returns>
    public static bool TryReadInt(string key, out int value)
    {
      value = -1;
      NameValueCollection appSettings = ConfigurationManager.AppSettings;

      if (double.TryParse(appSettings[key], out double doubleValue))
      {
        value = Convert.ToInt32(doubleValue);
        return true;
      }

      return false;
    }

    /// <summary>
    /// If exists, this method will return the corresponding value of the specified key.
    /// </summary>
    /// <param name="key">The key that maps to a specific setting.</param>
    /// <param name="value">The actual settings <see cref="double"/> value of the specified key.</param>
    /// <returns><c>true</c> when an entry for the specified <paramref name="key"/> was found. Otherwise <c>false</c>.</returns>
    public static bool TryReadDouble(string key, out double value)
    {
      NameValueCollection appSettings = ConfigurationManager.AppSettings;
      return double.TryParse(appSettings[key], out value);
    }

    /// <summary>
    /// If exists, this method will return the corresponding value of the specified key.
    /// </summary>
    /// <param name="key">The key that maps to a specific setting.</param>
    /// <param name="value">The actual settings <see cref="bool"/> value of the specified key.</param>
    /// <returns><c>true</c> when an entry for the specified <paramref name="key"/> was found. Otherwise <c>false</c>.</returns>
    public static bool TryReadBool(string key, out bool value)
    {
      NameValueCollection appSettings = ConfigurationManager.AppSettings;
      return bool.TryParse(appSettings[key], out value);
    }

    /// <summary>
    /// Writes a <see cref="string"/> value to the settings file which is stored using the specified lookup <paramref name="key"/>. If the <paramref name="key"/> already exists, the existing value will be overwritten.
    /// </summary>
    /// <param name="key">The lookup key for the <paramref name="value"/>.</param>
    /// <param name="value">The settings value to save to the file.</param>
    public static void WriteString(string key, string value)
    {
      AppSettingsConnector.AddUpdateAppSettings(key, value);
    }


    /// <summary>
    /// Writes a <see cref="int"/> value to the settings file which is stored using the specified lookup <paramref name="key"/>. If the <paramref name="key"/> already exists, the existing value will be overwritten.
    /// </summary>
    /// <param name="key">The lookup key for the <paramref name="value"/>.</param>
    /// <param name="value">The settings value to save to the file.</param>
    public static void WriteInt(string key, int value)
    {
      AppSettingsConnector.AddUpdateAppSettings(key, value);
    }


    /// <summary>
    /// Writes a <see cref="double"/> value to the settings file which is stored using the specified lookup <paramref name="key"/>. If the <paramref name="key"/> already exists, the existing value will be overwritten.
    /// </summary>
    /// <param name="key">The lookup key for the <paramref name="value"/>.</param>
    /// <param name="value">The settings value to save to the file.</param>
    public static void WriteDouble(string key, double value)
    {
      AppSettingsConnector.AddUpdateAppSettings(key, value);
    }


    /// <summary>
    /// Writes a <see cref="bool"/> value to the settings file which is stored using the specified lookup <paramref name="key"/>. If the <paramref name="key"/> already exists, the existing value will be overwritten.
    /// </summary>
    /// <param name="key">The lookup key for the <paramref name="value"/>.</param>
    /// <param name="value">The settings value to save to the file.</param>
    public static void WriteBool(string key, bool value)
    {
      AppSettingsConnector.AddUpdateAppSettings(key, value);
    }


    private static void AddUpdateAppSettings<TValue>(string key, TValue value)
    {
      Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;
      if (settings[key] == null)
      {
        settings.Add(key, Convert.ToString(value));
      }
      else
      {
        settings[key].Value = Convert.ToString(value);
      }

      configFile.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
    }
  }
}
