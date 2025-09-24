string input = "./task0.csv";

bool directed = true; // ориентированный граф - true

var matrix = BuildAdjacencyFromCsv(input, directed);

foreach (var row in matrix)
    Console.WriteLine(string.Join(" ", row));

List<List<int>> BuildAdjacencyFromCsv(string csvOrPath, bool directed)
{
    var edges = new List<(int u, int v)>();
    IEnumerable<string> lines = File.Exists(csvOrPath)
        ? File.ReadLines(csvOrPath)
        : csvOrPath.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

    foreach (var raw in lines)
    {
        var line = raw.Trim();
        if (line.Length == 0 || line.StartsWith("#")) continue;

        var parts = line.Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) continue;

        if (int.TryParse(parts[0], out var u) && int.TryParse(parts[1], out var v))
            edges.Add((u, v));
    }

    var vertices = edges.SelectMany(e => new[] { e.u, e.v })
        .Distinct()
        .OrderBy(x => x)
        .ToList();

    var idx = vertices.Select((val, i) => (val, i)).ToDictionary(t => t.val, t => t.i);
    int n = vertices.Count;

    var matrix = Enumerable.Range(0, n)
        .Select(_ => Enumerable.Repeat(0, n).ToList())
        .ToList();

    foreach (var (u, v) in edges)
    {
        matrix[idx[u]][idx[v]] = 1;       // направление u -> v
        if (!directed) matrix[idx[v]][idx[u]] = 1;
    }

    return matrix;
}