﻿using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace RPGMods.Commands
{/*
    [Command("pvp", Usage = "pvp [<on>|<off>|<top>]", Description = "Display your PvP statistics or toggle PvP/Castle Siege state")]
    public static class PvP
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            if (ctx.Args.Length == 0)
            {
                Database.PvPStats.TryGetValue(SteamID, out var PvPStats);

                ctx.Reply($"Name: {Color.White(CharName)}");
                if (PvPSystem.isHonorSystemEnabled)
                {
                    Database.SiegeState.TryGetValue(SteamID, out var siegeState);
                    Cache.HostilityState.TryGetValue(charEntity, out var hostilityState);

                    double tLeft = 0;
                    if (siegeState.IsSiegeOn)
                    {
                        TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                        tLeft = Math.Round(TimeLeft.TotalHours, 2);
                        if (PvPStats.Reputation <= -20000)
                        {
                            tLeft = -1;
                        }
                    }

                    string hostilityText = hostilityState.IsHostile ? "Aggresive" : "Passive";
                    string siegeText = siegeState.IsSiegeOn ? "Sieging" : "Defensive";

                    Cache.ReputationLog.TryGetValue(SteamID, out var RepLog);
                    TimeSpan ReputationSpan = DateTime.Now - RepLog.TimeStamp;

                    var TimeLeftUntilRefresh = PvPSystem.HonorGainSpanLimit - ReputationSpan.TotalMinutes;
                    if (TimeLeftUntilRefresh > 0)
                    {
                        TimeLeftUntilRefresh = Math.Round(TimeLeftUntilRefresh, 2);
                    }
                    else
                    {
                        TimeLeftUntilRefresh = 0;
                        RepLog.TotalGained = 0;
                    }
                    
                    int HonorGainLeft = PvPSystem.MaxHonorGainPerSpan - RepLog.TotalGained;

                    ctx.Reply($"Reputation: {Color.White(PvPStats.Reputation.ToString())}");
                    ctx.Reply($"-- Time Left Until Refresh: {Color.White(TimeLeftUntilRefresh.ToString())} minute(s)");
                    ctx.Reply($"-- Available Reputation Gain: {Color.White(HonorGainLeft.ToString())} point(s)");
                    ctx.Reply($"Hostility: {Color.White(hostilityText)}");
                    ctx.Reply($"Siege: {Color.White(siegeText)}");
                    ctx.Reply($"-- Time Left: {Color.White(tLeft.ToString())} hour(s)");
                }
                ctx.Reply($"K/D: {Color.White(PvPStats.KD.ToString())} [{Color.White(PvPStats.Kills.ToString())}/{Color.White(PvPStats.Deaths.ToString())}]");
            }

            if (ctx.Args.Length > 0)
            {
                var isPvPShieldON = false;

                if (ctx.Args[0].ToLower().Equals("on")) isPvPShieldON = false;
                else if (ctx.Args[0].ToLower().Equals("off")) isPvPShieldON = true;

                if (ctx.Args.Length == 1)
                {
                    if (ctx.Args[0].ToLower().Equals("top"))
                    {
                        if (PvPSystem.isLadderEnabled)
                        {
                            _ = PvPSystem.TopRanks(ctx);
                            return;
                        }
                        else
                        {
                            ctx.Reply("Leaderboard is not enabled.");
                            return;
                        }
                    }

                    if (PvPSystem.isHonorSystemEnabled)
                    {
                        if (Helper.IsPlayerInCombat(charEntity))
                        {
                            ctx.Reply($"Unable to change state, you are in combat!");
                            return;
                        }

                        Database.PvPStats.TryGetValue(SteamID, out var PvPStats);
                        Database.SiegeState.TryGetValue(SteamID, out var siegeState);

                        if (ctx.Args[0].ToLower().Equals("on"))
                        {
                            PvPSystem.HostileON(SteamID, charEntity, userEntity);
                            ctx.Reply("Entering aggresive state!");
                            return;
                        }
                        else if (ctx.Args[0].ToLower().Equals("off"))
                        {
                            if (PvPStats.Reputation <= -1000)
                            {
                                ctx.Reply($"You're [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], aggresive state is enforced.");
                                return;
                            }

                            if (siegeState.IsSiegeOn)
                            {
                                ctx.Reply($"You're in siege mode, aggressive state is enforced.");
                                return;
                            }
                            PvPSystem.HostileOFF(SteamID, charEntity);
                            ctx.Reply("Entering passive state!");
                            return;
                        }
                    }
                    else
                    {
                        if (!PvPSystem.isPvPToggleEnabled)
                        {
                            ctx.Reply("PvP toggling is not enabled!");
                            return;
                        }
                        if (Helper.IsPlayerInCombat(charEntity))
                        {
                            ctx.Reply($"Unable to change PvP Toggle, you are in combat!");
                            return;
                        }
                        Helper.SetPvPShield(charEntity, isPvPShieldON);
                        string s = isPvPShieldON ? "OFF" : "ON";
                        ctx.Reply($"PvP is now {s}");
                    }
                    return;
                }
                else if (ctx.Args.Length >= 2 && (ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "pvp_args")))
                {
                    if (ctx.Args[0].ToLower().Equals("rep") && PvPSystem.isHonorSystemEnabled)
                    {
                        if (int.TryParse(ctx.Args[1], out var value))
                        {
                            if (value > 9999) value = 9999;
                            string name = CharName;
                            if (ctx.Args.Length == 3)
                            {
                                name = ctx.Args[2];
                                if (Helper.FindPlayer(name, false, out _, out var targetUser))
                                {
                                    SteamID = Plugin.Server.EntityManager.GetComponentData<User>(targetUser).PlatformId;
                                }
                                else
                                {
                                    ctx.Reply($"Unable to find the specified player!");
                                    return;
                                }
                            }
                            Database.PvPStats.TryGetValue(SteamID, out var PvPData);
                            PvPData.Reputation = value;
                            Database.PvPStats[SteamID] = PvPData;
                            ctx.Reply($"Player \"{name}\" reputation is now set to {value}");
                        }
                    }
                    else
                    {
                        if (PvPSystem.isHonorSystemEnabled)
                        {
                            string name = ctx.Args[1];
                            if (Helper.FindPlayer(name, false, out Entity targetChar, out Entity targetUser))
                            {
                                SteamID = Plugin.Server.EntityManager.GetComponentData<User>(targetUser).PlatformId;
                                Database.PvPStats.TryGetValue(SteamID, out var PvPStats);
                                if (ctx.Args[0].ToLower().Equals("on"))
                                {
                                    PvPSystem.HostileON(SteamID, targetChar, targetUser);
                                    ctx.Reply($"Vampire \"{name}\" is now in aggresive state!");
                                    return;
                                }
                                else if (ctx.Args[0].ToLower().Equals("off"))
                                {
                                    if (PvPStats.Reputation <= -1000)
                                    {
                                        ctx.Reply($"Vampire \"{name}\" is [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], aggresive state is enforced.");
                                        return;
                                    }
                                    PvPSystem.HostileOFF(SteamID, targetChar);
                                    ctx.Reply($"Vampire \"{name}\" is now in passive state!");
                                    return;
                                }
                                return;
                            }
                            else
                            {
                                ctx.Reply($"Unable to find the specified player!");
                                return;
                            }
                        }
                        else
                        {
                            string name = ctx.Args[1];
                            if (Helper.FindPlayer(name, false, out Entity targetChar, out _))
                            {
                                Helper.SetPvPShield(targetChar, isPvPShieldON);
                                string s = isPvPShieldON ? "OFF" : "ON";
                                ctx.Reply($"Player \"{name}\" PvP is now {s}");
                                return;
                            }
                            else
                            {
                                ctx.Reply($"Unable to find the specified player!");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Output.InvalidArguments(ctx);
                    return;
                }
            }
        }
    }*/
}
