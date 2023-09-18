﻿using Fumo.Shared.Models;
using System.Collections.ObjectModel;

namespace Fumo.Shared.Interfaces.Command;

public interface IChatCommand
{
    /// <summary>
    /// Code that is executed when the command is ran
    /// </summary>
    /// <returns>
    /// A string that is outputted in chat
    /// </returns>
    public ValueTask<CommandResult> Execute(CancellationToken ct);

    /// <summary>
    /// Optional function that can generate extra data on the website
    /// <br />
    /// Supports markdown
    /// <br />
    /// Every entry in the list is by itself a line and is automatically split by newlines
    /// </summary>
    /// <returns>
    /// A list of data.
    /// </returns>
    public ValueTask<List<string>> GenerateWebsiteDescription(string prefix, CancellationToken ct);
}
