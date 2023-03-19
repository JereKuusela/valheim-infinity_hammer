using System;
using System.Linq;
namespace InfinityHammer;
public class ToolParameters
{
  static bool IsParameter(string arg, string par) => arg == par || arg.EndsWith("=" + par, StringComparison.OrdinalIgnoreCase);
  static string ReplaceEnd(string arg, string par, int amount) => arg.Substring(0, arg.Length - amount) + par;

  static string Replace(string arg, string par)
  {
    while (IsParameter(arg, par))
    {
      var str = string.Join(",", par.Split(',').Select(s => $"#{s}"));
      return ReplaceEnd(arg, str, par.Length);
    }
    return arg;
  }
  public static string Parametrize(string command)
  {
    command = command.Replace("hammer_tool ", "");
    var args = command.Split(' ').ToArray();
    var parameters = new[]{
      "id", "r", "r1-r2", "d", "w", "w1-w2", "h", "a", "w,d", "w1-w2,d", "x", "y", "z", "tx", "ty", "tz",
      "x,y", "x,z", "y,x", "y,z", "z,x", "z,y", "tx,ty", "tx,tz", "ty,tx", "ty,tz", "tz,tx", "tz,ty",
      "x,y,z", "x,z,y", "y,x,z", "y,z,x", "z,x,y", "z,y,x",
      "tx,ty,tz", "tx,tz,ty", "ty,tx,tz", "ty,tz,tx", "tz,tx,ty", "tz,ty,tx",
      "ignore"
    };
    RulerParameters pars = new();
    for (var i = 0; i < args.Length; i++)
    {
      foreach (var par in parameters)
        args[i] = Replace(args[i], par);
    }
    return string.Join(" ", args);
  }
  public static RulerParameters ParseParameters(string command)
  {
    var args = command.Split(' ').ToArray();

    RulerParameters pars = new();
    pars.RotateWithPlayer = true;
    for (var i = 0; i < args.Length; i++)
    {
      if (args[i].Contains("#id"))
        pars.IsId = true;
      if (args[i].Contains("#a"))
        pars.RotateWithPlayer = false;
      if (args[i].Contains("#r"))
        pars.Radius = true;
      if (args[i].Contains("#r1-r2"))
      {
        pars.Radius = true;
        pars.Ring = true;
      }
      if (args[i].Contains("#w"))
        pars.Width = true;
      if (args[i].Contains("#w1-w2"))
      {
        pars.Width = true;
        pars.Grid = true;
      }
      if (args[i].Contains("#d"))
        pars.Depth = true;
      if (args[i].Contains("#h"))
        pars.Height = true;
      if (args[i].Contains("#tx"))
        pars.IsTargeted = true;
      if (args[i].Contains("#ty"))
        pars.IsTargeted = true;
      if (args[i].Contains("#tz"))
        pars.IsTargeted = true;
    }
    return pars;
  }
}
