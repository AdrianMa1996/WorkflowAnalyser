using System.IO;
using System.Text.Json;

namespace WorkflowAnalyser
{
    public static class FunctionIndexDocs
    {
        private static Dictionary<int, FunctionDocEntry> _map = new();

        public static void Load(string path)
        {
            if (!File.Exists(path))
                throw new Exception("FunctionIndexDescriptions.json nicht gefunden.");

            var json = File.ReadAllText(path);

            var root = JsonSerializer.Deserialize<FunctionDocRoot>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new FunctionDocRoot();

            _map = root.Functions.ToDictionary(f => f.FunctionIndex, f => f);
        }

        public static FunctionDocEntry? TryGet(int functionIndex)
            => _map.TryGetValue(functionIndex, out var entry)
                ? entry
                : null;
    }
}
