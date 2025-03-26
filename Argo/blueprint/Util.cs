using System;
using System.Collections.Generic;
using UnityEngine;

namespace Argo.Blueprint;
public class Util
{
    // copied from infinityhammer
    public static bool IsSnapPoint(GameObject obj)
        => obj && obj.CompareTag("snappoint");
    // copied from infinityhammer
    public static bool IsSnapPoint(Transform tr) => IsSnapPoint(tr.gameObject);
    // copied from infinityhammer
    public static List<GameObject?> GetChildren(GameObject? obj) {
        List<GameObject?> children = [];
        foreach (Transform tr in obj.transform)
        {
            if (!IsSnapPoint(tr)) children.Add(tr.gameObject);
        }
        return children;
    }
    // copied from infinityhammer
    public static List<GameObject> GetSnapPoints(GameObject? obj) {
        List<GameObject> snapPoints = [];
        foreach (Transform tr in obj.transform)
        {
            if (IsSnapPoint(tr)) snapPoints.Add(tr.gameObject);
        }
        return snapPoints;
    }

    public static string GetPrefabName(GameObject? obj) {
        return obj.name.Split(new[] { ' ', '(' }, 2)[0];
    }

    public static GameObject? CreateDummy(string prefab) {
        var go = ZNetScene.instance.GetPrefab(prefab);
        if (!go) { return null; }
        var view = go.GetComponent<ZNetView>();
        return UnityEngine.Object.Instantiate(view.gameObject);
    }
    public static GameObject? CreateDummy(int hash) {
        var go = ZNetScene.instance.GetPrefab(hash);
        if (!go) { return null; }
        var view = go.GetComponent<ZNetView>();
        return UnityEngine.Object.Instantiate(view.gameObject);
    }
    // copied from infinityhammer
      private static void DestroyComponents<T>(GameObject obj)
        where T : MonoBehaviour {
        // copied from infinityhammer
        // DestroyImmediate is needed to prevent Awake.
        // However some scripts rely on each other, so it's better to disable them if possible.
        foreach (MonoBehaviour component in obj.GetComponentsInChildren<T>())
            UnityEngine.Object.DestroyImmediate(component);
    }

    // copied from infinityhammer
    private static void ResetHighlight(GameObject obj) {
        // copied from infinityhammer
        // Must be done for the original because m_oldMaterials is not copied.
        if (obj.TryGetComponent<WearNTear>(out var wear))
            wear.ResetHighlight();
    }
    // copied from infinityhammer
    private static void DisableComponents<T>(GameObject obj)
        where T : MonoBehaviour {
        // copied from infinityhammer
        // Disable is enough to prevent Start.
        foreach (MonoBehaviour component in obj.GetComponentsInChildren<T>())
            component.enabled = false;
    }
    // copied from infinityhammer
    private static void CleanObject(GameObject obj) {
        // copied from infinityhammer
        DisableComponents<FootStep>(obj);
        DisableComponents<Growup>(obj);
        DisableComponents<RandomFlyingBird>(obj);
        DisableComponents<Windmill>(obj);
        DisableComponents<MineRock>(obj);

        DestroyComponents<MineRock5>(obj);
        DestroyComponents<Fish>(obj);
        DestroyComponents<CharacterAnimEvent>(obj);
        DestroyComponents<TreeLog>(obj);
        DestroyComponents<TreeBase>(obj);
        DestroyComponents<CreatureSpawner>(obj);
        DestroyComponents<TombStone>(obj);
        DestroyComponents<Aoe>(obj);
        DestroyComponents<DungeonGenerator>(obj);
        DestroyComponents<MusicLocation>(obj);
        DestroyComponents<SpawnArea>(obj);
        DestroyComponents<Procreation>(obj);
        DestroyComponents<StaticPhysics>(obj);
        DestroyComponents<Tameable>(obj);
        DestroyComponents<MonsterAI>(obj);
        DestroyComponents<AnimalAI>(obj);
        DestroyComponents<Catapult>(obj);

        // Many things rely on Character so better just undo the Awake.
        var c = obj.GetComponent<Character>();
        if (c)
            Character.s_characters.Remove(c);
    }

    public static GameObject
        ChildInstantiate(ZNetView view, GameObject parent) {
        // copied from infinityhammer
        var obj = view.gameObject;
        ResetHighlight(obj);
        var ret = UnityEngine.Object.Instantiate(obj, parent.transform);
        CleanObject(ret);
        return ret;
    }
}

public struct Vector3_
{
    public Vector3_(Vector3 v) => this.v = v;

    public Vector3 v;
    public float   x { get => v.x; set => v.x = value; }
    public float   y { get => v.x; set => v.x = value; }
    public float   z { get => v.x; set => v.x = value; }

    public static implicit operator Vector3(Vector3_ vector) =>
        vector.v;
    public static implicit operator Vector3_(Vector3 vector) =>
        new(vector);
    public static implicit operator float[](Vector3_ vector) =>
        new[] { vector.x, vector.y, vector.z };
    public static implicit operator Vector3_(float[] arr) {
        if (arr.Length >= 3)
        {
            return new Vector3_(new Vector3(arr[0], arr[1], arr[2]));
        }

        throw new ArgumentException("Invalid array length");
    }
}