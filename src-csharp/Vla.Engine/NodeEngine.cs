using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Vla.Helpers;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;

namespace Vla.Engine;

public class NodeEngine
{
	private readonly IServiceProvider _serviceProvider;

	public NodeEngine(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}
	
	public NodeEngine SetStructures(params NodeStructure[] structures)
	{
		Structures = structures.ToImmutableArray();
		return this;
	}
	
	public NodeEngine SetGraph(ImmutableArray<NodeInstance> instances,
		ImmutableArray<NodeConnection> connections)
	{
		var sorter = new TopologicalSorter(connections.Select(x => (x.Source.InstanceId, x.Target.InstanceId)).ToArray());
		Instances = sorter.Sort().Select(x => instances.First(y => y.Id == x.value)).ToImmutableArray();

		Connections = connections;
		
		return this;
	}

	public ImmutableArray<NodeStructure> Structures { get; private set; } = ImmutableArray<NodeStructure>.Empty;
	public ImmutableArray<NodeConnection> Connections { get; private set; } = ImmutableArray<NodeConnection>.Empty;
	public ImmutableArray<NodeInstance> Instances { get; private set; } = ImmutableArray<NodeInstance>.Empty;

	public ImmutableDictionary<string, object> Values => ExplicitValues
		.Concat(ImplicitValues.Where(x => !ExplicitValues.ContainsKey(x.Key)))
		.ToImmutableDictionary(x => x.Key, x => x.Value);
	
	public ImmutableDictionary<string, object> ImplicitValues { get; private set; } = ImmutableDictionary<string, object>.Empty;
	public ImmutableDictionary<string, object> ExplicitValues { get; private set; } = ImmutableDictionary<string, object>.Empty;

	
	private ImmutableDictionary<string, object> _instances = ImmutableDictionary<string, object>.Empty;
	
	public ImmutableDictionary<string, NodeExecutionResult> Tick()
	{
		var results = new Dictionary<string, NodeExecutionResult>();
		
		foreach (var instance in Instances)
		{
			var result = ExecuteNode(instance);
			results.Add(instance.Id, result);
		}

		return results.ToImmutableDictionary();
	}

	private NodeExecutionResult ExecuteNode(NodeInstance instance)
	{
		var structure = Structures.First(x => x.NodeType == instance.NodeType);
		
		if (!_instances.ContainsKey(instance.Id))
			_instances = _instances.Add(instance.Id, ActivatorUtilities.CreateInstance(_serviceProvider, instance.NodeType));
		
		var nodeInstance = _instances[instance.Id];
		
		// Properties
		foreach (var property in structure.Properties)
		{
			var propInfo = structure.NodeType.GetProperty(property.Name);
			var propType = propInfo?.PropertyType;
                    
			if(propType == null)
				continue;
                    
			var castedDefaultValue = JsonConvert.DeserializeObject(property.DefaultValue, propType);
			propInfo?.SetValue(nodeInstance, castedDefaultValue);
		}

		foreach (var property in instance.Properties)
		{
			var propInfo = structure.NodeType.GetProperty(property.Id);
			var propType = propInfo?.PropertyType;
                    
			if(propType == null)
				continue;
                    
			var castedValue = JsonConvert.DeserializeObject(property.Value, propType);
			propInfo?.SetValue(nodeInstance, castedValue);
		}
		
		// Invocation
		var invokeMethod = structure.NodeType.GetMethod(structure.ExecuteMethod);
		
		if(invokeMethod == null)
			throw new ArgumentException($"Could not find execute method {structure.ExecuteMethod} on type {structure.NodeType.Name}");
		
		var parameters = ConstructInvocationParameters(invokeMethod, instance, structure);
		
		try
		{
			invokeMethod.Invoke(nodeInstance, parameters);
		
			for (var i = 0; i < parameters.Length; i++)
			{
				var parameter = GetParameterStructureFromMethod(invokeMethod, structure, i);

				if (parameter is OutputParameterStructure outputParameter)
				{
					SetValue(instance, outputParameter, parameters[i]);
				}
			}

			return new NodeExecutionResult();
		} 
		catch (Exception e)
		{
			for (var i = 0; i < parameters.Length; i++)
			{
				var parameter = GetParameterStructureFromMethod(invokeMethod, structure, i);

				if (parameter is OutputParameterStructure outputParameter)
				{
					var fallback = parameter.Type.GetDefaultValueForType()!;
					SetValue(instance, outputParameter, fallback);
				}
			}
			
			return new NodeExecutionResult(e.InnerException);
		}
	}

	private void SetValue(NodeInstance instance, OutputParameterStructure parameter, object value)
	{
		var outputId = $"{instance.Id}.{parameter.Id}";
		SetValue(outputId, value);

		// Set the casted value on all the connected inputs
		var connections = Connections.Where(x => x.Source.InstanceId == instance.Id && x.Source.PropertyId == parameter.Id).ToArray();
		
		foreach (var connection in connections)
		{
			var targetInstance = Instances.First(x => x.Id == connection.Target.InstanceId);
			var targetParameter = Structures.First(x => x.NodeType == targetInstance.NodeType).Inputs.First(x => x.Id == connection.Target.PropertyId);
			
			var inputId = $"{targetInstance.Id}.{targetParameter.Id}";
			var castedValue = Convert.ChangeType(value, targetParameter.Type);
			SetValue(inputId, castedValue);
		}
	}

	private void SetValue(string id, object value)
	{
		if (ExplicitValues.ContainsKey(id))
			ExplicitValues = ExplicitValues.SetItem(id, value);
		else
			ExplicitValues = ExplicitValues.Add(id, value);
	}
	
	private object GetValue(NodeInstance instance, InputParameterStructure parameter)
	{
		var id = $"{instance.Id}.{parameter.Id}";
		
		if(ExplicitValues.TryGetValue(id, out var value))
			return value;

		return GetImplicitValue(id, instance, parameter);
	}

	private object GetImplicitValue(string id, NodeInstance instance, InputParameterStructure parameter)
	{
		// Default values might be changed by the user while the graph is running
		// if (ImplicitValues.TryGetValue(id, out var value))
		// 	return value;
		var instanceParameter = GetParameterInstanceFromMethod(instance, parameter);

		var defaultValue = JsonConvert.DeserializeObject(parameter.DefaultValue, parameter.Type);

		if (instanceParameter?.Id == parameter.Id)
			defaultValue = JsonConvert.DeserializeObject(instanceParameter.Value.DefaultValue, parameter.Type);

		if (ImplicitValues.ContainsKey(id))
			ImplicitValues = ImplicitValues.SetItem(id, defaultValue);
		else
			ImplicitValues = ImplicitValues.Add(id, defaultValue);

		return defaultValue;
	}

	private dynamic[] ConstructInvocationParameters(MethodBase method, NodeInstance instance, NodeStructure structure)
	{
		var methodParameters = method.GetParameters();
		var parameters = new dynamic[methodParameters.Length];
		
		for (var i = 0; i < methodParameters.Length; i++)
		{
			var structureParameter = GetParameterStructureFromMethod(method, structure, i);
			
			dynamic value = null;
			
			// If the parameter is an input, get the default value from the structure/instance
			if (structureParameter is InputParameterStructure inputParameter)
				value = GetValue(instance, inputParameter);
			
			parameters[i] = value;
		}

		return parameters;
	}

	private IParameterStructure GetParameterStructureFromMethod(MethodBase method, NodeStructure structure, int parameterIndex)
	{
		var structureParameters = structure
			.Inputs
			.Select(x => x as IParameterStructure)
			.Concat(structure.Outputs
				.Select(x => x as IParameterStructure));
		
		var parameter = method.GetParameters()[parameterIndex];
		
		return structureParameters.First(x => x.Id == parameter.Name);
	}
	
	private ParameterInstance? GetParameterInstanceFromMethod(NodeInstance instance, IParameterStructure parameterStructure)
	{
		return instance.Inputs.FirstOrDefault(x => x.Id == parameterStructure.Id);
	}
}