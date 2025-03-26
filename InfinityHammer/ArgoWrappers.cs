using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Argo.Blueprint;

namespace InfinityHammer;

public class SelectedObjects : SelectionBase
{
    protected GameObject m_placementGhost;
    protected Vector3    m_Rotation;
    protected string     m_Name;

    public SelectedObjects(GameObject? placementGhost_) :
        base(Util.GetChildren(placementGhost_)) {
        if (placementGhost_ is null)
            throw new ArgumentNullException("No objects selected.");
        m_placementGhost = placementGhost_;
        m_Rotation       = placementGhost_.transform.rotation.eulerAngles;
        var piece = placementGhost_.GetComponent<Piece>();
        m_Name = piece
            ? Localization.instance.Localize(piece.m_name)
            : Utils.GetPrefabName(placementGhost_);
    }
    public override Vector3 Rotation { get;           set; }
    public override string  Name     { get => m_Name; set => m_Name = value; }
    public virtual GameObject PlacementGhost {
        get => m_placementGhost;
        set => m_placementGhost = value;
    }
    
    public override List<GameObject> GetSnapPoints() => Util.GetSnapPoints(m_placementGhost); 
   
    
}

