using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;
public class ToolScaling(bool sanityY, bool minXZ)
{
  private readonly bool SanityY = sanityY;
  private readonly bool MinXZ = minXZ;
  public Vector3 Value = Vector3.one;
  public float X => MinXZ ? Mathf.Max(0.25f, Value.x) : Value.x;
  public float Y => Value.y;
  public float Z => MinXZ ? Mathf.Max(0.25f, Value.z) : Value.z;

  private void Sanity()
  {
    Value.x = Mathf.Max(0f, Value.x);
    if (SanityY)
      Value.y = Mathf.Max(0f, Value.y);
    Value.z = Mathf.Max(0f, Value.z);
    // Little hack to keep scalings more consistent.
    if (Mathf.Approximately(Value.x, Value.z))
      Value.x = Value.z;
  }

  public void SetPrecisionXZ(float min, float precision)
  {
    Value.x = min + precision * Mathf.Floor((Value.x - min) / precision);
    Value.z = min + precision * Mathf.Floor((Value.z - min) / precision);
  }
  public void Zoom(float amount, float percentage)
  {
    Value += new Vector3(amount, amount, amount);
    if (percentage < 0f) Value /= 1f - percentage;
    else Value *= 1f + percentage;
    Sanity();
  }
  public void ZoomX(float amount, float percentage)
  {
    Value.x += amount;
    if (percentage < 0f) Value.x /= 1f - percentage;
    else Value.x *= 1f + percentage;
    Sanity();
  }
  public void ZoomY(float amount, float percentage)
  {
    Value.y += amount;
    if (percentage < 0f) Value.y /= 1f - percentage;
    else Value.y *= 1f + percentage;
    Sanity();
  }

  public void ZoomZ(float amount, float percentage)
  {
    Value.z += amount;
    if (percentage < 0f) Value.z /= 1f - percentage;
    else Value.z *= 1f + percentage;
    Sanity();
  }
  public void SetScale(float value)
  {
    Value = value * Vector3.one;
    Sanity();
  }
  public void SetScaleX(float value)
  {
    Value.x = value;
    Sanity();
  }
  public void SetScaleY(float value)
  {
    Value.y = value;
    Sanity();
  }
  public void SetScaleZ(float value)
  {
    Value.z = value;
    Sanity();
  }
  public void SetScale(Vector3 value)
  {
    Value = value;
    Sanity();
  }
  public void Print(Terminal terminal)
  {
    if (Configuration.DisableScaleMessages) return;
    if (X != Y || X != Z)
      Helper.AddMessage(terminal, $"Scale set to X: {X:P0}, Z: {Z:P0}, Y: {Y:P0}.");
    else
      Helper.AddMessage(terminal, $"Scale set to {Y:P0}.");
  }
}
