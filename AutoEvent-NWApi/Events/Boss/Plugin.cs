﻿using AutoEvent.Events.Boss.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = AutoEvent.Interfaces.Event;

namespace AutoEvent.Events.Boss
{
    public class Plugin : Event
    {
        public override string Name { get; set; } = Translation.BossName;
        public override string Description { get; set; } = Translation.BossDescription;
        public override string Author { get; set; } = "KoT0XleB";
        public override string MapName { get; set; } = "DeathParty";
        public override string CommandName { get; set; } = "boss";
        public TimeSpan EventTime { get; set; }
        public SchematicObject GameMap { get; set; }
        public List<GameObject> Workstations { get; set; }

        EventHandler _eventHandler;

        Player Boss;
        public override void OnStart()
        {
            _eventHandler = new EventHandler();
            EventManager.RegisterEvents(_eventHandler);
            OnEventStarted();
        }
        public override void OnStop()
        {
            EventManager.UnregisterEvents(_eventHandler);
            _eventHandler = null;
            Timing.CallDelayed(10f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 2, 0);
            GameMap = Extensions.LoadMap(MapName, new Vector3(6f, 1030f, -43.5f), Quaternion.Euler(Vector3.zero), Vector3.one);

            foreach (Player player in Player.GetPlayers())
            {
                player.SetRole(RoleTypeId.NtfSergeant, RoleChangeReason.None);
                player.Position = RandomClass.GetSpawnPosition(GameMap);
                player.Health = 200;

                RandomClass.CreateSoldier(player);
                Timing.CallDelayed(0.1f, () =>
                {
                    player.CurrentItem = player.Items.First();
                });
            }

            Timing.RunCoroutine(OnEventRunning(), "battle_time");
        }

        public IEnumerator<float> OnEventRunning()
        {
            for (int time = 15; time > 0; time--)
            {
                Extensions.Broadcast($"{Translation.BossTimeLeft.Replace("{time}", $"{time}")}", 5);
                yield return Timing.WaitForSeconds(1f);
            }

            Extensions.PlayAudio("Boss.ogg", 7, false, Name);

            Boss = Player.GetPlayers().Where(r => r.IsNTF).ToList().RandomItem();
            Boss.SetRole(RoleTypeId.ChaosConscript, RoleChangeReason.None);
            Boss.Position = RandomClass.GetSpawnPosition(GameMap);

            Extensions.SetPlayerScale(Boss, new Vector3(5, 5, 5));

            Boss.ClearInventory();
            Boss.AddItem(ItemType.GunLogicer);
            Timing.CallDelayed(0.1f, () => { Boss.CurrentItem = Boss.Items.First(); });

            while (EventTime.TotalSeconds > 0 && Player.GetPlayers().Count(r => r.Team == Team.FoundationForces) > 0 && Player.GetPlayers().Count(r => r.Team == Team.ChaosInsurgency) > 0)
            {
                var text = Translation.BossCounter;
                text = text.Replace("%hp%", $"{(int)Boss.Health}");
                text = text.Replace("%count%", $"{Player.GetPlayers().Count(r => r.IsNTF)}");
                text = text.Replace("%time%", $"{EventTime.Minutes}:{EventTime.Seconds}");

                Extensions.Broadcast(text, 1);

                yield return Timing.WaitForSeconds(1f);
                EventTime -= TimeSpan.FromSeconds(1f);
            }

            Extensions.SetPlayerScale(Boss, new Vector3(1, 1, 1));

            if (Player.GetPlayers().Count(r => r.Team == Team.FoundationForces) == 0)
            {
                Extensions.Broadcast(Translation.BossWin.Replace("%hp%", $"{(int)Boss.Health}"), 10);
            }
            else if (Player.GetPlayers().Count(r => r.Team == Team.ChaosInsurgency) == 0)
            {
                Extensions.Broadcast(Translation.BossHumansWin.Replace("%count%", $"{Player.GetPlayers().Count(r => r.IsNTF)}"), 10);
            }

            OnStop();
            yield break;
        }

        public void EventEnd()
        {
            Extensions.CleanUpAll();
            Extensions.TeleportEnd();
            Extensions.UnLoadMap(GameMap);
            Extensions.StopAudio();
            AutoEvent.ActiveEvent = null;
        }
    }
}
