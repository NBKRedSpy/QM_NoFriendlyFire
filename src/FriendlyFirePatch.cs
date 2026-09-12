using HarmonyLib;
using MGSC;

namespace NoFriendlyFire
{
    /// <summary>
    /// Blocks damage between allied creatures (friendly fire).
    /// Player-side allies are always protected; enemy-vs-enemy damage can be
    /// re-enabled via ModConfig.AllowEnemyFriendlyFire.
    /// </summary>
    [HarmonyPatch(typeof(Creature), nameof(Creature.Injure))]
    public static class FriendlyFirePatch
    {
        public static bool Prefix(Creature __instance, DamageHitInfo hitInfo, ref InjureResult __result)
        {
            Creature attacker = hitInfo.damageDealer;

            //Note that the player does not have FF off so they can still kill friendlies.  Just incase the
            //  player needs or wants to kill a friendly unit.
            if (attacker is Player || attacker == null || attacker == __instance || !attacker.IsAlly(__instance))
            {
                return true;
            }


            bool isPlayerSide = attacker.CreatureData.CreatureAlliance.HasFlag(CreatureAlliance.PlayerAlliance)
                || __instance.CreatureData.CreatureAlliance.HasFlag(CreatureAlliance.PlayerAlliance);

            if (!isPlayerSide && !Plugin.Config.PreventEnemyFriendlyFire)
            {
                return true;
            }


            if(Plugin.Config.DebugLog)
            {
                Plugin.Logger.LogWarning($"Blocking friendly fire: {hitInfo.info.damage} {attacker.CreatureData.LocalizationId} {attacker.CreatureData.UniqueId} -> {__instance.CreatureData.LocalizationId} {__instance.CreatureData.UniqueId}");
            }

            // Same no-op result the base method returns for a miss/immune hit.
            __result = default(InjureResult);
            return false;
        }
    }
}
