using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Immutable;
using Argo.Blueprint;
using Argo.Blueprint.Util;
using Argo.Util;

namespace Argo.Zdo;

public struct Hash
{
    public readonly int          hash;
    public readonly string       name;
    public readonly string       shortname;
    public          ZDOTypeFlags knowntypes;
    public Hash(int hash, string name, string shortname) {
        hash      = hash;
        name      = name;
        shortname = shortname;
    }
    public Hash(int hash, string name) {
        hash      = hash;
        name      = name;
        shortname = name;
    }
    public Hash(string name) {
        hash      = name.GetStableHashCode();
        name      = name;
        shortname = name;
    }
    public static implicit operator int(Hash hash) => hash.hash;
}

public class HashRegister : ObjectPool<HashRegister>
{
    public ImmutableDictionary<int, Hash>    m_hashes     = ImmutableDictionary<int, Hash>.Empty;
    public ImmutableDictionary<string, Hash> m_names      = ImmutableDictionary<string, Hash>.Empty;
    public ImmutableDictionary<string, Hash> m_shortnames = ImmutableDictionary<string, Hash>.Empty;

    // common instances are used to share infos amongst all mods etc

    private static HashRegister CreateDefaultInstance() {
        var instance = new HashRegister(GetDefault());
        return instance;
    }

    protected HashRegister CreateEmpty() { return new HashRegister(); }
    /// <summary>
    /// Reads all fields of the struct Hashes and generates lookup tables to translate the hashes back
    /// into the names aswell as for looking up hash values. Also generates Short names for import and export
    /// todo do real short names 
    /// </summary>
    /// <param name="instance"></param>
    private static void MakeDefaultLookupTables(ref HashRegister instance) {
        Type THashes = typeof(KnownHashes);
        FieldInfo[] fields = THashes.GetFields(BindingFlags.Static | BindingFlags.Public);
        Dictionary<int, Hash> hashes = [];
        Dictionary<string, Hash> names = [];
        Dictionary<string, Hash> shortnames = [];
        Dictionary<int, string> HashToName = [];
        Dictionary<int, string> HashToShortname = [];
        Dictionary<string, int> NameToHash = [];
        Dictionary<string, string> NameToShortname = [];
        Dictionary<string, string> ShortNameToName = [];
        Dictionary<string, int> ShortNameToHash = [];
        foreach (var field in fields) {
            var values    = (Hash)field.GetValue(null);
            var name      = values.name;
            var shortname = values.shortname;
            var hash      = values.hash;
            hashes.Add(hash, values);
            names.Add(name, values);
            shortnames.Add(shortname, values);
        }
        instance.m_hashes     = hashes.ToImmutableDictionary();
        instance.m_names      = names.ToImmutableDictionary();
        instance.m_shortnames = shortnames.ToImmutableDictionary();
    }
    protected HashRegister() : base(true) {
        if (_defaultInstance != null) {
            throw new ArgumentException("Default instance already set");
        }
    }
    protected HashRegister(ModIdentifier mod) : base(false) { _modInstances.Add(mod, this); }
    protected HashRegister(HashRegister other) : base(false) {
        m_hashes     = other.m_hashes;
        m_names      = other.m_names;
        m_shortnames = other.m_shortnames;
    }

    public void Add(params Hash[] hashes) {
        if (_isDefaultInstance) {
            throw new AccessViolationException("Its not permited to modify the default instance");
        }
        m_hashes = m_hashes.AddRange(hashes.Select(h =>
            new KeyValuePair<int, Hash>(h.hash, h)));
        m_names = m_names.AddRange(hashes.Select(h =>
            new KeyValuePair<string, Hash>(h.name, h)));
        m_shortnames = m_shortnames.AddRange(hashes.Select(h =>
            new KeyValuePair<string, Hash>(h.shortname, h)));
    }

    /// <summary>
    /// Tries to get the original string representation of ZDO hash from the provided lookup Table.
    /// Sets the out parameter name to the found value and returns true on Success.  
    /// Otherwise, it returns false and converts the hash number to a String and sets it to the out parameter. 
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="name">Is Either set to the orignial string representation or just converts the
    /// number to string.</param>
    /// <param name="info">The Object where to look for the hashes</param>
    /// <returns>Returns true if the hash could be found in the lookup table or false if not</returns>
    public static bool GetNameOrToString(int hash, out string name, HashRegister info) {
        bool ret = info.TryGetName(hash, out name);
        if (!ret) { name = hash.ToString(); }
        return ret;
    }
    /// <summary>
    /// Tries to get the original string representation of ZDO hash from the default lookup Table.
    /// Sets the out parameter name to the found value and returns true on Success.  
    /// Otherwise, it returns false and converts the hash number to a String and sets it to the out parameter. 
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="name">Is Either set to the orignial string representation or just converts the
    /// number to string.</param>
    /// <returns>Returns true if the hash could be found in the lookup table or false if not</returns>
    public bool GetNameOrToString(int hash, out string name) {
        bool ret = TryGetName(hash, out name);
        if (!ret) { name = hash.ToString(); }
        return ret;
    }
    private string ToJsonPropertyName(int hash, char prefix) {
        if (TryGetName(hash, out var str)) {
            return prefix + str;
        } else {
            return "u" + prefix + hash.ToString();
        }
    }
    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameInt(int hash) =>
        ToJsonPropertyName(hash, 'i');

    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameLong(int hash) =>
        ToJsonPropertyName(hash, 'l');

    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameFloat(int hash) =>
        ToJsonPropertyName(hash, 'f');

    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameString(int hash) =>
        ToJsonPropertyName(hash, 's');

    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameVec3(int hash) =>
        ToJsonPropertyName(hash, 'v');

    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameQuat(int hash) =>
        ToJsonPropertyName(hash, 'q');

    /// <summary>
    /// translates hash and adds type prefix
    /// </summary>
    public string ToJsonPropertyNameByteArray(int hash) =>
        ToJsonPropertyName(hash, 'b');

    public bool TryGetName(int hash, out string name) {
        if (m_hashes.TryGetValue(hash, out var hashstruct)) {
            name = hashstruct.name;
            return true;
        } else {
            name = "";
            return false;
        }
    }
    public bool TryGetName(string shortname, out string name) {
        if (m_shortnames.TryGetValue(shortname, out var hashstruct)) {
            name = hashstruct.name;
            return true;
        } else {
            name = "";
            return false;
        }
    }
    public bool TryGetShortName(int hash, out string shortname) {
        if (m_hashes.TryGetValue(hash, out var hashstruct)) {
            shortname = hashstruct.shortname;
            return true;
        } else {
            shortname = "";
            return false;
        }
    }
    public bool TryGetShortName(string name, out string shortname) {
        if (m_names.TryGetValue(name, out var hashstruct)) {
            shortname = hashstruct.shortname;
            return true;
        } else {
            shortname = "";
            return false;
        }
    }
    public bool TryGetHash(string name, out int hash) {
        Hash hashstruct;
        if (m_names.TryGetValue(name, out hashstruct)) {
            hash = hashstruct.hash;
            return true;
        } else if (m_shortnames.TryGetValue(name, out hashstruct)) {
            hash = hashstruct.hash;
            return true;
        }
        hash = default(int);
        return false;
    }
    public override HashRegister Clone() {
        return new HashRegister(this); //
    }

    public override void AddToMod(ModIdentifier mod, params object[] pairs) {
        for (int i = 0; i < pairs.Length; i++) {
            if (typeof(Hash).IsAssignableFrom(pairs[i].GetType())) {
                var hash = (Hash)pairs[i];
                _modInstances[mod].Add(hash);
            }
        }
    }
}

public static class HashGroups
{
    public static class ArmorStandSlots
    {
        public static int[] Items = new int[] {
            "0_item".GetStableHashCode(),
            "1_item".GetStableHashCode(),
            "2_item".GetStableHashCode(),
            "3_item".GetStableHashCode(),
            "4_item".GetStableHashCode(),
            "5_item".GetStableHashCode(),
            "6_item".GetStableHashCode(),
            "7_item".GetStableHashCode(),
        };
        public static int[] Variant = new int[] {
            "0_variant".GetStableHashCode(),
            "1_variant".GetStableHashCode(),
            "2_variant".GetStableHashCode(),
            "3_variant".GetStableHashCode(),
            "4_variant".GetStableHashCode(),
            "5_variant".GetStableHashCode(),
            "6_variant".GetStableHashCode(),
            "7_variant".GetStableHashCode(),
        };
        public static int[] Quality = new int[] {
            "0_quality".GetStableHashCode(),
            "1_quality".GetStableHashCode(),
            "2_quality".GetStableHashCode(),
            "3_quality".GetStableHashCode(),
            "4_quality".GetStableHashCode(),
            "5_quality".GetStableHashCode(),
            "6_quality".GetStableHashCode(),
            "7_quality".GetStableHashCode()
        };
    }
}

public static class KnownHashes
{
    public static readonly Hash ArmorStand_item_0    = new Hash("0_item");
    public static readonly Hash ArmorStand_item_1    = new Hash("1_item");
    public static readonly Hash ArmorStand_item_2    = new Hash("2_item");
    public static readonly Hash ArmorStand_item_3    = new Hash("3_item");
    public static readonly Hash ArmorStand_item_4    = new Hash("4_item");
    public static readonly Hash ArmorStand_item_5    = new Hash("5_item");
    public static readonly Hash ArmorStand_item_6    = new Hash("6_item");
    public static readonly Hash ArmorStand_item_7    = new Hash("7_item");
    public static readonly Hash ArmorStand_variant_0 = new Hash("0_variant");
    public static readonly Hash ArmorStand_variant_1 = new Hash("1_variant");
    public static readonly Hash ArmorStand_variant_2 = new Hash("2_variant");
    public static readonly Hash ArmorStand_variant_3 = new Hash("3_variant");
    public static readonly Hash ArmorStand_variant_4 = new Hash("4_variant");
    public static readonly Hash ArmorStand_variant_5 = new Hash("5_variant");
    public static readonly Hash ArmorStand_variant_6 = new Hash("6_variant");
    public static readonly Hash ArmorStand_variant_7 = new Hash("7_variant");
    public static readonly Hash ArmorStand_quality_0 = new Hash("0_quality");
    public static readonly Hash ArmorStand_quality_1 = new Hash("1_quality");
    public static readonly Hash ArmorStand_quality_2 = new Hash("2_quality");
    public static readonly Hash ArmorStand_quality_3 = new Hash("3_quality");
    public static readonly Hash ArmorStand_quality_4 = new Hash("4_quality");
    public static readonly Hash ArmorStand_quality_5 = new Hash("5_quality");
    public static readonly Hash ArmorStand_quality_6 = new Hash("6_quality");
    public static readonly Hash ArmorStand_quality_7 = new Hash("7_quality");
    public static readonly Hash s_accTime            = new Hash("accTime");
    public static readonly Hash s_addedDefaultItems  = new Hash("addedDefaultItems");
    public static readonly Hash s_aggravated         = new Hash("aggravated");
    public static readonly Hash s_alert              = new Hash("alert");
    public static readonly Hash s_aliveTime          = new Hash("alive_time");
    public static readonly Hash s_ammo               = new Hash("ammo");
    public static readonly Hash s_ammoType           = new Hash("ammoType");
    public static readonly Hash s_attachJointHash    = new Hash("attachJoint");
    public static readonly Hash s_author             = new Hash("author");
    public static readonly Hash s_authorDisplayName  = new Hash("authorPlatformDisplayName");
    public static readonly Hash s_bait               = new Hash("Bait");
    public static readonly Hash s_bakeTimer          = new Hash("bakeTimer");
    public static readonly Hash s_baseValue          = new Hash("baseValue");
    public static readonly Hash s_beardItem          = new Hash("BeardItem");
    public static readonly Hash s_bodyAVelHash       = new Hash("body_avel");
    public static readonly Hash s_bodyVelHash        = new Hash("body_vel");
    public static readonly Hash s_bodyVelocity       = new Hash("BodyVelocity");
    public static readonly Hash BuildingSkillLevel
        = new Hash(WorldEditHash.BuildingSkillLevel, "BuildingSkillLevel");
    public static readonly Hash s_catchID   = new Hash("catchID");
    public static readonly Hash s_chestItem = new Hash("ChestItem");
    public static readonly Hash CLLC_BossEffect
        = new Hash(WorldEditHash.CLLC_BossEffect, "CL&LC effect");
    public static readonly Hash CLLC_Effect
        = new Hash(WorldEditHash.CLLC_Effect, "CL&LC effect");
    public static readonly Hash CLLC_Infusion
        = new Hash(WorldEditHash.CLLC_Infusion, "CL&LC infusion");
    public static readonly Hash s_content = new Hash("Content");
    public static readonly Hash s_crafterID = new Hash("crafterID");
    public static readonly Hash s_crafterName = new Hash("crafterName");
    public static readonly Hash s_creator = new Hash("creator");
    public static readonly Hash s_creatorName = new Hash("creatorName");
    public static readonly Hash s_data = new Hash("data");
    public static readonly Hash s_dataCount = new Hash("dataCount");
    public static readonly Hash s_dead = new Hash("dead");
    public static readonly Hash s_debugFly = new Hash("DebugFly");
    public static readonly Hash s_despawnInDay = new Hash("DespawnInDay");
    public static readonly Hash s_dodgeinv = new Hash("dodgeinv");
    public static readonly Hash s_done = new Hash("done");
    public static readonly Hash s_drops = new Hash("drops");
    public static readonly Hash s_durability = new Hash("durability");
    public static readonly Hash s_eitr = new Hash("eitr");
    public static readonly Hash s_emote = new Hash("emote");
    public static readonly Hash s_emoteOneshot = new Hash("emote_oneshot");
    public static readonly Hash s_emoteID = new Hash("emoteID");
    public static readonly Hash s_enabled = new Hash("enabled");
    public static readonly Hash s_escape = new Hash("escape");
    public static readonly Hash s_eventCreature = new Hash("EventCreature");
    public static readonly Hash Faction = new Hash(WorldEditHash.Faction, "faction");
    public static readonly Hash s_follow = new Hash("follow");
    public static readonly Hash s_forward = new Hash("forward");
    public static readonly Hash s_fuel = new Hash("fuel");
    public static readonly Hash s_growStart = new Hash("GrowStart");
    public static readonly Hash s_hairColor = new Hash("HairColor");
    public static readonly Hash s_hairItem = new Hash("HairItem");
    public static readonly Hash HasFields = new Hash(WorldEditHash.HasFields, "HasFields");
    public static readonly Hash s_haveSaddleHash = new Hash("HaveSaddle");
    public static readonly Hash s_haveTargetHash = new Hash("haveTarget");
    public static readonly Hash s_health = new Hash("health");
    public static readonly Hash s_helmetItem = new Hash("HelmetItem");
    public static readonly Hash s_hitDir = new Hash("HitDir");
    public static readonly Hash s_hitPoint = new Hash("HitPoint");
    public static readonly Hash s_hooked = new Hash("hooked");
    public static readonly Hash s_hue = new Hash("Hue");
    public static readonly Hash s_huntPlayer = new Hash("huntplayer");
    public static readonly Hash s_inBed = new Hash("inBed");
    public static readonly Hash s_initVel = new Hash("InitVel");
    public static readonly Hash s_inUse = new Hash("InUse");
    public static readonly Hash s_inWater = new Hash("inWater");
    public static readonly Hash s_isBlockingHash = new Hash("IsBlocking");
    public static readonly Hash s_item = new Hash("item");
    public static readonly Hash s_item0 = new Hash("item0");
    public static readonly Hash s_itemPrefab = new Hash("itemPrefab");
    public static readonly Hash s_items = new Hash("items");
    public static readonly Hash s_itemStack = new Hash("itemStack");
    public static readonly Hash s_landed = new Hash("landed");
    public static readonly Hash s_lastAttack = new Hash("lastAttack");
    public static readonly Hash s_lastSpawn = new Hash("LastSpawn");
    public static readonly Hash s_lastTime = new Hash("lastTime");
    public static readonly Hash s_worldTimeHash = new Hash("lastWorldTime");
    public static readonly Hash s_leftBackItem = new Hash("LeftBackItem");
    public static readonly Hash s_leftBackItemVariant = new Hash("LeftBackItemVariant");
    public static readonly Hash s_leftItem = new Hash("LeftItem");
    public static readonly Hash s_leftItemVariant = new Hash("LeftItemVariant");
    public static readonly Hash s_legItem = new Hash("LegItem");
    public static readonly Hash s_level = new Hash("level");
    public static readonly Hash s_liquidData = new Hash("LiquidData");
    public static readonly Hash s_location = new Hash("location");
    public static readonly Hash s_locked = new Hash("Locked");
    public static readonly Hash s_lookTarget = new Hash("LookTarget");
    public static readonly Hash s_lovePoints = new Hash("lovePoints");
    public static readonly Hash s_maxHealth = new Hash("max_health");
    public static readonly Hash s_maxInstances = new Hash("MaxInstances");
    public static readonly Hash s_modelIndex = new Hash("ModelIndex");
    public static readonly Hash s_noise = new Hash("noise");
    public static readonly Hash s_offset = new Hash("offset");
    public static readonly Hash Amount = new Hash(WorldEditHash.Amount, "override_amount");
    public static readonly Hash Attacks = new Hash(WorldEditHash.Attacks, "override_attacks");
    public static readonly Hash Biome = new Hash(WorldEditHash.Biome, "override_biome");
    public static readonly Hash Boss = new Hash(WorldEditHash.Boss, "override_boss");
    public static readonly Hash CloseEffect
        = new Hash(WorldEditHash.CloseEffect, "override_close_effect");
    public static readonly Hash NoCollision
        = new Hash(WorldEditHash.NoCollision, "override_collision");
    public static readonly Hash Command = new Hash(WorldEditHash.Command, "override_command");
    public static readonly Hash Compendium
        = new Hash(WorldEditHash.Compendium, "override_compendium");
    public static readonly Hash Component
        = new Hash(WorldEditHash.Component, "override_component");
    public static readonly Hash Conversion
        = new Hash(WorldEditHash.Conversion, "override_conversion");
    public static readonly Hash CoverOffset
        = new Hash(WorldEditHash.CoverOffset, "override_cover_offset");
    public static readonly Hash Data    = new Hash(WorldEditHash.Data, "override_data");
    public static readonly Hash Delay   = new Hash(WorldEditHash.Delay, "override_delay");
    public static readonly Hash Destroy = new Hash(WorldEditHash.Destroy, "override_destroy");
    public static readonly Hash DestroyEffect
        = new Hash(WorldEditHash.DestroyEffect, "override_destroy_effect");
    public static readonly Hash
        Discover = new Hash(WorldEditHash.Discover, "override_discover");
    public static readonly Hash DoorConsume
        = new Hash(WorldEditHash.DoorConsume, "override_door_consume");
    public static readonly Hash DoorKey = new Hash(WorldEditHash.DoorKey, "override_door_key");
    public static readonly Hash DoorNoClose
        = new Hash(WorldEditHash.DoorNoClose, "override_door_no_close");
    public static readonly Hash DungeonEnterHover
        = new Hash(WorldEditHash.DungeonEnterHover, "override_dungeon_enter_hover");
    public static readonly Hash DungeonEnterText
        = new Hash(WorldEditHash.DungeonEnterText, "override_dungeon_enter_text");
    public static readonly Hash DungeonExitHover
        = new Hash(WorldEditHash.DungeonExitHover, "override_dungeon_exit_hover");
    public static readonly Hash DungeonExitText
        = new Hash(WorldEditHash.DungeonExitText, "override_dungeon_exit_text");
    public static readonly Hash DungeonWeather
        = new Hash(WorldEditHash.DungeonWeather, "override_dungeon_weather");
    public static readonly Hash Effect = new Hash(WorldEditHash.Effect, "override_effect");
    public static readonly Hash Event  = new Hash(WorldEditHash.Event, "override_event");
    public static readonly Hash Fall   = new Hash(WorldEditHash.Fall, "override_fall");
    public static readonly Hash
        FarRadius = new Hash(WorldEditHash.FarRadius, "override_far_radius");
    public static readonly Hash
        OverrideFuel = new Hash(WorldEditHash.OverrideFuel, "override_fuel");
    public static readonly Hash FuelEffect
        = new Hash(WorldEditHash.FuelEffect, "override_fuel_effect");
    public static readonly Hash
        FuelUsage = new Hash(WorldEditHash.FuelUsage, "override_fuel_usage");
    public static readonly Hash GlobalKey
        = new Hash(WorldEditHash.GlobalKey, "override_globalkey");
    public static readonly Hash Growth = new Hash(WorldEditHash.Growth, "override_growth");
    public static readonly Hash
        SpawnHealth = new Hash(WorldEditHash.SpawnHealth, "override_health");
    public static readonly Hash InputEffect
        = new Hash(WorldEditHash.InputEffect, "override_input_effect");
    public static readonly Hash
        NoInteract = new Hash(WorldEditHash.NoInteract, "override_interact");
    public static readonly Hash
        OverrideItem = new Hash(WorldEditHash.OverrideItem, "override_item");
    public static readonly Hash ItemOffset
        = new Hash(WorldEditHash.ItemOffset, "override_item_offset");
    public static readonly Hash ItemStandPrefix
        = new Hash(WorldEditHash.ItemStandPrefix, "override_item_stand_prefix");
    public static readonly Hash ItemStandRange
        = new Hash(WorldEditHash.ItemStandRange, "override_item_stand_range");
    public static readonly Hash OverrideItems
        = new Hash(WorldEditHash.OverrideItems, "override_items");
    public static readonly Hash LevelChance
        = new Hash(WorldEditHash.LevelChance, "override_level_chance");
    public static readonly Hash LockedEffect
        = new Hash(WorldEditHash.LockedEffect, "override_locked_effect");
    public static readonly Hash MaxNear = new Hash(WorldEditHash.MaxNear, "override_max_near");
    public static readonly Hash MaxTotal
        = new Hash(WorldEditHash.MaxTotal, "override_max_total");
    public static readonly Hash MaxAmount
        = new Hash(WorldEditHash.MaxAmount, "override_maximum_amount");
    public static readonly Hash MaxCover
        = new Hash(WorldEditHash.MaxCover, "override_maximum_cover");
    public static readonly Hash MaxFuel
        = new Hash(WorldEditHash.MaxFuel, "override_maximum_fuel");
    public static readonly Hash MaxLevel
        = new Hash(WorldEditHash.MaxLevel, "override_maximum_level");
    public static readonly Hash MinAmount
        = new Hash(WorldEditHash.MinAmount, "override_minimum_amount");
    public static readonly Hash MinLevel
        = new Hash(WorldEditHash.MinLevel, "override_minimum_level");
    public static readonly Hash
        OverrideName = new Hash(WorldEditHash.OverrideName, "override_name");
    public static readonly Hash NearRadius
        = new Hash(WorldEditHash.NearRadius, "override_near_radius");
    public static readonly Hash OpenEffect
        = new Hash(WorldEditHash.OpenEffect, "override_open_effect");
    public static readonly Hash OutputEffect
        = new Hash(WorldEditHash.OutputEffect, "override_output_effect");
    public static readonly Hash RespawnPickable
        = new Hash(WorldEditHash.RespawnPickable, "override_pickable_respawn");
    public static readonly Hash SpawnPickable
        = new Hash(WorldEditHash.SpawnPickable, "override_pickable_spawn");
    public static readonly Hash NoRender = new Hash(WorldEditHash.NoRender, "override_render");
    public static readonly Hash Resistances
        = new Hash(WorldEditHash.Resistances, "override_resistances");
    public static readonly Hash Respawn = new Hash(WorldEditHash.Respawn, "override_respawn");
    public static readonly Hash
        NoRestrict = new Hash(WorldEditHash.NoRestrict, "override_restrict");
    public static readonly Hash Smoke = new Hash(WorldEditHash.Smoke, "override_smoke");
    public static readonly Hash Spawn = new Hash(WorldEditHash.Spawn, "override_spawn");
    public static readonly Hash SpawnCondition
        = new Hash(WorldEditHash.SpawnCondition, "override_spawn_condition");
    public static readonly Hash SpawnEffect
        = new Hash(WorldEditHash.SpawnEffect, "override_spawn_effect");
    public static readonly Hash
        SpawnItem = new Hash(WorldEditHash.SpawnItem, "override_spawn_item");
    public static readonly Hash SpawnMaxY
        = new Hash(WorldEditHash.SpawnMaxY, "override_spawn_max_y");
    public static readonly Hash SpawnOffset
        = new Hash(WorldEditHash.SpawnOffset, "override_spawn_offset");
    public static readonly Hash SpawnRadius
        = new Hash(WorldEditHash.SpawnRadius, "override_spawn_radius");
    public static readonly Hash RespawnSpawnArea
        = new Hash(WorldEditHash.RespawnSpawnArea, "override_spawnarea_respawn");
    public static readonly Hash SpawnSpawnArea
        = new Hash(WorldEditHash.SpawnSpawnArea, "override_spawnarea_spawn");
    public static readonly Hash Speed = new Hash(WorldEditHash.Speed, "override_speed");
    public static readonly Hash StartEffect
        = new Hash(WorldEditHash.StartEffect, "override_start_effect");
    public static readonly Hash Status = new Hash(WorldEditHash.Status, "override_status");
    public static readonly Hash
        OverrideText = new Hash(WorldEditHash.OverrideText, "override_text");
    public static readonly Hash
        TextBiome = new Hash(WorldEditHash.TextBiome, "override_text_biome");
    public static readonly Hash
        TextCheck = new Hash(WorldEditHash.TextCheck, "override_text_check");
    public static readonly Hash TextExtract
        = new Hash(WorldEditHash.TextExtract, "override_text_extract");
    public static readonly Hash
        TextHappy = new Hash(WorldEditHash.TextHappy, "override_text_happy");
    public static readonly Hash
        TextSleep = new Hash(WorldEditHash.TextSleep, "override_text_sleep");
    public static readonly Hash
        TextSpace = new Hash(WorldEditHash.TextSpace, "override_text_space");
    public static readonly Hash Topic = new Hash(WorldEditHash.Topic, "override_topic");
    public static readonly Hash TriggerDistance
        = new Hash(WorldEditHash.TriggerDistance, "override_trigger_distance");
    public static readonly Hash TriggerNoise
        = new Hash(WorldEditHash.TriggerNoise, "override_trigger_noise");
    public static readonly Hash Unlock = new Hash(WorldEditHash.Unlock, "override_unlock");
    public static readonly Hash
        UseEffect = new Hash(WorldEditHash.UseEffect, "override_use_effect");
    public static readonly Hash Water = new Hash(WorldEditHash.Water, "override_water");
    public static readonly Hash Wear = new Hash(WorldEditHash.Wear, "override_wear");
    public static readonly Hash Weather = new Hash(WorldEditHash.Weather, "override_weather");
    public static readonly Hash s_owner = new Hash("owner");
    public static readonly Hash s_ownerName = new Hash("ownerName");
    public static readonly Hash s_patrol = new Hash("patrol");
    public static readonly Hash s_patrolPoint = new Hash("patrolPoint");
    public static readonly Hash s_permitted = new Hash("permitted");
    public static readonly Hash s_pickable = new Hash("pickable");
    public static readonly Hash s_picked = new Hash("picked");
    public static readonly Hash s_pickedTime = new Hash("picked_time");
    public static readonly Hash s_pickedUp = new Hash("pickedUp");
    public static readonly Hash s_piece = new Hash("piece");
    public static readonly Hash s_plantTime = new Hash("plantTime");
    public static readonly Hash s_played = new Hash("played");
    public static readonly Hash Player = new Hash(WorldEditHash.Player, "Player");
    public static readonly Hash s_playerID = new Hash("playerID");
    public static readonly Hash s_playerName = new Hash("playerName");
    public static readonly Hash s_plays = new Hash("plays");
    public static readonly Hash Portal = new Hash(WorldEditHash.Portal, "portal_wood");
    public static readonly Hash s_pose = new Hash("pose");
    public static readonly Hash s_pregnant = new Hash("pregnant");
    public static readonly Hash s_product = new Hash("product");
    public static readonly Hash s_pvp = new Hash("pvp");
    public static readonly Hash s_quality = new Hash("quality");
    public static readonly Hash s_queued = new Hash("queued");
    public static readonly Hash s_randomSkillFactor = new Hash("RandomSkillFactor");
    public static readonly Hash s_relPosHash = new Hash("relPos");
    public static readonly Hash s_relRotHash = new Hash("relRot");
    public static readonly Hash s_rightBackItem = new Hash("RightBackItem");
    public static readonly Hash s_rightItem = new Hash("RightItem");
    public static readonly Hash s_rodOwner = new Hash("rodOwner");
    public static readonly Hash s_roomData = new Hash("roomData");
    public static readonly Hash s_rooms = new Hash("rooms");
    public static readonly Hash s_rudder = new Hash("rudder");
    public static readonly Hash s_saturation = new Hash("Saturation");
    public static readonly Hash s_scaleHash = new Hash("scale");
    public static readonly Hash s_scaleScalarHash = new Hash("scaleScalar");
    public static readonly Hash s_seAttrib = new Hash("seAttrib");
    public static readonly Hash s_seed = new Hash("seed");
    public static readonly Hash s_shoulderItem = new Hash("ShoulderItem");
    public static readonly Hash s_shoulderItemVariant = new Hash("ShoulderItemVariant");
    public static readonly Hash s_shownAlertMessage = new Hash("ShownAlertMessage");
    public static readonly Hash s_skinColor = new Hash("SkinColor");
    public static readonly Hash s_sleeping = new Hash("sleeping");
    public static readonly Hash s_spawn_time__DontUse = new Hash("spawn_time");
    public static readonly Hash s_spawnAmount = new Hash("SpawnAmount");
    public static readonly Hash s_spawnOre = new Hash("SpawnOre");
    public static readonly Hash SpawnPoint = new Hash(WorldEditHash.SpawnPoint, "spawnpoint");
    public static readonly Hash s_spawnPoint = new Hash("spawnpoint");
    public static readonly Hash s_SpawnPoint__DontUse = new Hash("SpawnPoint");
    public static readonly Hash SpawnTime = new Hash(WorldEditHash.SpawnTime, "spawntime");
    public static readonly Hash s_spawnTime = new Hash("spawntime");
    public static readonly Hash s_SpawnTime__DontUse = new Hash("SpawnTime");
    public static readonly Hash s_spread = new Hash("spread");
    public static readonly Hash s_stack = new Hash("stack");
    public static readonly Hash s_stamina = new Hash("stamina");
    public static readonly Hash s_startTime = new Hash("StartTime");
    public static readonly Hash s_state = new Hash("state");
    public static readonly Hash s_stealth = new Hash("Stealth");
    public static readonly Hash s_support = new Hash("support");
    public static readonly Hash s_tag = new Hash("tag");
    public static readonly Hash s_tagauthor = new Hash("tagauthor");
    public static readonly Hash s_tamed = new Hash("tamed");
    public static readonly Hash s_tamedName = new Hash("TamedName");
    public static readonly Hash s_tamedNameAuthor = new Hash("TamedNameAuthor");
    public static readonly Hash s_tameLastFeeding = new Hash("TameLastFeeding");
    public static readonly Hash s_tameTimeLeft = new Hash("TameTimeLeft");
    public static readonly Hash s_targets = new Hash("targets");
    public static readonly Hash s_TCData = new Hash("TCData");
    public static readonly Hash s_terrainModifierTimeCreated
        = new Hash("terrainModifierTimeCreated");
    public static readonly Hash s_text         = new Hash("text");
    public static readonly Hash s_tiltrot      = new Hash("tiltrot");
    public static readonly Hash s_timeOfDeath  = new Hash("timeOfDeath");
    public static readonly Hash s_triggered    = new Hash("triggered");
    public static readonly Hash s_type         = new Hash("type");
    public static readonly Hash s_user         = new Hash("user");
    public static readonly Hash s_utilityItem  = new Hash("UtilityItem");
    public static readonly Hash s_value        = new Hash("Value");
    public static readonly Hash s_variant      = new Hash("variant");
    public static readonly Hash s_velHash      = new Hash("vel");
    public static readonly Hash s_visual       = new Hash("visual");
    public static readonly Hash s_wakeup       = new Hash("wakeup");
    public static readonly Hash s_weaponLoaded = new Hash("WeaponLoaded");
    public static readonly Hash s_worldLevel   = new Hash("worldLevel");
}