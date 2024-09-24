using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;
public class HammerPosCommand
{
  public HammerPosCommand()
  {
    AutoComplete.Register("hammer_pos", (int index, int subIndex) => index == 0 ? ParameterInfo.XZY("Coordinates", subIndex) : index == 1 ? ParameterInfo.YXZ("Rotation", subIndex) : ParameterInfo.None);

    Helper.Command("hammer_pos", "[posX,posZ,posY=0,0,0] [rotY,rotX,rotZ=0,0,0] - Sets the hammer position.", (args) =>
    {
      HammerHelper.CheatCheck();
      Hammer.Equip();
      var value = Vector3.zero;
      if (args.Length > 1) value = Parse.VectorXZY(args[1]);
      Position.Override = value;
      if (args.Length > 2)
      {
        value = Parse.VectorYXZ(args[2]);
        PlaceRotation.Set(Quaternion.Euler(value));
      }
    });
  }
}
