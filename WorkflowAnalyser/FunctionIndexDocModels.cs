using System.Text.Json.Serialization;

namespace WorkflowAnalyser
{
    public sealed class FunctionDocRoot
    {
        [JsonPropertyName("functions")]
        public List<FunctionDocEntry> Functions { get; set; } = new();
    }

    public sealed class FunctionDocEntry
    {
        [JsonPropertyName("functionIndex")]
        public int FunctionIndex { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";
    }
}
