using Somfic.Common;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;

namespace Vla.Nodes.Web;

public static class WebExtensions
{
    public static Web WithInstances(this Web web, params NodeInstance[] instances)
    {
        return web with { Instances = instances };
    }

    public static Web WithConnections(this Web web, params NodeConnection[] connections)
    {
        return web with { Connections = connections };
    }

    public static Result<Web> Validate(this Web web, IReadOnlyCollection<NodeStructure> structures)
    {
        return Somfic.Common.Result.Value(web)
            .Guard(StructureEnsureUniqueNodeTypes, "All structures must only be registered once")
            .Guard(StructureEnsureUniqueInputIds, "All inputs must have a unique id")
            .Guard(StructureEnsureUniqueOutputIds, "All outputs must have a unique id")
            .Guard(InstancesEnsureStructureExists, "All instances must have a registered structure")
            .Guard(InstancesEnsureStructurePropertyExists,"All instance properties must have a registered structure property")
            .Guard(InstancesEnsureStructurePropertyTypeMatches, "All instance properties must have a matching structure property type")
            .Guard(ConnectionsEnsureInstanceExists, "All connections must have a registered instance")
            .Guard(ConnectionsEnsureStructureInputOutputExists, "All connections must have a registered structure input/output");

        // FIXME: This check does not work. When using the .WithProperty extension method, the type is not promised to be the same as the structure property type.
        //  This check *should* fail if that is the case, but it does not.
        bool InstancesEnsureStructurePropertyTypeMatches(Web w) =>
            w.Instances.SelectMany(x => x.Properties).All(property =>
                structures.Any(structure => structure.Properties.All(x => x.Name == property.Name && x.Type == property.Type)));
        
        // TODO: Add check that all connection properties exist on the structure
        
        bool ConnectionsEnsureStructureInputOutputExists(Web w) =>
            w.Connections.All(connection =>
                structures.Any(structure => structure.Outputs.Any(x => x.Id == connection.From.PropertyId)) &&
                structures.Any(structure => structure.Inputs.Any(x => x.Id == connection.To.PropertyId)));

        bool ConnectionsEnsureInstanceExists(Web w) =>
            w.Connections.All(connection =>
                w.Instances.Any(instance => instance.Id == connection.From.InstanceId) &&
                w.Instances.Any(instance => instance.Id == connection.To.InstanceId));

        bool InstancesEnsureStructureExists(Web w) =>
            w.Instances.All(instance => structures.Any(structure => structure.NodeType == instance.NodeType));

        bool InstancesEnsureStructurePropertyExists(Web w) =>
            w.Instances.SelectMany(x => x.Properties).All(property =>
                structures.Any(structure => structure.Properties.Any(x => x.Name == property.Name)));

        bool StructureEnsureUniqueNodeTypes(Web w) =>
            structures
                .Select(x => x.NodeType)
                .Distinct()
                .Count() == structures.Count;

        bool StructureEnsureUniqueInputIds(Web w) =>
            structures.All(structure =>
                structure.Inputs
                    .Select(x => x.Id)
                    .Distinct()
                    .Count() == structure.Inputs.Length);

        bool StructureEnsureUniqueOutputIds(Web w) =>
            structures.All(structure =>
                structure.Outputs
                    .Select(x => x.Id)
                    .Distinct()
                    .Count() == structure.Outputs.Length);
    }
}