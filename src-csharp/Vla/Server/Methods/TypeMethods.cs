using System.Collections.Immutable;
using Vla.Server.Messages;
using Vla.Server.Messages.Requests;
using Vla.Server.Messages.Response;
using Vla.Services;

namespace Vla.Server.Methods;

[ServerMethods("type")]
public class TypeMethods : IServerMethods
{
	private readonly TypeService _types;

	public TypeMethods(TypeService types)
	{
		_types = types;
	}

	[ServerMethod("generate")]
	public ISocketResponse Generate(GenerateTypeRequest request)
	{
		return new TypeDefinitionResponse(_types.GenerateDefinition(request.Type));
	}
}
