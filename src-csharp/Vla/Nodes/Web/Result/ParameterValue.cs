namespace Vla.Nodes.Web.Result;

public readonly struct ParameterValue
{
    public ParameterValue(string id, string value)
    {
        Id = id;
        Value = value;
    }

    public string Id { get; init; }
    
    public string Value { get; init; }
}