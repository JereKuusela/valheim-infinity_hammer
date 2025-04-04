using System;
using System.Collections.Generic;
using System.Linq;
using Argo.DataAnalysis;
using Argo.Zdo;
using UnityEngine;

namespace Argo.Blueprint
{
    using static ArgoWrappers.Configuration;
    using TExportBefore = Func<string, BuilderRegister, bool>;
    using TExportWorker = Func<ExportIterator, bool, BpjObject>;
    using TImportBefore = Func<string, BuilderRegister, bool>;
    using TImportWorker = Func<ImportIterator, bool, BpjObject>;

    namespace hidden { }

    /// <summary>
    /// flags will be set based on the flags submitted on creation, if you want to set additional flags
    /// you can do it within the Worker function
    /// </summary>
    public class BpjFetcher
    {
        public static BpjFetcher? m_Default;
        public static BpjFetcher GetDefault() {
            if (m_Default is null) { m_Default = new BpjFetcher(new BPOFlags[] { }); }
            return m_Default;
        }
        public static BpjFetcher Default { get => GetDefault(); }
        public        BPOFlags   m_Flags;
        public BpjFetcher(params BPOFlags[] flags_) {
            this.m_Flags = 0;
            foreach (var flag in flags_) {
                this.m_Flags |= flag;
            }
        }
#if DEBUG
        public virtual bool IsKnown() => true;
#endif
        public virtual bool ExportBefore(string prefab)
            => ExportBefore(prefab, BuilderRegister.Default);
        public virtual bool ExportBefore(string prefab, BuilderRegister register) {
            return register.IncludedPrefabs.Contains(prefab);
        }
        public virtual BpjObject? ExportWorker(ExportIterator it, SaveExtraData Ext) {
            // todo move creation back out of class and only care about extra data
            if (Ext == SaveExtraData.None) {
                return new BpjObject(it.Prefab, it.g_obj, Ext, this.m_Flags);
            } else {
                return new BpjObject(it.Prefab, it.g_obj, Ext, this.m_Flags, it.z_vars);
            }
        }
        public virtual bool ImportBefore(string prefab)
            => ImportBefore(prefab, BuilderRegister.Default);
        public virtual bool ImportBefore(string prefab, BuilderRegister register) {
            return register.IncludedPrefabs.Contains(prefab) == false;
        }
        public virtual BpjObject? ImportWorker(ImportIterator it, SaveExtraData Ext) {
            // todo move creation back out of class and only care about extra data
            // maybe make a funtion for read lines and creating an object
            var data = JsonUtility.FromJson<BpjObject.TData>(it.BpjLine.data);
            return new BpjObject(it.BpjLine.prefab, data);
        }
    }

    /*public class CustomBpjFetcher(
        TExportBefore? expBefore,
        TExportWorker? expWorker, TImportBefore? impBefore,
        TImportWorker? impWorker, BPOFlags[]     flags_) : BpjFetcher( flags_ )
    {
        public TExportBefore m_ExportBefore
            =expBefore ?? BpjFetcher.Default.ExportBefore;
        public TExportWorker m_ExportLoop
            =expWorker ?? BpjFetcher.Default.ExportWorker;
        public TImportBefore m_ImportBefore
            =impBefore ?? BpjFetcher.Default.ImportBefore;
        public TImportWorker m_ImportLoop
            =impWorker ?? BpjFetcher.Default.ImportWorker;

        public override bool ExportBefore(string prefab, BpoRegister register)
            =>
                m_ExportBefore( prefab, register );
        public override BpjObject? ExportWorker(
            ExportIterator data, bool ext) =>
            m_ExportLoop( data, ext );
        public override bool ImportBefore(string prefab, BpoRegister register)
            =>
                m_ImportBefore( prefab, register );
        public override BpjObject? ImportWorker(
            ImportIterator data, bool ext) =>
            m_ImportLoop( data, ext );
    };*/

    public class FetcherDefault(params BPOFlags[] flags_) : BpjFetcher(flags_) { }

    public class FetcherStatic(params BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override bool ExportBefore(
            string      prefab,
            BuilderRegister register) {
            if (register.IsIncluded(prefab)) {
                var gameObject = Utility.CreateDummy(prefab);
                if (gameObject &&
                    gameObject.TryGetComponent<Piece>(out var piece)) {
                    if (piece.m_comfort > 0) { m_Flags |= BPOFlags.Compfort; }
                    ;
#if DEBUG
                    if (piece.m_comfort > 0) {
                        System.Diagnostics.Debug.WriteLine(prefab +
                            " not tagged as comfort Piece");
                    }
#endif
                }
                return true;
            }
            return false;
        }
    }

    public class FetcherItemStand(params BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(ExportIterator data, SaveExtraData Ext) {
            if (data.g_obj) {
                var bp_obj = base.ExportWorker(data, Ext);

                if ((Ext == SaveExtraData.None) && (data.g_obj.TryGetComponent<ItemStand>(
                        out var itemStand))) {
                    // adding vas, Count > 0 zero means the var was not found and we skipp the rest 
                    if (itemStand.m_name != "") {
                        bp_obj.ZVars.TryAdd<string>(ZDOVars.s_item, itemStand.m_visualName);
                        if (itemStand.m_visualVariant != 0) {
                            bp_obj.ZVars.TryAdd<int>(ZDOVars.s_variant, itemStand.m_visualVariant
                            );
                        }
                    }
                }
                return bp_obj;
            } else {
                System.Diagnostics.Debug.WriteLine(
                    "data.obj null or not itemstand");
                return null;
            }
        }
    }

    public class FetcherArmorStand : BpjFetcher
    {
        // making a hashset containing keys for all amour stand specific ints
        static HashSet<int> int_kexs = HashGroups.ArmorStandSlots.Variant
                                                 .Union(HashGroups.ArmorStandSlots.Quality)
                                                 .Append(KnownHashes.s_pose)
                                                 .ToHashSet();
        // making a hashset containing keys for all amour stand specific strings
        static HashSet<int> string_kess = HashGroups.ArmorStandSlots.Items.ToHashSet();
        public FetcherArmorStand(params BPOFlags[] flags) : base(flags) {
            foreach (var flag in flags) {
                this.m_Flags |= flag;
            }
        }
        public override BpjObject? ExportWorker(
            ExportIterator it, SaveExtraData Ext) {
            BpjObject? bp_obj;

            if ((Ext != SaveExtraData.None) && (it.z_vars.Count > 0)) {
                var            info = Config.GetHashLookup();
                ArmorStandComponent component = new();

                // removing all amour stand specific ints and tranfer them to the armourstand data
                var ints = it.z_vars.PopAll<int>(int_kexs);
                component.m_values.SetValues(ints);
                // removing all amour stand specific strings and tranfer them to the armourstand data
                var strings = it.z_vars.PopAll<string>(string_kess);
                component.m_values.SetValues(strings);

                bp_obj = base.ExportWorker(it, Ext);

                bp_obj.Add(component);
                return bp_obj;
            } else {
                bp_obj = base.ExportWorker(it, Ext);
                if ((Ext == SaveExtraData.None) &&
                    (it.g_obj.TryGetComponent<ArmorStand>(out var armorStand))) {
                    ArmorStandComponent component = new();

                    for (int i = 0; i < armorStand.m_slots.Count; i++) {
                        var itemname = armorStand.m_slots[i].m_item.m_shared.m_name;
                        if (itemname != "") {
                            bp_obj.ZVars.TryAdd(i + "_item", itemname);
                            bp_obj.ZVars.TryAdd(i + "_variant",
                                armorStand.m_slots[i].m_visualVariant
                            );
                        }
                    }
                    bp_obj.ZVars.TryAdd(ZDOVars.s_pose, armorStand.m_pose);
                }
                // }
                return bp_obj;
            }
        }
    }

    public class FetcherGuardStone(params BPOFlags[] flags_) : BpjFetcher(flags_)
        {
            public override BpjObject? ExportWorker(ExportIterator data, SaveExtraData Ext) {
                var bp_obj = base.ExportWorker(data, Ext);
                // todo
                return bp_obj;
            }
        }

        public class FetcherFuel(params BPOFlags[] flags_) : BpjFetcher(flags_)
        {
            public override BpjObject? ExportWorker(ExportIterator data, SaveExtraData Ext) {
                var bp_obj = base.ExportWorker(data, Ext);
                // todo
                return bp_obj;
            }
        }

        public class FetcherContainer(params BPOFlags[] flags_) : BpjFetcher(flags_)
        {
            public override BpjObject? ExportWorker(ExportIterator data, SaveExtraData Ext) {
                var bp_obj = base.ExportWorker(data, Ext);
                // todo
                return bp_obj;
            }
        }

        public class FetcherFractured(params BPOFlags[] flags_) : BpjFetcher(flags_)
        {
            public override BpjObject? ExportWorker(ExportIterator data, SaveExtraData Ext) {
                var bp_obj = base.ExportWorker(data, Ext);
                // todo
                return bp_obj;
            }
        }

        public class FetcherTameable(params BPOFlags[] flags_) : BpjFetcher(flags_)
        {
            public override BpjObject? ExportWorker(ExportIterator it, SaveExtraData Ext) {
                var bp_obj = base.ExportWorker(it, Ext);

                if ((Ext == SaveExtraData.None) && (bp_obj != null) &&
                    (it.g_obj.TryGetComponent(out Tameable tameable))) {
                    if (it.z_vars.TryGetValue<string>(ZDOVars.s_tamedName, out var name)) {
                        bp_obj.ZVars.TryAdd<string>(ZDOVars.s_tamedName, name);
                    }
                }
                return bp_obj;
            }
        }

        public class FetcherNameable(params BPOFlags[] flags_) : BpjFetcher(flags_)
        {
            public override BpjObject? ExportWorker(ExportIterator data, SaveExtraData Ext) {
                var bp_obj = base.ExportWorker(data, Ext);
                if (Ext == SaveExtraData.None) {
                    if (data.g_obj.TryGetComponent(out ZNetView view)) {
                        var name = ZDOExtraData.GetString(view.GetZDO().m_uid, ZDOVars.s_tamedName);
                        bp_obj.ZVars.TryAdd<string>(ZDOVars.s_tamedName, name);
                    }
                }
                return bp_obj;
            }
        }
    }
