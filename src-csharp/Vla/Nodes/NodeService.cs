using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Vla.Abstractions;
using Vla.Addon;

namespace Vla.Nodes;

public class NodeService
{
    private readonly ILogger<NodeService> _log;
    private readonly IServiceProvider _services;
    
    private ImmutableDictionary<Type, NodeStructure> _nodeStructures = ImmutableDictionary<Type, NodeStructure>.Empty;
    private ImmutableDictionary<Type, TypeDefinition> _typeDefinitions = ImmutableDictionary<Type, TypeDefinition>.Empty;

    public NodeService(ILogger<NodeService> log, IServiceProvider services)
    {
        _log = log;
        _services = services;
    }

    public (ImmutableArray<NodeStructure> structures, ImmutableArray<TypeDefinition> typeDefinitions) Generate()
    {
        var structures = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsAssignableTo(typeof(Node)))
            .Select(GenerateStructureFromType)
            .ToImmutableArray();

        var definitions = structures
            .SelectMany(x => x.Properties)
            .Select(x => x.Type)
            .Distinct()
            .Select(GenerateTypeDefinitionFromType)
            .ToImmutableArray();
        
        return (structures, definitions);
    }

    private NodeStructure GenerateStructureFromType(Type type)
    {
        // Return the cached structure if it exists
        if (_nodeStructures.TryGetValue(type, out var structure))
            return structure;
        
        return new NodeStructure
        {
            Name = type.Name.EndsWith("Node") ? type.Name[..^4] : type.Name,
            Category = GetCategory(type),
            Description = GetDescription(type),
            SearchTerms = GetSearchTerms(type),
            Purity = GetPurity(type),
            Properties = GetProperties(type),
        };
    }
    
    private TypeDefinition GenerateTypeDefinitionFromType(Type type)
    {
        // Return the cached definition if it exists
        if (_typeDefinitions.TryGetValue(type, out var definition))
            return definition;
        
        return new TypeDefinition
        {
            Type = type,
            Name = type.Name,
            Description = GetDescription(type),
            PossibleValues = GetPossibleValues(type),
        };
    }
    
    private string? GetCategory(Type type)
    {
        return type.GetCustomAttributes(true)
            .Where(x => x.GetType() == typeof(NodeCategoryAttribute))
            .Select(x => x as NodeCategoryAttribute)
            .Select(x => x!.Name)
            .FirstOrDefault();
    }
    
    private string? GetDescription(Type type)
    {
        // TODO: Add support for getting the description from the XML documentation
        return null;
    }
    
    private ImmutableArray<string> GetSearchTerms(Type type)
    {
        var tags = type.GetCustomAttributes(true)
            .Where(x => x.GetType() == typeof(NodeTagsAttribute))
            .Select(x => x as NodeTagsAttribute)
            .SelectMany(x => x!.Tags)
            .ToImmutableArray();
        
        var categoryValues = type.GetCustomAttributes(true)
            .Where(x => x.GetType() == typeof(NodeCategoryAttribute))
            .Select(x => x as NodeCategoryAttribute)
            .Select(x => x!.Name)
            .ToImmutableArray();
        
        var propertyPossibleValues = type.GetProperties()
            .Where(x => x.GetCustomAttributes(true).Any(x => x.GetType() == typeof(NodePropertyAttribute)))
            .Select(x => x.PropertyType)
            .Select(GenerateTypeDefinitionFromType)
            .SelectMany(x => x.PossibleValues)
            .Select(x => x.Label)
            .ToImmutableArray();
        
        return tags.AddRange(categoryValues).AddRange(propertyPossibleValues);
    }

    private ImmutableArray<NodeStructure.Property> GetProperties(Type type)
    {
        // Get all the properties with the NodeProperty attribute
        return type.GetProperties()
            .Where(x => x.GetCustomAttributes(true).Any(x => x.GetType() == typeof(NodePropertyAttribute)))
            .Select(x => new NodeStructure.Property
            {
                Name = x.Name,
                Type = x.PropertyType,
                Description = null
            })
            .ToImmutableArray();
    }
    
    private NodePurity GetPurity(ICustomAttributeProvider type)
    {
        return type.GetCustomAttributes(true)
            .Where(x => x.GetType() == typeof(NodeAttribute))
            .Select(x => x as NodeAttribute)
            .Select(x => x!.Purity)
            .FirstOrDefault();
    }

    private ImmutableArray<TypeDefinition.PossibleValue> GetPossibleValues(Type type)
    {
        if (type.IsEnum)
            return Enum.GetNames(type)
                .Select(x => (value: x, label: EnumExtensions.GetValueNameFromEnum(type, x)))
                .Select(x => new TypeDefinition.PossibleValue(x.label, Enum.Parse(type, x.value)))
                .ToImmutableArray();
        
        return ImmutableArray<TypeDefinition.PossibleValue>.Empty;
    } 
}