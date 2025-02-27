﻿using Fumo.Database.DTO;
using Fumo.Shared.Enums;
using Fumo.Shared.Interfaces.Command;
using MiniTwitch.Irc.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Fumo.Shared.Models;

public abstract class ChatCommand : ChatCommandArguments, IChatCommand
{
    #region Properties

    public ChannelDTO Channel { get; set; }
    public UserDTO User { get; set; }
    public List<string> Input { get; set; }
    public Privmsg Privmsg { get; set; }

    /// <summary>
    /// The command invocation is the part of the message that matches the command name
    /// </summary>
    public string CommandInvocationName { get; set; }

    /// <summary>
    /// Regex that matches the command
    /// </summary>
    public Regex NameMatcher { get; protected set; }


    private ChatCommandFlags _flags = ChatCommandFlags.None;

    /// <summary>
    /// Flags that change behaviour
    /// </summary>
    public ChatCommandFlags Flags
    {
        get => _flags;
        protected set => _flags = value;
    }

    private List<string> _permissions = new() { "default" };

    /// <summary>
    /// Permissions a user requires to execute this command
    /// 
    /// Every user has "default"
    /// </summary>
    public List<string> Permissions
    {
        get => _permissions;
        protected set => _permissions = value;
    }

    private string? _description;
    public string Description
    {
        get => _description ?? "No description provided";
        protected set => _description = value;
    }

    /// <summary>
    /// If Moderators and Broadcasters are the only ones that can execute this command in a chat
    /// </summary>
    public bool ModeratorOnly => (Flags & ChatCommandFlags.ModeratorOnly) != 0;

    /// <summary>
    /// If the Broadcaster of the chat is the only one that can execute this command in a chat
    /// </summary>
    public bool BroadcasterOnly => (Flags & ChatCommandFlags.BroadcasterOnly) != 0;


    private TimeSpan _cooldown = TimeSpan.FromSeconds(5);
    public TimeSpan Cooldown
    {
        get => _cooldown;
        protected set => _cooldown = value;
    }

    #endregion

    public virtual ValueTask<CommandResult> Execute(CancellationToken ct)
        => throw new NotImplementedException();

    #region Setters

    protected void SetName([StringSyntax(StringSyntaxAttribute.Regex)] string regex)
        => this.NameMatcher = new($"^{regex}", RegexOptions.Compiled);

    protected void SetCooldown(TimeSpan cd)
        => this.Cooldown = cd;

    protected void SetFlags(ChatCommandFlags flags)
        => Flags = flags;

    protected void AddPermission(string permission)
        => this.Permissions.Add(permission);

    protected void SetDescription(string description)
        => this.Description = description;

    #endregion
}
