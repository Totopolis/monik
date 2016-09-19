using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;

namespace Monik.Service
{
  public class Settings
  {
    public static string DBConnectionString = null;

    // TODO: use concurent when online updates
    private static Dictionary<string, string> FSettings = null;

    public static void CheckUpdates()
    {
      if (FSettings == null)
      {
        FSettings = new Dictionary<string, string>();

        var _proto = new { name = default(string), value = default(string) };
        var _settings = SimpleCommand.ExecuteQueryAnonymous(_proto, DBConnectionString, "select Name, Value from mon.Settings");
        foreach (var it in _settings)
          FSettings[it.name] = it.value;
      }
    }

    public static string GetValue(string aName)
    {
      if (!FSettings.ContainsKey(aName))
        throw new KeyNotFoundException("Setting name not found");

      return FSettings[aName];
    }

    public static void SetValue(string aName, string aValue)
    {
      if (!FSettings.ContainsKey(aName))
        throw new KeyNotFoundException("Setting name not found");

      FSettings[aName] = aValue;
    }
  
  }//end of class
}
