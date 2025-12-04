using System.Text.Json;

List<string> Flatten(JsonElement arr)
{
    var res = new List<string>();

    foreach (var x in arr.EnumerateArray())
    {
        if (x.ValueKind == JsonValueKind.Array)
        {
            foreach (var y in x.EnumerateArray())
                res.Add(y.GetString()!);
        }
        else
        {
            res.Add(x.GetString()!);
        }
    }

    return res;
}

List<string> CollectObjects(JsonElement A, JsonElement B)
{
    var objs = new List<string>();

    void AddIfNotExists(string val)
    {
        if (!objs.Contains(val))
            objs.Add(val);
    }

    foreach (var x in Flatten(A))
        AddIfNotExists(x);

    foreach (var x in Flatten(B))
        AddIfNotExists(x);

    return objs;
}

List<List<string>> BuildBlocks(JsonElement r)
{
    var blocks = new List<List<string>>();

    foreach (var el in r.EnumerateArray())
    {
        if (el.ValueKind == JsonValueKind.Array)
        {
            var block = el.EnumerateArray().Select(e => e.GetString()!).ToList();
            blocks.Add(block);
        }
        else
        {
            blocks.Add(new List<string> { el.GetString()! });
        }
    }

    return blocks;
}

int[][] BuildMatrix(List<List<string>> blocks, List<string> objs)
{
    int n = objs.Count;
    var pos = objs.Select((o, i) => (o, i)).ToDictionary(t => t.o, t => t.i);

    var M = new int[n][];
    for (int i = 0; i < n; i++)
        M[i] = new int[n];

    for (int i = 0; i < blocks.Count; i++)
    {
        var block = blocks[i];

        foreach (var a in block)
            foreach (var b in block)
                M[pos[a]][pos[b]] = 1;

        for (int j = i + 1; j < blocks.Count; j++)
        {
            var right = blocks[j];

            foreach (var a in block)
                foreach (var b in right)
                    M[pos[a]][pos[b]] = 1;
        }
    }

    return M;
}

List<List<string>> FindConflicts(int[][] MA, int[][] MB, List<string> objs)
{
    int n = objs.Count;
    var conflicts = new List<List<string>>();

    for (int i = 0; i < n; i++)
    {
        for (int j = i + 1; j < n; j++)
        {
            bool a1 = MA[i][j] == 1 && MA[j][i] == 0;
            bool a2 = MA[j][i] == 1 && MA[i][j] == 0;

            bool b1 = MB[i][j] == 1 && MB[j][i] == 0;
            bool b2 = MB[j][i] == 1 && MB[i][j] == 0;

            if ((a1 && b2) || (a2 && b1))
                conflicts.Add(new List<string> { objs[i], objs[j] });
        }
    }

    return conflicts;
}

object ApplyConflicts(List<List<string>> conflicts, List<string> objs)
{
    var used = new HashSet<string>();
    var result = new List<object>();

    foreach (var obj in objs)
    {
        if (used.Contains(obj))
            continue;

        var group = new List<string> { obj };

        foreach (var c in conflicts)
        {
            if (c.Contains(obj))
            {
                foreach (var x in c)
                {
                    if (x != obj)
                    {
                        group.Add(x);
                        used.Add(x);
                    }
                }
            }
        }

        used.Add(obj);

        if (group.Count > 1)
        {
            group.Sort();
            result.Add(group);
        }
        else
        {
            result.Add(obj);
        }
    }

    return result;
}

JsonElement LoadRanking(string path)
{
    var json = File.ReadAllText(path);
    using var doc = JsonDocument.Parse(json);
    return doc.RootElement.Clone();
}

void SaveJson(object data)
{
    string savePath = Path.Combine(AppContext.BaseDirectory, "output.json");

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    File.WriteAllText(savePath, JsonSerializer.Serialize(data, options));
}


if (args.Length < 2)
{
    Console.WriteLine("Usage: dotnet run <pathA.json> <pathB.json>");
    return;
}

var A = LoadRanking(args[0]);
var B = LoadRanking(args[1]);

var objs = CollectObjects(A, B);
var blocksA = BuildBlocks(A);
var blocksB = BuildBlocks(B);

var MA = BuildMatrix(blocksA, objs);
var MB = BuildMatrix(blocksB, objs);

var conflicts = FindConflicts(MA, MB, objs);
var ranking = ApplyConflicts(conflicts, objs);

var output = new
{
    conflicts,
    ranking
};

SaveJson(output);
var printOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

Console.WriteLine(JsonSerializer.Serialize(ranking, printOptions));
