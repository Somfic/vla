﻿using System.Drawing;
using Newtonsoft.Json;

namespace Vla.Nodes.Types;

public readonly struct NodeTypeDefinition
{
    public NodeTypeDefinition(Type type, string? name = "")
    {
        Type = type;
        Name = string.IsNullOrWhiteSpace(name) ? type.Name : name;
        Color = TypeToColor(type);

        if (Name.EndsWith('&'))
            Name = Name[..^1];

        Shape = type == typeof(NodeFlow) ? "diamond" : "circle";
    }

    [JsonProperty("type")]
    public Type Type { get; init; }

    [JsonProperty("name")]
    public string Name { get; init; }

    [JsonProperty("color")]
    public Color Color { get; init; }

    [JsonProperty("shape")]
    public string Shape { get; init; }

    private static Color TypeToColor(Type type)
    {
        var typeColors = new Dictionary<Type, Color>
        {
            { typeof(int), Color.FromArgb(139, 255, 109) },
            { typeof(double), Color.FromArgb(255, 223, 109) },
            { typeof(string), Color.FromArgb(109, 159, 255) },
            { typeof(bool), Color.FromArgb(255, 109, 109) },
            { typeof(NodeFlow), Color.FromArgb(255, 255, 255) },
        };

        return typeColors.TryGetValue(type, out var color) ? color : Color.FromArgb(217, 109, 255);
    }
}