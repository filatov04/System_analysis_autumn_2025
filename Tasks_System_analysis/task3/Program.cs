var path = "./task2.csv";

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не найден: {path}");
    return;
}


string csv = File.ReadAllText(path).Trim();

var edgesTemp = csv.Split(';', StringSplitOptions.RemoveEmptyEntries)
    .Select(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
    .Select(x => (From: int.Parse(x[0].Trim()), To: int.Parse(x[1].Trim())))
    .ToList();

var allNodes = edgesTemp.SelectMany(e => new[] { e.From, e.To }).Distinct();
var childNodes = edgesTemp.Select(e => e.To).Distinct();
var rootCandidates = allNodes.Except(childNodes).ToList();
var rootId = rootCandidates.FirstOrDefault();

var result = CalculateGraphComplexity(csv, rootId);

Console.WriteLine();
Console.WriteLine($"Энтропия структуры: {result.entropy}");
Console.WriteLine($"Нормированная структурная сложность: {result.normalizedComplexity}");
return;

(double entropy, double normalizedComplexity) CalculateGraphComplexity(string csv, int rootId)
{
    var edges = csv.Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
        .Select(x => (From: int.Parse(x[0]), To: int.Parse(x[1])))
        .ToList();

    var nodes = edges.SelectMany(e => new[] { e.From, e.To }).Distinct().ToList();
    int n = nodes.Count;
    int k = 5;

    var r1 = edges.ToList();

    var r2 = edges.Select(e => (e.To, e.From)).ToList();

    var r3 = (from a in r1
        from b in r1
        where a.To == b.From
        select (a.From, b.To)).ToList();

    var r4 = r3.Select(e => (e.To, e.From)).ToList();

    var r5 = new List<(int, int)>();
    foreach (var parent in nodes)
    {
        var children = r1.Where(x => x.From == parent).Select(x => x.To).ToList();
        foreach (var c1 in children)
        foreach (var c2 in children)
            if (c1 != c2)
                r5.Add((c1, c2));
    }

    var relations = new List<List<(int, int)>> { r1, r2, r3, r4, r5 };

    double totalEntropy = 0;
    foreach (var node in nodes)
    {
        foreach (var rel in relations)
        {
            int lij = rel.Count(r => r.Item1 == node);
            if (lij == 0) continue;

            double p = lij / (double)(n - 1);
            double h = -p * Math.Log2(p);
            totalEntropy += h;
        }
    }

    double Href = 0.5307 * n * k;
    double normalized = totalEntropy / Href;

    return (Math.Round(totalEntropy, 1), Math.Round(normalized, 2));
}