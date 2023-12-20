using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vla.Abstractions.Web;
using Vla.Helpers;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
using Vla.Web.Result;

namespace Vla.Web;

public class WebExecutor(ILogger<WebExecutor> log, IServiceProvider services)
{
    private readonly ILogger<WebExecutor> _log = log;

    private readonly Dictionary<string, object?> _instances = new();
    private readonly Dictionary<string, object?> _values = new();

    public WebResult ExecuteWeb(Abstractions.Web.Web web, IReadOnlyCollection<NodeStructure> structures)
    {
        Console.WriteLine(JsonConvert.SerializeObject(web, Formatting.Indented));

        foreach (var instance in web.Instances)
        {
            if (structures.All(s => s.NodeType != instance.NodeType))
                throw new Exception($"Could not find structure for node {instance.NodeType.Name}");

            var structure = structures.First(s => s.NodeType == instance.NodeType);

            var nodeInstance = ActivatorUtilities.CreateInstance(services, structure.NodeType);

            if (structure.Properties.Any())
            {
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
            }

            _instances.Add(instance.Id, nodeInstance);
        }

        foreach (var connection in web.Connections)
        {
            SetNodeOutput(web, structures, connection.Target.InstanceId);
        }

        return new WebResult()
            .WithValues(_values)
            .WithInstances(_instances);
    }

    private void SetNodeOutput(Abstractions.Web.Web web, IReadOnlyCollection<NodeStructure> structures, string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            return;

        if (web.Instances.All(i => i.Id != instanceId))
            throw new Exception($"Could not find instance with id {instanceId}");

        if (structures.All(s => s.NodeType != web.Instances.First(i => i.Id == instanceId).NodeType))
            throw new Exception(
                $"Could not find structure for node {web.Instances.First(i => i.Id == instanceId).NodeType.Name}");

        var instance = web.Instances.First(i => i.Id == instanceId);
        var structure = structures.First(s => s.NodeType == instance.NodeType);

        if (structure.Inputs.Any())
        {
            // For all the inputs that do not have a value yet, set the value
            foreach (var input in structure.Inputs.Where(x => !_values.ContainsKey($"{instance.Id}.{x.Id}")))
            {
                var hasConnection =
                    web.Connections.Any(c => c.Target.InstanceId == instanceId && c.Target.PropertyId == input.Id);

                // If there is a connection, set the value of the input to the value of the output
                if (hasConnection)
                    SetNodeOutput(web, structures, web.Connections.First(c => c.Target.InstanceId == instanceId && c.Target.PropertyId == input.Id).Source.InstanceId);
                else
                {
                    // If there is no connection, set the value of the input to the default value
                    var value = instance.Inputs.First(x => x.Id == input.Id).Value;
                    _values.TryAdd($"{instance.Id}.{input.Id}", value);
                }
            }
        }


        var method = structure.NodeType.GetMethod(structure.ExecuteMethod);

        var inputParameters =
            structure.Inputs.Select<InputParameterStructure, dynamic?>(i => GetValue(instance, i)).ToArray();
        var outputParameters = structure.Outputs.Select<OutputParameterStructure, dynamic?>(_ => null).ToArray();
        var methodParameters = inputParameters.Concat(outputParameters).ToArray();

        try
        {
            method?.Invoke(_instances[instanceId], methodParameters);
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not execute node {instance.NodeType.Name}", ex);
        }

        for (var index = 0; index < structure.Outputs.Length; index++)
        {
            var o = structure.Outputs[index];

            // Set the output values to the _values dictionary
            {
                var key = $"{instance.Id}.{o.Id}";
                var value = methodParameters[inputParameters.Length + index];
                _values.TryAdd(key, value);
            }

            // Find all the inputs that use this output, and set their values
            var connections = web.Connections
                .Where(c => c.Source.InstanceId == instanceId && c.Source.PropertyId == o.Id).Distinct();
            foreach (var connection in connections)
            {
                var key = $"{connection.Target.InstanceId}.{connection.Target.PropertyId}";
                var value = methodParameters[inputParameters.Length + index];
                _values.TryAdd(key, value);
            }
        }
    }

    private object? GetValue(NodeInstance instance, InputParameterStructure parameter)
    {
        var id = $"{instance.Id}.{parameter.Id}";
        var defaultValue = parameter.Type.GetDefaultValueForType();
        return _values.TryGetValue(id, out var value) ? Convert.ChangeType(value, parameter.Type) : defaultValue;
    }
}

