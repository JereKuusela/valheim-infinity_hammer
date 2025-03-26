using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Argo.Blueprint
{
    using static ArgoWrappers.Configuration;
    using TExportBefore = Func<string, BpoRegister, bool>;
    using TExportWorker = Func<ExportIterator, bool, BpjObject>;
    using TImportBefore = Func<string, BpoRegister, bool>;
    using TImportWorker = Func<ImportIterator, bool, BpjObject>;

    namespace hidden { }

    /// <summary>
    /// flags will be set based on the flags submitted on creation, if you want to set additional flags
    /// you can do it within the Worker function
    /// </summary>
    public class BpjFetcher
    {
        public static BpjFetcher? m_Default
            = new(new BPOFlags[] { });
        public static BpjFetcher GetDefault() {
            if (m_Default is null) { m_Default = new BpjFetcher(new BPOFlags[] { }); }
            return m_Default;
        }
        public static BpjFetcher Default { get => GetDefault(); }
        public        BPOFlags   m_Flags;
        public BpjFetcher(BPOFlags[] flags_) {
            foreach (var flag in flags_) { m_Flags |= flag; }
        }
#if DEBUG
        public virtual bool IsKnown() => true;
#endif
        public virtual bool ExportBefore(string prefab)
            => ExportBefore(prefab, BpoRegister.Default);
        public virtual bool ExportBefore(string prefab, BpoRegister register) {
            return register.IncludedPrefabs.Contains(prefab) == false;
        }
        public virtual BpjObject? ExportWorker(
            ExportIterator it, bool Ext = true) {
            // todo move creation back out of class and only care about extra data
            BpoZDOVars? zdo_vars = null;
            if (Ext && it.g_obj.TryGetComponent(out ZNetView zNetView)) {
                zdo_vars = BpoZDOVars.MakeFromZdo(zNetView.GetZDO());
            } 
            return new BpjObject(it.Prefab, it.g_obj,
                Ext, m_Flags, zdo_vars);
        }
        public virtual bool ImportBefore(string prefab)
            => ImportBefore(prefab, BpoRegister.Default);
        public virtual bool ImportBefore(string prefab, BpoRegister register) {
            return register.IncludedPrefabs.Contains(prefab) == false;
        }
        public virtual BpjObject? ImportWorker(
            ImportIterator it, bool Ext = true) {
            // todo move creation back out of class and only care about extra data
            // maybe make a funtion for read lines and creating an object
            var data = JsonUtility.FromJson<BpjObject.TData>(it.BpjLine.data);
            return new BpjObject(it.BpjLine.prefab, data);
        }
    }

    public class CustomBpjFetcher(
        TExportBefore? expBefore,
        TExportWorker? expWorker, TImportBefore? impBefore,
        TImportWorker? impWorker, BPOFlags[]     flags_) : BpjFetcher(flags_)
    {
        public TExportBefore m_ExportBefore
            = expBefore ?? BpjFetcher.Default.ExportBefore;
        public TExportWorker m_ExportLoop
            = expWorker ?? BpjFetcher.Default.ExportWorker;
        public TImportBefore m_ImportBefore
            = impBefore ?? BpjFetcher.Default.ImportBefore;
        public TImportWorker m_ImportLoop
            = impWorker ?? BpjFetcher.Default.ImportWorker;

        public override bool ExportBefore(string prefab, BpoRegister register)
            =>
                m_ExportBefore(prefab, register);
        public override BpjObject? ExportWorker(
            ExportIterator data, bool ext) =>
            m_ExportLoop(data, ext);
        public override bool ImportBefore(string prefab, BpoRegister register)
            =>
                m_ImportBefore(prefab, register);
        public override BpjObject? ImportWorker(
            ImportIterator data, bool ext) =>
            m_ImportLoop(data, ext);
    };

    public class FetcherDefault(BPOFlags[] flags_) : BpjFetcher(flags_) { }

    public class FetcherStatic(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override bool ExportBefore(
            string      prefab,
            BpoRegister register) {
            if (register.IsIncluded(prefab)) {
                var gameObject = Util.CreateDummy(prefab);
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

    public class FetcherItemStand(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            if (data.g_obj &&
                data.g_obj.TryGetComponent<ItemStand>(out ItemStand itemStand)) {
                var bp_obj = base.ExportWorker(data, Ext);

                if ((!Ext) && (data.g_obj.TryGetComponent<ZNetView>(
                        out ZNetView zNetView))) {
                    
                    // adding vas, Count > 0 zero means the var was not found and we skipp the rest 
                    _ = (bp_obj.ZVars.TryAddFromZdo<string>(zNetView.GetZDO(), ZDOVars.s_item))
                     && (bp_obj.ZVars.TryAddFromZdo<int>(zNetView.GetZDO(), ZDOVars.s_variant))
                     && (bp_obj.ZVars.TryAddFromZdo<int>(zNetView.GetZDO(), ZDOVars.s_quality));
                }

                return bp_obj;
            } else {
                System.Diagnostics.Debug.WriteLine(
                    "data.obj null or not itemstand");
                return null;
            }
        }
    }

    public class FetcherArmorStand(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            var bp_obj = base.ExportWorker(data, Ext);
            if ((!Ext) && (data.g_obj.TryGetComponent<ZNetView>(
                    out ZNetView zNetView))) {
                /*for (int j = 0; j < cnt; j++)
                {
                    var item    = fields[j * 2 + 2];
                    var variant = int.Parse(fields[j * 2 + 3]);
                    zNetView.m_zdo.Set($"{j}_item", item);
                    zNetView.m_zdo.Set($"{j}_variant", variant);
                    armorStand.SetVisualItem(j, item, variant);
                }

                zdo.get("0_item")*/
            }
            if ((!Ext) && data.g_obj.TryGetComponent<ArmorStand>(out var armorStand)) {
                for (int i = 0; i < armorStand.m_slots.Count; i++) {
                    bp_obj.ZVars.AddPair(armorStand.m_slots[i].m_visualName, i    + "_item");
                    bp_obj.ZVars.AddPair(armorStand.m_slots[i].m_visualVariant, i + "_variant");
                }
                bp_obj.ZVars.AddPair(armorStand.m_pose,
                    ZDOVars.s_pose);
            }
            return bp_obj;
        }
    }

    public class FetcherGuardStone(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            var bp_obj = base.ExportWorker(data, Ext);
            // todo
            return bp_obj;
        }
    }

    public class FetcherFuel(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            var bp_obj = base.ExportWorker(data, Ext);
            // todo
            return bp_obj;
        }
    }

    public class FetcherContainer(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            var bp_obj = base.ExportWorker(data, Ext);
            // todo
            return bp_obj;
        }
    }

    public class FetcherFractured(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            var bp_obj = base.ExportWorker(data, Ext);
            // todo
            return bp_obj;
        }
    }

    public class FetcherTameable(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            
            var bp_obj = base.ExportWorker(data, Ext);
            if ((!Ext) && (bp_obj != null) && ( data.g_obj.TryGetComponent(out ZNetView view))){
                bp_obj.ZVars.TryAddFromZdo<string>(view.GetZDO(), ZDOVars.s_tamedName);
            }
            return bp_obj;
        }
    }

    public class FetcherNameable(BPOFlags[] flags_) : BpjFetcher(flags_)
    {
        public override BpjObject? ExportWorker(
            ExportIterator data, bool Ext) {
            var bp_obj = base.ExportWorker(data, Ext);
            if(!Ext)
            if (data.g_obj.TryGetComponent(out ZNetView view)) {
                bp_obj.ZVars.TryAddFromZdo<string>(view.GetZDO(), ZDOVars.s_tamedName);
            }
            return bp_obj;
        }
    }
}