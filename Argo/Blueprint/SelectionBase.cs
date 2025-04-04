using System;
using System.Collections.Generic;
using UnityEngine;

namespace Argo.Blueprint;

public abstract class SelectionBase
{
    private   List<GameObject?> m_Objects;
    private   List<AExtraData>  m_ZVars;
    protected Vector3           m_Rotation;
    protected Vector3           m_Position;
    protected string            m_Name;

    public virtual List<GameObject?> Objects {
        get => m_Objects;
        set => m_Objects
            = value ?? throw new ArgumentNullException(nameof(value));
    }
    public virtual List<AExtraData> ZVars {
        get => m_ZVars;
        set => m_ZVars
            = value ?? throw new ArgumentNullException(nameof(value));
    }
    public SelectionBase(List<GameObject?> gameObjects, Vector3 position, Vector3 rotation, string name = "") {
        if (gameObjects.Count == 0) throw new ArgumentNullException("SelectioBase: gameObjects is empty" );
        this.m_Objects  = gameObjects;
        this.m_ZVars    = [];
        this.m_Rotation = rotation;
        this.m_Position = position;
        this.m_Name     = name;
    }
    public SelectionBase(List<GameObject?> gameObjects, List<AExtraData> zVars, Vector3 position, Vector3 rotation, string name = "") {
        if (gameObjects.Count == 0) throw new ArgumentNullException("SelectioBase: gameObjects is empty" );
        this.m_Objects  = gameObjects;
        this.m_ZVars    = zVars;
        this.m_Rotation = rotation;
        this.m_Position = position;
        this.m_Name     = name;
    }
    public virtual Vector3 Rotation { get => m_Rotation;           set => m_Rotation = value; }
    
    public virtual Vector3 Position { get => m_Position;           set => m_Position = value; }

    public virtual  string           Name     { get => m_Name; set => m_Name = value; }
    public abstract List<GameObject> GetSnapPoints();
}