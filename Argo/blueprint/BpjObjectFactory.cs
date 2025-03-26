using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Argo.Blueprint
{
    public struct BPListObject(
        string prefab_, GameObject obj_)
    {
        public string     prefab = prefab_;
        public GameObject obj    = obj_;
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
            if (m_LifeTimeExtender == null)
            {
                m_LifeTimeExtender
                    = new LifeTimeExtender(Blueprint, Parent);
                return true;
            }

            throw new InvalidOperationException(
                "ExtendLifeTime object already exists");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal bool UnExtendLifeTime() {
            if (m_LifeTimeExtender != null)
            {
                m_LifeTimeExtender = null;
                return true;
            }

            throw new InvalidOperationException(
                "ExtendLifeTime object allready removed");
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
            onExportComplete.AddListener(_onCoroutineComplete);
        }

        private WeakReference<GameObject> m_Parent
            = new WeakReference<GameObject>(null!);
        internal GameObject? Parent {
            get => m_Parent.TryGetTarget(out var parent) ? parent : null;
        }
        internal GameObject SetParent { set => m_Parent.SetTarget(value); }
        
        private WeakReference<BlueprintJson> m_Blueprint
            = new WeakReference<BlueprintJson>(null!);
        
        internal BlueprintJson SetBlueprint {            set => m_Blueprint.SetTarget(value);        }
        internal BlueprintJson? Blueprint {
            get => m_Blueprint.TryGetTarget(out var blueprint)
                ? blueprint
                : null;
        }

        public static (GameObject, BpjObjectFactory) MakeInstance(
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
            foreach (var component in components)
            {
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

        internal void BuildBluePrintCoroutine() {
            BlueprintJson blueprint
                = Blueprint ?? throw new ArgumentException("bp is null");
            if (blueprint.Objects.Count == 1)
            {
                BuildBluePrintSingle(GetPlacementPos(blueprint));
                return;
            }
            StartCoroutine(BuildBluePrint());
        }

        internal Vector3 GetPlacementPos(BlueprintJson blueprint) {
            return blueprint.player.m_placementGhost.transform.position;
        }
        internal void BuildBluePrintSingle(Vector3 placement_pos) {
            System.Diagnostics.Debug.WriteLine("AddSingleObject");

            // todo instead of calling Export Objects just call the stuff here for a
            // todo   single object.
            ExportObjects();

            if (Blueprint == null) return;
            BlueprintJson        blueprint  = Blueprint!;
            BpjObject? bpjObject  = blueprint.objects[0];
            var                  gameObject = blueprint.GameObjects[0].obj;
            if (bpjObject == null) return;
            if (!gameObject) return;
            if (blueprint.Objects.Count > 0)
            {
                bpjObject.Pos = Vector3.zero;
                bpjObject.Rot = Quaternion.identity;
                bpjObject.Scale
                    = gameObject.transform.localScale;
            }
            bpjObject.Chance = 1f;

            var snaps = Util.GetSnapPoints(gameObject);
            foreach (var snap in snaps)
                blueprint.SnapPoints.Add(snap.transform
                                             .localPosition);

            var offset = blueprint.Center(blueprint.CenterPiece);
            blueprint.Coordinates
                = placement_pos - offset;
            onExportComplete.Invoke();
        }
        private IEnumerator BuildBluePrint() {
            ExtendLifeTime();

            BlueprintJson blueprint
                = Blueprint ?? throw new ArgumentException("bp is null");
            Vector3 placement_pos
                = blueprint.player.m_placementGhost.transform.position;
            // todo add chase for single object

            yield return StartCoroutine(ExportObjects());

            var bp_ob = blueprint.Objects ??
                throw new ArgumentException("objects is null");

// todo look at infinityhammer, this might add every snapppoint from every piece
            if (blueprint.snapPiece == "")
            {
                var snaps
                    = blueprint.selection?.GetSnapPoints() ?? new List<GameObject>();
                foreach (var snap in snaps)
                    blueprint.SnapPoints.Add(snap.transform
                                                 .localPosition);
            }

            var offset = blueprint.Center(blueprint.CenterPiece);
            blueprint.Coordinates
                = placement_pos - offset;

            onExportComplete.Invoke();
            UnExtendLifeTime();
            yield break;
        }

#if DEBUG
        private void DebugAddObjects(BpjFetcher     fetcher,
                                     ExportIterator iterator) {
            var g_obj = iterator.g_obj;
            var prefab =  ArgoWrappers.GetPrefabName(g_obj) == ""
                ? iterator.Prefab
                : ArgoWrappers.GetPrefabName(g_obj);

            if (!fetcher
                   .IsKnown()) // dump infos to if prefab is not in register
            {
                var components = g_obj.GetComponents<Component>() ??
                    new Component[0];
                var text = "Prefab " + prefab +
                    " not in register, components: ";
                foreach (var component in components)
                {
                    text += component.GetType().Name + " ";
                }

                System.Diagnostics.Debug.WriteLine(text);
            }

// test if pieces are correctly tagged as buildpieces
            if (g_obj.TryGetComponent<Piece>(
                    out _)) //todo seems buildpieces are not pieces?
            {
                if ((fetcher.m_Flags &
                        BPOFlags.BuildPiece) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Prefab " + prefab +
                        " not tagged as buildpiece");
                }
            } else
            {
                if ((fetcher.m_Flags &
                        BPOFlags.BuildPiece) != 0)
                {
                    System.Diagnostics.Debug.WriteLine("Prefab " + prefab +
                        " is incorrectly tagged as buildpiece");
                }
            }
        }
#endif
        internal void AddSnapPoints(ImportIterator iterator) {
            // todo
        }
        internal void AddSnapPoints(ExportIterator iterator) {
            var prefab = iterator.Prefab;
            do
            {
                var bp = Blueprint!;
                try
                {
                    bp.SnapPoints.Add(iterator.g_obj.transform
                                              .localPosition);
                    iterator++;
                } catch (Exception)
                {
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

            // m_bp.TryGetTarget(out var bp);
            BlueprintJson  bp       = Blueprint!;
            ExportIterator iterator = new ExportIterator(bp);
            BpjFetcher     fetcher;

            while (iterator)
            {
                var prefab = iterator.Prefab;

                // todo add option to ignore certain pieces
                // especially if pieces are marked as custom pieces
                // todo maybe move fetcher access to outer loop
                if ((bp.snapPiece != "") && (iterator.Prefab == bp.snapPiece))
                {
                    AddSnapPoints(iterator);
                } else if (bp.ingoredPieces.Contains(iterator.Prefab))
                {
                    // sadly there is no upper_bound in net, to lazy to write one myself
                    do
                    {
                        iterator++;
                    } while (iterator &&
                             iterator.Prefab == prefab);
                } else
                {
                    fetcher = bp.bpoRegister.Get(prefab);
#if DEBUG
                    DebugAddObjects(fetcher, iterator);
#endif
// runs the provided custom function within the loop        

                    fetcher.ExportBefore(prefab, bp.bpoRegister);

                    string prefab_next;
                    do
                    {
                        // todo maybe run Before here and rename Loop/Worker to after and rename the
                        // todo     function "Before" before loop to init

                        // runs the provided custom function within the loop
                        var bpo = fetcher.ExportWorker(iterator, true);
                        bp.Add(bpo);
                        iterator++;

                        if ((iterator.Index - last_yield) >= yield_steps)
                        {
                            last_yield = iterator.Index;
                            System.Console.WriteLine(iterator.Index +
                                " BlueprintObjects generated");

                            yield return null;
                        }
                    } while ((iterator.idx    < iterator.Count) &&
                             (iterator.Prefab != prefab));

                    ;
                }
            }

            /*file.WriteLine(bp.GetJsonObjects());
            file.WriteLine(bp.GetJsonFooter());
            file.Close();
            file.Dispose();
            initialized = false;
            bp = null;*/
            ;
        }

        private IEnumerator ImportObjects() { return null; }

        private IEnumerator ToGameObjects() {
            BlueprintJson  bp       = Blueprint!;
            ImportIterator iterator = new ImportIterator(bp);
            BpjFetcher     fetcher;

            while (iterator)
            {
                var prefab = iterator.Prefab;

                // todo add option to ignore certain pieces
                // especially if pieces are marked as custom pieces
                // todo maybe move fetcher access to outer loop
                if ((bp.snapPiece != "") && (iterator.Prefab == bp.snapPiece))
                {
                    AddSnapPoints(iterator);
                } else if (bp.ingoredPieces.Contains(iterator.Prefab))
                {
                    // sadly there is no upper_bound in net, to lazy to write one myself
                    do
                    {
                        iterator++;
                    } while (iterator &&
                             iterator.Prefab == prefab);
                } else
                {
                    fetcher = bp.bpoRegister.Get(prefab);
#if DEBUG
                    // todo DebugAddObjects(fetcher, iterator);
#endif
// runs the provided custom function within the loop        

                    fetcher.ImportBefore(prefab, bp.bpoRegister);

                    do
                    {
                        // todo maybe run Before here and rename Loop/Worker to after and rename the
                        // todo     function "Before" before loop to init

                        // runs the provided custom function within the loop
                        var bpo = fetcher.ImportWorker(iterator, true);
                        bp.Add(bpo);
                        iterator++;

                        if ((iterator.Index - last_yield) >= yield_steps)
                        {
                            last_yield = iterator.Index;
                            System.Console.WriteLine(iterator.Index +
                                " BlueprintObjects generated");

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
        internal int                        idx;

        public static ImportIterator operator ++(ImportIterator it)
            => (it.idx++, it).Item2;
        public static implicit operator bool(ImportIterator it)
            => (it.idx < it.Count);
        public BlueprintJson.BpjLine BpjLine    { get => m_lines[idx]; }
        public int                  Index     { get => idx; }
        public int                  Count     { get => m_lines.Count; }
        public bool                 HasNext() { return (idx < Count); }
        public string               Prefab    { get => m_lines[idx].prefab; }
    }

    public struct ExportIterator
    {
        public ExportIterator(BlueprintJson bp) {
            idx      = 0;
            m_GOList = bp.GameObjects;
        }
        internal int idx;

        internal List<BPListObject> m_GOList;

        public static ExportIterator operator ++(ExportIterator data)
            => (data.idx++, data).Item2;
        public static implicit operator bool(ExportIterator it)
            => (it.idx < it.Count);

        internal List<BPListObject> GObjects  { get => m_GOList; }
        public   GameObject         g_obj     { get => m_GOList[idx].obj; }
        public   int                Index     { get => idx; }
        public   int                Count     { get => m_GOList.Count; }
        internal bool               HasNext() { return (idx < Count); }
        public   string             Prefab    { get => m_GOList[idx].prefab; }
    }
}