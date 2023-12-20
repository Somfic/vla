using System.Collections.Immutable;
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
	public ImmutableDictionary<string, object> Values { get; private set; } = ImmutableDictionary<string, object>.Empty;
	
	private ImmutableDictionary<string, object> _instances = ImmutableDictionary<string, object>.Empty;
	
	public void Tick()
	{
		foreach (var instance in Instances)
		{
			ExecuteNode(instance);
		}
	}

	private void ExecuteNode(NodeInstance instance)
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

		var inputParameters = structure.Inputs.Select<InputParameterStructure, dynamic?>(x => GetValue(instance, x)).ToArray();
		var outputParameters = structure.Outputs.Select<OutputParameterStructure, dynamic?>(x => null).ToArray();
		
		var parameters = inputParameters.Concat(outputParameters).ToArray();

		try
		{
			invokeMethod.Invoke(nodeInstance, parameters);
		} catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
		
		for (var i = inputParameters.Length; i < parameters.Length; i++)
		{
			var parameter = structure.Outputs[i - inputParameters.Length];
			SetValue(instance, parameter, parameters[i]);
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
		if (Values.ContainsKey(id))
			Values = Values.SetItem(id, value);
		else
			Values = Values.Add(id, value);
	}
	
	private object? GetValue(NodeInstance instance, InputParameterStructure parameter)
	{
		var id = $"{instance.Id}.{parameter.Id}";
		var defaultValue = parameter.Type.GetDefaultValueForType();
		return Values.TryGetValue(id, out var value) ? Convert.ChangeType(value, parameter.Type) : defaultValue;
	}
}