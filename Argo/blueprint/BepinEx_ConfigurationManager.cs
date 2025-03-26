using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using System.Linq;
using UnityEngine;
using System.IO;
using BepInEx;
using HarmonyLib;

namespace Argo.Blueprint;
[BepInPlugin(GUID, NAME, VERSION)]
public class Argonaut : BaseUnityPlugin
{
    public const  string GUID = "Argonaut";
    public const  string NAME = "Argo";
    public const  string VERSION = "0.1.0";
    public static ConfigEntry<string> configEnableBuildPlayer;
    public static ConfigEntry<string> configEnableBuildPiece;
    public static ConfigEntry<string> configEnablePlaceable;
    public static ConfigEntry<string> configEnableCompfort;
    public static ConfigEntry<string> configEnableLightSource;
    public static ConfigEntry<string> configEnableHoverable;
    public static ConfigEntry<string> configEnableTextReceiver;
    public static ConfigEntry<string> configEnableInteractable;
    public static ConfigEntry<string> configEnableObjectHolder;
    public static ConfigEntry<string> configEnableContainerPiece;
    public static ConfigEntry<string> configEnableCraftingStation;
    public static ConfigEntry<string> configEnableFuel;
    public static ConfigEntry<string> configEnablePickable;
    public static ConfigEntry<string> configEnableCultivated;
    public static ConfigEntry<string> configEnableDestroyableTerrain;
    public static ConfigEntry<string> configEnableFractured;
    public static ConfigEntry<string> configEnableHoverableResourceNode;
    public static ConfigEntry<string> configEnableNonStatic;
    public static ConfigEntry<string> configEnableCreature;
    public static ConfigEntry<string> configEnableTameable;
    public static ConfigEntry<string> configEnableVehicle;
    public static ConfigEntry<string> configEnableAnimated;
    public static ConfigEntry<string> configEnableSpecialInterface;
    public static ConfigEntry<string> configEnableIndestructible;
    public static ConfigEntry<string> configEnableCustomNotVanilla;
    public static string EnableBuildPlayer => configEnableBuildPlayer.Value;
    public static string EnableBuildPiece => configEnableBuildPiece.Value;
    public static string EnablePlaceable => configEnablePlaceable.Value;
    public static string EnableCompfort => configEnableCompfort.Value;
    public static string EnableLightSource => configEnableLightSource.Value;
    public static string EnableHoverable => configEnableHoverable.Value;
    public static string EnableTextReceiver => configEnableTextReceiver.Value;
    public static string EnableInteractable => configEnableInteractable.Value;
    public static string EnableObjectHolder => configEnableObjectHolder.Value;
    public static string EnableContainerPiece
        => configEnableContainerPiece.Value;
    public static string EnableCraftingStation
        => configEnableCraftingStation.Value;
    public static string EnableFuel       => configEnableFuel.Value;
    public static string EnablePickable   => configEnablePickable.Value;
    public static string EnableCultivated => configEnableCultivated.Value;
    public static string EnableDestroyableTerrain
        => configEnableDestroyableTerrain.Value;
    public static string EnableFractured => configEnableFractured.Value;
    public static string EnableHoverableResourceNode
        => configEnableHoverableResourceNode.Value;
    public static string EnableNonStatic => configEnableNonStatic.Value;
    public static string EnableCreature  => configEnableCreature.Value;
    public static string EnableTameable  => configEnableTameable.Value;
    public static string EnableVehicle   => configEnableVehicle.Value;
    public static string EnableAnimated  => configEnableAnimated.Value;
    public static string EnableSpecialInterface
        => configEnableSpecialInterface.Value;
    public static string EnableIndestructible
        => configEnableIndestructible.Value;
    public static string EnableCustomNotVanilla
        => configEnableCustomNotVanilla.Value;

    private static ConfigFile? ConfigFile;
    public void Awake() {
        if (ConfigFile == null)
        {
            ConfigFile = Config;
        }
        Init(ConfigFile);
    }
    public static ConfigEntry<T> Bind<T>(ConfigFile config, string group, string name, T value,
                                         ConfigDescription description) {
        
        ConfigEntry<T> configEntry
            = config.Bind<T>(group, name, value, description);
       return configEntry;
    }
    public static void Init(ConfigFile config) {
        string section = "8. Json Blueprint Tests";

        configEnableBuildPlayer = Bind(config, section, "Enable BuildPlayer",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude BuildPlayer Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableBuildPiece = Bind(config, section, "Enable BuildPiece",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude BuildPiece Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnablePlaceable = Bind(config, section, "Enable Placeable",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Placeable Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableCompfort = Bind(config, section, "Enable Compfort",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Compfort Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableLightSource = Bind(config, section, "Enable LightSource",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude LightSource Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableHoverable = Bind(config, section, "Enable Hoverable",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Hoverable Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableTextReceiver = Bind(config, section, "Enable TextReceiver",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude TextReceiver Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableInteractable = Bind(config, section, "Enable Interactable",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Interactable Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableObjectHolder = Bind(config, section, "Enable ObjectHolder",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude ObjectHolder Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableContainerPiece = Bind(config, section,
            "Enable ContainerPiece", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude ContainerPiece Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableCraftingStation = Bind(config, section,
            "Enable CraftingStation", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude CraftingStation Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableFuel = Bind(config, section, "Enable Fuel",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Fuel Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnablePickable = Bind(config, section, "Enable Pickable",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Pickable Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableCultivated = Bind(config, section, "Enable Cultivated",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Cultivated Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableDestroyableTerrain = Bind(config, section,
            "Enable DestroyableTerrain", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude DestroyableTerrain Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableFractured = Bind(config, section, "Enable Fractured",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Fractured Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableHoverableResourceNode = Bind(config, section,
            "Enable HoverableResourceNode", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude HoverableResourceNode Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableNonStatic = Bind(config, section, "Enable NonStatic",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude NonStatic Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableCreature = Bind(config, section, "Enable Creature",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Creature Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableTameable = Bind(config, section, "Enable Tameable",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Tameable Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableVehicle = Bind(config, section, "Enable Vehicle",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Vehicle Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableAnimated = Bind(config, section, "Enable Animated",
            CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Animated Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableSpecialInterface = Bind(config, section,
            "Enable SpecialInterface", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude SpecialInterface Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableIndestructible = Bind(config, section,
            "Enable Indestructible", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude Indestructible Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
        configEnableCustomNotVanilla = Bind(config, section,
            "Enable CustomNotVanilla", CategorySettingsStrings.Include,
            new ConfigDescription(
                "Includes, excludes or forces to exclude CustomNotVanilla Pieces",
                new AcceptableValueList<string>(CategorySettingsStrings.Include,
                    CategorySettingsStrings.Exclude,
                    CategorySettingsStrings.ForceExclude)));
    }
}