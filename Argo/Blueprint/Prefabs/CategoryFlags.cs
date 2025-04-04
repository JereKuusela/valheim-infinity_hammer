using System;

namespace Argo.Blueprint;

[Flags]
public enum BPOFlags : uint
{
    // todo flags setzen nach Piece -> PieceCategory, ComfortGroup, m_cultivatedGroundOnly
    // todo     harvest -> m_harvest;m_harvestRadius;  m_harvestRadiusMaxLevel;
    // todo     requrement -> m_craftingStation??
    // todo     "piece_nonsolid"
    // todo     Component<Pickable>
    // todo    is m_targetNonPlayerBuilt != BuildPlayer? (may rename build Player Playerbuildable) 
    //	 for pieces buildable by the player with the hammer
    BuildPiece = 1u << 31,
    // BuildPiecePlayer = 1u << 30, //	 for pieces buildable by the player with the hammer
    BuildPlayer = 1u << 30,
    //	 for all build piece even spawn only like the dverger probs
    Placeable = 1 << 29, //	for pieces like food placed with the serving tray
    //	 for the option to exclude compfort pieces from static pieces (which do nothing)
    Compfort    = 1 << 28,
    LightSource = 1 << 27, //	 for pieces like armor stands and item stands	

    Hoverable    = 1 << 25, // todo add those tags, kinda forgott
    TextReceiver = 1 << 24, //		
    Interactable = 1 << 23, //		
//	 for containers like the chest and things that need	have fuel
// todo, can maybe combined with containerpiece & pickable
    ObjectHolder    = 1 << 21,
    ContainerPiece  = 1 << 20, //		
    CraftingStation = 1 << 19, //		
    // todo, can maybe combined with Craftig station and Containerpiece
    Fuel = 1 << 18, //	 for pieces with fuel like the furnace	
    // todo maybe combine with placeable, if its a buildpieceplayer that means ists like
    // placed food, if only buildpiece its like the pickable dverger lanterns and if its neither
    // its like muhsrooms, rasperries and wild crops 
    // wild mushrooms seem to have ZNetView, StaticPhysics and Pickable
    // placed meads (same for dear mead) have: ZNetView, ZSyncTransform, ItemDrop, Piece, WearNTear, MaterialManNofier
    Pickable = 1 << 17, //	 e.g for food placed with the serving tray or crops	
//	 for player grown pieces which need cultivated ground (vegtables basically)
    Cultivated            = 1 << 15,
    DestroyableTerrain    = 1 << 14, //	 Objects like trees and rocks 	
    Fractured             = 1 << 13, //	 for partially broken stones etc	
    HoverableResourceNode = 1 << 12, //	 	
    NonStatic
        = 1 << 11, //	 pieces with some sort of funktion but not direktly interactable	 like ladders and whisplights
    Creature = 1 << 10, //	 for all creatures	
    // can maybe combined with TextReceiver and creature
    Tameable = 1 << 9, //	 for all creatures that can be tamed
    Vehicle  = 1 << 8, //	 for all vehicles
//	 for player grown pieces which need cultivated ground (vegtables basically)
    Animated = 1 << 6,
    //	 for stuff like WaterInteractable  PieceMarker ... which are pretty rare      PieceMarker  WaterInteractable  DooDadControler  Projectile: condensed into Special Interface
    SpecialInterface = 1 << 5,
    //	 for interfaces Hoverable	 IHasHoverMenu & IHasHoverMenuExtended
    Indestructible = 1 << 4,
    // todo  
    NoCollider = 1 << 3,
    // piece categories from Valheim Piece.cs PieceCategory.
    // Uses the last 4 bits but leave the last 8 open for future additions
    IsVanilla = 1 << 0, // todo add for every vanilla piece
}