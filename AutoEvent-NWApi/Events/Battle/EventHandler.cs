﻿using InventorySystem.Configs;
using PlayerRoles;
using System.Collections.Generic;

namespace AutoEvent.Events.Battle
{
    public class EventHandler
    {
        /*
        public void OnJoin(VerifiedEventArgs ev)
        {
            ev.Player.Role.Set(RoleTypeId.Spectator);
        }
        public void OnReloading(ReloadingWeaponEventArgs ev)
        {
            SetMaxAmmo(ev.Player);
        }
        public void OnSpawned(SpawnedEventArgs ev)
        {
            SetMaxAmmo(ev.Player);
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev) => ev.IsAllowed = false;
        public void OnSpawnRagdoll(SpawningRagdollEventArgs ev) => ev.IsAllowed = false;
        public void OnPlaceBullet(PlacingBulletHole ev) => ev.IsAllowed = false;
        public void OnPlaceBlood(PlacingBloodEventArgs ev) => ev.IsAllowed = false;
        public void OnDropItem(DroppingItemEventArgs ev) => ev.IsAllowed = false;
        public void OnDropAmmo(DroppingAmmoEventArgs ev) => ev.IsAllowed = false;
        private void SetMaxAmmo(Player pl)
        {
            foreach (KeyValuePair<ItemType, ushort> AmmoLimit in InventoryLimits.StandardAmmoLimits)
                pl.SetAmmo(AmmoLimit.Key.GetAmmoType(), AmmoLimit.Value);
        }
        */
    }
}
