﻿using Fumo.ThirdParty.Emotes.SevenTV;
using System.Text.Json.Serialization;

namespace Fumo.ThirdParty.Emotes.SevenTV;

public record SevenTVConnection(
[property: JsonPropertyName("id")] string Id,
[property: JsonPropertyName("platform")] string Platform,
[property: JsonPropertyName("emote_set_id")] string EmoteSetId
);

public static class IReadOnlyListSevenTVConnectionsExtensions
{
    public static SevenTVConnection GetTwitchConnection(this IReadOnlyList<SevenTVConnection> list)
        => list.FirstOrDefault(c => c.Platform == "TWITCH") ?? default!;
}