namespace Argo.Blueprint;

// todo when loading stuff write everything thats not here in a file
using static BPOFlags;

public static class Configuration
{

   
    public static void AddToRegister(BpoRegister instance) {
     
   // todo maybe create default instance and determine some of the
        // todo   flags there
        
        instance.Add<FetcherStatic>
            ( BuildPiece.Static, [BuildPlayer] );

        instance.Add<FetcherNameable>( BuildPiece.Nameable, [
            BuildPlayer, TextReceiver
        ] );
        instance.Add<FetcherStatic>( BuildPiece.Interactable, [
            BuildPlayer, Interactable,
        ] );
        instance.Add<FetcherStatic>( BuildPiece.NonStatic, [
            BuildPlayer,
            NonStatic,
            SpecialInterface
        ] );
        instance.Add<FetcherStatic>( BuildPiece.Vehicle, [
            BuildPlayer,
            Vehicle,
            Interactable,
            Hoverable,
        ] );

        instance.Add<FetcherItemStand>( ["itemstand", "itemstandh"], [
            BuildPlayer | LightSource | ObjectHolder
        ] );

        instance.Add<FetcherArmorStand>( ["ArmorStand"],
        [
            BuildPlayer,
            LightSource,
            ObjectHolder,
            Compfort
        ] );

        instance.Add<FetcherGuardStone>( ["guard_stone",], [
            BuildPlayer,
            LightSource,
            Hoverable,
            SpecialInterface,
            TextReceiver,
        ] );

        instance.Add<FetcherStatic>( ["piece_wisplure",], [
            BuildPlayer,
            LightSource,
            Hoverable,
        ] );
        instance.Add<FetcherDefault>( BuildPiece.Comfort.Static, [
            BuildPlayer,
            Compfort
        ] );
        instance.Add<FetcherDefault>( BuildPiece.Comfort.Interactable,
        [
            LightSource,
            BuildPlayer,
            Compfort
        ] );
        instance.Add<FetcherFuel>( BuildPiece.Comfort.LightFuel, [
            BuildPlayer,
            Compfort,
            LightSource,
            Fuel,
        ] );

        instance.Add<FetcherStatic>( BuildPiece.Workbench.Light, [
            BuildPlayer,
            CraftingStation,
            LightSource
        ] );
        instance.Add<FetcherFuel>( BuildPiece.Workbench.FuelLight, [
            BuildPlayer,
            CraftingStation,
            LightSource,
            Fuel,
        ] );
        instance.Add<FetcherFuel>( BuildPiece.Workbench.Fuel, [
            BuildPlayer,
            CraftingStation,
            Fuel,
        ] );
        instance.Add<FetcherStatic>( BuildPiece.Workbench.Static,
        [
            BuildPlayer,
            CraftingStation,
        ] );
        instance.Add<FetcherStatic>( BuildPiece.Workbench.Animated, [
            BuildPlayer,
            CraftingStation,
            Animated,
        ] );

        instance.Add<FetcherContainer>( BuildPiece.Container.Player, [
            BuildPlayer,
            ContainerPiece
        ] );
        instance.Add<FetcherContainer>( BuildPiece.Container.NonPlayer, [
            BPOFlags.BuildPiece,
            ContainerPiece
        ] );
        instance.Add<FetcherFuel>( BuildPiece.Lights.Fuel, [
            BuildPlayer,
            Fuel,
            LightSource,
        ] );
        instance.Add<FetcherStatic>( BuildPiece.Lights.NoFuel, [
            BuildPlayer,
            LightSource,
        ] );

        instance.Add<FetcherDefault>( pickable, [
            Pickable,
            NonStatic,
        ] );
        instance.Add<FetcherStatic>( PieceNonPlayer.NonStatic, [
            BuildPlayer,
            NonStatic,
        ] );
        instance.Add<FetcherStatic>( PieceNonPlayer.Static, [
            BPOFlags.BuildPiece,
        ] );
        instance.Add<FetcherStatic>( PieceNonPlayer.Interactable,
        [
            BPOFlags.BuildPiece,
            NonStatic,
        ] );
        instance.Add<FetcherStatic>( PieceNonPlayer.LightSource, [
            BPOFlags.BuildPiece,
            LightSource,
        ] );

        instance.Add<FetcherStatic>( "piece_xmastree", [
            BuildPlayer,
            LightSource,
            Compfort,
        ] );
        instance.Add<FetcherFuel>( "piece_bathtub", [
            BuildPlayer,
            LightSource,
            Interactable,
            Fuel,
            Compfort,
        ] );
        // todo split in struly static pieces like rocks and those who have animation like trees in wind
        instance.Add<FetcherDefault>( Terrain.Static, [
            DestroyableTerrain,
        ] );
        // todo, prefabname might be the same as non fractured
        instance.Add<FetcherFractured>( Terrain.Fractured, [
            DestroyableTerrain,
            Fractured,
        ] );
        // todo, prefabname might be the same as non fractured
        instance.Add<FetcherFractured>( Terrain.Animated, [
            DestroyableTerrain,
            Animated,
        ] );
        instance.Add<FetcherFractured>( Terrain.Iteractable, [
            Interactable,
            Animated,
        ] );
        instance.Add<FetcherDefault>( Creatures.Enemy, [
            Creature,
        ] );
        instance.Add<FetcherTameable>( Creatures.Tameable, [
            Creature,
            Tameable,
        ] );
        instance.Add<FetcherDefault>( Creatures.Special, [
            Creature,
            SpecialInterface,
        ] );
        instance.Add<FetcherDefault>( Creatures.Spawner, [
            Creature,
            SpecialInterface,
        ] );
        instance.Add<FetcherDefault>( Creatures.Fish, [
            Creature,
            SpecialInterface,
        ] );
        instance.Add<FetcherStatic>( Indestructible.Static, [
            BPOFlags.Indestructible
        ] );
        instance.Add<FetcherStatic>( Indestructible.Interactable,
        [
            BPOFlags.Indestructible,
            Interactable
        ] );
        instance.Add<FetcherDefault>( "player_tombstone", [
            BPOFlags.Indestructible,
            SpecialInterface,
            Interactable,
            ContainerPiece
        ] );
        instance.Add<FetcherStatic>( Indestructible.Runestones, [
            BPOFlags.Indestructible,
            SpecialInterface,
            LightSource,
        ] );
    }

    public struct BuildPiece
    {
        public static string[] Static = [
            "ashwood_arch_big",
            "ashwood_beam_1m",
            "ashwood_beam_2m",
            "ashwood_bed",
            "ashwood_deco_floor",
            "ashwood_decowall_2x2",
            "ashwood_decowall_divider",
            "ashwood_decowall_tree",
            "ashwood_door",
            "ashwood_floor_1x1",
            "ashwood_floor_2x2",
            "ashwood_halfwall_1x2",
            "ashwood_pole_1m",
            "ashwood_pole_2m",
            "ashwood_quarterwall_1x1",
            "ashwood_stair",
            "ashwood_wall_2x2",
            "ashwood_wall_arch",
            "ashwood_wall_beam_26",
            "ashwood_wall_beam_45",
            "ashwood_wall_roof_26",
            "blackmarble_1x1",
            "blackmarble_2x1x1",
            "blackmarble_2x2_enforced",
            "blackmarble_2x2x1",
            "blackmarble_2x2x2",
            "blackmarble_arch",
            "blackmarble_base_1",
            "blackmarble_base_2",
            "blackmarble_basecorner",
            "blackmarble_column_1",
            "blackmarble_column_2",
            "blackmarble_column_3",
            "blackmarble_creep_4x1x1",
            "blackmarble_creep_4x2x1",
            "blackmarble_creep_slope_inverted_1x1x2",
            "blackmarble_creep_slope_inverted_2x2x1",
            "blackmarble_creep_stair",
            "blackmarble_floor",
            "blackmarble_floor_large",
            "blackmarble_floor_triangle",
            "blackmarble_out_1",
            "blackmarble_out_2",
            "blackmarble_outcorner",
            "blackmarble_pile",
            "blackmarble_post01",
            "blackmarble_slope_1x2",
            "blackmarble_slope_inverted_1x2",
            "blackmarble_stair",
            "blackmarble_stair_corner",
            "blackmarble_stair_corner_left",
            "blackmarble_tile_floor_1x1",
            "blackmarble_tile_floor_2x2",
            "blackmarble_tile_wall_1x1",
            "blackmarble_tile_wall_2x2",
            "blackmarble_tile_wall_2x4",
            "blackmarble_tip",
            "coal_pile",
            "crystal_wall_1x1",
            "darkwood_arch",
            "darkwood_beam",
            "darkwood_beam_26",
            "darkwood_beam_45",
            "darkwood_beam4x4",
            "darkwood_decowall",
            "darkwood_gate",
            "darkwood_pole",
            "darkwood_pole4",
            "darkwood_raven",
            "darkwood_roof",
            "darkwood_roof_45",
            "darkwood_roof_icorner",
            "darkwood_roof_icorner_45",
            "darkwood_roof_ocorner",
            "darkwood_roof_ocorner_45",
            "darkwood_roof_top",
            "darkwood_roof_top_45",
            "darkwood_wolf",
            "iron_floor_1x1",
            "iron_floor_1x1_v2",
            "iron_floor_2x2",
            "iron_grate",
            "iron_wall_1x1",
            "iron_wall_2x2",
            "metalbar_1x2",
            "piece_cloth_hanging_door",
            "piece_dvergr_metal_wall_2x2",
            "piece_dvergr_spiralstair",
            "piece_dvergr_spiralstair_right",
            "piece_dvergr_stake_wall",
            "Piece_flametal_beam",
            "Piece_flametal_pillar",
            "Piece_grausten_floor_1x1",
            "Piece_grausten_floor_2x2",
            "Piece_grausten_floor_4x4",
            "Piece_grausten_pillar_arch",
            "Piece_grausten_pillar_arch_small",
            "Piece_grausten_pillarbase_medium",
            "Piece_grausten_pillarbase_small",
            "Piece_grausten_pillarbase_tapered",
            "Piece_grausten_pillarbeam_medium",
            "Piece_grausten_pillarbeam_small",
            "piece_grausten_roof_45",
            "piece_grausten_roof_45_arch",
            "piece_grausten_roof_45_arch_corner",
            "piece_grausten_roof_45_arch_corner2",
            "piece_grausten_roof_45_corner",
            "piece_grausten_roof_45_corner2",
            "Piece_grausten_wall_1x2",
            "Piece_grausten_wall_2x2",
            "Piece_grausten_wall_4x2",
            "Piece_grausten_wall_arch",
            "Piece_grausten_wall_arch_inverted",
            "piece_mistletoe",
            "piece_xmascrown",
            "piece_xmasgarland",
            "skull_pile",
            "stone_arch",
            "stone_floor",
            "stone_floor_2x2",
            "stone_pile",
            "stone_pillar",
            "stone_stair",
            "stone_wall_1x1",
            "stone_wall_2x1",
            "stone_wall_4x2",
            "wood_beam",
            "wood_beam_1",
            "wood_beam_26",
            "wood_beam_45",
            "wood_core_stack",
            "wood_dragon1",
            "wood_fence",
            "wood_fine_stack",
            "wood_floor",
            "wood_floor_1x1",
            "wood_ledge",
            "wood_log_26",
            "wood_log_45",
            "wood_pole",
            "wood_pole_log",
            "wood_pole_log_4",
            "wood_pole2",
            "wood_roof",
            "wood_roof_45",
            "wood_roof_icorner",
            "wood_roof_icorner_45",
            "wood_roof_ocorner",
            "wood_roof_ocorner_45",
            "wood_roof_top",
            "wood_roof_top_45",
            "wood_stack",
            "wood_stair",
            "wood_wall_half",
            "wood_wall_log",
            "wood_wall_log_4x0",
            "wood_wall_log_4x0.5",
            "wood_wall_quarter",
            "wood_wall_roof",
            "wood_wall_roof_45",
            "wood_wall_roof_45_upsidedown",
            "wood_wall_roof_top",
            "wood_wall_roof_top_45",
            "wood_wall_roof_upsidedown",
            "wood_window",
            "wood_yggdrasil_stack",
            "woodiron_beam",
            "woodiron_beam_26",
            "woodiron_beam_45",
            "woodiron_pole",
            "woodwall",
        ];
        public static string[] Special = [
            "itemstand", // done
            "itemstandh", // done
            "ArmorStand", // todo add comfort flag
            "guard_stone", // done
            "piece_wisplure", // done
            "piece_xmastree", // done
            "piece_bathtub", // done
        ];
        public static string[] Interactable = [
            "wood_door",
            "wood_gate",
            "piece_hexagonal_door",
            "piece_turret",
            "piece_barber",
            "flametal_gate",
            "piece_asksvinskeleton",
        ];

        public static string[] Nameable = // todo?
        [
            "portal",
            "portal_wood",
            "portal_stone",
            "sign",
            "sign_notext",
        ];

        // for pieces which have some sort of function ingame
        public static string[] NonStatic = [
            // todo maybe exclude those with lightsources
            "piece_groundtorch_mist",
            "piece_sharpstakes",
            "piece_dvergr_sharpstakes",
            "stake_wall",
            "wood_stepladder",
            "Piece_grausten_stone_ladder"
        ];
        public static string[] Vehicle = [ // todo
            "cart",
            "karve",
            "raft",
            "trailership",
            "vikingship",
        ];

        public struct Comfort
        {
            public static string[] Interactable = [
                "bed",
                "piece_blackmarble_bench",
                "piece_blackmarble_throne",
                "piece_logbench01",
                "piece_throne01",
                "piece_throne02",
                "piece_chair",
                "piece_chair02",
                "piece_chair03",
                "piece_bed02",
                "piece_bench01",
                "piece_bone_throne",
                "piece_blackwood_bench01",
            ];

            public static string[] Static = [
                "jute_carpet",
                "rug_deer",
                "rug_fur",
                "rug_wolf",
                "jute_carpet_blue",
                "piece_blackmarble_table",
                "rug_hare",
                "rug_straw",
                "piece_banner01",
                "piece_banner02",
                "piece_banner03",
                "piece_banner04",
                "piece_banner05",
                "piece_banner06",
                "piece_banner07",
                "piece_banner08",
                "piece_banner09",
                "piece_banner10",
                "piece_banner11",
                "piece_maypole",
                "piece_table",
                "piece_table_oak",
                "piece_table_round",
            ];
            public static string[] LightFuel = [
                "fire_pit",
                "piece_brazierceiling01",
                "piece_brazierfloor01",
                "bonfire",
                "hearth",
                "fire_pit_iron",
                "piece_brazierfloor02",
            ];
            public static string[] Light = [
                "piece_Lavalantern",
            ];
        }

        public struct Lights
        {
            public static string[] Fuel = [
                "piece_groundtorch",
                "piece_groundtorch_blue",
                "piece_groundtorch_green",
                "piece_groundtorch_wood",
                "piece_jackoturnip",
                "piece_walltorch",
            ];
            public static string[] NoFuel = [
                "piece_dvergr_lantern",
                "piece_dvergr_lantern_pole",
            ];
        }

        public struct Container
        {
            public static string[] Player = [
                "barrell",
                "chest",
                "piece_gift1",
                "piece_gift2",
                "piece_gift3",
                "piece_chest_private",
                "piece_chest_wood",
                "piece_chest",
                "piece_chest_blackmetal",
                "piece_chest_treasure",
                "piece_chest_barrel",
            ];
            public static string[] NonPlayer = [
                "stonechest",
                "loot_chest_stone",
                "loot_chest_wood",
                "treasurechest_blackforest",
                "treasurechest_fcrypt",
                "treasurechest_forestcrypt",
                "treasurechest_heath",
                "treasurechest_meadows",
                "treasurechest_mountaincave",
                "treasurechest_plains_stone",
                "treasurechest_sunkencrypt",
                "treasurechest_swamp",
                "dvergrprops_crate",
                "dvergrprops_crate_long",
                "treasurechest_meadows_buried",
                "treasurechest_mountains",
                "treasurechest_trollcave",
            ];
        }

        public struct Workbench
        {
            // todo interactable workbenches
            // for pieces that shed light but dont need fuel or have fuel
            public static string[] Light = [
                "piece_magetable",
                "piece_magetable_ext",
                "blackforge",
                "forge",
                "piece_artisanstation",
            ];
            public static string[] FuelLight = [
                "piece_spinningwheel", "piece_oven",
                "blastfurnace",
                "smelter",
                "windmill", "piece_beehive",
                "charcoal_kiln",
                "eitrrefinery", "piece_sapcollector",
            ];
            public static string[] Fuel = [
                "fermenter",
            ];
            public static string[] Static = [
                "piece_stonecutter",
                "piece_workbench",
                "piece_workbench_ext1",
                "piece_workbench_ext2",
                "piece_workbench_ext3",
                "piece_workbench_ext4",
                "piece_cartographytable",
                "piece_cookingstation",
                "piece_cookingstation_iron",
                "piece_cauldron",
                "cauldron_ext1_spice",
                "cauldron_ext3_butchertable",
                "cauldron_ext4_pots",
                "cauldron_ext5_mortarandpestle",
                "cauldron_ext6_rollingpins",
                "incinerator", // buildpiece
                "forge_ext1",
                "forge_ext2",
                "forge_ext3",
                "forge_ext4",
                "forge_ext5",
                "forge_ext6",
                "blackforge_ext1",
                "blackforge_ext2",
                "blackforge_ext2_vise",
                "blackforge_ext3_metalcutter",
                "blackforge_ext4_gemcutter",
                "piece_magetable_ext2",
                "piece_preptable",
            ];
            public static string[] Animated = ["artisan_ext1"];
        }

        public static string[] Food = [ // todo
            "SerpentMeatCooked",
            "SerpentStew",
            "Cloudberry",
            "FeastAshlands",
            "FeastMountains",
            "FeastOceans",
            "Honey",
            "MeadBugRepellent",
            "MeadBzerker",
            "MeadEitrLingering",
            "MeadEitrMinor",
            "MeadFrostResist",
            "MeadHasty",
            "MeadHealthLingering",
            "MeadHealthMajor",
            "MeadHealthMedium",
            "MeadHealthMinor",
            "MeadLightfoot",
            "MeadPoisonResist",
            "MeadStaminaLingering",
            "MeadStaminaMedium",
            "MeadStaminaMinor",
            "MeadStrength",
            "MeadSwimmer",
            "MeadTamer",
            "MeadTasty",
            "MeadTrollPheromones",
            "MinceMeatSauce",
            "MushroomJotunPuffs",
            "MushroomMagecap",
            "MushroomOmelette",
            "MushroomSmokePuff",
            "MushroomYellow",
            "OnionSoup",
        ];
    }

    public static string[] pickable = [
        "pickable_blackcorestand",
        "pickable_dvergerthing",
        "pickable_dvergrlantern",
        "pickable_dvergrminetreasure",
        "pickable_dvergrstein",
        "pickable_mushroom_jotunpuffs",
        "pickable_mushroom_magecap",
        "pickable_royaljelly",
        "pickable_dragonegg",
        "pickable_mountainremains01_buried",
        "pickable_barley",
        "pickable_barley_wild",
        "pickable_bogironore",
        "pickable_branch",
        "pickable_carrot",
        "pickable_dandelion",
        "pickable_fishingrod",
        "pickable_flax",
        "pickable_flax_wild",
        "pickable_flint",
        "pickable_forestcryptremains01",
        "pickable_forestcryptremains02",
        "pickable_forestcryptremains03",
        "pickable_forestcryptremains04",
        "pickable_hairstrands01",
        "pickable_hairstrands02",
        "pickable_meatpile",
        "pickable_meteorite",
        "pickable_mountaincavecrystal",
        "pickable_mountaincaveobsidian",
        "pickable_mushroom",
        "pickable_mushroom_blue",
        "pickable_mushroom_yellow",
        "pickable_obsidian",
        "pickable_onion",
        "pickable_seedcarrot",
        "pickable_seedonion",
        "pickable_seedturnip",
        "pickable_stone",
        "pickable_surtlingcorestand",
        "pickable_tar",
        "pickable_tarbig",
        "pickable_thistle",
        "pickable_tin",
        "pickable_turnip",
        "treasure_pile",
        "treasure_stack",
    ];

    public struct PieceNonPlayer
    {
        public static string[] Static = [
            "blackmarble_altar_crystal",
            "blackmarble_altar_crystal_broken",
            "blackmarble_head_big01",
            "blackmarble_head_big02",
            "blackmarble_head01",
            "blackmarble_head02",
            "stoneblock_fracture",
            "mountainkit_brazier",
            "mountainkit_chair",
            "mountainkit_table",
            "ice_floor",
            "ice_floor_fractured",
            "ice_rock1",
            "ice_rock1_frac",
            "goblin_bed",
            "goblin_stairs",
            "old_wood_roof_icorner",
            "old_wood_roof_ocorner",
            "old_wood_roof_top",
            "old_wood_wall_roof",
            "goblin_banner",
            "goblin_fence",
            "goblin_pole",
            "goblin_pole_small",
            "goblin_roof_45d",
            "goblin_roof_45d_corner",
            "goblin_roof_cap",
            "goblin_totempole",
            "goblin_woodwall_1m",
            "goblin_woodwall_2m",
            "goblin_woodwall_2m_ribs",
            "old_wood_roof",
            "turf_roof",
            "turf_roof_top",
            "turf_roof_wall",
            "creepprop_egg_hanging01",
            "creepprop_egg_hanging02",
            "creepprop_entrance1",
            "creepprop_entrance2",
            "creepprop_hanging01",
            "creepprop_wall01",
            "dvergrprops_banner",
            "dvergrprops_barrel",
            "dvergrprops_bed",
            "dvergrprops_chair",
            "dvergrprops_curtain",
            "dvergrprops_hooknchain",

            "dvergrprops_pickaxe",
            "dvergrprops_shelf",
            "dvergrprops_stool",
            "dvergrprops_table",
            "dvergrprops_wood_beam",
            "dvergrprops_wood_floor",
            "dvergrprops_wood_pole",
            "dvergrprops_wood_stair",
            "dvergrprops_wood_stake",
            "dvergrprops_wood_stakewall",
            "dvergrprops_wood_wall",
            "piece_dvergr_pole",
            "piece_dvergr_wood_wall",
            "trader_wagon_destructable",
            "treasurechest_dvergr_loose_stone",
            "treasurechest_dvergrtower",
            "treasurechest_dvergrtown",
            "dvergrtown_arch",
            "dvergrtown_stair_corner_wood_left",
            "dvergrtown_wood_beam",
            "dvergrtown_wood_crane",
            "dvergrtown_wood_pole",
            "dvergrtown_wood_stake",
            "dvergrtown_wood_stakewall",
            "dvergrtown_wood_support",
            "dvergrtown_wood_wall01",
            "dvergrtown_wood_wall02",
            "dvergrtown_wood_wall03",
            "castlekit_groundtorch_unlit",
            "castlekit_metal_groundtorch_unlit",
            "shipwreck_karve_bottomboards",
            "shipwreck_karve_bow",
            "shipwreck_karve_chest",
            "shipwreck_karve_dragonhead",
            "shipwreck_karve_stern",
            "shipwreck_karve_sternpost",
            "caverock_ice_pillar_wall",
            "caverock_ice_stalagmite",
            "caverock_ice_stalagmite_broken",
            "caverock_ice_stalagtite",
            "caverock_ice_stalagtite_falling",
            "cargocrate",
            "bucket",
            "lox_ribs", // buildpiece nonplayer
        ];
        public static string[] NonStatic = [
            "goblin_stepladder",
            "beehive",
            "dverger_guardstone",
        ];
        public static string[] Interactable = [
            "mountainkit_wood_gate",
            "piece_dvergr_wood_door",
            "dvergrtown_creep_door",
            "dvergrtown_secretdoor",
            "dvergrtown_slidingdoor",
            "dungeon_forestcrypt_door",
            "dungeon_sunkencrypt_irongate",
        ];
        public static string[] LightSource = [
            "dvergrprops_lantern",
            "dvergrprops_lantern_standing",
            "castlekit_brazier",
            "castlekit_groundtorch",
            "castlekit_groundtorch_green",
        ];
    }

    // todo split in treebase and onther pieces. beechlog for example should be some other game object
    // also move stuff like swamptrees are also 
    public struct Terrain
    {
        // todo may move logs (except old logs)
        public static string[] Static = [
            "beech_log",
            "beech_log_half",
            "beech_stub",
            "birch_log",
            "birch_log_half",
            "birchstub",
            "oak_log",
            "oak_log_half",
            "oakstub",
            "pinetree_01_stub",
            "pinetree_log_half",
            "pinetree_log_halfold",
            "pinetree_logold",
            "swamptree2_log",
            "swamptree1_stub",
            "pinetree_log",
            "shootstump",
            "yggashoot_log",
            "yggashoot_log_half",
            "firtree_log",
            "firtree_log_half",
            "firtree_oldlog",
            "firtree_stub",
            "minerock_copper",
            "minerock_iron",
            "minerock_meteorite",
            "minerock_obsidian",
            "minerock_stone",
            "minerock_tin",
            "rock_3", "rock_4",
            "rock_4_plains",
            "rock_destructible_test",
            "rock1_mountain",
            "rock2_heath",
            "rock2_mountain",
            "rock3_mountain",
            "rock3_silver",
            "rock4_coast",
            "rock4_copper",
            "rock4_heath",
            "rock4_forest",
            "rockfinger",
            "silvervein",
            "rock_7",
            "rock_destructible",
            "cliff_mistlands1",
            "giant_arm",
            "giant_brain",
            "giant_brain_frac",
            "giant_helmet1",
            "cliff_mistlands1_creep",
            "cliff_mistlands2",
            "giant_helmet2",
            "giant_ribs",
            "giant_skull",
            "giant_sword1",
            "giant_sword2",
            "rock_mistlands1",
            "rock_mistlands2",
            "rock1_mistlands",
            "rockdolmen_1",
            "rockdolmen_2",
            "rockdolmen_3",
            "mudpile",
            "mudpile_beacon",
            "mudpile_old",
            "mudpile2",
            "marker01", // stone pile
            "marker02", // stone pile
            "rockthumb",
            "iceblocker", // terrainpiece
            "gucksack",
            "gucksack_small",
            "widestone",
        ];
        public static string[] Iteractable = [
            "blueberrybush",
            "cloudberrybush",
            "raspberrybush",
            "glowingmushroom",
        ];
        public static string[] Animated = [
            "vines",
            "sapling_barley",
            "sapling_carrot",
            "sapling_flax",
            "sapling_onion",
            "sapling_seedcarrot",
            "sapling_seedonion",
            "sapling_seedturnip",
            "sapling_turnip",
            "beech_sapling",
            "beech_small1",
            "beech_small2",

            "birch_sapling",
            "birch1",
            "birch1_aut",
            "birch2",
            "birch2_aut",

            "bush01",
            "bush01_heath",
            "bush02_en",
            "oak_sapling",
            "pinetree",
            "pinetree_01",
            "pinetree_sapling",

            "oak1",
            "yggashoot_small1",
            "yggashoot1",
            "yggashoot2",
            "yggashoot3",
            "swamptree1_log",
            "sapling_jotunpuffs",
            "sapling_magecap",
            "firtree",
            "firtree_sapling",
            "firtree_small",
            "firtree_small_dead",
            "beech1",
        ];

        public static string[] Fractured = [
            "widestone_frac",
            "rock_3_frac",
            "rock1_mountain_frac",
            "rock2_heath_frac",
            "rock2_mountain_frac",
            "rock3_mountain_frac",
            "rock3_silver_frac",
            "rock4_coast_frac",
            "rock4_copper_frac",
            "rock4_forest_frac",
            "rock4_heath_frac",
            "rockfinger_frac",
            "rockfingerbroken_frac",
            "silvervein_frac",
            "cliff_mistlands1_creep_frac",
            "cliff_mistlands1_frac",
            "cliff_mistlands2_frac",
            "giant_helmet1_destruction",
            "giant_helmet2_destruction",
            "giant_ribs_frac",
            "giant_skull_frac",
            "giant_sword1_destruction",
            "giant_sword2_destruction",
            "rock_mistlands1_frac",
            "mudpile_frac",
            "mudpile2_frac",
            "rockthumb_frac",
        ];
    }

    public struct Creatures
    {
        public static string[] Tameable = [
            "lox",
            "lox_calf",
            "wolf", "chicken",
            "wolf_cub",
            "hen",
            "boar",
            "boar_piggy",
        ];

        public static string[] Special = [
            "bonemass",
            "eikthyr",
            "seekerqueen",
            "gd_king", // Elder
            "dragon", // Moder?
            "goblin_king", // ??? spellec right?
            "odin",
        ];

        public static string[] Fish = [
            "fish1",
            "fish2",
            "fish3",
            "fish4_cave",
            "fish5",
            "fish6",
            "fish7",
            "fish8",
            "fish9",
            "fish10",
            "fish11",
        ];
        public static string[] Spawner = [
            "triggerspawner_brood",
            "triggerspawner_seeker",
            "stubbe_spawner",
            "spawner_draugrpile",
            "spawner_greydwarfnest",
            "bonepilespawner",
        ];
        public static string[] Enemy = [
            "surtling",
            "wraith",
            "fenring",
            "fenring_cultist",
            "skeleton_friendly",
            "tick",
            "goblin",
            "goblinarcher",
            "goblinbrute",
            "goblinking",
            "goblinshaman",
            "goblinshaman_protect_aoe",
            "greydwarf_elite",
            "greydwarf_root",
            "leech",
            "serpent",
            "skeleton",
            "skeleton_noarcher",
            "skeleton_poison",
            "stonegolem",
            "troll",
            "ulv",
            "greydwarf",
            "greydwarf_shaman",
            "greyling",
            "neck",
            "dverger",
            "dvergermage",
            "dvergermagefire",
            "dvergermageice",
            "dvergermagesupport",
            "gjall",
            "hare",
            "seeker",
            "seekerbrood",
            "seekerbrute",
            "abomination",
            "bat",
            "blob",
            "blobelite",
            "blobtar",
            "crow",
            "hatchling",
            "deathsquito",
            "deer",
            "draugr",
            "draugr_elite",
            "draugr_ranged",
        ];
    }

    public struct Indestructible
    {
        public static string[] Static = [ // todo
            "statuecorgi",
            "statuedeer",
            "statuehare",
            "statueseed",
            "root08",
            "root11",
            "root12",
            "swamptree1",
            "swamptree2",
            "swamptree2_darkland",
        ];

        public static string[] Interactable = [
            "dungeon_queen_door", // dodo maybe move to specialbuildpieces
            "sunken_crypt_gate",
        ];
        public static string[] Special = [
            "player_tombstone",
        ];

        // todo like for runstones etc
        public static string[] Runestones = [
            "luredwisp",
            "bossstone_bonemass",
            "bossstone_dragonqueen",
            "bossstone_theelder",
            "bossstone_yagluth",
            "dverger_demister",
            "dverger_demister_broken",
            "dverger_demister_large",
            "dverger_demister_ruins",
            "yggdrasilroot",
            "bossstone_eikthyr",
            "bossstone_thequeen",
            "flying_core",
            "rockfingerbroken",
            // probably like itemstand
        ];
    }

    // todo
    public static string[] unsorted_prefabs = [
        "piece_pot1",
        "piece_pot2",
        "piece_pot3",
        "piece_trap_troll",
        "BarleyWine",
        "CastleKit_groundtorch_green",
        "CastleKit_groundtorch_gr",
        "goblinking_totemholder",
        "castlekit_braided_box01",
        "armorstand_female",
        "armorstand_male",
        "bonemass_aoe",
        "bow_projectile_needle",
        "cloth_hanging_door",
        "cloth_hanging_door_double",
        "cloth_hanging_long",
        "dragoneggcup",
        "evilheart_forest",
        "evilheart_swamp",
        "fishingrodfloat",
        "ghost",
        "guard_stone_test",
        "haldor",
        "hanging_hairstrands",
        "heathrockpillar",
        "heathrockpillar_frac",
        "highstone",
        "highstone_frac",
        "hugeroot1",

        "ice1", // floating ice

        "leech_cave",
        "leviathan",

        "rockformation1",

        "root07",

        "seagal",

        "shaman_attack_aoe",
        "shaman_heal_aoe",
        "ship_construction",
        "shrub_2",
        "shrub_2_heath",

        "skull1",
        "skull2",

        "statueevil",
        "stubbe",
        "tarliquid",
        "tarlump1",
        "tarlump1_frac",
        "tentaroot",
        "tolroko_flyer",
        "trainingdummy",
        "vfx_barrle_destroyed",
        "vfx_destroyed_karve",
        "vfx_destroyed_raft",
        "vfx_destroyed_vikingship",
        "vfx_greydwarfnest_destroyed",
        "vfx_stonegolem_death",
        "fenrirhide_hanging",
        "fenrirhide_hanging_door",
        "mountaingravestone01",
        "ancient_skull",
        "bow_projectile_carapace",
        "dvergerstaffblocker_blockcircle",
        "dvergerstaffblocker_blockcirclebig",
        "dvergerstaffblocker_blockhemisphere",
        "dvergerstaffblocker_blocku",
        "dvergerstaffblocker_blockwall",
        "dvergerstaffblocker_projectile",
        "dvergerstafffire_clusterbomb_aoe",
        "dvergerstafffire_fire_aoe",
        "dvergerstaffheal_aoe",
        "dvergerstaffnova_aoe",
        "dvergerstaffsupport_aoe",
        "dvergertest",
        "hive",
        "mistile",
        "piece_cloth_hanging_door_blue",
        "piece_cloth_hanging_door_blue2",
        "piece_trap_troll",
        "seekeregg",
        "seekeregg_alwayshatch",
        "thehive",

        "turret_projectile",
        "waterliquid",
        "fenring_attack_flames_aoe",
    ];
}