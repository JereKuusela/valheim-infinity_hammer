using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Argo.Blueprint.Util;

namespace Argo.Blueprint;

using static Argo.Blueprint.Argonaut;
using static Argo.Blueprint.Configuration;
using static Argo.Blueprint.BPOFlags;

public class PrefabRegister : ObjectPool<PrefabRegister>
{
    private ImmutableList<ImmutableDictionary<string, BPOFlags>> m_byFlag;
    private ImmutableDictionary<string, BPOFlags>                m_prefabs;
    private ImmutableDictionary<string, BPOFlags>                m_includedPrefabs;
    private ImmutableDictionary<string, BPOFlags>                m_ignoredPrefabs;
    private PrefabRegister() : base(true) {
        m_prefabs         = ImmutableDictionary<string, BPOFlags>.Empty;
        m_byFlag          = ImmutableList<ImmutableDictionary<string, BPOFlags>>.Empty;
        m_includedPrefabs = ImmutableDictionary<string, BPOFlags>.Empty;
        m_ignoredPrefabs  = ImmutableDictionary<string, BPOFlags>.Empty;
    }

    protected static PrefabRegister CreateEmpty() { return new PrefabRegister(); }
    private PrefabRegister(PrefabRegister other) : base(false) {
        m_prefabs         = other.m_prefabs;
        m_byFlag          = other.m_byFlag;
        m_includedPrefabs = other.m_includedPrefabs;
        m_ignoredPrefabs  = other.m_ignoredPrefabs;
    }
    static PrefabRegister CreateDefaultInstance() {
        // todo maybe create default instance and determine some of the
        // todo   flags there
        Dictionary<string, BPOFlags>   byName = new Dictionary<string, BPOFlags>();
        Dictionary<string, BPOFlags>[] byFlag = new Dictionary<string, BPOFlags>[32];
        Add(ref byFlag, ref byName, BuildPieces.Static, [BuildPlayer]);

        Add(ref byFlag, ref byName, BuildPieces.Nameable, [
            BuildPlayer, TextReceiver
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Interactable, [
            BuildPlayer, Interactable,
        ]);
        Add(ref byFlag, ref byName, BuildPieces.NonStatic, [
            BuildPlayer,
            NonStatic,
            SpecialInterface
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Vehicle, [
            BuildPlayer,
            Vehicle,
            Interactable,
            Hoverable,
        ]);

        Add(ref byFlag, ref byName, ["itemstand", "itemstandh"], [
            BuildPlayer | LightSource | ObjectHolder
        ]);

        Add(ref byFlag, ref byName, ["ArmorStand"],
        [
            BuildPlayer,
            LightSource,
            ObjectHolder,
            Compfort
        ]);

        Add(ref byFlag, ref byName, ["guard_stone",], [
            BuildPlayer,
            LightSource,
            Hoverable,
            SpecialInterface,
            TextReceiver,
        ]);

        Add(ref byFlag, ref byName, ["piece_wisplure",], [
            BuildPlayer,
            LightSource,
            Hoverable,
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Comfort.Static, [
            BuildPlayer,
            Compfort
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Comfort.Interactable,
        [
            LightSource,
            BuildPlayer,
            Compfort
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Comfort.LightFuel, [
            BuildPlayer,
            Compfort,
            LightSource,
            Fuel,
        ]);

        Add(ref byFlag, ref byName, BuildPieces.Workbench.Light, [
            BuildPlayer,
            CraftingStation,
            LightSource
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Workbench.FuelLight, [
            BuildPlayer,
            CraftingStation,
            LightSource,
            Fuel,
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Workbench.Fuel, [
            BuildPlayer,
            CraftingStation,
            Fuel,
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Workbench.Static,
        [
            BuildPlayer,
            CraftingStation,
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Workbench.Animated, [
            BuildPlayer,
            CraftingStation,
            Animated,
        ]);

        Add(ref byFlag, ref byName, BuildPieces.Container.Player, [
            BuildPlayer,
            ContainerPiece
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Container.NonPlayer, [
            BPOFlags.BuildPiece,
            ContainerPiece
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Lights.Fuel, [
            BuildPlayer,
            Fuel,
            LightSource,
        ]);
        Add(ref byFlag, ref byName, BuildPieces.Lights.NoFuel, [
            BuildPlayer,
            LightSource,
        ]);

        Add(ref byFlag, ref byName, pickable, [
            Pickable,
            NonStatic,
        ]);
        Add(ref byFlag, ref byName, PieceNonPlayer.NonStatic, [
            BuildPlayer,
            NonStatic,
        ]);
        Add(ref byFlag, ref byName, PieceNonPlayer.Static, [
            BPOFlags.BuildPiece,
        ]);
        Add(ref byFlag, ref byName, PieceNonPlayer.Interactable,
        [
            BPOFlags.BuildPiece,
            NonStatic,
        ]);
        Add(ref byFlag, ref byName, PieceNonPlayer.LightSource, [
            BPOFlags.BuildPiece,
            LightSource,
        ]);

        Add(ref byFlag, ref byName, ["piece_xmastree"], [
            BuildPlayer,
            LightSource,
            Compfort,
        ]);
        Add(ref byFlag, ref byName, ["piece_bathtub"], [
            BuildPlayer,
            LightSource,
            Interactable,
            Fuel,
            Compfort,
        ]);
        // todo split in struly static pieces like rocks and those who have animation like trees in wind
        Add(ref byFlag, ref byName, Terrain.Static, [
            DestroyableTerrain,
        ]);
        // todo, prefabname might be the same as non fractured
        Add(ref byFlag, ref byName, Terrain.Fractured, [
            DestroyableTerrain,
            Fractured,
        ]);
        // todo, prefabname might be the same as non fractured
        Add(ref byFlag, ref byName, Terrain.Animated, [
            DestroyableTerrain,
            Animated,
        ]);
        Add(ref byFlag, ref byName, Terrain.Iteractable, [
            Interactable,
            Animated,
        ]);
        Add(ref byFlag, ref byName, Creatures.Enemy, [
            Creature,
        ]);
        Add(ref byFlag, ref byName, Creatures.Tameable, [
            Creature,
            Tameable,
        ]);
        Add(ref byFlag, ref byName, Creatures.Special, [
            Creature,
            SpecialInterface,
        ]);
        Add(ref byFlag, ref byName, Creatures.Spawner, [
            Creature,
            SpecialInterface,
        ]);
        Add(ref byFlag, ref byName, Creatures.Fish, [
            Creature,
            SpecialInterface,
        ]);
        Add(ref byFlag, ref byName, Indestructibles.Static, [
            BPOFlags.Indestructible
        ]);
        Add(ref byFlag, ref byName, Indestructibles.Interactable,
        [
            BPOFlags.Indestructible,
            Interactable
        ]);
        Add(ref byFlag, ref byName, ["player_tombstone"], [
            BPOFlags.Indestructible,
            SpecialInterface,
            Interactable,
            ContainerPiece
        ]);
        Add(ref byFlag, ref byName, Indestructibles.Runestones, [
            BPOFlags.Indestructible,
            SpecialInterface,
            LightSource,
        ]);
        var instance = new PrefabRegister();
        instance.m_prefabs = byName.ToImmutableDictionary(x => x.Key, x => x.Value);
        ImmutableList<ImmutableDictionary<string, BPOFlags>> list = [];
        for (int i = 0; i < 32; i++) {
            var item = byFlag[i].ToImmutableDictionary(x => x.Key, x => x.Value);
            list = list.Add(item);
        }
        instance.m_byFlag = list;
        return instance;
    }

    /// <summary>
    /// before Accessing AllPrefabs for the first time oder after prefabs where added or removed
    /// call UpdatePrefabs otherwise the collection will not be up to date.
    /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
    /// </summary>
    public ImmutableDictionary<string, BPOFlags> AllPrefabs {
        get => m_prefabs;
        internal set => m_prefabs = value;
    }
    /// <summary>
    /// before Accessing IncludedPrefabs for the first time oder after prefabs where added or removed
    /// call UpdatePrefabs otherwise the collection will not be up to date.
    /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
    /// </summary>
    public ImmutableDictionary<string, BPOFlags> IncludedPrefabs {
        get => m_includedPrefabs;
        internal set => m_includedPrefabs = value;
    }
    /// <summary>
    /// before Accessing ExcludedPrefabs for the first time oder after prefabs where added or removed
    /// call UpdatePrefabs otherwise the collection will not be up to date.
    /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
    /// </summary>
    public ImmutableDictionary<string, BPOFlags> IgnoredPrefabs {
        get => ImmutableDictionary<string, BPOFlags>.Empty;
        //internal set => _ignoredPrefabs = value;
    }
    public bool IsIncluded(string prefab) { return IncludedPrefabs.ContainsKey(prefab); }

    private CategorySettings[] categorySettings
        = Enumerable.Repeat(CategorySettings.Include, 32)
                    .ToArray();

    private (BPOFlags, string)[] GetCategorySettings() {
        // todo move to config or bepinex loader or something
        return [
            (BPOFlags.BuildPlayer, EnableBuildPlayer),
            (BPOFlags.BuildPiece, EnableBuildPiece),
            (BPOFlags.Placeable, EnablePlaceable),
            (BPOFlags.Compfort, EnableCompfort),
            (BPOFlags.LightSource, EnableLightSource),
            (BPOFlags.Hoverable, EnableHoverable),
            (BPOFlags.TextReceiver, EnableTextReceiver),
            (BPOFlags.Interactable, EnableInteractable),
            (BPOFlags.ObjectHolder, EnableObjectHolder),
            (BPOFlags.ContainerPiece, EnableContainerPiece),
            (BPOFlags.CraftingStation, EnableCraftingStation),
            (BPOFlags.Fuel, EnableFuel),
            (BPOFlags.Pickable, EnablePickable),
            (BPOFlags.Cultivated, EnableCultivated),
            (BPOFlags.DestroyableTerrain, EnableDestroyableTerrain),
            (BPOFlags.Fractured, EnableFractured),
            (BPOFlags.HoverableResourceNode,
                EnableHoverableResourceNode),
            (BPOFlags.NonStatic, EnableNonStatic),
            (BPOFlags.Creature, EnableCreature),
            (BPOFlags.Tameable, EnableTameable),
            (BPOFlags.Vehicle, EnableVehicle),
            (BPOFlags.Animated, EnableAnimated),
            (BPOFlags.SpecialInterface, EnableSpecialInterface),
            (BPOFlags.Indestructible, EnableIndestructible),
            (BPOFlags.IsVanilla, EnableCustomNotVanilla),
        ];
    }
    public void SetCategorySettings() {
        var settings = GetCategorySettings();
        foreach (var (flags, str) in settings) {
            try {
                var index = GetIndex(flags);

                switch (str) {
                    case CategorySettingsStrings.Exclude:
                        categorySettings[index]
                            = CategorySettings.Exclude;
                        break;
                    case CategorySettingsStrings.ForceExclude:
                        categorySettings[index]
                            = CategorySettings.ForceExclude;
                        break;
                    default:
                        categorySettings[index]
                            = CategorySettings.Include;
                        break;
                }
            } catch (ArgumentException e) {
                System.Console.WriteLine(e + " Flag:" + flags);
            }
        }
    }

    private ImmutableDictionary<string, BPOFlags> UpdateIncludedPrefabs() {
        // todo check for old categorysettings to not have to make in completly new
        for (int i = 0; i < m_byFlag.Count; i++) {
            if (categorySettings[i] ==
                CategorySettings.Include) {
                foreach (var pair in m_byFlag[i])
                    IncludedPrefabs = IncludedPrefabs.Add(pair.Key, pair.Value);
            } else if (categorySettings[i] ==
                       CategorySettings.ForceExclude) {
                foreach (var pair in m_byFlag[i])
                    IncludedPrefabs = IncludedPrefabs.Remove(pair.Key);
            }
        }

        return IncludedPrefabs;
    }

    private ImmutableDictionary<string, BPOFlags> UpdateExcludedPrefabs() {
        m_ignoredPrefabs
            = AllPrefabs.Except(IncludedPrefabs).ToImmutableDictionary(x => x.Key, x => x.Value);
        return IgnoredPrefabs;
    }
    public void UpdatePrefabs() {
        UpdateIncludedPrefabs();
        UpdateExcludedPrefabs();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BPOFlags CombineFlags(params BPOFlags[] flags) {
        BPOFlags combined = 0;
        foreach (var flag in flags) {
            combined |= flag;
        }
        return combined;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableDictionary<string, BPOFlags>[] GetAll(params BPOFlags[] flags) {
        return GetAll(CombineFlags(flags));
    }
    private ImmutableDictionary<string, BPOFlags> IntersectUnchecked(int[] indices) {
        (int count, ImmutableDictionary<string, BPOFlags> list)[] arr =
            new (int, ImmutableDictionary<string, BPOFlags>)[indices.Length];
        for (int i = 0; i < indices.Length; i++) {
            arr[i] = (m_byFlag[i].Count, m_byFlag[i]);
        }

        // orders the array by the length of the lists to start with the list
        // with the least items to break loops as soon as possible
        arr = arr.OrderBy(x => x.count).ToArray();
        ImmutableDictionary<string, BPOFlags> result = ImmutableDictionary<string, BPOFlags>.Empty;
        foreach (var pair in arr[0].list) {
            // searches for the prefab of first list in the other lists 
            bool found = true;
            for (int i = 1; i < arr.Length; i++) {
                // iterates through all all other lists and breaks if an element isnt
                //  found in any of the lists. if it is contained in all lists its added
                //  to the result list

                if (arr[i].list.Contains(pair) == false) {
                    found = false;
                    break;
                }
            }

            if (found) {
                result = result.Add(pair.Key, pair.Value);
            }
        }

        return result;
    }
    public ImmutableDictionary<string, BPOFlags> Intersect(params BPOFlags[] flags) {
        var                          indices = GetAllIndices(CombineFlags(flags));
        Dictionary<string, BPOFlags> result  = new Dictionary<string, BPOFlags>();
        switch (indices.Length) {
            case 0: throw new ArgumentException("No lists found");
            case 1:
                return m_byFlag[indices[0]];
            case 2:
                return Enumerable.Intersect(m_byFlag[indices[0]], m_byFlag[indices[1]])
                                 .ToImmutableDictionary();
            default:
                return IntersectUnchecked(indices);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndex(BPOFlags flag) {
        uint comp = 1u;
        for (int i = 0; i < 32; i++) {
            comp = 1u << i;
            // tests if a flag is set and adds the list to the collection
            if (comp == (uint)flag) {
                return i;
            }
        }

        throw new ArgumentException(
            "No flag is set or multiple flags are set");
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int[] GetAllIndices(BPOFlags flag) {
        var  lists = new List<int>();
        uint comp  = 1;
        for (int i = 0; i < 32; i++) {
            comp = comp << i;
            // tests if a flag is set and adds the list to the collection
            if ((comp & (uint)flag) != 0) {
                lists.Add(i);
            }
        }

        return lists.ToArray();
    }
    public ImmutableDictionary<string, BPOFlags>[] GetAll(BPOFlags flag) {
        var indices = GetAllIndices(flag);
        ImmutableDictionary<string, BPOFlags>[] lists
            = new ImmutableDictionary<string, BPOFlags>[indices.Length];
        foreach (var i in indices) {
            lists[i] = m_byFlag[i];
        }

        return lists;
    }
    protected ImmutableDictionary<string, BPOFlags> Get(int index) {
        if (index is < 0 or > 31) {
            throw new ArgumentOutOfRangeException(
                "Index out of range, must be between 0 and 31");
        } else {
            return m_byFlag[index];
        }
    }

    public ImmutableDictionary<string, BPOFlags> Get(BPOFlags flag) {
        uint unit = (uint)flag;
        switch ((uint)flag) {
            case 1u << 0:  return m_byFlag[0];
            case 1u << 1:  return m_byFlag[1];
            case 1u << 2:  return m_byFlag[2];
            case 1u << 3:  return m_byFlag[3];
            case 1u << 4:  return m_byFlag[4];
            case 1u << 5:  return m_byFlag[5];
            case 1u << 6:  return m_byFlag[6];
            case 1u << 7:  return m_byFlag[7];
            case 1u << 8:  return m_byFlag[8];
            case 1u << 9:  return m_byFlag[9];
            case 1u << 10: return m_byFlag[10];
            case 1u << 11: return m_byFlag[11];
            case 1u << 12: return m_byFlag[12];
            case 1u << 13: return m_byFlag[13];
            case 1u << 14: return m_byFlag[14];
            case 1u << 15: return m_byFlag[15];
            case 1u << 16: return m_byFlag[16];
            case 1u << 17: return m_byFlag[17];
            case 1u << 18: return m_byFlag[18];
            case 1u << 19: return m_byFlag[19];
            case 1u << 20: return m_byFlag[20];
            case 1u << 21: return m_byFlag[21];
            case 1u << 22: return m_byFlag[22];
            case 1u << 23: return m_byFlag[23];
            case 1u << 24: return m_byFlag[24];
            case 1u << 25: return m_byFlag[25];
            case 1u << 26: return m_byFlag[26];
            case 1u << 27: return m_byFlag[27];
            case 1u << 28: return m_byFlag[28];
            case 1u << 29: return m_byFlag[29];
            case 1u << 30: return m_byFlag[30];
            case 1u << 31: return m_byFlag[31];
            default:
                throw new ArgumentOutOfRangeException(
                    "Flag not found probably because mutliply flags are used" +
                    "For getting multiple Flags use getall");
        }
    }

    private static void Add(
        ref Dictionary<string, BPOFlags>[] byFlag, ref Dictionary<string, BPOFlags> byName,
        string[]                           prefabs,
        params BPOFlags[]                  flags) {
        var      indices  = GetAllIndices(CombineFlags(flags));
        BPOFlags combined = CombineFlags(flags);
        foreach (string prefab in prefabs) {
            byName[prefab] = combined;
            foreach (var index in indices) {
                byFlag[index].Add(prefab, combined);
            }
        }
    }
    public void Add(
        string[]          prefabs,
        params BPOFlags[] flags) {
        if (_isDefaultInstance)
            throw new AccessViolationException(
                "Its not permited to modify the default instance");        var      indices  = GetAllIndices(CombineFlags(flags));
        BPOFlags combined = CombineFlags(flags);
        foreach (string prefab in prefabs) {
            this.m_prefabs = m_prefabs.SetItem(prefab, combined);
            foreach (var index in indices) {
                var dict = m_byFlag[index].SetItem(prefab, combined);
                this.m_byFlag = this.m_byFlag.SetItem(index, dict);
            }
        }
    }
    public void Add(
        string            prefab,
        params BPOFlags[] flags) {
        if (_isDefaultInstance)
            throw new AccessViolationException(
                "Its not permited to modify the default instance");
        var      indices  = GetAllIndices(CombineFlags(flags));
        BPOFlags combined = CombineFlags(flags);
        {
            this.m_prefabs = m_prefabs.SetItem(prefab, combined);
            foreach (var index in indices) {
                var dict = m_byFlag[index].SetItem(prefab, combined);
                this.m_byFlag = this.m_byFlag.SetItem(index, dict);
            }
        }
    }

    private static void Remove(
            ImmutableDictionary<string, BPOFlags>[] Prefablists, string[] prefabs,
            params BPOFlags[]                       flags) {
            var indices = GetAllIndices(CombineFlags(flags));
            foreach (var index in indices) {
                foreach (var prefab in prefabs) {
                    // todo rework since this wont remove anything
                    Prefablists[index].Remove(prefab);
                }
            }
        }
    public override PrefabRegister Clone() { return new PrefabRegister(this); }

    // params[] must by of type KeyValuePair<string, BPOFlags>;
    public override void AddToMod(ModIdentifier mod, params object[] pairs) {
        for (int i = 0; i < pairs.Length; i++) {
            if (typeof(KeyValuePair<string, BPOFlags>) != pairs[i].GetType()) {
                throw new ArgumentException(
                    "Arguements must be of type KeyValuePair<string, BPOFlags>");
            }
            var pair = (KeyValuePair<string, BPOFlags>)pairs[i];
            _modInstances[mod].Add([pair.Key], pair.Value);
        }
    }
}