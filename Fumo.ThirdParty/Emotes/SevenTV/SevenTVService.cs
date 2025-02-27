﻿using Fumo.ThirdParty.Emotes.SevenTV.Enums;
using Fumo.ThirdParty.Emotes.SevenTV.Models;
using Fumo.ThirdParty.GraphQL;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fumo.ThirdParty.Emotes.SevenTV;

public class SevenTVService : AbstractGraphQLClient, ISevenTVService
{
    public SevenTVService(IConfiguration config)
        : base("https://7tv.io/v3/gql")
    {
        var token = config["SevenTV:Bearer"];

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Bearer token is missing from configuration", nameof(config));

        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    public async Task<SevenTVRoles> GetGlobalRoles(CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"query GetRoles{roles {name id}}"
        };

        return await SendAsync<SevenTVRoles>(request, ct);
    }

    public async Task<SevenTVUser> GetUserInfo(string twitchID, CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"query GetUserByConnection($platform: ConnectionPlatform! $id: String!) {userByConnection (platform: $platform, id: $id) {id type username roles created_at connections{id platform emote_set_id}emote_sets{id emotes{id} capacity}}}",
            Variables = new
            {
                platform = "TWITCH",
                id = twitchID
            }
        };

        return (await SendAsync<OuterSevenTVUser>(request, ct)).UserByConnection;
    }

    public async Task<SevenTVEditorEmoteSets> GetEditorEmoteSetsOfUser(string twitchID, CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"query GetUserByConnection($platform: ConnectionPlatform!, $id: String!) {userByConnection(platform: $platform, id: $id){id username connections{id platform emote_set_id}editor_of{id user{username connections{id platform emote_set_id}}}}}",
            Variables = new
            {
                platform = "TWITCH",
                id = twitchID
            }
        };

        return (await SendAsync<SevenTVEditorEmoteSetsRoot>(request, ct)).UserByConnection;
    }

    public async Task<SevenTVEditors> GetEditors(string twitchID, CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"query GetUserByConnection($platform: ConnectionPlatform!, $id: String!) {userByConnection(platform: $platform, id: $id) {id username editors{id user{username connections{platform id emote_set_id}}}}}",
            Variables = new
            {
                platform = "TWITCH",
                id = twitchID
            }
        };

        return (await SendAsync<SevenTVEditorsRoot>(request, ct)).UserByConnection;
    }

    public async Task<SevenTVBasicEmote> SearchEmoteByID(string Id, CancellationToken ct)
    {
        GraphQLRequest request = new()
        {
            Query = @"query SearchEmote($id: ObjectID!){emote(id: $id){id name}}",
            Variables = new
            {
                id = Id
            }
        };

        return (await SendAsync<EmoteRoot>(request, ct)).Emote;
    }

    public async Task<SevenTVEmoteByName> SearchEmotesByName(string name, bool exact = false, CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"query SearchEmotes($query: String! $page: Int $limit: Int $filter: EmoteSearchFilter) {emotes(query: $query page: $page limit: $limit filter: $filter){items{id name owner{username id}}}}",
            Variables = new
            {
                query = name,
                page = 1,
                limit = 100,
                filter = new
                {
                    exact_match = exact
                }
            }
        };

        return (await SendAsync<SevenTVEmoteByNameRoot>(request, ct)).Emotes;
    }

    public async Task<string?> ModifyEmoteSet(string emoteSet, ListItemAction action, string emoteID, string? name = null, CancellationToken ct = default)
    {
        var stringAction = action switch
        {
            ListItemAction.Add => "ADD",
            ListItemAction.Remove => "REMOVE",
            ListItemAction.Update => "UPDATE",
            _ => throw new NotImplementedException()
        };

        GraphQLRequest request = new()
        {
            Query = @"mutation ChangeEmoteInSet($id: ObjectID! $action: ListItemAction! $emote_id: ObjectID! $name: String) {emoteSet(id: $id){id emotes(id: $emote_id action: $action name: $name){id name}}}",
            Variables = new
            {
                id = emoteSet,
                action = stringAction,
                emote_id = emoteID,
                name,
            }
        };

        var response = await SendAsync<SevenTVModifyEmoteSetRoot>(request, ct);

        return response.EmoteSet.Emote.Where(x => x.Id == emoteID).FirstOrDefault()?.Name ?? default;
    }

    public async Task<List<SevenTVEnabledEmote>> GetEnabledEmotes(string emoteSet, CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"query GetEmoteSet($id: ObjectID!){emoteSet(id: $id){id name emotes{id name data{name}}}}",
            Variables = new
            {
                id = emoteSet
            }
        };

        var response = await SendAsync<JsonDocument>(request, ct);

        return response
            .RootElement
            .GetProperty("emoteSet")
            .GetProperty("emotes")
            .Deserialize<List<SevenTVEnabledEmote>>() ?? new();
    }

    public async Task ModifyEditorPermissions(string channelId, string userId, UserEditorPermissions permissions, CancellationToken ct = default)
    {
        GraphQLRequest request = new()
        {
            Query = @"mutation UpdateUserEditors($id: ObjectID! $editor_id: ObjectID! $d: UserEditorUpdate!){user(id: $id){editors(editor_id: $editor_id data: $d){id}}}",
            Variables = new
            {
                id = channelId,
                editor_id = userId,
                d = new
                {
                    permissions
                }
            }
        };

        await SendAsync<JsonDocument>(request, ct);
    }
}

file record EmoteRoot([property: JsonPropertyName("emote")] SevenTVBasicEmote Emote);

file record SevenTVEditorsRoot(
    [property: JsonPropertyName("userByConnection")] SevenTVEditors UserByConnection
);

file record SevenTVEditorEmoteSetsRoot(
    [property: JsonPropertyName("userByConnection")] SevenTVEditorEmoteSets UserByConnection
);

file record SevenTVEmoteByNameRoot(
    [property: JsonPropertyName("emotes")] SevenTVEmoteByName Emotes
);

file record SevenTVModifyEmoteSetRoot(
    [property: JsonPropertyName("emoteSet")] SevenTVModifyEmoteSet EmoteSet);

file record SevenTVModifyEmoteSet(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("emotes")] IReadOnlyList<SevenTVBasicEmote> Emote);
