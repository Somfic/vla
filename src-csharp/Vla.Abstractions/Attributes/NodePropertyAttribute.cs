﻿namespace Vla.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class NodePropertyAttribute(string? name = null) : Attribute
{
	public string? Name { get; init; } = name;
}