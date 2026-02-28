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
                    n.Attr.FillColor = Color.LightBlue;
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

        public static (Graph Graph, Dictionary<int, Workflow> ByIndex) BuildBidirectionalSubgraphFromJsonFile(
        string jsonPath,
        int startIndex,
        int depthIncoming,
        int depthOutgoing)
        {
            var json = File.ReadAllText(jsonPath);
            var root = JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("JSON konnte nicht gelesen werden.");

            var byIndex = root.Workflows.ToDictionary(w => w.Index, w => w);

            if (!byIndex.ContainsKey(startIndex))
                throw new Exception($"Start-Index {startIndex} existiert nicht in der Datei.");

            // Build outgoing adjacency and incoming adjacency
            var outgoing = new Dictionary<int, List<(int To, string? Cond)>>();
            var incoming = new Dictionary<int, List<(int From, string? Cond)>>();

            foreach (var w in root.Workflows)
            {
                var outs = ParseNext(w.NextWorkflows);

                if (outs.Count > 0)
                    outgoing[w.Index] = outs;

                foreach (var (to, cond) in outs)
                {
                    if (!incoming.TryGetValue(to, out var list))
                    {
                        list = new List<(int From, string? Cond)>();
                        incoming[to] = list;
                    }
                    list.Add((w.Index, cond));
                }
            }

            var nodes = new HashSet<int> { startIndex };
            var edges = new HashSet<(int From, int To, string? Cond)>();

            // OUTGOING traversal (what startIndex triggers)
            if (depthOutgoing > 0)
            {
                var q = new Queue<(int Node, int Depth)>();
                var visited = new HashSet<int> { startIndex };
                q.Enqueue((startIndex, 0));

                while (q.Count > 0)
                {
                    var (cur, depth) = q.Dequeue();
                    if (depth >= depthOutgoing) continue;

                    if (!outgoing.TryGetValue(cur, out var outs)) continue;

                    foreach (var (to, cond) in outs)
                    {
                        nodes.Add(to);
                        edges.Add((cur, to, cond));

                        if (visited.Add(to))
                            q.Enqueue((to, depth + 1));
                    }
                }
            }

            // INCOMING traversal (who calls startIndex)
            if (depthIncoming > 0)
            {
                var q = new Queue<(int Node, int Depth)>();
                var visited = new HashSet<int> { startIndex };
                q.Enqueue((startIndex, 0));

                while (q.Count > 0)
                {
                    var (cur, depth) = q.Dequeue();
                    if (depth >= depthIncoming) continue;

                    if (!incoming.TryGetValue(cur, out var ins)) continue;

                    foreach (var (from, cond) in ins)
                    {
                        nodes.Add(from);

                        // IMPORTANT: edge direction stays real: from -> cur
                        edges.Add((from, cur, cond));

                        if (visited.Add(from))
                            q.Enqueue((from, depth + 1));
                    }
                }
            }

            // Build MSAGL graph
            var g = new Graph
            {
                Attr = { LayerDirection = LayerDirection.LR }
            };

            // Nodes (only those collected)
            foreach (var idx in nodes.OrderBy(i => i))
            {
                var wf = byIndex[idx];
                var n = g.AddNode(idx.ToString());

                // Requirement: node shows "Index (FunctionIndex)"
                n.LabelText = $"{wf.Index} ({wf.FunctionIndex})";
                n.UserData = wf;

                // Decision -> diamond else box
                var type = (wf.Type ?? "").Trim();
                n.Attr.Shape = type.Equals("Decision", StringComparison.OrdinalIgnoreCase)
                    ? Shape.Diamond
                    : Shape.Box;

                // Requirement: FunctionIndex colors
                if (wf.FunctionIndex == 6)
                    n.Attr.FillColor = Color.Red;
                else if (wf.FunctionIndex == 7)
                    n.Attr.FillColor = Color.Orange;

                // start node slightly emphasized
                if (idx == startIndex)
                    n.Attr.FillColor = Color.LightBlue;
            }

            // Edges (only those collected)
            foreach (var (from, to, cond) in edges.OrderBy(e => e.From).ThenBy(e => e.To))
            {
                // avoid edges to nodes not present (shouldn't happen, but safe)
                if (!nodes.Contains(from) || !nodes.Contains(to)) continue;

                var e = g.AddEdge(from.ToString(), to.ToString());

                // Requirement: label on edge if condition exists
                if (!string.IsNullOrWhiteSpace(cond))
                {
                    e.LabelText = cond;
                    e.Attr.Color = Color.Orange; // conditional edges orange
                    e.Attr.LineWidth = 2;
                }
                else
                {
                    e.Attr.Color = Color.Green;  // unconditional edges green
                }
            }

            return (g, byIndex);
        }
    }
}
