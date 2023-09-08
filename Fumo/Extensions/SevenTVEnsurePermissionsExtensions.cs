﻿using Fumo.Database;
using Fumo.Database.DTO;
using Fumo.Database.Extensions;
using Fumo.Exceptions;
using Fumo.ThirdParty.Emotes.SevenTV;
using StackExchange.Redis;
using System.Threading.Channels;

namespace Fumo.Extensions;

// TODO: Im not entirely sure if this is the best way of doing this, just thinking of that the Fumo.ThirdParty library shouldn't know of redis and whatnot...
internal static class SevenTVEnsurePermissionsExtensions
{
    public static string EditorKey(this ISevenTVService _, string channelID) => $"channel:{channelID}:seventv:editors";

    /// <summary>
    /// Ensures the current user is allowed to change emotes in the channel
    /// </summary>
    public static async Task<(string EmoteSet, string UserID)> EnsureCanModify(this ISevenTVService service, string botID, IDatabase redis, ChannelDTO channel, UserDTO invoker)
    {
        var currentEmoteSet = channel.GetSetting(ChannelSettingKey.SevenTV_EmoteSet)
            ?? throw new InvalidInputException("The channel is missing an emote set");

        var sevenTVId = channel.GetSetting(ChannelSettingKey.SevenTV_UserID)
            ?? throw new InvalidInputException("The channel is missing a 7TV user ID");

        // Is broadcaster
        if (channel.TwitchID == invoker.TwitchID)
        {
            return (currentEmoteSet, sevenTVId);
        }

        var botIsMember = await redis.SetContainsAsync(service.EditorKey(channel.TwitchID), botID);
        var invokerIsMember = await redis.SetContainsAsync(service.EditorKey(channel.TwitchID), invoker.TwitchID);

        // StackExchange.Redis.RedisServerException: ERR unknown command `SMISMEMBER`

        //RedisValue[] redisValues = new[] { new RedisValue(botID), new RedisValue(invoker.TwitchID) };
        //var contains = await redis.SetContainsAsync(service.EditorKey(channel.TwitchID), redisValues);

        if (botIsMember == false)
        {
            throw new InvalidInputException("I am not an editor in this channel");
        }

        if (invokerIsMember == false)
        {
            throw new InvalidInputException("You're not an editor in this channel");
        }

        return (currentEmoteSet, sevenTVId);
    }
}
