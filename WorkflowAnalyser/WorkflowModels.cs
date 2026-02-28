using System.Text.Json.Serialization;

namespace WorkflowAnalyser
{
    public sealed class Root
    {
        [JsonPropertyName("workflows")]
        public List<Workflow> Workflows { get; set; } = new();
    }

    public sealed class Workflow
    {
        public int Index { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? NextWorkflows { get; set; }
        public int FunctionIndex { get; set; }
        public string? FunctionParameters { get; set; }
        public string? Changed { get; set; }
    }

    public readonly record struct EdgeSpec(int From, int To, string? Condition);
}
