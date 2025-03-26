using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;
using UnityEngine;

namespace Argo
{

    namespace Blueprint
    {
        using static Argo.Blueprint.Argonaut;

        
        using TExportBefore = Func<string, BpoRegister, bool>;
        using TExportWorker = Func<ExportIterator, bool, BpjObject>;
        using TImportBefore = Func<string, BpoRegister, bool>;
        using TImportWorker = Func<ImportIterator, bool, BpjObject>;

        public enum CategorySettings : byte
        {
            Include      = 1,
            Exclude      = 2, // todo change to DoNotInclude or Ignore
            ForceExclude = 3
        }

        public static class CategorySettingsStrings
        {
            public const string Include = "Include";
            public const string
                Exclude = "Exclude"; // todo change to DoNotInclude or Ignore
            public const string ForceExclude = "ForceExclude";
        }

        public class BpjFetcherUnknown(BPOFlags[] flags_)
            : BpjFetcher(flags_)
        {
#if DEBUG
            public override bool IsKnown() => false;
#endif
        };



        public class BpoRegister
        {
            internal Dictionary<string, BpjFetcher> fetchers =
                new Dictionary<string, BpjFetcher>();
            internal BpjFetcherUnknown m_default =
                new BpjFetcherUnknown([0]);
            internal       Configuration Configuration = new Configuration();
            public         BpoRegisterByFlag ByFlag = new BpoRegisterByFlag();
            private static BpoRegister? DefaultInstance;
            public static BpoRegister Default {
                get => BpoRegister.GetDefault();
            }

            /// <summary>
            /// before Accessing AllPrefabs for the first time oder after prefabs where added or removed
            /// call UpdatePrefabs otherwise the collection will not be up to date.
            /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
            /// </summary>
            public SortedSet<string> AllPrefabs {
                get => ByFlag.AllPrefabs;
                internal set => ByFlag.AllPrefabs = value;
            }
            /// <summary>
            /// before Accessing IncludedPrefabs for the first time oder after prefabs where added or removed
            /// call UpdatePrefabs otherwise the collection will not be up to date.
            /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
            /// </summary>
            public SortedSet<string> IncludedPrefabs {
                get => ByFlag.IncludedPrefabs;
                internal set => ByFlag.IncludedPrefabs = value;
            }
            /// <summary>
            /// before Accessing ExcludedPrefabs for the first time oder after prefabs where added or removed
            /// call UpdatePrefabs otherwise the collection will not be up to date.
            /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
            /// </summary>
            public SortedSet<string> ExcludedPrefabs {
                get => ByFlag.ExcludedPrefabs;
                internal set => ByFlag.ExcludedPrefabs = value;
            }
            public bool IsIncluded(string prefab) {
                return IncludedPrefabs.Contains(prefab);
            }
            public static BpoRegister GetDefault() {
                if (DefaultInstance == null)
                {
                    DefaultInstance = new BpoRegister();
                    Configuration.AddToRegister(DefaultInstance);
                }

                return DefaultInstance;
            }
            public void Add(string prefab, BpjFetcher fetcher) {
                if (fetchers.ContainsKey(prefab))
                {
                    System.Console.WriteLine(
                        $"{prefab} allready registered, skipped");
                    return;
                }

                fetchers.Add(prefab, fetcher);
            }

            public static void AddToDefault(string[]   prefabs,
                                            BpjFetcher fetcher)
                => GetDefault().Add(prefabs, fetcher);
            public void Add(string[] prefabs, BpjFetcher fetcher) {
                foreach (var prefab in prefabs)
                {
                    if (fetchers.ContainsKey(prefab))
                    {
                        System.Console.WriteLine(
                            $"{prefab} allready registered, skipped");
                        continue;
                    }

                    fetchers.Add(prefab, fetcher);
                }
            }
            public static void AddToDefault<Fetcher>(string[] prefab,
                BPOFlags[]                                    flags)
                where Fetcher : BpjFetcher =>
                GetDefault().Add<Fetcher>(prefab, flags);

            public void Add<Fetcher>(string[] prefabs, BPOFlags[] flags)
                where Fetcher : BpjFetcher {
                BpjFetcher fetcher =
                    (Fetcher)Activator.CreateInstance(typeof(Fetcher), flags);
                Add(prefabs, fetcher);
                ByFlag.Add(prefabs, flags);
            }
            public static void AddToDefault<Fetcher>(string prefab,
                BPOFlags[]                                  flags)
                where Fetcher : BpjFetcher
                => GetDefault().Add<Fetcher>(prefab, flags);

            public void Add<Fetcher>(string prefab, BPOFlags[] flags)
                where Fetcher : BpjFetcher {
                BpjFetcher fetcher =
                    (Fetcher)Activator.CreateInstance(typeof(Fetcher), flags);
                Add(prefab, fetcher);
                ByFlag.Add([prefab], flags);
            }
            public static void
                AddToDefault(string[]       prefabs,   TExportBefore? expBefore,
                             TExportWorker? expWorker, TImportBefore? impBefore,
                             TImportWorker? impWorker, BPOFlags[]     flags)
                => GetDefault().Add(prefabs, expBefore, expWorker, impBefore,
                    impWorker, flags);

            public void
                Add(string[]       prefabs, TExportBefore? expBefore,
                    TExportWorker? expWorker,
                    TImportBefore? impBefore, TImportWorker? impWorker,
                    BPOFlags[]     flags)
                => Add(prefabs,
                    new CustomBpjFetcher(expBefore, expWorker, impBefore,
                        impWorker, flags));

            public static BpjFetcher GetFromDefault(string prefab)
                => GetDefault().Get(prefab);

            public BpjFetcher Get(string prefab) {
                if (fetchers.TryGetValue(prefab, out var fetcher))
                {
                    return fetcher;
                }

                return m_default;
            }
        }

        public class BpoRegisterByFlag
        {
            private SortedSet<string>[] Prefablists = new SortedSet<string>[32];
            private SortedSet<string> allPrefabs = new SortedSet<string>();
            private SortedSet<string> includedPrefabs = new SortedSet<string>();
            private SortedSet<string> excludedPrefabs = new SortedSet<string>();

            /// <summary>
            /// before Accessing AllPrefabs for the first time oder after prefabs where added or removed
            /// call UpdatePrefabs otherwise the collection will not be up to date.
            /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
            /// </summary>
            public SortedSet<string> AllPrefabs {
                get => allPrefabs;
                internal set => allPrefabs = value;
            }
            /// <summary>
            /// before Accessing IncludedPrefabs for the first time oder after prefabs where added or removed
            /// call UpdatePrefabs otherwise the collection will not be up to date.
            /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
            /// </summary>
            public SortedSet<string> IncludedPrefabs {
                get => includedPrefabs;
                internal set => includedPrefabs = value;
            }
            /// <summary>
            /// before Accessing ExcludedPrefabs for the first time oder after prefabs where added or removed
            /// call UpdatePrefabs otherwise the collection will not be up to date.
            /// todo maybe implement changecount to automatically detect changes since its kinda expensive to call update everytime
            /// </summary>
            public SortedSet<string> ExcludedPrefabs {
                get => excludedPrefabs;
                internal set => excludedPrefabs = value;
            }

            private CategorySettings[] categorySettings
                = Enumerable.Repeat(CategorySettings.Include, 32)
                            .ToArray();
            public BpoRegisterByFlag() {
                for (int i = 0; i < 32; i++)
                {
                    Prefablists[i] = new SortedSet<string>();
                }

                allPrefabs      = new SortedSet<string>();
                includedPrefabs = new SortedSet<string>();
                excludedPrefabs = new SortedSet<string>();
                categorySettings
                    = Enumerable.Repeat(CategorySettings.Include, 32)
                                .ToArray();
                SetCategorySettings();
                //   UpdatePrefabs();
            }
            private (BPOFlags, string)[] GetCategorySettings() {
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
                    (BPOFlags.CustomNotVanilla, EnableCustomNotVanilla),
                ];
            }
            public void SetCategorySettings() {
                var settings = GetCategorySettings();
                foreach (var (flags, str) in settings)
                {
                    try
                    {
                        var index = GetIndex(flags);

                        switch (str)
                        {
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
                    } catch (ArgumentException e)
                    {
                        System.Console.WriteLine(e + " Flag:" + flags);
                    }
                }
            }
            private SortedSet<string> UpdateAllPrefabs() {
                foreach (var prefabs in Prefablists)
                {
                    foreach (var prefab in prefabs)
                    {
                        AllPrefabs.Add(prefab);
                    }
                }

                return AllPrefabs;
            }

            private SortedSet<string> UpdateIncludedPrefabs() {
                for (int i = 0; i < Prefablists.Length; i++)
                {
                    if (categorySettings[i] ==
                        CategorySettings.Include)
                    {
                        foreach (var prefab in Prefablists[i])
                            IncludedPrefabs.Add(prefab);
                    }
                }

                for (int i = 0; i < Prefablists.Length; i++)
                {
                    if (categorySettings[i] ==
                        CategorySettings.ForceExclude)
                    {
                        foreach (var prefab in Prefablists[i])
                            IncludedPrefabs.Remove(prefab);
                    }
                }

                return IncludedPrefabs;
            }

            private SortedSet<string> UpdateExcludedPrefabs() {
                ExcludedPrefabs
                    = new SortedSet<string>(Enumerable.Except(AllPrefabs,
                        IncludedPrefabs));
                return ExcludedPrefabs;
            }
            private void UpdatePrefabs() {
                UpdateAllPrefabs();
                UpdateIncludedPrefabs();
                UpdateExcludedPrefabs();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static BPOFlags Combine(BPOFlags[] flags) {
                BPOFlags combined = 0;
                foreach (var flag in flags)
                {
                    combined |= flag;
                }

                return combined;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SortedSet<string>[] GetAll(BPOFlags[] flags) {
                return GetAll(Combine(flags));
            }
            private SortedSet<string> IntersectUnchecked(int[] indices) {
                (int count, SortedSet<string> list)[] arr =
                    new (int, SortedSet<string>)[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    arr[i] = (Prefablists[i].Count, Prefablists[i]);
                }

                // orders the array by the length of the lists to start with the list
                // with the least items to break loops as soon as possible
                arr = arr.OrderBy(x => x.count).ToArray();
                SortedSet<string> result = new SortedSet<string>();
                foreach (var prefab in arr[0].list)
                {
                    // searches for the prefab of first list in the other lists 
                    bool found = true;
                    for (int i = 1; i < arr.Length; i++)
                    {
                        // iterates through all all other lists and breaks if an element isnt
                        //  found in any of the lists. if it is contained in all lists its added
                        //  to the result list

                        if (arr[i].list.Contains(prefab) == false)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        result.Add(prefab);
                    }
                }

                return result;
            }
            public SortedSet<string> Intersect(BPOFlags[] flags) {
                var               indices = GetAllIndices(Combine(flags));
                SortedSet<string> result  = new SortedSet<string>();
                switch (indices.Length)
                {
                    case 0: throw new ArgumentException("No lists found");
                    case 1:
                        return new SortedSet<string>(Prefablists[indices[0]]);
                    case 2:
                        return new SortedSet<string>(Prefablists[indices[0]]
                           .Intersect(Prefablists[indices[1]]));
                    default:
                        return IntersectUnchecked(indices);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetIndex(BPOFlags flag) {
                uint comp = 1u;
                for (int i = 0; i < 32; i++)
                {
                    comp = 1u << i;
                    // tests if a flag is set and adds the list to the collection
                    if (comp == (uint)flag)
                    {
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
                for (int i = 0; i < 32; i++)
                {
                    comp = comp << i;
                    // tests if a flag is set and adds the list to the collection
                    if ((comp & (uint)flag) != 0)
                    {
                        lists.Add(i);
                    }
                }

                return lists.ToArray();
            }
            public SortedSet<string>[] GetAll(BPOFlags flag) {
                var indices = GetAllIndices(flag);
                SortedSet<string>[] lists
                    = new SortedSet<string>[indices.Length];
                foreach (var i in indices)
                {
                    lists[i] = new SortedSet<string>(Prefablists[i]);
                }

                return lists;
            }
            public SortedSet<string> Get(int index) {
                if (index is < 0 or > 31)
                {
                    throw new ArgumentOutOfRangeException(
                        "Index out of range, must be between 0 and 31");
                } else
                {
                    return Prefablists[index];
                }
            }
            public SortedSet<string> Get(BPOFlags flag) {
                uint unit = (uint)flag;
                switch ((uint)flag)
                {
                    case 1u << 0: return new SortedSet<string>(Prefablists[0]);
                    case 1u << 1: return new SortedSet<string>(Prefablists[1]);
                    case 1u << 2: return new SortedSet<string>(Prefablists[2]);
                    case 1u << 3: return new SortedSet<string>(Prefablists[3]);
                    case 1u << 4: return new SortedSet<string>(Prefablists[4]);
                    case 1u << 5: return new SortedSet<string>(Prefablists[5]);
                    case 1u << 6: return new SortedSet<string>(Prefablists[6]);
                    case 1u << 7: return new SortedSet<string>(Prefablists[7]);
                    case 1u << 8: return new SortedSet<string>(Prefablists[8]);
                    case 1u << 9: return new SortedSet<string>(Prefablists[9]);
                    case 1u << 10:
                        return new SortedSet<string>(Prefablists[10]);
                    case 1u << 11:
                        return new SortedSet<string>(Prefablists[11]);
                    case 1u << 12:
                        return new SortedSet<string>(Prefablists[12]);
                    case 1u << 13:
                        return new SortedSet<string>(Prefablists[13]);
                    case 1u << 14:
                        return new SortedSet<string>(Prefablists[14]);
                    case 1u << 15:
                        return new SortedSet<string>(Prefablists[15]);
                    case 1u << 16:
                        return new SortedSet<string>(Prefablists[16]);
                    case 1u << 17:
                        return new SortedSet<string>(Prefablists[17]);
                    case 1u << 18:
                        return new SortedSet<string>(Prefablists[18]);
                    case 1u << 19:
                        return new SortedSet<string>(Prefablists[19]);
                    case 1u << 20:
                        return new SortedSet<string>(Prefablists[20]);
                    case 1u << 21:
                        return new SortedSet<string>(Prefablists[21]);
                    case 1u << 22:
                        return new SortedSet<string>(Prefablists[22]);
                    case 1u << 23:
                        return new SortedSet<string>(Prefablists[23]);
                    case 1u << 24:
                        return new SortedSet<string>(Prefablists[24]);
                    case 1u << 25:
                        return new SortedSet<string>(Prefablists[25]);
                    case 1u << 26:
                        return new SortedSet<string>(Prefablists[26]);
                    case 1u << 27:
                        return new SortedSet<string>(Prefablists[27]);
                    case 1u << 28:
                        return new SortedSet<string>(Prefablists[28]);
                    case 1u << 29:
                        return new SortedSet<string>(Prefablists[29]);
                    case 1u << 30:
                        return new SortedSet<string>(Prefablists[30]);
                    case 1u << 31:
                        return new SortedSet<string>(Prefablists[31]);
                    default:
                        throw new ArgumentOutOfRangeException(
                            "Flag not found probably because mutliply flags are used" +
                            "For getting multiple Flags use getall");
                }
            }

            public void Add(string[] prefabs, BPOFlags[] flags) {
                var indices = GetAllIndices(Combine(flags));
                foreach (var index in indices)
                {
                    foreach (var prefab in prefabs)
                    {
                        Prefablists[index].Add(prefab);
                    }
                }
            }

            public void Remove(string[] prefabs, BPOFlags[] flags) {
                var indices = GetAllIndices(Combine(flags));
                foreach (var index in indices)
                {
                    foreach (var prefab in prefabs)
                    {
                        Prefablists[index].Remove(prefab);
                    }
                }
            }
        }


    }
}