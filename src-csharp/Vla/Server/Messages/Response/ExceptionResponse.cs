using Newtonsoft.Json;

namespace Vla.Server.Messages.Response;

public readonly struct ExceptionResponse(Exception exception) : ISocketResponse
{
	[JsonProperty("exception")]
	public Exception Exception { get; init; } = exception;
}