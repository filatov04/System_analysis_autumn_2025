

string filePath = "./task2.csv";
if (!File.Exists(filePath))
{
    Console.WriteLine($"Файл {filePath} не найден.");
    return;
}

string csvContent = File.ReadAllText(filePath).Trim();
string root = "1";

var result = ProcessGraph(csvContent, root);

PrintMatrix(result.Item1, "r1 — непосредственное управление");
PrintMatrix(result.Item2, "r2 — непосредственное подчинение");
PrintMatrix(result.Item3, "r3 — опосредованное управление");
PrintMatrix(result.Item4, "r4 — опосредованное подчинение");
PrintMatrix(result.Item5, "r5 — соподчинение на одном уровне");


void PrintMatrix(List<List<bool>> matrix, string title)
{
    Console.WriteLine($"\n{title}");
    foreach (var row in matrix)
    {
        Console.WriteLine(string.Join(" ", row.Select(x => x ? "1" : "0")));
    }
}

Tuple<
    List<List<bool>>,
    List<List<bool>>,
    List<List<bool>>,
    List<List<bool>>,
    List<List<bool>>> ProcessGraph(string s, string e)
{
    var edges = s.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(line => line.Split(',').Select(int.Parse).ToArray())
                 .ToList();

    var vertices = edges.SelectMany(x => x).Distinct().OrderBy(x => x).ToList();
    int n = vertices.Count;
    var index = vertices.Select((v, i) => new { v, i }).ToDictionary(x => x.v, x => x.i);

    var adj = CreateMatrix(n);
    foreach (var e1 in edges)
        adj[index[e1[0]]][index[e1[1]]] = true;

    // r1 — непосредственное управление
    var r1 = CloneMatrix(adj);

    // r2 — непосредственное подчинение
    var r2 = TransposeMatrix(r1);

    // r3 — опосредованное управление
    var reachable = TransitiveClosure(adj);
    var r3 = CreateMatrix(n);
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            if (reachable[i][j] && !r1[i][j])
                r3[i][j] = true;

    // r4 — опосредованное подчинение
    var r4 = TransposeMatrix(r3);

    // r5 — соподчинение (одинаковый уровень)
    var r5 = CreateMatrix(n);
    var rootIndex = index[int.Parse(e)];
    var levels = GetLevels(rootIndex, adj);
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            if (i != j && levels[i] != -1 && levels[i] == levels[j])
                r5[i][j] = true;

    return Tuple.Create(r1, r2, r3, r4, r5);
}

List<List<bool>> CreateMatrix(int n)
{
    var m = new List<List<bool>>(n);
    for (int i = 0; i < n; i++)
        m.Add(Enumerable.Repeat(false, n).ToList());
    return m;
}

List<List<bool>> CloneMatrix(List<List<bool>> src)
{
    return src.Select(row => new List<bool>(row)).ToList();
}

List<List<bool>> TransposeMatrix(List<List<bool>> m)
{
    int n = m.Count;
    var t = CreateMatrix(n);
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            t[j][i] = m[i][j];
    return t;
}

List<List<bool>> TransitiveClosure(List<List<bool>> adj)
{
    int n = adj.Count;
    var closure = CloneMatrix(adj);
    for (int k = 0; k < n; k++)
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                closure[i][j] = closure[i][j] || (closure[i][k] && closure[k][j]);
    return closure;
}

int[] GetLevels(int root, List<List<bool>> adj)
{
    int n = adj.Count;
    int[] levels = Enumerable.Repeat(-1, n).ToArray();
    var q = new Queue<int>();
    levels[root] = 0;
    q.Enqueue(root);

    while (q.Count > 0)
    {
        int v = q.Dequeue();
        for (int i = 0; i < n; i++)
        {
            if (adj[v][i] && levels[i] == -1)
            {
                levels[i] = levels[v] + 1;
                q.Enqueue(i);
            }
        }
    }
    return levels;
}
