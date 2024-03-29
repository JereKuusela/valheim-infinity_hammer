using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace Service;

///<summary>Contains functions for parsing arguments, etc.</summary>
public static class Parse2
{
  private static Range<string> TryRange(string arg)
  {
    var range = arg.Split('-').ToList();
    if (range.Count > 1 && range[0] == "")
    {
      range[0] = "-" + range[1];
      range.RemoveAt(1);
    }
    if (range.Count > 2 && range[1] == "")
    {
      range[1] = "-" + range[2];
      range.RemoveAt(2);
    }
    if (range.Count == 1) return new(range[0]);
    else return new(range[0], range[1]);

  }
  public static int TryInt(string arg, int defaultValue = 0)
  {
    if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static int TryInt(string[] args, int index, int defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return TryInt(args[index], defaultValue);
  }
  public static Range<int> TryIntRange(string arg, int defaultValue = 0)
  {
    var range = TryRange(arg);
    return new(TryInt(range.Min, defaultValue), TryInt(range.Max, defaultValue));
  }
  public static Range<int> TryIntRange(string[] args, int index, int defaultValue = 0)
  {
    if (args.Length <= index) return new(defaultValue);
    return TryIntRange(args[index], defaultValue);
  }
  public static uint TryUInt(string arg, uint defaultValue = 0)
  {
    if (!uint.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static uint TryUInt(string[] args, int index, uint defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return TryUInt(args[index], defaultValue);
  }
  public static Range<uint> TryUIntRange(string arg, uint defaultValue = 0)
  {
    var range = TryRange(arg);
    return new(TryUInt(range.Min, defaultValue), TryUInt(range.Max, defaultValue));
  }
  public static Range<uint> TryUIntRange(string[] args, int index, uint defaultValue = 0)
  {
    if (args.Length <= index) return new(defaultValue);
    return TryUIntRange(args[index], defaultValue);
  }
  public static long TryLong(string arg, long defaultValue = 1)
  {
    if (!long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static long TryLong(string[] args, int index, long defaultValue = 1)
  {
    if (args.Length <= index) return defaultValue;
    return TryLong(args[index], defaultValue);
  }
  public static Range<long> TryLongRange(string arg, long defaultValue = 1)
  {
    var range = TryRange(arg);
    return new(TryLong(range.Min, defaultValue), TryLong(range.Max, defaultValue));
  }
  public static Range<long> TryLongRange(string[] args, int index, long defaultValue = 1)
  {
    if (args.Length <= index) return new(defaultValue);
    return TryLongRange(args[index], defaultValue);
  }
  public static float TryFloat(string arg, float defaultValue = 1)
  {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static float? TryFloatNull(string arg)
  {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return null;
    return result;
  }
  public static float TryFloat(string[] args, int index, float defaultValue = 1f)
  {
    if (args.Length <= index) return defaultValue;
    return TryFloat(args[index], defaultValue);
  }
  public static Range<float> TryFloatRange(string arg, float defaultValue = 1)
  {
    var range = TryRange(arg);
    return new(TryFloat(range.Min, defaultValue), TryFloat(range.Max, defaultValue));
  }
  public static Range<float?> TryFloatNullRange(string arg)
  {
    var range = TryRange(arg);
    return new(TryFloatNull(range.Min), TryFloatNull(range.Max));
  }
  public static Range<float> TryFloatRange(string[] args, int index, float defaultValue = 1)
  {
    if (args.Length <= index) return new(defaultValue);
    return TryFloatRange(args[index], defaultValue);
  }
  public static string String(string arg) => arg;
  public static string TryString(string[] args, int index, string defaultValue = "")
  {
    if (args.Length <= index) return defaultValue;
    return args[index];
  }
  private static readonly HashSet<string> Truthies = [
    "1",
    "true",
    "yes",
    "on"
  ];
  private static bool IsTruthy(string value) => Truthies.Contains(value);
  private static readonly HashSet<string> Falsies = [
    "0",
    "false",
    "no",
    "off"
  ];
  private static bool IsFalsy(string value) => Falsies.Contains(value);
  public static bool? Boolean(string arg)
  {
    if (IsTruthy(arg)) return true;
    if (IsFalsy(arg)) return false;
    return null;
  }
  public static Quaternion TryAngleYXZ(string arg) => TryAngleYXZ(arg, Quaternion.identity);
  public static Quaternion TryAngleYXZ(string arg, Quaternion defaultValue)
  {
    var values = Split(arg);
    var angle = Vector3.zero;
    angle.y = TryFloat(values, 0, defaultValue.eulerAngles.y);
    angle.x = TryFloat(values, 1, defaultValue.eulerAngles.x);
    angle.z = TryFloat(values, 2, defaultValue.eulerAngles.z);
    return Quaternion.Euler(angle);
  }
  public static Range<Quaternion> TryAngleYXZRange(string arg) => TryAngleYXZRange(arg, Quaternion.identity);
  public static Range<Quaternion> TryAngleYXZRange(string arg, Quaternion defaultValue)
  {
    var parts = Split(arg);
    var y = TryFloatRange(parts, 0, defaultValue.y);
    var x = TryFloatRange(parts, 1, defaultValue.x);
    var z = TryFloatRange(parts, 2, defaultValue.z);
    return ToAngleRange(x, y, z);
  }
  private static Range<Quaternion> ToAngleRange(Range<float> x, Range<float> y, Range<float> z)
  {
    var min = Quaternion.Euler(new(x.Min, y.Min, z.Min));
    var max = Quaternion.Euler(new(x.Max, y.Max, z.Max));
    return new(min, max);
  }
  ///<summary>Parses XZY vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 TryVectorXZY(string[] args) => TryVectorXZY(args, 0, Vector3.zero);
  ///<summary>Parses XZY vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 TryVectorXZY(string[] args, Vector3 defaultValue) => TryVectorXZY(args, 0, defaultValue);
  ///<summary>Parses XZY vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 TryVectorXZY(string[] args, int index) => TryVectorXZY(args, index, Vector3.zero);
  ///<summary>Parses XZY vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 TryVectorXZY(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = TryFloat(args, index, defaultValue.x);
    vector.y = TryFloat(args, index + 2, defaultValue.y);
    vector.z = TryFloat(args, index + 1, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> TryVectorXZYRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = TryFloatRange(parts, 0, defaultValue.x);
    var y = TryFloatRange(parts, 2, defaultValue.y);
    var z = TryFloatRange(parts, 1, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  ///<summary>Parses ZXY vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 TryVectorZXY(string[] args) => TryVectorZXY(args, 0, Vector3.zero);
  ///<summary>Parses ZXY vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 TryVectorZXY(string[] args, Vector3 defaultValue) => TryVectorZXY(args, 0, defaultValue);
  ///<summary>Parses ZXY vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 TryVectorZXY(string[] args, int index) => TryVectorZXY(args, index, Vector3.zero);
  ///<summary>Parses ZXY vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 TryVectorZXY(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = TryFloat(args, index + 1, defaultValue.x);
    vector.y = TryFloat(args, index + 2, defaultValue.y);
    vector.z = TryFloat(args, index, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> TryVectorZXYRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = TryFloatRange(parts, 1, defaultValue.x);
    var y = TryFloatRange(parts, 2, defaultValue.y);
    var z = TryFloatRange(parts, 0, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  private static Range<Vector3> ToVectorRange(Range<float> x, Range<float> y, Range<float> z)
  {
    Vector3 min = new(x.Min, y.Min, z.Min);
    Vector3 max = new(x.Max, y.Max, z.Max);
    return new(min, max);
  }
  ///<summary>Parses YXZ vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 TryVectorYXZ(string[] args) => TryVectorYXZ(args, 0, Vector3.zero);
  ///<summary>Parses YXZ vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 TryVectorYXZ(string[] args, Vector3 defaultValue) => TryVectorYXZ(args, 0, defaultValue);
  ///<summary>Parses YXZ vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 TryVectorYXZ(string[] args, int index) => TryVectorYXZ(args, index, Vector3.zero);
  ///<summary>Parses YXZ vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 TryVectorYXZ(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.y = TryFloat(args, index, defaultValue.y);
    vector.x = TryFloat(args, index + 1, defaultValue.x);
    vector.z = TryFloat(args, index + 2, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> TryVectorYXZRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = TryFloatRange(parts, 1, defaultValue.x);
    var y = TryFloatRange(parts, 0, defaultValue.y);
    var z = TryFloatRange(parts, 2, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  ///<summary>Parses scale starting at zero index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 TryScale(string[] args) => TryScale(args, 0);
  ///<summary>Parses scale starting at given index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 TryScale(string[] args, int index) => SanityCheck(TryVectorXZY(args, index));
  private static Vector3 SanityCheck(Vector3 scale)
  {
    // Sanity check and also adds support for setting all values with a single number.
    if (scale.x == 0) scale.x = 1;
    if (scale.y == 0) scale.y = scale.x;
    if (scale.z == 0) scale.z = scale.x;
    return scale;
  }
  public static Range<Vector3> TryScaleRange(string arg)
  {
    var parts = Split(arg);
    var x = TryFloatRange(parts, 0, 0f);
    var y = TryFloatRange(parts, 1, 0f);
    var z = TryFloatRange(parts, 2, 0f);
    var range = ToVectorRange(x, y, z);
    range.Min = SanityCheck(range.Min);
    range.Max = SanityCheck(range.Max);
    return range;
  }

  public static string[] Split(string arg, char separator = ',') => arg.Split(separator).Select(s => s.Trim()).ToArray();
  public static string[] TrySplit(string[] args, int index, char separator)
  {
    if (args.Length <= index) return [];
    return Split(args[index], separator);
  }

  public static float Direction(string value)
  {
    var direction = TryFloat(value, 1);
    if (direction > 0) return 1f;
    return -1f;
  }

  public static Vector3Int ParseXYZInt(string value)
  {
    var vector = Vector3Int.zero;
    var split = value.Split(',');
    if (split.Length > 0) vector.x = TryInt(split[0]);
    if (split.Length > 1) vector.y = TryInt(split[1]);
    if (split.Length > 2) vector.z = TryInt(split[2]);
    return vector;
  }
}
