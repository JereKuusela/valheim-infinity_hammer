using System;
using System.Collections.Generic;
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
public abstract class SelectionBase
{
    private List<GameObject?> m_Objects;
    public virtual List<GameObject?> Objects {
        get => m_Objects;
        set => m_Objects
            = value ?? throw new ArgumentNullException(nameof(value));
    }
   public SelectionBase(List<GameObject?> gameObjects) {
       if (gameObjects.Count == 0) throw new ArgumentNullException("SelectioBase: gameObjects is empty" );
        this.m_Objects = gameObjects;
    }
    public abstract Vector3 Rotation { get; set; }
    public abstract string  Name     { get; set; }
    
    public abstract List<GameObject> GetSnapPoints();
}



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

