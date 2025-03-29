using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Argo.DataAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace Argo.Blueprint
{
    public struct BPListObject
    {
        public string     prefab;
        public GameObject obj;
        public BpjZVars   zvars;
        public BPListObject(
            string prefab_, GameObject obj_) {
            prefab = prefab_;
            obj    = obj_;
            zvars  = new BpjZVars();
        }
        public BPListObject(string prefab_, GameObject obj_, BpjZVars zvars_) {
            prefab = prefab_;
            obj    = obj_;
            zvars  = zvars_;
        }
        public BPListObject(string prefab_) {
            prefab = prefab_;
            obj    = null;
            zvars  = new BpjZVars();
        }
    }

    public class BpjObjectFactory : MonoBehaviour
    {
        //  internal WeakReference<BlueprintJson> m_bp = new WeakReference<BlueprintJson>(null);

        internal struct LifeTimeExtender(
            BlueprintJson? bp_, GameObject? parent_
        )
        {
            internal BlueprintJson? bp     = bp_;
            internal GameObject?    parent = parent_;
        }

        internal LifeTimeExtender? m_LifeTimeExtender;

        private BpjObjectFactory() { }
        /// <summary>
        /// Extends the lifetime of the factory so it doesnt get destroyed while the
        /// Coroutine(s) run. Dont forget to unexted afterwards or this will cause
        /// an memory leak. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// todo auto call this if no listener is added
        /// todo add listener count and call automatically if all listeners did complete
        /// todo extend description
        internal bool ExtendLifeTime() {
            if (m_LifeTimeExtender == null) {
                m_LifeTimeExtender
                    = new LifeTimeExtender( Blueprint, Parent );
                return true;
            }

            throw new InvalidOperationException(
                "ExtendLifeTime object already exists" );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal bool UnExtendLifeTime() {
            if (m_LifeTimeExtender != null) {
                m_LifeTimeExtender = null;
                return true;
            }

            throw new InvalidOperationException(
                "ExtendLifeTime object allready removed" );
        }
        private                  int        last_yield       = 0;
        [SerializeField] private UnityEvent onExportComplete = new UnityEvent();

        /// A member variable within the `BlueprintObjectFac` class that determines the number of iterations
        /// between yielding during large operations. Useful for preventing frame drops in Unity by allowing
        /// time for other operations to run in between intensive processing loops.
        internal readonly int yield_steps = 5000;

        /// <summary>
        /// Attatches a function which is called when the export coroutine is finished
        /// </summary>
        /// <param name="_onCoroutineComplete"></param>
        public void AddExportListener(UnityAction _onCoroutineComplete) {
            onExportComplete.AddListener( _onCoroutineComplete );
        }

        private WeakReference<GameObject> m_Parent
            = new WeakReference<GameObject>( null! );
        internal GameObject? Parent {
            get => m_Parent.TryGetTarget( out var parent ) ? parent : null;
        }
        internal GameObject SetParent { set => m_Parent.SetTarget( value ); }

        private WeakReference<BlueprintJson> m_Blueprint
            = new WeakReference<BlueprintJson>( null! );

        internal BlueprintJson SetBlueprint { set => m_Blueprint.SetTarget( value ); }
        internal BlueprintJson? Blueprint {
            get => m_Blueprint.TryGetTarget( out var blueprint )
                ? blueprint
                : null;
        }

        internal static (GameObject, BpjObjectFactory) MakeInstance(
            BlueprintJson bp) {
            var gameObject_ = new GameObject();
            BpjObjectFactory instance =
                gameObject_.AddComponent<BpjObjectFactory>();
            instance.SetParent    = gameObject_;
            instance.SetBlueprint = bp;
            return (gameObject_, instance);
        }

        // todo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public BPOFlags GetFlags(GameObject obj) {
            var components = obj.GetComponents<Component>();

            // todo first check for buildpieces, trees and rocks which are usually the majority of objects.
            BPOFlags flags = 0;
            foreach (var component in components) {
                if (component is Interactable) flags |= BPOFlags.Interactable;
                if ((component is Hoverable)     ||
                    (component is IHasHoverMenu) ||
                    (component is IHasHoverMenuExtended))
                    flags |= BPOFlags.Hoverable;
                if (component is TextReceiver) flags |= BPOFlags.TextReceiver;
                if (component is IDestructible)
                    flags |= BPOFlags.DestroyableTerrain;

                // only boat-helm & saddle
                if ((component is IDoodadController)  |
                    (component is IPieceMarker)       |
                    (component is IWaterInteractable) |
                    (component is IProjectile))
                    flags |=
                        BPOFlags
                           .SpecialInterface; // only character floating fish
            }

            return flags;
        }

        internal void BuildFromSelectionCoroutine() {
            BlueprintJson blueprint
                = Blueprint ?? throw new ArgumentException( "bp is null" );
            if (blueprint.Objects.Count == 1) {
                BuildFromSelectionSingle();
                return;
            }
            StartCoroutine( BuildFromSelection() );
        }

        internal void BuildFromSelectionSingle() {
            System.Diagnostics.Debug.WriteLine( "AddSingleObject" );

            // todo instead of calling Export Objects just call the stuff here for a
            // todo   single object.
            if (Blueprint == null) return;

            ExportObjects();

            BlueprintJson blueprint  = Blueprint!;
            BpjObject?    bpjObject  = blueprint.objects[0];
            var           gameObject = blueprint.GameObjects[0].obj;
            if (bpjObject == null) return;
            if (!gameObject) return;
            if (blueprint.Objects.Count > 0) {
                bpjObject.Pos = Vector3.zero;
                bpjObject.Rot = Quaternion.identity;
                bpjObject.Scale
                    = gameObject.transform.localScale;
            }
            bpjObject.Chance = 1f;

            var snaps = Util.GetSnapPoints( gameObject );
            foreach (var snap in snaps)
                blueprint.SnapPoints.Add( snap.transform
                                              .localPosition );

            var offset = blueprint.Center( blueprint.CenterPiece );
            blueprint.Coordinates
                = blueprint.selection.Position - offset;
            onExportComplete.Invoke();
        }
        private IEnumerator BuildFromSelection() {
            ExtendLifeTime();
            BlueprintJson blueprint;
            if (Blueprint != null) {
                blueprint = Blueprint;
            } else {
                System.Console.WriteLine( "BuildFromSelection: bp is null" );
                throw new ArgumentException( "bp is null" );
            }

            // todo add chase for single object

            yield return StartCoroutine( ExportObjects() );
            List<BpjObject?> bp_ob;
            if (Blueprint.Objects.Count > 0) {
                bp_ob = blueprint.Objects;
            } else {
                System.Console.WriteLine( "BuildFromSelection: objects is empty" );

                throw new ArgumentException( "objects is empty" );
            }
// todo look at infinityhammer, this might add every snapppoint from every piece
            if (blueprint.snapPiece == "") {
                var snaps
                    = blueprint.selection?.GetSnapPoints() ?? new List<GameObject>();
                foreach (var snap in snaps)
                    blueprint.SnapPoints.Add( snap.transform
                                                  .localPosition );
            }

            var offset = blueprint.Center( blueprint.CenterPiece );
            blueprint.Coordinates
                = blueprint.selection.Position - offset;

            onExportComplete.Invoke();
            UnExtendLifeTime();
            yield break;
        }

#if DEBUG
        private void DebugAddObjects(
            BpjFetcher     fetcher,
            ExportIterator iterator) {
            try {
                var g_obj = iterator.g_obj;
                var prefab = ArgoWrappers.GetPrefabName( g_obj ) == ""
                    ? iterator.Prefab
                    : ArgoWrappers.GetPrefabName( g_obj );

                if (!fetcher
                       .IsKnown()) // dump infos to if prefab is not in register
                {
                    var components = g_obj.GetComponents<Component>() ??
                        new Component[0];
                    var text = "Prefab " + prefab +
                        " not in register, components: ";
                    foreach (var component in components) {
                        text += component.GetType().Name + " ";
                    }

                    System.Console.WriteLine( text );
                }

// test if pieces are correctly tagged as buildpieces
                if (g_obj.TryGetComponent<Piece>(
                        out _ )) //todo seems buildpieces are not pieces?
                {
                    if (((fetcher.m_Flags & BPOFlags.BuildPiece)  == 0)
                     && ((fetcher.m_Flags & BPOFlags.BuildPlayer) == 0)) {
                        System.Console.WriteLine( "Prefab " + prefab +
                            " not tagged as buildpiece" );
                    }
                } else {
                    if (((fetcher.m_Flags & BPOFlags.BuildPiece)  == 0)
                     && ((fetcher.m_Flags & BPOFlags.BuildPlayer) == 0)) {
                        System.Console.WriteLine( "Prefab " + prefab +
                            " is incorrectly tagged as buildpiece" );
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine( "DebugAddObjects: " + e );
            }
        }
#endif
        internal void AddSnapPoints(ImportIterator iterator) {
            // todo
        }
        internal void AddSnapPoints(ExportIterator iterator) {
            var prefab = iterator.Prefab;
            do {
                var bp = Blueprint!;
                try {
                    bp.SnapPoints.Add( iterator.g_obj.transform
                                               .localPosition );
                    iterator++;
                } catch (Exception) {
                    iterator++; // avoid ifinite loop in case of error
                    throw;
                }
            } while (iterator.HasNext() &&
                     iterator.Prefab == prefab);
        }
        /// <summary> todo fix description
        /// calls the methods ExportBefore and ExportWorker of matched prefabs in BpoRegister
        /// ExortBefore is called on every Prefab once befor the Blueprint Objects are generated
        /// Export Worker is called on every generated blueprint object
        /// To use inherit form PieceFetcherBase and overload the methods and Register the object
        /// <param name="iterator"></param>
        private IEnumerator ExportObjects() {
            /*var file = new StreamWriter(filepath, append: false);
            foreach (var line in bp.GetJsonHeader())
            {
                file.WriteLine(line);
            }*/

            BlueprintJson  bp;
            ExportIterator iterator;
            BpjFetcher     fetcher;

            // m_bp.TryGetTarget(out var bp);
            if (Blueprint != null) {
                bp       = Blueprint;
                iterator = new ExportIterator( bp );
            } else {
                System.Console.WriteLine( "ExportObjects: bp is null" );
                throw new ArgumentException( "\"ExportObjects: bp is null" );
            }

            while (iterator) {
                var prefab = iterator.Prefab;
#if DEBUG
                fetcher = bp.bpoRegister.Get( prefab );
                System.Console.WriteLine( "ExportObjects, fetcher selected; " + prefab );
#endif
                // todo add option to ignore certain pieces
                // especially if pieces are marked as custom pieces
                // todo maybe move fetcher access to outer loop
                if ((bp.snapPiece != "") && (iterator.Prefab == bp.snapPiece)) {
                    AddSnapPoints( iterator );
                } else if (bp.IngoredPieces.Contains( iterator.Prefab )) {
                    iterator.skipPrefab();
                } else {
                    if (prefab == null) {
                        System.Diagnostics.Debug.WriteLine( "prefab is null" );
                        //continue;
                        prefab = "";
                    }
                    try {
                        // todo mayby get default fetcher here and only 
                        // check if an addtional fetcher is awailable, might safe a bit time 
                        // instead of getting bassically the same fetcher every time
                        fetcher = bp.bpoRegister.Get( prefab );
                    } catch (Exception e) {
                        System.Diagnostics.Debug.WriteLine( "prefab is null" );
                        fetcher = bp.bpoRegister.m_default;
                    }
                    try {
#if DEBUG
                        DebugAddObjects( fetcher, iterator );
#endif
// runs the provided custom function within the loop        

                        fetcher.ExportBefore( prefab, bp.bpoRegister );
                    } catch (Exception e) {
                        System.Diagnostics.Debug.WriteLine( "ExportWorker1: " + e );
                    }
                    do {
                        // todo maybe run Before here and rename Loop/Worker to after and rename the
                        // todo     function "Before" before loop to init
                        try {
                            // runs the provided custom function within the loop
                            var bpo = fetcher.ExportWorker( iterator, true );
                            bp.Add( bpo );
                            iterator++;
                        } catch (Exception e) {
                            System.Diagnostics.Debug.WriteLine( "ExportWorker2: " + e );
                            iterator++;
                        }
                        if ((iterator.Index - last_yield) >= yield_steps) {
                            last_yield = iterator.Index;
                            System.Console.WriteLine( iterator.Index +
                                " BlueprintObjects generated" );

                            yield return null;
                        }
                    } while ((iterator.idx    < iterator.Count) &&
                             (iterator.Prefab == prefab));
                }
            }
        }

        private IEnumerator ImportObjects() { return null; }

        private IEnumerator ToGameObjects() {
            BlueprintJson  bp       = Blueprint!;
            ImportIterator iterator = new ImportIterator( bp );
            BpjFetcher     fetcher;

            while (iterator) {
                var prefab = iterator.Prefab;

                // todo add option to ignore certain pieces
                // especially if pieces are marked as custom pieces
                // todo maybe move fetcher access to outer loop
                if ((bp.snapPiece != "") && (iterator.Prefab == bp.snapPiece)) {
                    AddSnapPoints( iterator );
                } else if (bp.IngoredPieces.Contains( iterator.Prefab )) {
                    // sadly there is no upper_bound in net, to lazy to write one myself
                    do { iterator++; } while (iterator &&
                                              iterator.Prefab == prefab);
                } else {
                    fetcher = bp.bpoRegister.Get( prefab );
#if DEBUG
                    // todo DebugAddObjects(fetcher, iterator);
#endif
// runs the provided custom function within the loop        

                    fetcher.ImportBefore( prefab, bp.bpoRegister );

                    do {
                        // todo maybe run Before here and rename Loop/Worker to after and rename the
                        // todo     function "Before" before loop to init

                        // runs the provided custom function within the loop
                        var bpo = fetcher.ImportWorker( iterator, true );
                        bp.Add( bpo );
                        iterator++;

                        if ((iterator.Index - last_yield) >= yield_steps) {
                            last_yield = iterator.Index;
                            System.Console.WriteLine( iterator.Index +
                                " BlueprintObjects generated" );

                            yield return null;
                        }
                    } while ((iterator.idx    < iterator.Count) &&
                             (iterator.Prefab != prefab));
                }
            }
        }
    }

    public struct ImportIterator

    {
        public ImportIterator(BlueprintJson bp) {
            m_lines = bp.Lines;
            idx     = 0;
        }
        internal List<BlueprintJson.BpjLine> m_lines;
        internal int                         idx;

        public static ImportIterator operator ++(ImportIterator it)
            => (it.idx++, it).Item2;
        public static implicit operator bool(ImportIterator it)
            => (it.idx < it.Count);
        public BlueprintJson.BpjLine BpjLine   { get => m_lines[idx]; }
        public int                   Index     { get => idx; }
        public int                   Count     { get => m_lines.Count; }
        public bool                  HasNext() { return (idx < Count); }
        public string                Prefab    { get => m_lines[idx].prefab; }
    }

    public struct ExportIterator
    {
        internal int idx;

        internal List<BPListObject> m_GOList;
        public ExportIterator(BlueprintJson bp) {
            idx      = 0;
            m_GOList = bp.GameObjects;
        }

        internal void skipPrefab() {
            string next = Prefab + " "; // simulating upper bound to find next prefab
            var pos = m_GOList.BinarySearch( idx, m_GOList.Count,
                new BPListObject( next ),
                Comparer<BPListObject>.Create( (a, b) =>
                    string.Compare( a.prefab, b.prefab, StringComparison.OrdinalIgnoreCase ) ) );
            idx = (pos < 0) ? ~pos : pos;
        }
        public static ExportIterator operator ++(ExportIterator data)
            => (data.idx++, data).Item2;
        public static implicit operator bool(ExportIterator it)
            => (it.idx < it.Count);

        internal List<BPListObject> GObjects { get => m_GOList; }
        public   GameObject         g_obj    { get => m_GOList[idx].obj; }
        public   BpjZVars           z_vars   { get => m_GOList[idx].zvars; }

        public   int    Index     { get => idx; }
        public   int    Count     { get => m_GOList.Count; }
        internal bool   HasNext() { return (idx < Count); }
        public   string Prefab    { get => m_GOList[idx].prefab; }
    }
}