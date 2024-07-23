using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace DoNotOverwater;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {

        var harmony = new Harmony(ModManifest.UniqueID);
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.BeginUsingTool)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Farmer_BeginUsingTool_Prefix))
        );
    }

    /// <summary>Prefix method to check if the tile is already watered or the can can be refilled before using the tool</summary>
    /// <param name="__instance">The Farmer can instance.</param>
    /// <returns>Returns true to continue with the original method, false to prevent tool usage</returns>
    private static bool Farmer_BeginUsingTool_Prefix(Farmer __instance)
    {

        // if not watering can -> business as usual
        if (__instance.CurrentTool is not WateringCan)
        {
            return true;
        }
        
        var x = (int)__instance.GetToolLocation().X / 64;
        var y = (int)__instance.GetToolLocation().Y / 64;
        
        var tile = new Vector2(x, y); 
        __instance.currentLocation.terrainFeatures.TryGetValue(tile, out var terrainFeature);
        
        // check if terrainFeature is HoeDirt
        if (terrainFeature is HoeDirt hoeDirt)
        {
            // if it is -> check if it has a crop that has not been watered yet
            if (
                hoeDirt.crop != null &&
                hoeDirt.state.Value != HoeDirt.watered
            ) {
                // water
                return true; 
            } 
        } else // check if watering can can be refilled
        if (Game1.currentLocation.CanRefillWateringCanOnTile(x, y))
        {
            // refill
            return true;
        }

        // otherwise do not water
        return false; 
    }
}