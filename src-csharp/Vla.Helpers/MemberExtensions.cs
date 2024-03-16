using System.Reflection;
using LoxSmoke.DocXml;

namespace Vla.Helpers;

public static class DocumentationExtensions
{
	private static readonly DocXmlReader Reader = new();

	public static string GetDocumentation(this Type type)
	{
		try
		{
			return Reader.GetTypeComments(type).Summary.Trim();
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public static string GetDocumentation(this ParameterInfo parameter)
	{
		try
		{
			return Reader.GetMethodComments(parameter.Member as MethodBase).Parameters
				.First(x => x.Name == parameter.Name).Text
				.Trim();
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public static string GetDocumentation(this PropertyInfo parameter)
	{
		try
		{
			return Reader.GetMemberComments(parameter).Summary
				.Trim();
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}
}