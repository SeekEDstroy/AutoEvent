﻿using CustomPlayerEffects;
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

namespace AutoEvent.Events.Survival
{
    public class Plugin : Event
    {
        public override string Name { get; set; } = Translation.SurvivalName;
        public override string Description { get; set; } = Translation.SurvivalDescription;
        public override string Author { get; set; } = "KoT0XleB";
        public override string MapName { get; set; } = "Survival";
        public override string CommandName { get; set; } = "zombie2";
        public SchematicObject GameMap { get; set; }
        public TimeSpan EventTime { get; set; }

        private bool isFriendlyFireEnabled;
        EventHandler _eventHandler;
        public Player firstZombie;

        public override void OnStart()
        {
            isFriendlyFireEnabled = Server.FriendlyFire;
            Server.FriendlyFire = false;
            OnEventStarted();

            _eventHandler = new EventHandler(this);
            EventManager.RegisterEvents(_eventHandler);
        }
        public override void OnStop()
        {
            Server.FriendlyFire = isFriendlyFireEnabled;

            EventManager.UnregisterEvents(_eventHandler);
            _eventHandler = null;
            Timing.CallDelayed(10f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 5, 0);

            GameMap = Extensions.LoadMap(MapName, new Vector3(15f, 1030f, -43.68f), Quaternion.identity, Vector3.one);

            foreach (Player player in Player.GetPlayers())
            {
                player.SetRole(RoleTypeId.NtfSergeant, RoleChangeReason.None);
                player.Position = RandomClass.GetSpawnPosition(GameMap);
                player.ArtificialHealth = 100;//player.AddAhp(100, 100, 0, 0, 0, true);

                player.AddItem(RandomClass.GetRandomGun());
                player.AddItem(ItemType.GunCOM18);
                player.AddItem(ItemType.ArmorCombat);

                Timing.CallDelayed(0.1f, () =>
                {
                    player.CurrentItem = player.Items.ElementAt(0);
                });
            }

            Timing.RunCoroutine(OnEventRunning(), "survival_run");
        }

        public IEnumerator<float> OnEventRunning()
        {
            Extensions.PlayAudio("Survival.ogg", 10, false, Name);
            for (float _time = 20; _time > 0; _time--)
            {
                Extensions.Broadcast(Translation.SurvivalBeforeInfection.Replace("%name%", Name).Replace("%time%", $"{_time}"), 1);
                yield return Timing.WaitForSeconds(1f);
            }

            Extensions.StopAudio();
            Timing.CallDelayed(0.1f, () =>
            {
                Extensions.PlayAudio("Zombie2.ogg", 7, false, Name);
            });

            for (int i = 0; i <= Player.GetPlayers().Count() / 10; i++)
            {
                var player = Player.GetPlayers().Where(r => r.IsHuman).ToList().RandomItem();
                player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.None);
                player.EffectsManager.EnableEffect<Disabled>();
                player.EffectsManager.EnableEffect<Scp1853>();
                player.Health = 5000;

                if (Player.GetPlayers().Count(r => r.IsSCP) == 1)
                {
                    firstZombie = player;
                }
            }

            var teleport = GameMap.AttachedBlocks.First(x => x.name == "Teleport");
            var teleport1 = GameMap.AttachedBlocks.First(x => x.name == "Teleport1");

            while (Player.GetPlayers().Count(r => r.IsHuman) > 0 && Player.GetPlayers().Count(r => r.IsSCP) > 0 && EventTime.TotalSeconds > 0)
            {
                var text = Translation.SurvivalAfterInfection;
                text = text.Replace("%name%", Name);
                text = text.Replace("%humanCount%", Player.GetPlayers().Count(r => r.IsHuman).ToString());
                text = text.Replace("%time%", $"{EventTime.Minutes}:{EventTime.Seconds}");

                foreach (var player in Player.GetPlayers())
                {
                    player.ClearBroadcasts();
                    player.SendBroadcast(text, 1);

                    if (Vector3.Distance(player.Position, teleport.transform.position) < 1)
                    {
                        player.Position = teleport1.transform.position;
                    }
                }

                yield return Timing.WaitForSeconds(1f);
                EventTime -= TimeSpan.FromSeconds(1f);
            }

            if (Player.GetPlayers().Count(r => r.IsHuman) == 0)
            {
                Extensions.Broadcast(Translation.SurvivalZombieWin, 10);

                Extensions.StopAudio();
                Timing.CallDelayed(0.1f, () =>
                {
                    Extensions.PlayAudio("ZombieWin.ogg", 7, false, Name);
                });
            }
            else if (Player.GetPlayers().Count(r => r.IsSCP) == 0)
            {
                Extensions.Broadcast(Translation.SurvivalHumanWin, 10);

                Extensions.StopAudio();
                Timing.CallDelayed(0.1f, () =>
                {
                    Extensions.PlayAudio("HumanWin.ogg", 7, false, Name);
                });
            }
            else
            {
                Extensions.Broadcast(Translation.SurvivalHumanWinTime, 10);

                Extensions.StopAudio();
                Timing.CallDelayed(0.1f, () =>
                {
                    Extensions.PlayAudio("HumanWin.ogg", 7, false, Name);
                });
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
