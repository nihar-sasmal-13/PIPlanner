using Graphviz4Net.Graphs;
using PIPlanner.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace PIPlanner.Helpers
{
    static class GitGraphBuider
    {
        public static Graph<GitItem> Build(IEnumerable<ChangeRequest> crs)
        {
            var graph = new Graph<GitItem> { Rankdir = RankDirection.LeftToRight, };
            graph.AddVertex(buildItem("r6.2start"));
            graph.AddVertex(buildItem("r6.2end"));

            int minSprintId = int.MaxValue;
            int maxSprintId = int.MinValue;
            int previousSprintId = 0;
            foreach (int sprintId in crs.Where(cr => cr.SprintId != 0).Select(cr => cr.SprintId).Distinct().OrderBy(i => i))
            {
                if (minSprintId > sprintId) minSprintId = sprintId;
                if (maxSprintId < sprintId) maxSprintId = sprintId;
                graph.AddVertex(buildItem($"sp{sprintId}s"));
                graph.AddVertex(buildItem($"sp{sprintId}e"));
                graph.AddEdge(new Edge<GitItem>(buildItem($"sp{sprintId}s"), buildItem($"sp{sprintId}e")) { Weight = 16 });

                if (previousSprintId != 0)
                    graph.AddEdge(new Edge<GitItem>(buildItem($"sp{previousSprintId}e"), buildItem($"sp{sprintId}s")) { Weight = 16 });
                previousSprintId = sprintId;
            }

            graph.AddEdge(new Edge<GitItem>(buildItem("r6.2start"), buildItem($"sp{minSprintId}s")) { Weight = 16 });
            graph.AddEdge(new Edge<GitItem>(buildItem($"sp{maxSprintId}e"), buildItem("r6.2end")) { Weight = 16 });

            int fCounter = 1;
            foreach (var functionalGroup in crs.Where(cr => cr.SprintId != 0).GroupBy(cr => cr.FunctionalArea))
            {
                if (string.IsNullOrEmpty(functionalGroup.Key) || functionalGroup.Key.Contains("Infra"))
                    continue;

                int firstSprintId = functionalGroup.Min(cr => cr.SprintId).GetValueOrDefault();
                int lastSprintId = functionalGroup.Max(cr => cr.SprintId).GetValueOrDefault();

                if (firstSprintId == lastSprintId)
                {
                    graph.AddVertex(buildItem($"f{fCounter}sp{firstSprintId}s"));
                    graph.AddVertex(buildItem($"f{fCounter}sp{firstSprintId}e"));
                    graph.AddEdge(new Edge<GitItem>(buildItem($"f{fCounter}sp{firstSprintId}s"), buildItem($"f{fCounter}sp{firstSprintId}e")) { Weight = 8 });
                    graph.AddEdge(new Edge<GitItem>(buildItem($"sp{firstSprintId}s"), buildItem($"f{fCounter}sp{firstSprintId}s")) { Weight = 1 });
                    graph.AddEdge(new Edge<GitItem>(buildItem($"f{fCounter}sp{firstSprintId}e"), buildItem($"sp{firstSprintId}e")) { Weight = 1 });
                }
                else
                {
                    for (int i = firstSprintId; i <= lastSprintId; i++)
                    {
                        graph.AddVertex(buildItem($"f{fCounter}sp{i}s", (i > firstSprintId && i < lastSprintId)));
                        graph.AddVertex(buildItem($"f{fCounter}sp{i}e", (i > firstSprintId && i < lastSprintId)));
                        graph.AddEdge(new Edge<GitItem>(buildItem($"f{fCounter}sp{i}s"), buildItem($"f{fCounter}sp{i}e")) { Weight = 8 });

                        if (i == firstSprintId)
                            graph.AddEdge(new Edge<GitItem>(buildItem($"sp{i}s"), buildItem($"f{fCounter}sp{i}s")) { Weight = 1 });
                        else
                        {
                            graph.AddEdge(new Edge<GitItem>(buildItem($"f{fCounter}sp{i - 1}e"), buildItem($"f{fCounter}sp{i}s")) { Weight = 8 });

                            if (i == lastSprintId)
                                graph.AddEdge(new Edge<GitItem>(buildItem($"f{fCounter}sp{i}e"), buildItem($"sp{i}e")) { Weight = 1 });
                        }
                    }
                }
                foreach (var cr in functionalGroup)
                {
                    graph.AddVertex(buildItem($"cr{cr.Id}"));
                    GitItem src = buildItem($"f{fCounter}sp{cr.SprintId}s");
                    GitItem dest = buildItem($"f{fCounter}sp{cr.SprintId}e");
                    graph.AddEdge(new Edge<GitItem>(src, buildItem($"cr{cr.Id}")) { Weight = 1 });
                    graph.AddEdge(new Edge<GitItem>(buildItem($"cr{cr.Id}"), dest) { Weight = 1 });
                }
                fCounter++;
            }
            return graph;
        }

        private static GitItem buildItem(string itemName, bool isIntermediateSprint = false)
        {
            if (cache.ContainsKey(itemName))
                return cache[itemName];

            GitItem result = null;
            if (itemName.StartsWith("r"))
                result = new GitRelease { Name = itemName };
            else if (itemName.StartsWith("sp"))
                result = new GitSprint { Name = itemName };
            else if (itemName.StartsWith("f") && !isIntermediateSprint)
                result = new GitFeature { Name = itemName };
            else if (itemName.StartsWith("f") && isIntermediateSprint)
                result = new GitFeatureSprint { Name = itemName };
            else if (itemName.StartsWith("cr"))
                result = new GitDCR { Name = itemName };
            else
                result = new GitItem { Name = itemName };
            cache.Add(itemName, result);
            return result;
        }

        private static Dictionary<string, GitItem> cache = new Dictionary<string, GitItem>();
    }
}
