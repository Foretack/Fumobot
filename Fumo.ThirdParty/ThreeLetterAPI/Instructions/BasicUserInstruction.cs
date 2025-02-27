﻿using Fumo.ThirdParty.GraphQL;

namespace Fumo.ThirdParty.ThreeLetterAPI.Instructions;


/// <summary>
/// id -> String
/// login -> String
/// </summary>
public class BasicUserInstruction : IGraphQLInstruction
{
    public string? Id { get; }
    public string? Login { get; }

    public BasicUserInstruction(string? id = null, string? login = null)
    {
        Id = id;
        Login = login;
    }

    public GraphQLRequest Create()
    {
        return new()
        {
            Query = @"query($id: ID, $login: String){user(id: $id, login: $login, lookupType: ALL) {id login}}",
            Variables = new
            {
                id = Id,
                login = Login
            }
        };
    }
}
