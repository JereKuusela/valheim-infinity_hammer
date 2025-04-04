using UnityEngine;


namespace Argo.Blueprint;

/*public BlueprintJson BuildBluePrint(Player player, GameObject obj, string centerPiece,
    string snapPiece, bool saveData)
{
    var fac = Setup(player, obj, centerPiece, snapPiece, saveData);
    // todo add chase for single object
    fac.AddObjects(snapPiece, saveData);
    if (snapPiece == "")
    {
        var snaps = Snapping.GetSnapPoints(obj);
        foreach (var snap in snaps)
            fac.bp.SnapPoints.Add(snap.transform.localPosition);
    }

    return fac.bp;
}*/

public static class ArgoWrappers
{ // todo
    public static class Configuration
    {
        public static bool UseBlueprintChance = true;
    }

    public static string GetPrefabName(GameObject? obj)
        => Utils.GetPrefabName(obj);
}

interface IPieceData { }

