using UnityEngine;
namespace InfinityHammer;
public class HammerPosCommand
{
  public HammerPosCommand()
  {
    CommandWrapper.Register("hammer_pos", (int index, int subIndex) => index == 0 ? CommandWrapper.XZY("Coordinates", subIndex) : index == 1 ? CommandWrapper.YXZ("Rotation", subIndex) : null);

    new Terminal.ConsoleCommand("hammer_pos", "[posX,posZ,posY=0,0,0] [rotY,rotX,rotZ=0,0,0] - Sets the hammer position.", (args) =>
    {
      var value = Vector3.zero;
      if (args.Length > 1) value = Helper.ParseXZY(args[1]);
      Position.Override = value;
      if (args.Length > 2)
      {
        value = Helper.ParseYXZ(args[2]);
        Rotating.Set(Quaternion.Euler(value));
      }
    });
  }
}
