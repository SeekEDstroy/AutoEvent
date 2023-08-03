﻿using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using PlayerRoles;
using MapEditorReborn.API.Features.Objects;
using UnityEngine;
using AutoEvent.Events.Puzzle.Features;
using PluginAPI.Core;
using PluginAPI.Events;
using Random = UnityEngine.Random;
using Event = AutoEvent.Interfaces.Event;

namespace AutoEvent.Events.Puzzle
{
    public class Plugin : Event
    {
        public override string Name { get; set; } = Translation.PuzzleName;
        public override string Description { get; set; } = Translation.PuzzleDescription;
        public override string Author { get; set; } = "KoT0XleB";
        public override string MapName { get; set; } = "Puzzle";
        public override string CommandName { get; set; } = "puzzle";
        public SchematicObject GameMap { get; set; }
        public TimeSpan EventTime { get; set; }
        public List<GameObject> Platformes { get; set; }
        public GameObject Lava { get; set; }

        private readonly string _broadcastName = "<color=#F59F00>P</color><color=#F68523>u</color><color=#F76B46>z</color><color=#F85169>z</color><color=#F9378C>l</color><color=#FA1DAF>e</color>";

        EventHandler _eventHandler;

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
            EventTime = new TimeSpan(0, 0, 0);

            GameMap = Extensions.LoadMap(MapName, new Vector3(76f, 1026.5f, -43.68f), Quaternion.Euler(Vector3.zero), Vector3.one);
            Extensions.PlayAudio("Puzzle.ogg", 15, true, Name);

            Platformes = GameMap.AttachedBlocks.Where(x => x.name == "Platform").ToList();
            Lava = GameMap.AttachedBlocks.First(x => x.name == "Lava");
            Lava.AddComponent<LavaComponent>();

            foreach (Player player in Player.GetPlayers())
            {
                player.SetRole(RoleTypeId.ClassD, RoleChangeReason.None);
                player.Position = RandomClass.GetSpawnPosition(GameMap);
            }

            Timing.RunCoroutine(OnEventRunning(), "puzzle_run");
        }
        public IEnumerator<float> OnEventRunning()
        {
            for (int time = 15; time > 0; time--)
            {
                Extensions.Broadcast($"{_broadcastName}\n{Translation.PuzzleStart.Replace("%time%", $"{time}")}", 1);
                yield return Timing.WaitForSeconds(1f);
            }

            int stage = 1;
            int finaleStage = 10;
            float speed = 5;
            float timing = 0.5f;
            List<GameObject> ListPlatformes = Platformes;

            while (stage <= finaleStage && Player.GetPlayers().Count(r => r.IsAlive) > 0)
            {
                var stageText = Translation.PuzzleStage;
                stageText = stageText.Replace("%stageNum%", $"{stage}");
                stageText = stageText.Replace("%stageFinal%", $"{finaleStage}");
                stageText = stageText.Replace("%plyCount%", $"{Player.GetPlayers().Count(r => r.IsAlive)}");

                for (float time = speed * 2; time > 0; time--)
                {
                    foreach (var platform in Platformes)
                    {
                        //platform.GetComponent<PrimitiveObject>().Primitive.Color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
                    }
                    
                    Extensions.Broadcast($"<b>{Name}</b>\n{stageText}", 1);
                    yield return Timing.WaitForSeconds(timing);
                }

                var randPlatform = ListPlatformes.RandomItem();
                ListPlatformes = new List<GameObject>();
                //randPlatform.GetComponent<PrimitiveObject>().Primitive.Color = UnityEngine.Color.green;

                foreach (var platform in Platformes)
                {
                    if (platform != randPlatform)
                    {
                        //platform.GetComponent<PrimitiveObject>().Primitive.Color = UnityEngine.Color.magenta;
                        ListPlatformes.Add(platform);
                    }
                }
                Extensions.Broadcast($"<b>{_broadcastName}</b>\n{stageText}", (ushort)(speed + 1));
                yield return Timing.WaitForSeconds(speed);

                foreach (var platform in Platformes)
                {
                    if (platform != randPlatform)
                    {
                        platform.transform.position += Vector3.down * 5;
                    }
                }
                Extensions.Broadcast($"<b>{_broadcastName}</b>\n{stageText}", (ushort)(speed + 1));
                yield return Timing.WaitForSeconds(speed);

                foreach (var platform in Platformes)
                {
                    if (platform != randPlatform)
                    {
                        platform.transform.position += Vector3.up * 5;
                    }
                }
                Extensions.Broadcast($"<b>{_broadcastName}</b>\n{stageText}", (ushort)(speed + 1));
                yield return Timing.WaitForSeconds(speed);

                speed -= 0.4f;
                stage++;
                timing -= 0.04f;
            }

            if (Player.GetPlayers().Count(r => r.IsAlive) < 1)
            {
                Extensions.Broadcast($"<b>{_broadcastName}</b>\n{Translation.PuzzleAllDied}", 10);
            }
            else if (Player.GetPlayers().Count(r => r.IsAlive) == 1)
            {
                var player = Player.GetPlayers().First(r => r.IsAlive).DisplayNickname;
                Extensions.Broadcast($"<b>{_broadcastName}</b>\n{Translation.PuzzleWinner.Replace("%plyWinner%", $"{player}")}", 10);
            }
            else
            {
                Extensions.Broadcast($"<b>{_broadcastName}</b>\n{Translation.PuzzleSeveralSurvivors}", 10);
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
