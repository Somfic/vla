﻿using Newtonsoft.Json;

namespace Vla.Abstractions.Instance;

public readonly struct ParameterInstance(string id, object? value)
{
	[JsonProperty("id")]
	public string Id { get; init; } = id;

	[JsonProperty("value")]
	public object? Value { get; init; } = value;
}