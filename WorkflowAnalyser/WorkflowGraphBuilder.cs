using Microsoft.Msagl.Drawing;
using System.IO;
using System.Text.Json;

namespace WorkflowAnalyser
{
    public static class WorkflowGraphBuilder
    {
        public static (Graph Graph, Dictionary<int, Workflow> ByIndex) BuildSubgraphFromJsonFile(
            string jsonPath,
            int startIndex,
            int maxDepth)
        {
            var json = File.ReadAllText(jsonPath);
            var root = JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("JSON konnte nicht gelesen werden.");

            var byIndex = root.Workflows.ToDictionary(w => w.Index, w => w);

            if (!byIndex.ContainsKey(startIndex))
                throw new Exception($"Start-Index {startIndex} existiert nicht in der Datei.");

            // adjacency: from -> outgoing edges
            var adj = new Dictionary<int, List<(int To, string? Cond)>>();
            foreach (var w in root.Workflows)
            {
                var outs = ParseNext(w.NextWorkflows);
                if (outs.Count > 0)
                    adj[w.Index] = outs;
            }

            // BFS forward to collect nodes/edges
            var nodes = new HashSet<int> { startIndex };
            var edges = new List<EdgeSpec>();
            var q = new Queue<(int Node, int Depth)>();
            q.Enqueue((startIndex, 0));

            while (q.Count > 0)
            {
                var (cur, depth) = q.Dequeue();
                if (depth >= maxDepth) continue;

                if (!adj.TryGetValue(cur, out var outs)) continue;

                foreach (var (to, cond) in outs)
                {
                    edges.Add(new EdgeSpec(cur, to, cond));

                    if (nodes.Add(to))
                        q.Enqueue((to, depth + 1));
                }
            }

            // Build MSAGL graph
            var g = new Graph
            {
                Attr =
            {
                // Layout/Appearance tuning
                LayerDirection = LayerDirection.LR
            }
            };

            // Nodes
            foreach (var idx in nodes.OrderBy(i => i))
            {
                var wf = byIndex[idx];
                var n = g.AddNode(idx.ToString());

                n.LabelText = $"{wf.Index} ({wf.FunctionIndex})";
                n.UserData = wf;

                // shape: Decision diamond, else box
                var type = (wf.Type ?? "").Trim();
                n.Attr.Shape = type.Equals("Decision", StringComparison.OrdinalIgnoreCase)
                    ? Shape.Diamond
                    : Shape.Box;

                // special node colors
                if (wf.FunctionIndex == 6)
                    n.Attr.FillColor = Color.Red;
                else if (wf.FunctionIndex == 7)
                    n.Attr.FillColor = Color.Orange;

                // highlight start node a bit
                if (idx == startIndex)
                    n.Attr.LineWidth = 2;
            }

            // Edges
            foreach (var e in edges)
            {
                var edge = g.AddEdge(e.From.ToString(), e.To.ToString());

                if (!string.IsNullOrWhiteSpace(e.Condition))
                {
                    edge.LabelText = e.Condition;         // condition text shown on edge
                    edge.Attr.Color = Color.Orange;       // conditional edges orange
                    edge.Attr.LineWidth = 2;
                }
                else
                {
                    edge.Attr.Color = Color.Green;        // unconditional edges green
                }
            }

            return (g, byIndex);
        }

        private static List<(int To, string? Cond)> ParseNext(string? nextWorkflows)
        {
            var res = new List<(int To, string? Cond)>();
            var next = (nextWorkflows ?? "").Trim();
            if (string.IsNullOrWhiteSpace(next)) return res;

            foreach (var token in next.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string? cond = null;
                var target = token;

                var p = token.IndexOf(':');
                if (p > 0)
                {
                    cond = token[..p].Trim();
                    target = token[(p + 1)..].Trim();
                }

                if (int.TryParse(target, out var to))
                    res.Add((to, cond));
            }

            return res;
        }
    }
}
