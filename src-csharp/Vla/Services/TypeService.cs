using System.Collections.Immutable;
using Vla.Abstractions;

namespace Vla.Services;

public class TypeService
{
	public ImmutableArray<TypeDefinition> GenerateAddonDefinitions(Addon.Metadata.Addon addon)
	{
		var definitions = ImmutableArray.CreateBuilder<TypeDefinition>();
		
		foreach (var type in addon.)
			definitions.Add(GenerateDefinition(type));
		
		return definitions.ToImmutable();
	}
	
	public TypeDefinition GenerateDefinition(Type type)
	{
		// Numerical
		if (type == typeof(int) || type == typeof(float) || type == typeof(double))
		{
			// var range = member.GetCustomAttribute<RangeAttribute>();
			//
			// // If the range attribute is present, use a range input
			// if (range != null)
			// 	return new TypeDefinition(member.PropertyType, TypeDefinition.HtmlType.Range,
			// 		new TypeDefinition.LabeledValue(range.Minimum, range.Minimum.ToString() ?? "min"),
			// 		new TypeDefinition.LabeledValue(range.Maximum, range.Maximum.ToString() ?? "max"));
			
			// Otherwise, use a number input
			return new TypeDefinition(type, TypeDefinition.HtmlType.Number);
		}
		
		// Boolean
		if (type == typeof(bool))
			return new TypeDefinition(type, TypeDefinition.HtmlType.Checkbox);
		
		// Enum
		if (type.IsEnum)
		{
			var values = type.GetEnumValues();
			var labeledValues = new TypeDefinition.LabeledValue[values.Length];
			
			for (var i = 0; i < values.Length; i++)
				labeledValues[i] = new TypeDefinition.LabeledValue(values.GetValue(i)!, values.GetValue(i)!.ToString() ?? "ERROR");
			
			return new TypeDefinition(type, TypeDefinition.HtmlType.Select, labeledValues);
		}
		
		// Default to text
		return new TypeDefinition(type, TypeDefinition.HtmlType.Text);
	}
}