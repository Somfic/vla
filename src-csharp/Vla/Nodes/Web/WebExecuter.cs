using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;

namespace Vla.Nodes.Web;

public class WebExecutor
{
    private readonly Dictionary<string, object> _instances = new();
    private readonly Dictionary<string, object> _values = new();

    public void ExecuteWeb(Web web)
    {
        foreach (var instance in web.Instances)
        {
            var structure = web.Structures.First(s => s.NodeType == instance.NodeType);

            var nodeInstance = Activator.CreateInstance(structure.NodeType)!;

            if (structure.Properties.Any())
            {
                foreach (var property in structure.Properties)
                {
                    var propInfo = structure.NodeType.GetProperty(property.Name);
                    var propType = propInfo?.PropertyType;
                    var castedDefaultValue = JsonConvert.DeserializeObject(property.DefaultValue, propType);
                    propInfo?.SetValue(nodeInstance, castedDefaultValue);
                }

                foreach (var property in instance.Properties)
                {
                    var propInfo = structure.NodeType.GetProperty(property.Name);
                    var propType = propInfo?.PropertyType;
                    var castedValue = JsonConvert.DeserializeObject(property.Value, propType);
                    propInfo?.SetValue(nodeInstance, castedValue);
                }
            }
            
            _instances.Add(instance.Id, nodeInstance);
        }

        foreach (var connection in web.Connections)
        {
            SetNodeOutput(web, connection.To.InstanceId);
        }
    }

    private void SetNodeOutput(Web web, string instanceId)
    {
        var instance = web.Instances.First(i => i.Id == instanceId);
        var structure = web.Structures.First(s => s.NodeType == instance.NodeType);
        
        if (structure.Inputs.Any())
        {
            if (!structure.Inputs.All(output =>
                    _values.ContainsKey($"{instance.Id}.{output.Id}")))
            {
                foreach (var input in structure.Inputs)
                {
                    var connection = web.Connections.FirstOrDefault(c =>
                        c.To.InstanceId == instanceId && c.To.PropertyId == input.Id);
                    SetNodeOutput(web, connection.From.InstanceId);
                }
            }
        }

        // Execute the node method if all inputs are set
        if (structure.Inputs.All(input =>
            web.Connections.Any(c => c.To.InstanceId == instanceId && c.To.PropertyId == input.Id)))
        {
            var method = structure.NodeType.GetMethod(structure.ExecuteMethod);
            
            var inputParameters = structure.Inputs.Select<ParameterStructure, dynamic>(i => _values[$"{instance.Id}.{i.Id}"]).ToArray();
            var outputParameters = structure.Outputs.Select<ParameterStructure, dynamic>(_ => null).ToArray();
            var methodParameters = inputParameters.Concat(outputParameters).ToArray();

            method?.Invoke(_instances[instanceId], methodParameters);

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
                var connections = web.Connections.Where(c => c.From.InstanceId == instanceId && c.From.PropertyId == o.Id).Distinct();
                foreach (var connection in connections)
                {
                    var key = $"{connection.To.InstanceId}.{connection.To.PropertyId}";
                    var value = methodParameters[inputParameters.Length + index];
                    _values.TryAdd(key, value);
                }
            }
        }
    }
}