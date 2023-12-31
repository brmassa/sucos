using System.Collections.Generic;
using Microsoft.Extensions.FileSystemGlobbing;

namespace SuCoS.Models;

/// <summary>
/// Basic structure needed to generate user content and system content
/// </summary>
public class FrontMatterResources : IFrontMatterResources
{
    /// <inheritdoc/>
    public string Src { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string? Title { get; set; }

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public Dictionary<string, object> Params { get; set; } = new();

    /// <inheritdoc/>
    public Matcher? GlobMatcher { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public FrontMatterResources() { }
}
