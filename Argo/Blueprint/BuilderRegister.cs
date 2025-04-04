using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Argo.Blueprint.Util;
using Argo.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace Argo
{
    namespace Blueprint
    {
        using static Argo.Blueprint.Argonaut;
        using static Argo.Blueprint.Configuration;
        using static Argo.Blueprint.BPOFlags;
        using TExportBefore = Func<string, BuilderRegister, bool>;
        using TExportWorker = Func<ExportIterator, bool, BpjObject>;
        using TImportBefore = Func<string, BuilderRegister, bool>;
        using TImportWorker = Func<ImportIterator, bool, BpjObject>;

        public class BpjFetcherUnknown(params BPOFlags[] flags_)
            : BpjFetcher(flags_)
        {
#if DEBUG
            public override bool IsKnown() => false;
#endif
        };

        public class BuilderRegister : ObjectPool<BuilderRegister>
        {
            protected ImmutableDictionary<ConstStr, BpjFetcher> fetchers =
                ImmutableDictionary<ConstStr, BpjFetcher>.Empty;

            internal BpjFetcherUnknown m_default =
                new BpjFetcherUnknown([0]);
            //   internal static      Configuration Configuration_ ;
            protected static BuilderRegister CreateDefaultInstance() {
                // _prefabs = PrefabRegister.Get
                ImmutableDictionary<ConstStr, BpjFetcher> fetchers_
                    = ImmutableDictionary<ConstStr, BpjFetcher>.Empty;
                Add<FetcherNameable>(ref fetchers_, BuildPieces.Nameable, [
                    BuildPlayer, TextReceiver
                ]);
                Add<FetcherItemStand>(ref fetchers_, ["itemstand", "itemstandh"], [
                    BuildPlayer | LightSource | ObjectHolder
                ]);

                Add<FetcherArmorStand>(ref fetchers_, ["ArmorStand"],
                [
                    BuildPlayer,
                    LightSource,
                    ObjectHolder,
                    Compfort
                ]);
                Add<FetcherGuardStone>(ref fetchers_, ["guard_stone",], [
                    BuildPlayer,
                    LightSource,
                    Hoverable,
                    SpecialInterface,
                    TextReceiver,
                ]);
                Add<FetcherFuel>(ref fetchers_, BuildPieces.Comfort.LightFuel, [
                    BuildPlayer,
                    Compfort,
                    LightSource,
                    Fuel,
                ]);
                Add<FetcherFuel>(ref fetchers_, BuildPieces.Workbench.FuelLight, [
                    BuildPlayer,
                    CraftingStation,
                    LightSource,
                    Fuel,
                ]);
                Add<FetcherFuel>(ref fetchers_, BuildPieces.Workbench.Fuel, [
                    BuildPlayer,
                    CraftingStation,
                    Fuel,
                ]);
                Add<FetcherContainer>(ref fetchers_, BuildPieces.Container.Player, [
                    BuildPlayer,
                    ContainerPiece
                ]);
                Add<FetcherContainer>(ref fetchers_, BuildPieces.Container.NonPlayer, [
                    BPOFlags.BuildPiece,
                    ContainerPiece
                ]);
                Add<FetcherFuel>(ref fetchers_, BuildPieces.Lights.Fuel, [
                    BuildPlayer,
                    Fuel,
                    LightSource,
                ]);
                Add<FetcherFuel>(ref fetchers_, ["piece_bathtub"], [
                    BuildPlayer,
                    LightSource,
                    Interactable,
                    Fuel,
                    Compfort,
                ]);

                // todo, prefabname might be the same as non fractured
                Add<FetcherFractured>(ref fetchers_, Terrain.Fractured, [
                    DestroyableTerrain,
                    Fractured,
                ]);
                // todo, prefabname might be the same as non fractured
                Add<FetcherFractured>(ref fetchers_, Terrain.Animated, [
                    DestroyableTerrain,
                    Animated,
                ]);
                Add<FetcherFractured>(ref fetchers_, Terrain.Iteractable, [
                    Interactable,
                    Animated,
                ]);
                Add<FetcherTameable>(ref fetchers_, Creatures.Tameable, [
                    Creature,
                    Tameable,
                ]);
                var instance = new BuilderRegister(true);
                instance.fetchers = fetchers_;
                return instance;
            }

            // todo add readonly VanillaUnstance to have something to reset and make the 
            //  default instance for iteroperabilty for mods, so thats the default place to put moddata in
            // mayby also add a field for mods to register their pieces with flags include/not include
            public BuilderRegister(bool isDefault) : base(isDefault) { }
            private BuilderRegister(BuilderRegister other) : base(false) {
                fetchers = other.fetchers;
            }

            public static void Add(
                ref ImmutableDictionary<ConstStr, BpjFetcher> fetchers_,
                BpjFetcher                                    fetcher, string prefab) {
                if (fetchers_.ContainsKey(prefab)) {
                    System.Console.WriteLine(
                        $"{prefab} allready registered, skipped");
                    return;
                }

                fetchers_ = fetchers_.Add(prefab, fetcher);
            }

            public void Add(BpjFetcher fetcher, params string[] prefabs) {
                if (_isDefaultInstance) {
                    throw new AccessViolationException(
                        "Its not permited to modify the default instance");
                }
                Add(ref fetchers, fetcher, prefabs);
            }
            
            public static void Add(
                ref ImmutableDictionary<ConstStr, BpjFetcher> fetchers,
                BpjFetcher                                    fetcher, string[] prefabs) {
                foreach (var prefab in prefabs) {
                    if (fetchers.ContainsKey(prefab)) {
                        System.Console.WriteLine(
                            $"{prefab} allready registered, skipped");
                        continue;
                    }

                    fetchers = fetchers.Add(prefab, fetcher);
                }
            }

            public void Add<T>(string[] prefabs,  BPOFlags[] flags) where T : BpjFetcher {
                if (_isDefaultInstance) {
                    throw new AccessViolationException(
                        "Its not permited to modify the default instance");
                }
                Add<T>(ref fetchers, prefabs, flags);
            }

            public static void Add<T>(
                ref    ImmutableDictionary<ConstStr, BpjFetcher> fetchers, string[] prefabs,
                params BPOFlags[]                                flags)
                where T : BpjFetcher {
                BpjFetcher fetcher =
                    (T)Activator.CreateInstance(typeof(T), flags);
                Add(ref fetchers, fetcher, prefabs);
            }

            public void Add<T>(string prefab, params BPOFlags[] flags) where T : BpjFetcher {
                if (_isDefaultInstance) {
                    throw new AccessViolationException(
                        "Its not permited to modify the default instance");
                }
                Add<T>([prefab], flags);
            }

            public BpjFetcher Get(string prefab) {
                if (fetchers.TryGetValue(prefab, out var fetcher)) {
                    return fetcher;
                }
                return m_default;
            }
            public override BuilderRegister Clone() {
                return new BuilderRegister(this); // 
            }

            // params[] must by of type KeyValuePair<string, BPOFlags>;
            public override void AddToMod(ModIdentifier mod, params object[] pairs) {
                for (int i = 0; i < pairs.Length; i++) {
                    if (typeof(KeyValuePair<string, BpjFetcher>) != pairs[i].GetType()) {
                        throw new ArgumentException(
                            "Arguements must be of type KeyValuePair<ConstStr, BpjFetcher>");
                    }
                    var pair = (KeyValuePair<string, BpjFetcher>)pairs[i];
                    _modInstances[mod].Add(pair.Value, pair.Key );
                }
            }
        }
    }
}